using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum.core.http.handlers
{
    public abstract class AsyncHandler : HttpRequestHandler
    {
        public override bool AsyncSupported
        {
            get { return true; }
        }

        protected override void OnHandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            //Log.Warn("Running async request handler in syncronous mode.");
            //OnHandleRequestAsync(request, response).RunSynchronously();
            throw new NotSupportedException("Async handlers do not support syncronous invokation.");
        }
    }
}
