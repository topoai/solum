using LightningDB;
using solum.extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    public class KeyValueStore : NamedComponent, IDisposable
    {
        const long DEFAULT_MAX_DB_SIZE = 4096000000; // 4GB

        public KeyValueStore(string dataDirectory, string name, Encoding encoding)
            : base(name)
        {
            this.DatabaseDirectory = dataDirectory;
            this.Encoding = encoding;
            this.IsOpened = false;
        }

        public string DatabaseDirectory { get; private set; }
        public Encoding Encoding { get; private set; }
        public bool IsOpened { get; private set; }
        public long NumRecords { get { return m_env.EntriesCount; } }
        
        LightningEnvironment m_env;
        
        public void Open()
        {
            Log.Verbose("Checking if database directory exists... {directory}", DatabaseDirectory);
            if (!Directory.Exists(DatabaseDirectory))
            {
                Log.Information("Creating database directory: {directory}", DatabaseDirectory);
                Directory.CreateDirectory(DatabaseDirectory);
            }

            var filePath = Path.Combine(DatabaseDirectory, "data.mdb");
            bool exists = File.Exists(filePath);

            Log.Debug("Initializing database environment... {databaseName}", Name);
            m_env = new LightningEnvironment(DatabaseDirectory, EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.WriteMap);            

            Log.Verbose("Opening the database environment... {databaseName}", Name);
            m_env.Open();
            
            // ** If the file was created when openeing the database, close the environment, Mark this file as "Sparse", and re-open
            if (exists == false)
            {
                m_env.Close();

                Log.Debug("Marking data file as sparse... {filePath}");
                FileExtensions.MarkAsSparseFile(filePath);

                Log.Verbose("Re-opening data file...");
                m_env = new LightningEnvironment(DatabaseDirectory, EnvironmentOpenFlags.NoSync | EnvironmentOpenFlags.WriteMap);
                m_env.MapSize = DEFAULT_MAX_DB_SIZE;

                Log.Verbose("Opening the database environment... {databaseName}", Name);
                m_env.Open();
            }
            else
            {
                m_env.MapSize = DEFAULT_MAX_DB_SIZE;
            }
            
            //m_env.CopyTo(filePath, true);
            
            IsOpened = true;
        }
        public void Close()
        {
            if (!IsOpened)
                return;

            IsOpened = false;

            Log.Verbose("Checking if database is initialized... {databaseName}", Name);
            if (m_env == null)
            {
                Log.Warning("[NOOP] Database is not initialized... {databaseName}", Name);
                return;
            }

            Log.Verbose("Checking if database is opened... {databaseName}", Name);
            if (m_env.IsOpened == false)
            {
                Log.Warning("[NOOP] Database is not opened... {databaseName}", Name);
                return;
            }

            //Log.Debug("Closing the databse... {databaseName}", Name);            
            //m_db.Close();

            Log.Debug("Flushing the database contents... {databaseName}", Name);
            m_env.Flush(force: true);

            Log.Information("Closing database... {databaseName}", Name);
            m_env.Close();
            m_env.Dispose();
        }


        public IEnumerable<string> Keys()
        {
            using (var txn = m_env.BeginTransaction(TransactionBeginFlags.ReadOnly))
            foreach (var kvp in txn.EnumerateDatabase())
            {
                var key = kvp.Key<string>();
                yield return key;
            }
        }

        public IEnumerable<string> Values()
        {
            using (var txn = m_env.BeginTransaction(TransactionBeginFlags.ReadOnly))
            foreach (var kvp in txn.EnumerateDatabase())
            {
                var value = kvp.Value<string>();
                yield return value;
            }
        }

        public bool ContainsKey(string key)
        {
            bool hasKey = false;

            using (var txn = m_env.BeginTransaction(TransactionBeginFlags.ReadOnly))
                hasKey = txn.ContainsKey(key);

            return hasKey;
        }

        public void Set(string key, string value)
        {
            ensureOpened();

            Log.Verbose("Setting key: {key}... value={value}", key, value);
            using (var txn = m_env.BeginTransaction())
            {
                txn.Put<string, string>(key, value);
                txn.Commit();
            }
        }
        public bool Get(string key, out string value)
        {
            ensureOpened();

            Log.Verbose("Getting key: {key}...", key);
            using (var txn = m_env.BeginTransaction(TransactionBeginFlags.ReadOnly))
            {
                value = txn.Get<string>(key);
            }

            return true;
        }
        public void Set(string key, long value)
        {
            Set(key, value.ToString());
        }

        public bool Get(string key, out long value)
        {
            value = 0;
            string sValue;

            if (Get(key, out sValue) == false)
                return false;

            value = long.Parse(sValue);
            return true;
        }
        public bool Remove(string key)
        {
            Log.Verbose("Removing key: {key}...", key);
            using (var txn = m_env.BeginTransaction())
            {
                txn.Delete<string>(key);
                txn.Commit();
            }

            return true;
        }

        void ensureOpened()
        {
            if (!IsOpened)
            {
                throw new Exception("The database is not opened: {databaseName}".format(Name));
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
