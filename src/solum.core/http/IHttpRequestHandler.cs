using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.http
{
    public interface IHttpRequestHandler
    {
        bool AsyncSupported { get; }
        bool AcceptRequest(HttpListenerRequest request);
        void HandleRequest(HttpListenerRequest request, HttpListenerResponse response);
        Task HandleRequestAsync(HttpListenerRequest request, HttpListenerResponse response);
    }
}
