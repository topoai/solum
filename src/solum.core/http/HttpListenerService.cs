using Newtonsoft.Json;
using solum.core;
using solum.extensions;
using solum.core.http.handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace solum.core.http
{
    public class HttpListenerService : Service
    {
        const int ACCESS_DENIED_ERROR_CODE = 5;
        readonly static int DEFAULT_MAX_REQUESTS = Environment.ProcessorCount;

        protected HttpListenerService()
        {
            this.MaxActiveRequests = DEFAULT_MAX_REQUESTS;
            this.NetshRegistrationEnabled = true;
            this.NetshRegistraionUser = "Everyone";
            this.Addresses = new List<string>();
            this.Handlers = new List<IHttpRequestHandler>();
        }

        #region Configurable Properties
        /// <summary>
        /// The addresses to listen for requests on the local machine.
        /// </summary>
        [JsonProperty("addresses")]
        public List<string> Addresses { get; private set; }
        /// <summary>
        /// Specifies whether to automatically register the url using netsh 
        /// if an access denied message is received when opening a listener
        /// </summary>
        [JsonProperty("netsh-registration-enabled", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool NetshRegistrationEnabled { get; private set; }
        /// <summary>
        /// The User to register urls with if Access Denied message is received when opening a listener
        /// </summary>
        [JsonProperty("netsh-registration-user", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("Everyone")]
        public string NetshRegistraionUser { get; private set; }
        [JsonProperty("default-response")]
        public string DefaultResponse { get; private set; }
        /// <summary>
        /// Collection of handlers to respond to various requests
        /// </summary>
        [JsonProperty("handlers")]
        public List<IHttpRequestHandler> Handlers { get; private set; }
        /// <summary>
        /// Sets the maximum number of active requests at any given time. 
        /// New requests will block once this limit is received.
        /// Setting to zero means unbounded.
        /// </summary>
        [JsonProperty("max-active-requests", DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaxActiveRequests { get; private set; }
        #endregion

        /// <summary>
        /// Flag to indicate we should stop receiving requests
        /// </summary>
        bool m_stopReceived;
        /// <summary>
        /// Listens for http requests over the specified addresses
        /// </summary>
        HttpListener m_listener;
        /// <summary>
        /// Background task to receive requests from clients
        /// </summary>
        Task m_receiveRequestTask;
        /// <summary>
        /// Concurrent
        /// </summary>
        //ConcurrentDictionary<long, Task> m_active_requests;
        /// <summary>
        /// Counter of requests received during the lifetime of this service.
        /// </summary>
        long m_requests_received;
        /// <summary>
        /// Incomming queue for active connections
        /// </summary>
        BufferBlock<HttpContext> m_receive_request_block;
        /// <summary>
        /// Provides in order handling of http requests.
        /// Can be parallized by setting MaxActiveRequests > 1
        /// </summary>
        ActionBlock<HttpContext> m_process_request_block;
        // ActionBlock<HttpContext> m_complete_context;

        protected override void OnLoad()
        {
            if (!HttpListener.IsSupported)
                throw new Exception("HttpListener class is not supported by this OS.");

            // *** Initialize context request queue
            var options = new ExecutionDataflowBlockOptions();
            //if (MaxActiveRequests > 0)
            //    options.BoundedCapacity = MaxActiveRequests;

            // *** Set number of parallel workers to the size of the processor
            // TODO: Make configurable
            Log.Debug("Max active requests: {0}", MaxActiveRequests);
            // options.BoundedCapacity = MaxActiveRequests;
            options.MaxDegreeOfParallelism = MaxActiveRequests;

            m_receive_request_block = new BufferBlock<HttpContext>();
            m_process_request_block = new ActionBlock<HttpContext>(context => HandleRequestAsync(context), options);

            m_receive_request_block.LinkTo(m_process_request_block, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });


            base.OnLoad();
        }
        protected override void OnStart()
        {
            // Create a listener.
            try
            {
                StartHttpListener();
            }
            catch (HttpListenerException ex)
            {
                // ** Ensure we received an access is denied error
                //    not some other issue such as running two servers as once
                if (ex.ErrorCode != ACCESS_DENIED_ERROR_CODE)
                {
                    Log.ErrorException("Failed to start http listener.", ex);
                    throw ex;
                }

                // ** Check if automatic registration is disabled
                if (NetshRegistrationEnabled == false)
                {
                    Log.ErrorException("Access denied received and Automatic Netsh Registration is disabled.", ex);
                    throw ex;
                }

                Log.WarnException("Access denied received when opening server listener.", ex);
                Log.Debug("Registering urls with netsh to provide access to the user account \"{0}\".", NetshRegistraionUser);
                foreach (var address in Addresses)
                {
                    Log.Info("Registering address... {0}", address);
                    var registrationSucceeded = NetSh.AddUrlAcl(SanitizeUrl(address), NetshRegistraionUser);
                    if (registrationSucceeded == false)
                        throw new Exception("Error registering address using netsh: {0}".format(address));
                }

                // ** Since we automatically registered the addresses using netsh
                //    Retry starting the service
                Log.Info("Registration successful...");
                Log.Info("Restarting listener...");
                StartHttpListener();
            }

            // ** Reset signal and create cancelation token
            m_stopReceived = false;

            // ** Start the request handler
            Log.Debug("Starting the request handler...");
            m_receiveRequestTask = HandleRequestsAsync();

            base.OnStart();
        }
        protected override void OnStop()
        {
            // ** Signal we are stopping
            m_stopReceived = true;

            // ** Complete Active Requests
            var numActiveRequests = m_process_request_block.InputCount;
            if (numActiveRequests > 0)
            {
                Log.Warn("Waiting for {0} active requests to complete.", m_receive_request_block.Count);
                m_receive_request_block.Complete();
                m_process_request_block.Completion.Wait();
            }

            // ** Stop the http listener
            Log.Debug("Stopping the http listener...");
            m_listener.Stop();

            Log.Info("Processed {0:N0} total requests.", m_requests_received);

            base.OnStop();
        }

        void StartHttpListener()
        {
            Log.Debug("Creating HTTP listener...");
            m_listener = new HttpListener();

            Addresses.ForEach(address =>
            {
                address = SanitizeUrl(address);

                Log.Trace("Registering address: {0}", address);
                m_listener.Prefixes.Add(address);
            });

            Log.Debug("Starting HTTP listener...");
            m_listener.Start();

            Addresses.ForEach(address => Log.Info("Listening on address: {0}", address));
        }

        async Task HandleRequestsAsync()
        {
            m_requests_received = 0;
            // ** Create a background thread
            // ** Check stop signal
            while (m_stopReceived == false)
            {
                // ** Receive the next request
                // HttpListenerContext context;

                if (m_receive_request_block.Count > 0 || m_process_request_block.InputCount > 0)
                    Log.Debug("{0}->{1} pending requests...", m_receive_request_block.Count, m_process_request_block.InputCount);

                Log.Trace("Waiting for request...");
                var context = await m_listener.GetContextAsync();

                var requestNum = ++m_requests_received;

                Log.Debug("Request #{0:N0} received...", requestNum);
                // TODO: Use a dataprocess here to queue up requests and control the number of active tasks and their completion
                // Handle the request in the background and move ont o rec
                var httpContext = new HttpContext(requestNum, context);

                // ** Post the current context to the queue and move onto accept the next request.
                //if (!m_process_request_block.Post(httpContext))
                if (!m_receive_request_block.Post(httpContext))
                {
                    throw new Exception("ERROR: Request BUFFER Full!!!");
                }
            }
        }
        async Task<long> HandleRequestAsync(HttpContext httpContext)
        {
            var requestNum = httpContext.RequestNum;
            var context = httpContext.Context;
            var processTimer = Stopwatch.StartNew();

            Log.Debug("--------------------------------");
            Log.Debug("- Request:Number.............. #{0:N0}", requestNum);

            if (httpContext.Elapsed.TotalMilliseconds > 1000)
                Log.Warn("- Request:Queued.............. {0:N2}ms", httpContext.Elapsed.TotalMilliseconds);
            else
                Log.Debug("- Request:Queued.............. {0:N2}ms", httpContext.Elapsed.TotalMilliseconds);

            Log.Debug("- Request:Url................. {0}", context.Request.RawUrl);
            Log.Debug("- Request:Content-Encoding.... {0}", context.Request.ContentEncoding);
            Log.Debug("- Request:Content-Type........ {0}", context.Request.HttpMethod);
            Log.Debug("- Request:Content-Length...... {0}", context.Request.ContentLength64);

            // ** Check if we can find a handler for this request
            var request = context.Request;
            var response = context.Response;

            var handler = Handlers.FirstOrDefault(h => h.AcceptRequest(request));
            if (handler == null)
            {
                // ** Load the default handler since we have none
                // TODO: Return 404

                //response.Write("text/plain", responseString);
                Log.Warn("No Handler found for request: {0}", request.RawUrl);
                response.StatusCode = 404;
            }
            else
            {
                // ** Check if this handler supports async in it's underlying implementation
                try
                {
                    if (handler.AsyncSupported)
                    {
                        Log.Trace("Handling request asyncronously...");
                        await handler.HandleRequestAsync(request, response);
                    }
                    else
                    {
                        Log.Trace("Handling request syncronously...");
                        handler.HandleRequest(request, response);
                    }
                }
                catch (HttpListenerException ex)
                {
                    Log.Error("Error Code: {0}", ex.ErrorCode);
                    Log.ErrorException("Error handling request: {0}".format(ex), ex);

                    response.StatusCode = 500;
                    response.StatusDescription = "Error handling request: {0}".format(ex).RemoveControlCharacters();
                }
            }

            // ** Flush and close the response
            Log.Trace("Flushing response...");
            response.OutputStream.Flush();
            Log.Trace("Closing response...");
            response.OutputStream.Close();

            processTimer.Stop();
            httpContext.Complete();
            Log.Debug("------------------------------------------");
            Log.Debug("- Request..................... #{0}", requestNum);

            var elapsedTimeMilliseconds = httpContext.Elapsed.TotalMilliseconds;
            if (elapsedTimeMilliseconds > 1000)
                Log.Warn("- Request:ElapsedTime......... {0:N2}ms", httpContext.Elapsed.TotalMilliseconds);
            else
                Log.Debug("- Request:ElapsedTime......... {0:N2}ms", httpContext.Elapsed.TotalMilliseconds);

            Log.Debug("- Request:ProcessTime......... {0:N2}ms", processTimer.ElapsedMilliseconds);
            Log.Debug("- Response:Content-Encoding... {0}", context.Response.ContentEncoding);
            Log.Debug("- Response:Content-Type....... {0}", context.Response.ContentType);
            Log.Debug("- Response:Content-Length..... {0}", context.Response.ContentLength64);
            Log.Debug("------------------------------------------");

            Log.Debug("{0}->{1} pending requests...", m_receive_request_block.Count, m_process_request_block.InputCount);

            return requestNum;
        }
        static string SanitizeUrl(string address)
        {
            // ** Sanitize the address
            address = address.Trim(' ', '\\');
            if (address.EndsWith("/") == false)
                address += "/";
            return address;
        }
    }
}