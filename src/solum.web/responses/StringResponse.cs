using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.web.responses
{
    public class StringResponse : WebResponse
    {
        public StringResponse(string content) : this("text/plain", content) { }

        public StringResponse(string contentType, string content)
            : base(contentType, content.Length)
        {
            this.m_content = content;
        }

        #region Private Members
        string m_content;
        #endregion

        public override System.IO.Stream Content
        {
            get
            {
                return m_content.toStream();
            }
        }
    }
}
