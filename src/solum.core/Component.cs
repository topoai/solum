using NLog;
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
            Log = LogManager.GetLogger(GetType().FullName);
        }

        protected Logger Log { get; private set; }
    }
}
