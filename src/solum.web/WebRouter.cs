using solum.core.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace solum.web
{
    public class WebRouter : HttpRequestHandler
    {
        public WebRouter(string method, string prefix, Func<HttpListenerRequest, Task<WebResponse>> getResponse)
        {
            this.m_method = method;
            this.m_url = sanitizePrefix(prefix);
            this.m_get_response_async = getResponse;
        }
        public WebRouter(string method, string prefix, Func<HttpListenerRequest, WebResponse> getResponse)
        {
            this.m_method = method;
            this.m_url = sanitizePrefix(prefix);
            this.m_get_response = getResponse;
        }
        public WebRouter(string method, string prefix, WebResponse response) : this(method, prefix, _ => response) { }

        #region Private members
        string m_method;
        string m_url;
        Func<HttpListenerRequest, WebResponse> m_get_response;
        Func<HttpListenerRequest, Task<WebResponse>> m_get_response_async;
        #endregion

        public override bool AsyncSupported { get { return m_get_response_async != null; } }
        protected override bool OnAcceptRequest(HttpListenerRequest request)
        {
            var path = request.Url.LocalPath;

            // Match EXACT
            if (m_url.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                return true;

            // Sanitize
            path = sanitizePrefix(path);

            // Check Exact matching after cleaning up the prefix
            if (m_url.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                return true;

            /*
            // Check if this starts with
            if (path.StartsWith(m_url, StringComparison.InvariantCultureIgnoreCase))
                return true;
            */

            return false;
        }
        protected override void OnHandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            WebResponse webResponse;
            // ** Run async action Synchronously
            if (m_get_response == null)
            {
                Log.Warning("Running async request handler in syncronous mode...");
                webResponse = m_get_response_async(request).Result;
            }
            else
            {
                webResponse = m_get_response(request);
            }

            // ** Write the Web response to the HTTP response stream
            response.Write(webResponse.ContentType, webResponse.ContentLength, webResponse.Content);
        }
        protected override Task OnHandleRequestAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken cancellationToken)
        {
            if (m_get_response_async == null)
            {
                Log.Warning("Async not supported - running sync request in a background thread...");
                return Task.Run(() => OnHandleRequest(request, response), cancellationToken);
            }

            return m_get_response_async(request);
        }

        static string sanitizePrefix(string prefix)
        {
            prefix = prefix.Trim().Replace('\\', '/');

            if (!prefix.StartsWith("/"))
                prefix = "/" + prefix;

            if (!prefix.EndsWith("/"))
                prefix += '/';

            return prefix;
        }
    }
}
