using Newtonsoft.Json;
using solum.extensions;
using solum.core.storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    public partial class Server : Service, IDisposable
    {
        const string DEFAULT_SERVER_NAME = "default-server";
        const string DEFAULT_SERVER_CONFG = "./server.config.json";
        const bool PROMPT_TO_START = false;

        protected Server() : this(DEFAULT_SERVER_NAME) { }

        protected Server(string name): base(name)
        {
            this.Storage = new StorageEngine();
            this.Services = new List<Service>();
        }

        public StorageEngine Storage { get; private set; }
        [JsonProperty("services")]
        protected List<Service> Services { get; private set; }

        protected override void OnLoad()
        {
            Log.Info("Loading server...");

            Log.Trace("DEFAULT ENCODING = {0}", SystemSettings.Encoding.EncodingName);

            Log.Info("Loading storage engine...");
            Storage.Open();

            Log.Info("Loading services...");
            Services.ForEach(s => s.Load());

            base.OnLoad();
        }
        protected override void OnStart()
        {
            Log.Info("Starting services...");
            Services.ForEach(s => s.Start());
        }
        protected override void OnStop()
        {
            Log.Info("Stopping services...");
            Services.ForEach(service =>
            {
                if (service.Status == ServiceStatus.Started)
                    service.Stop();
            });
        }
        protected override void OnUnload()
        {
            Log.Info("Unloading services...");
            Services.ForEach(service =>
            {
                service.Unload();
            });

            Log.Info("Closing storage...");
            Storage.Close();

            base.OnUnload();
        }        

        /// <summary>
        /// Get a loaded Service by Type.
        /// 
        /// NOTE: 
        /// If more than one service of the same type
        /// is loaded, then the first one it finds
        /// will be returned
        /// </summary>
        /// <typeparam name="T">They type of service to return.</typeparam>
        /// <param name="throwIfNotFound">
        /// Controls whether an exception 
        /// will be thrown if the service type is not found
        /// or if it will return null.
        /// </param>
        /// <returns>A service or null.</returns>
        public T Service<T>(bool throwIfNotFound = false) where T : Service
        {
            // ** Find the first service that matches the type specified
            var service = Services.Where(s => s is T)
                                  .Cast<T>()
                                  .FirstOrDefault();

            // ** Make sure we found the service
            if (service == null)
            {
                var serviceType = typeof(T);
                Log.Error("Serivce type {0} not found.", serviceType.FullName);

                if (throwIfNotFound)
                    throw new Exception("Service type {0} not found.".format(serviceType.FullName));
            }

            return service;
        }
    }
}
