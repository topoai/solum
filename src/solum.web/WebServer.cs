using Newtonsoft.Json;
using solum.core;
using solum.core.http;
using solum.core.http.handlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.web
{
    public class WebServer : Server
    {
        WebServer()
        {
            this.Modules = new List<WebModule>();
            this.m_listener = new WebServerListener();            
        }

        #region Configurable Properties
        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("http://+:80/")]
        public string Address { get; private set; }
        [JsonProperty("modules")]
        public List<WebModule> Modules { get; private set; }
        [JsonProperty("static-directories")]
        public Dictionary<string, string> StaticDirectories { get; private set; }
        #endregion

        WebServerListener m_listener;

        protected override void OnLoad()
        {
            // ** Load the listener as a service
            m_listener.Addresses.Add(Address);

            // ** Add Handler for static files
            foreach (var kvp in StaticDirectories)
            {
                var prefix = kvp.Key;
                var directory = kvp.Value;

                var staticFileHandler = new StaticDirectoryHandler(prefix, directory);
                m_listener.Handlers.Add(staticFileHandler);
            }
            
            // ** Add modules
            Modules.ForEach(module =>
            {
                foreach (var route in module.Routes)
                {
                    m_listener.Handlers.Add(route);
                }
            });

            Services.Add(m_listener);

            base.OnLoad();
        }
    }
}
