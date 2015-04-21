using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    public class NamedComponent : Component
    {
        protected NamedComponent(string name) : base(LogManager.GetLogger(name))
        {
            this.Name = name;
        }

        [JsonProperty("$name")]
        public string Name { get; private set; }
    }
}
