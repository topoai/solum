using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    public abstract class Component
    {
        protected Component()
        {
            this.Log = Serilog.Log.ForContext(this.GetType());
        }

        protected Component(ILogger log)
        {
            this.Log = log;
        }

        protected ILogger Log { get; private set; }
    }
}
