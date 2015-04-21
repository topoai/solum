using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace solum.core.http
{
    public class HttpContext
    {
        public HttpContext(long requestNum, HttpListenerContext context, TimeSpan timeout)
        {
            this.RequestNum = requestNum;
            this.Context = context;
            this.m_timer = Stopwatch.StartNew();
            this.IsCompleted = false;
            this.m_timeout = new CancellationTokenSource(timeout);
        }

        Stopwatch m_timer;
        CancellationTokenSource m_timeout;

        public bool IsCompleted { get; private set; }
        public long RequestNum { get; private set; }
        public HttpListenerContext Context { get; private set; }
        public TimeSpan Elapsed { get { return m_timer.Elapsed; } }
        public CancellationToken CancellationToken { get { return m_timeout.Token; } }
        public void Complete()
        {
            m_timer.Stop();
            IsCompleted = true;
        }
    }
}
