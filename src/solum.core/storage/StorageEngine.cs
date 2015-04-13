using solum.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    public class StorageEngine : Component, IDisposable
    {
        public const string DEFAULT_DATA_DIRECTORY = "./data/";

        public StorageEngine(string dataDirectory = DEFAULT_DATA_DIRECTORY)
        {
            this.DataDirectory = new DirectoryInfo(dataDirectory);
            this.openDatabases = new List<Database>();
            this.openKeyValueStores = new List<KeyValueStore>();
        }

        public DirectoryInfo DataDirectory { get; private set; }

        List<Database> openDatabases;
        List<KeyValueStore> openKeyValueStores;

        public void Open()
        {
            Log.Info("Opening storage engine...");

            // ** Ensure the data directory exists                        
            if (!DataDirectory.Exists)
            {
                Log.Info("Creating data directory: {0}...", DataDirectory);
                DataDirectory.Create();
                Log.Debug("Successfully created data directory: {0}", DataDirectory);
            }
        }

        public Database OpenDatabase(string name)
        {
            // ** Check if we have alreaedy opened this database
            var openedDatabase = openDatabases.Where(d => d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (openedDatabase != null)
                return openedDatabase;            

            // ** The requested database has not been previously opened            
            var database = new Database(DataDirectory, name);

            Log.Info("Opening database... {0}", name);            
            database.Open();

            openDatabases.Add(database);

            return database;
        }

        public KeyValueStore OpenKeyValueStore(string name)
        {
            // ** Check if we have alreaedy opened this database
            var openedKeyValueStore = openKeyValueStores.Where(d => d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (openedKeyValueStore != null)
                return openedKeyValueStore;

            // ** The requested database has not been previously opened            
            var keyValueStore = new KeyValueStore(DataDirectory, name);

            Log.Info("Opening key value store... {0}", name);
            keyValueStore.Open();

            openKeyValueStores.Add(keyValueStore);

            return keyValueStore;
        }

        public void Close()
        {
            Log.Info("Closing open databases...");
            openDatabases.ForEach(d => d.Close());

            Log.Info("Closing open key value stores...");
            openDatabases.ForEach(d => d.Close());
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
