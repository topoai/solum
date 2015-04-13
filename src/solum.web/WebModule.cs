using solum.core;
using solum.core.http;
using solum.core.http.handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace solum.web
{
    public class WebModule
    {
        public WebModule()
        {
            Routes = new List<WebRouter>();
        }
        public List<WebRouter> Routes { get; private set; }

        #region Static File/Directory Handlers
        public void StaticDirectory(string route, string directory)
        {
        }
        #endregion

        #region Http Methods
        public void Get(string route, Func<HttpListenerRequest, WebResponse> response)
        {
            var router = new WebRouter("GET", route, response);
            Routes.Add(router);
        }
        public void Get(string route, Func<HttpListenerRequest, Task<WebResponse>> response)
        {
            var router = new WebRouter("GET", route, response);
            Routes.Add(router);
        }
        public void Get<T>(string route, T response) where T : WebResponse
        {
            var router = new WebRouter("GET", route, response);
            Routes.Add(router);
        }
        #endregion        
    }
}
