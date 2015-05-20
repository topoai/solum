using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    public class NamedComponent : Component
    {
        /// <summary>
        /// Name of service will come from a deserializer
        /// </summary>
        protected NamedComponent() { }

        protected NamedComponent(string name) : base()
        {
            this.Name = name;
        }

        [JsonProperty("$name")]
        public string Name { get; private set; }
    }
}
