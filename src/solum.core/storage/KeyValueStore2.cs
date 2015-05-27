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
        public KeyValueStore2(string dataDirectory, string name)
            : base(name)
        {
            this.DatabaseDirectory = dataDirectory;
        }

        public string DatabaseDirectory { get; private set; }

        LightningEnvironment m_env;
        LightningTransaction m_txn;
        LightningDatabase m_db;

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
            m_env.MaxDatabases = 2;

            Log.Verbose("Opening the database environment... {databaseName}", Name);
            m_env.Open();

            Log.Debug("Opening transaction... {databaseName}", Name);
            m_txn = m_env.BeginTransaction();

            Log.Information("Opening database... {databaseName}", Name);            
            m_db =  m_txn.OpenDatabase(Name, new DatabaseOptions()
            {
                Encoding = SystemSettings.Encoding,
                Flags = DatabaseOpenFlags.Create
            });
        }

        public void Close()
        {
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

            Log.Debug("Closing the databse... {databaseName}", Name);            
            m_db.Close();

            Log.Debug("Flushing the database contents... {databaseName}", Name);
            m_env.Flush(force: true);

            Log.Information("Closing database... {databaseName}", Name);
            m_env.Close();
        }

        public void Set(string key, string value)
        {
            Log.Verbose("Setting key: {key}... value={value}", key, value);
            using (var txn = m_env.BeginTransaction(m_txn, TransactionBeginFlags.None))
            {
                txn.Put<string, string>(m_db, key, value);
                txn.Commit();
            }
        }

        public bool Get(string key, out string value)
        {
            Log.Verbose("Getting key: {key}...", key);
            using (var txn = m_env.BeginTransaction(m_txn, TransactionBeginFlags.None))
            {
                value = txn.Get<string>(m_db, key);
            }

            return true;
        }

        public bool Remove(string key)
        {
            Log.Verbose("Removing key: {key}...", key);
            using (var txn = m_env.BeginTransaction(m_txn, TransactionBeginFlags.None))
            {
                txn.Delete<string>(m_db, key);
                txn.Commit();
            }

            return true;
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
