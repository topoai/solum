using solum.core.http;
using solum.web;
using solum.web.responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace solum.modules
{
    public class IndexModule : WebModule
    {
        public IndexModule()
        {            
            Func<HttpListenerRequest, solum.web.WebResponse> notImplemented = request =>
            {
                return new StringResponse("text/html", "<html><body><h2>Not Implemented</h2></body></html>");
            };            

            Get("/hello", View.FromFile("views/hello-world.html"));
            Get("/dashboard", View.FromFile("views/dashboard.html"));

            var indexView = View.FromFile("views/index.html");
            Get("/index/", indexView);
            Get("/", indexView);
        }
    }
}
