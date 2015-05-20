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
            this.m_databases = new List<Database>();
            this.m_kv_stores = new List<KeyValueStore>();
        }

        public DirectoryInfo DataDirectory { get; private set; }

        List<Database> m_databases;
        List<KeyValueStore> m_kv_stores;

        public void Open()
        {
            Log.Information("Opening storage engine...");

            // ** Ensure the data directory exists                        
            if (!DataDirectory.Exists)
            {
                Log.Information("Creating data directory: {0}...", DataDirectory);
                DataDirectory.Create();
                Log.Debug("Successfully created data directory: {0}", DataDirectory);
            }
        }

        public Database OpenDatabase(string name)
        {
            // ** Check if we have alreaedy opened this database
            var openedDatabase = m_databases.Where(d => d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (openedDatabase != null)
                return openedDatabase;            

            // ** The requested database has not been previously opened            
            var database = new Database(DataDirectory, name, SystemSettings.Encoding);

            Log.Information("Opening database... {0}", name);            
            database.Open();

            m_databases.Add(database);

            return database;
        }
        public KeyValueStore OpenKeyValueStore(string name)
        {
            // ** Check if we have alreaedy opened this database
            var openedKeyValueStore = m_kv_stores.Where(d => d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (openedKeyValueStore != null)
                return openedKeyValueStore;

            // ** The requested database has not been previously opened            
            var keyValueStore = new KeyValueStore(DataDirectory, name, SystemSettings.Encoding);

            Log.Information("Opening key value store... {0}", name);
            keyValueStore.Open();

            m_kv_stores.Add(keyValueStore);

            return keyValueStore;
        }

        public IReadOnlyList<Database> Databases()
        {
            return m_databases.AsReadOnly();
        }

        public IReadOnlyList<KeyValueStore> KeyValueStores()
        {
            return m_kv_stores.AsReadOnly();
        }

        public void Close()
        {
            Log.Information("Closing open databases...");
            m_databases.ForEach(d => d.Close());

            Log.Information("Closing open key value stores...");
            m_kv_stores.ForEach(d => d.Close());
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
