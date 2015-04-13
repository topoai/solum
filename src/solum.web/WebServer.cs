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
            this.ViewManager = new ViewManager();
        }

        #region Configurable Properties
        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("http://+:80/")]
        public string Address { get; private set; }
        [JsonProperty("modules")]
        public List<WebModule> Modules { get; private set; }
        [JsonProperty("static-directories")]
        public Dictionary<string, string> StaticDirectories { get; private set; }
        [JsonProperty("view-manager")]
        public IViewManager ViewManager { get; private set; }
        #endregion

        WebServerListener m_listener;

        protected override void OnLoad()
        {
            // ** Load the listener as a service
            Log.Debug("Creating HTTP Listener... {0}", Address);
            m_listener.Addresses.Add(Address);

            // ** Add Handler for static files
            Log.Debug("Setting static directory mappings...");
            foreach (var kvp in StaticDirectories)
            {
                var prefix = kvp.Key;
                var directory = kvp.Value;

                Log.Trace("- Mapping: {0} -> {1}", prefix, directory);

                var staticFileHandler = new StaticDirectoryHandler(prefix, directory);
                m_listener.Handlers.Add(staticFileHandler);
            }

            Log.Debug("Registering templates...");
            ViewManager.RegisterTemplates();
            
            // ** Add modules
            Log.Debug("Adding web modules...");
            Modules.ForEach(module =>
            {
                Log.Debug("Registering routes...");
                module.RegisterRoutes();

                foreach (var route in module.Routes)
                {
                    m_listener.Handlers.Add(route);
                }
            });

            Log.Debug("Adding HTTP listener service...");
            Services.Add(m_listener);

            base.OnLoad();
        }
    }
}
