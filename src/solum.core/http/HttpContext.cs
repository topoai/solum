using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.http
{
    public class HttpContext
    {
        public HttpContext(long requestNum, HttpListenerContext context)
        {
            this.RequestNum = requestNum;
            this.Context = context;
            this.m_timer = Stopwatch.StartNew();
            this.IsCompleted = false;
        }

        Stopwatch m_timer;

        bool IsCompleted { get; private set; }
        public long RequestNum { get; private set; }        
        public HttpListenerContext Context { get; private set; }
        public TimeSpan Elapsed { get { return m_timer.Elapsed; } }
        public void Complete()
        {
            m_timer.Stop();
            IsCompleted = true;
        }
    }
}
