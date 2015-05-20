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
using solum.core.smtp;

namespace solum.core.http
{
    public class HttpListenerService : Service
    {
        const int ACCESS_DENIED_ERROR_CODE = 5;

        protected HttpListenerService(string name)
            : base(name)
        {            
            this.NetshRegistrationEnabled = true;
            this.NetshRegistraionUser = "Everyone";
            this.RequestHandlerTimeout = TimeSpan.FromSeconds(30);
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
        /// <summary>
        /// Collection of handlers to respond to various requests
        /// </summary>
        [JsonProperty("handlers")]
        public List<IHttpRequestHandler> Handlers { get; private set; }        
        /// <summary>
        /// Maximum amount of time to process each Request.  
        /// If requests take longer to compelete, they are canceled.
        /// </summary>
        [JsonProperty("request-handler-timeout")]
        public TimeSpan RequestHandlerTimeout { get; private set; }
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
        /// Concurrent
        /// </summary>
        ConcurrentDictionary<long, Task> m_active_requests;

        protected override void OnLoad()
        {
            if (!HttpListener.IsSupported)
                throw new Exception("HttpListener class is not supported by this OS.");            
            
            base.OnLoad();
        }
        protected override void OnStart()
        {            
            // Start HTTP listener.
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
                    Log.Error(ex, "Failed to start http listener.");
                    throw ex;
                }

                // ** Check if automatic registration is disabled
                if (NetshRegistrationEnabled == false)
                {
                    Log.Error(ex, "Access denied received and Automatic Netsh Registration is disabled.");
                    throw ex;
                }

                Log.Warning(ex, "Access denied received when opening server listener.");
                Log.Debug("Registering urls with netsh to provide access to the user account \"{0}\".", NetshRegistraionUser);
                foreach (var address in Addresses)
                {
                    Log.Information("Registering address... {0}", address);
                    var registrationSucceeded = NetSh.AddUrlAcl(SanitizeUrl(address), NetshRegistraionUser);
                    if (registrationSucceeded == false)
                        throw new Exception("Error registering address using netsh: {0}".format(address));
                }

                // ** Since we automatically registered the addresses using netsh
                //    Retry starting the service
                Log.Information("Registration successful...");
                Log.Information("Restarting listener...");
                StartHttpListener();
            }

            // ** Reset signal and create cancelation token
            m_stopReceived = false;

            // ** Keep a list of active requests
            m_active_requests = new ConcurrentDictionary<long, Task>();

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
            var numActiveRequests = m_active_requests.Count;
            if (numActiveRequests > 0)
            {
                Log.Warning("Waiting for {0} active requests to complete.", m_active_requests.Count);
                Task.WaitAll(m_active_requests.Values.ToArray());
            }

            // ** Complete Active Requests
            /*var numActiveRequests = m_process_request_block.InputCount;
            if (numActiveRequests > 0)
            {
                Log.Warning("Waiting for {0} active requests to complete.", m_receive_request_block.Count);
                m_receive_request_block.Complete();
                m_process_request_block.Completion.Wait();
            }*/

            // ** Stop the http listener
            Log.Debug("Stopping the http listener...");
            m_listener.Stop();

            Log.Information("Processed {0:N0} total requests.", m_requests_received);

            base.OnStop();
        }

        void StartHttpListener()
        {
            Log.Debug("Creating HTTP listener...");
            m_listener = new HttpListener();

            Addresses.ForEach(address =>
            {
                address = SanitizeUrl(address);

                Log.Verbose("Registering address: {0}", address);
                m_listener.Prefixes.Add(address);
            });

            Log.Debug("Starting HTTP listener...");
            m_listener.Start();

            Addresses.ForEach(address => Log.Information("Listening on address: {0}", address));
        }
        async Task HandleRequestsAsync()
        {
            m_requests_received = 0;
            // ** Create a background thread
            // ** Check stop signal
            while (m_stopReceived == false)
            {
                //checkPendingQueueSize();

                Log.Verbose("Waiting for request...");
                var context = await m_listener.GetContextAsync();

                var requestNum = ++m_requests_received;

                Log.Debug("Request #{0:N0} received...", requestNum);

                // Handle the request in the background and move onto the next request
                var httpContext = new HttpContext(requestNum, context, RequestHandlerTimeout);

                // ** Add this to the list of active tasks
                var activeReqeuestTask = HandleRequestAsync(httpContext).ContinueWith(task =>
                {
                    var taskResquestNum = task.Result.RequestNum;

                    Task _;
                    if (!m_active_requests.TryRemove(taskResquestNum, out _))
                        Log.Error("Error removing request #{0:N0} from active requests.", taskResquestNum);
                });

                if (!m_active_requests.TryAdd(requestNum, activeReqeuestTask))
                    Log.Error("Error adding request #{0:N0} to active requests.", requestNum);

                // ** Post the current context to the queue and move onto accept the next request.                
                //if (!m_receive_request_block.Post(httpContext))
                //    throw new Exception("ERROR: Request BUFFER Full!!!");
            }
        }
        async Task<HttpContext> HandleRequestAsync(HttpContext httpContext)
        {
            var requestNum = httpContext.RequestNum;
            var context = httpContext.Context;
            var processTimer = Stopwatch.StartNew();

            Log.Debug("--------------------------------");
            Log.Debug("- Request:Number.............. #{0:N0}", requestNum);

            if (httpContext.Elapsed.TotalMilliseconds > 1000)
                Log.Warning("- Request:Queued.............. {0:N2}ms", httpContext.Elapsed.TotalMilliseconds);
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
                Log.Warning("No Handler found for request: {0}", request.RawUrl);
                response.StatusCode = 404;
            }
            else
            {
                // ** Check if this handler supports async in it's underlying implementation
                try
                {
                    if (handler.AsyncSupported)
                    {
                        Log.Verbose("Handling request asyncronously...");
                        await handler.HandleRequestAsync(request, response, httpContext.CancellationToken);
                    }
                    else
                    {
                        Log.Verbose("Handling request syncronously...");
                        await Task.Run(() => handler.HandleRequest(request, response), httpContext.CancellationToken);
                        //handler.HandleRequest(request, response);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Log.Error("Error Code: {0}", ex.ErrorCode);
                    Log.Error(ex, "The request timed out after {0} seconds before completing...".format(RequestHandlerTimeout.TotalSeconds));

                    response.StatusCode = 500;
                    response.StatusDescription = "The request timed out after {0} seconds before completing...".format(RequestHandlerTimeout.TotalSeconds).RemoveControlCharacters();
                }
                catch (Exception ex)
                {
                    // Log.Error("Error Code: {0}", ex.ErrorCode);
                    Log.Error(ex, "Error handling request: {0}".format(ex));

                    response.StatusCode = 500;
                    response.StatusDescription = "Error handling request: {0}".format(ex).RemoveControlCharacters();
                }
            }

            // ** Flush and close the response
            Log.Verbose("Flushing response stream...");
            response.OutputStream.Flush();
            Log.Verbose("Closing response stream...");
            response.OutputStream.Close();

            processTimer.Stop();
            httpContext.Complete();
            Log.Debug("------------------------------------------");
            Log.Debug("- Request..................... #{0:N0}", requestNum);

            var elapsedTimeMilliseconds = httpContext.Elapsed.TotalMilliseconds;
            if (elapsedTimeMilliseconds > 1000)
                Log.Warning("- Request:ElapsedTime......... {0:N2}ms", httpContext.Elapsed.TotalMilliseconds);
            else
                Log.Debug("- Request:ElapsedTime......... {0:N2}ms", httpContext.Elapsed.TotalMilliseconds);

            Log.Debug("- Request:ProcessTime......... {0:N2}ms", processTimer.ElapsedMilliseconds);
            Log.Debug("- Response:Content-Encoding... {0}", context.Response.ContentEncoding);
            Log.Debug("- Response:Content-Type....... {0}", context.Response.ContentType);
            Log.Debug("- Response:Content-Length..... {0}", context.Response.ContentLength64);
            Log.Debug("------------------------------------------");

            //checkPendingQueueSize();

            return httpContext;
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