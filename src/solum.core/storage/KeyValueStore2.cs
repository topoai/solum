using LightningDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    public class KeyValueStore2 : NamedComponent, IDisposable
    {
        public KeyValueStore2(string dataDirectory, string name, Encoding encoding)
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
        LightningTransaction m_txn;

        public void Open()
        {
            Log.Verbose("Checking if database directory exists... {directory}", DatabaseDirectory);
            if (!Directory.Exists(DatabaseDirectory))
            {
                Log.Information("Creating database directory: {directory}", DatabaseDirectory);
                Directory.CreateDirectory(DatabaseDirectory);
            }

            Log.Debug("Initializing database environment... {databaseName}", Name);
            m_env = new LightningEnvironment(DatabaseDirectory);            

            Log.Verbose("Opening the database environment... {databaseName}", Name);
            m_env.Open();

            Log.Debug("Opening transaction... {databaseName}", Name);
            m_txn = m_env.BeginTransaction();

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

            Log.Debug("Commmiting transactions... {databaseName}", Name);
            m_txn.Commit();

            //Log.Debug("Closing the databse... {databaseName}", Name);            
            //m_db.Close();

            Log.Debug("Flushing the database contents... {databaseName}", Name);
            m_env.Flush(force: true);

            Log.Information("Closing database... {databaseName}", Name);
            m_env.Close();

            m_txn.Dispose();
            m_env.Dispose();
        }


        public IEnumerable<string> Keys()
        {
            foreach (var kvp in m_txn.EnumerateDatabase())
            {
                var key = kvp.Key<string>();
                yield return key;
            }
        }

        public IEnumerable<string> Values()
        {
            foreach (var kvp in m_txn.EnumerateDatabase())
            {
                var value = kvp.Value<string>();
                yield return value;
            }
        }

        public bool ContainsKey(string key)
        {
            return m_txn.ContainsKey(key);
        }

        public void Set(string key, string value)
        {
            ensureOpened();

            Log.Verbose("Setting key: {key}... value={value}", key, value);
            using (var txn = m_env.BeginTransaction(m_txn, TransactionBeginFlags.None))
            {
                txn.Put<string, string>(key, value);
                txn.Commit();
            }
        }
        public bool Get(string key, out string value)
        {
            ensureOpened();

            Log.Verbose("Getting key: {key}...", key);
            using (var txn = m_env.BeginTransaction(m_txn, TransactionBeginFlags.None))
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
            using (var txn = m_env.BeginTransaction(m_txn, TransactionBeginFlags.None))
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
