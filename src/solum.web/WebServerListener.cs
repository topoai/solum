using solum.core.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.web
{
    public class WebServerListener : HttpListenerService
    {
        public WebServerListener(WebServer server) : base("{0}-listener".format(server.Name)) { }
    }
}
