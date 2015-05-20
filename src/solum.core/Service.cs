using solum.core.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    public abstract partial class Service : NamedComponent, IDisposable
    {
        public enum ServiceStatus
        {
            Initialized,
            Loaded,
            Started,
            Stopped,
            Unloaded
        }

        /// <summary>
        /// Name of service will be deserialized
        /// </summary>
        protected Service() { }

        protected Service(string name)
            : base(name)
        {
            this.Status = ServiceStatus.Initialized;
        }

        public ServiceStatus Status { get; private set; }        

        public void Load()
        {
            if (Status != ServiceStatus.Initialized)
            {
                Log.Warning("Service {0} is already loaded.  Load skipped...", Name);
                return;
            }

            Log.Debug("Loading service: {0}...", Name);
            OnLoad();
            Status = ServiceStatus.Loaded;
        }
        public void Start()
        {
            if (Status == ServiceStatus.Initialized)
            {
                // ** Autoload the service
                Log.Debug("Autoloading service: {0}...", Name);
                Load();
            }

            Log.Debug("Starting service: {0}...", Name);
            OnStart();
            Status = ServiceStatus.Started;
        }
        public void Stop()
        {
            if (Status >= ServiceStatus.Stopped)
            {
                Log.Warning("Service {0} is already stopped.  Stop skipped...", Name);
                return;
            }

            if (Status <= ServiceStatus.Loaded)
            {
                Log.Warning("Service {0} is not started.  Stop skipped...", Name);
                return;
            }

            Log.Debug("Stopping service: {0}...", Name);
            OnStop();
            Status = ServiceStatus.Stopped;
        }
        public void Unload()
        {
            if (Status == ServiceStatus.Unloaded)
            {
                Log.Warning("Service {0} is already unloaded.  Unload skipped...", Name);
                return;
            }

            if (Status == ServiceStatus.Started)
            {
                Log.Warning("Service {0} is started.  Stopping before attempting Unload...", Name);
                try
                {
                    Stop();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Failed to successfully stop service {0}! {1}".format(Name, ex.Message));
                    Log.Information("Proceeding to Unload service {0}...", Name);
                }
            }

            Log.Debug("Unloading service: {0}...", Name);
            OnUnload();
            Status = ServiceStatus.Unloaded;
        }

        protected virtual void OnLoad() { }
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnUnload() { }

        void IDisposable.Dispose()
        {
            if (Status < ServiceStatus.Stopped)
                Stop();

            if (Status != ServiceStatus.Unloaded && Status != ServiceStatus.Initialized)
                Unload();
        }
    }
}
