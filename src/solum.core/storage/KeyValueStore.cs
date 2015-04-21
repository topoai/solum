﻿using RaptorDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace solum.core.storage
{
    public partial class KeyValueStore : Component, IDisposable
    {
        #region Converters to access underlying Database
        public static implicit operator Database(KeyValueStore store)
        {
            store.ensureOpened();
            return store.m_database;
        }
        #endregion

        public const byte DEFAULT_KEY_SIZE = 255;
        public const ushort DEFAULT_PAGE_SIZE = 10000;        

        public KeyValueStore(DirectoryInfo dataDirectory, string name, Encoding encoding)
        {
            // ** Undelying data store
            this.m_database = new Database(dataDirectory, name, encoding);

            this.Name = name;
            this.IsOpened = false;
            this.DataDirectory = dataDirectory;
            this.m_keysize = DEFAULT_KEY_SIZE;
            this.m_pagesize = DEFAULT_PAGE_SIZE;
        }

        public string Name { get; private set; }
        public DirectoryInfo DataDirectory { get; private set; }
        public bool IsOpened { get; private set; }
        public long NumRecords { get { return m_database.NumRecords; } }
        
        byte m_keysize;
        ushort m_pagesize;
        Database m_database;
        MGIndex<string> m_index; // TODO: This should support long's

        public void Open()
        {
            if (IsOpened)
            {
                Log.Warn("Key Value Store already opened. {0}", Name);
                return;
            }

            // ** Open the database
            Log.Debug("Opening the database...");
            m_database.Open();

            // ** Create an index
            Log.Debug("Opening the index...");
            var indexPath = Path.Combine(DataDirectory.FullName);
            var indexFileName = "{0}.idx".format(Name);
            this.m_index = new MGIndex<string>(indexPath, indexFileName, m_keysize, m_pagesize, false);

            this.IsOpened = true;
        }
        public void Close()
        {
            if (!IsOpened)
            {
                Log.Warn("The database is not opened.  {0}", Name);
                return;
            }

            Log.Debug("Shutting down the database...");
            m_database.Close();

            Log.Debug("Shutting down the index...");
            m_index.Shutdown();
        }
        

        public bool Remove(string key)
        {
            ensureOpened();

            lock (m_index)
            {
                // ** Search for the key in the index
                int id;
                if (m_index.Get(key, out id) == false)
                    return false;

                // ** Remove the key from the database
                m_database.Delete(id);

                // ** Remove the key from the index
                m_index.RemoveKey(key);

                return true;
            }
        }

        public bool ContainsKey(string key)
        {
            ensureOpened();

            int id;
            lock (m_index)
                return m_index.Get(key, out id);
        }

        /// <summary>
        /// Helper method to ensure the database is Open() before writing or reading
        /// </summary>
        void ensureOpened()
        {
            if (!IsOpened)
            {
                Log.Error("Key value store is not opened. name={0}", Name);
                throw new Exception("The key value store is not opened.");
            }
        }

        #region Explicit Interface
        void IDisposable.Dispose()
        {
            Close();
        }
        #endregion
    }
}
