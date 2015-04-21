using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum.core.http.handlers
{
    public abstract class SyncHandler : HttpRequestHandler
    {
        public override bool AsyncSupported
        {
            get { return false; }
        }

        protected override Task OnHandleRequestAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken cancellationToken)
        {
            //Log.Warn("Async not supported.");
            //Log.Trace("Running Sync Request Handler in a background thread.");
            //return Task.Run(() => OnHandleRequest(request, response));
            throw new Exception("Sync http handlers do not support async invokation");
        }
    }
}
