using solum.core;
using solum.core.http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.web
{
    public abstract class WebResponse : IWebResponse
    {
        public const string DEFAULT_CONTENT_TYPE = "text/plain";
        public const long DEFAULT_CONTENT_LENGTH = -1;

        public WebResponse() : this(DEFAULT_CONTENT_TYPE) { }
        protected WebResponse(string contentType) : this(contentType, DEFAULT_CONTENT_LENGTH) { }
        protected WebResponse(string contentType, long contentLength)
        {
            this.ContentType = contentType;
            this.ContentLength = contentLength;
        }

        public string ContentType { get; private set; }
        public long ContentLength { get; private set; }
        public abstract Stream Content { get; }
    }
}
