using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core
{
    public abstract class Service : Component, IDisposable
    {
        public enum ServiceStatus
        {
            Initialized,
            Loaded,
            Started,
            Stopped,
            Unloaded
        }

        protected Service()
        {
            this.Status = ServiceStatus.Initialized;
        }

        public ServiceStatus Status { get; private set; }

        public void Load()
        {
            if (Status != ServiceStatus.Initialized)
            {
                Log.Warn("Service is already loaded.  Load skipped...");
                return;
            }

            Log.Debug("Loading service...");
            OnLoad();
            Status = ServiceStatus.Loaded;
        }
        public void Start()
        {
            if (Status == ServiceStatus.Initialized)
            {
                // ** Autoload the service
                Log.Debug("Autoloading service...");
                Load();
            }

            Log.Debug("Starting service...");
            OnStart();
            Status = ServiceStatus.Started;
        }
        public void Stop()
        {
            if (Status >= ServiceStatus.Stopped)
            {
                Log.Warn("Service is already stopped.  Stop skipped...");
                return;
            }

            if (Status <= ServiceStatus.Loaded)
            {
                Log.Warn("Service is not started.  Stop skipped...");
                return;
            }

            Log.Debug("Stopping service...");
            OnStop();
            Status = ServiceStatus.Stopped;
        }
        public void Unload()
        {
            if (Status == ServiceStatus.Unloaded)
            {
                Log.Warn("Service is already unloaded.  Unload skipped...");
                return;
            }

            if (Status == ServiceStatus.Started)
            {
                Log.Warn("Service is started.  Stopping before attempting Unload...");
                try
                {
                    Stop();
                }
                catch (Exception ex)
                {
                    Log.FatalException("Failed to successfully stop the service! {0}".format(ex), ex);
                    Log.Info("Proceeding to Unload...");
                }
            }

            Log.Debug("Unloading service...");
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
