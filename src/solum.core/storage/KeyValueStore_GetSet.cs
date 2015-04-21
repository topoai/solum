using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using solum.extensions;

namespace solum.core.storage
{
    /// <summary>
    /// This class contains methods to assist storing objects
    /// by providing built-in conversions, and serializion
    /// for common object types.
    /// </summary>
    partial class KeyValueStore
    {
        #region Default - Binary
        public long Set(string key, byte[] value)
        {
            ensureOpened();

            lock (m_index)
            {
                // ** Check if the key already exits
                int existingId = -1;
                if (m_index.Get(key, out existingId))
                {
                    // Delete the existing record before adding
                    m_database.Delete(existingId);
                }

                // ** Store the new newValue as a record            
                var record = m_database.Store(value);

                // ** Index the record id with the key
                var id = record.Id;

                if (id > int.MaxValue)
                    throw new NotSupportedException("Id's larger than {0} are not supported.".format(id));

                m_index.Set(key, (int)id);

                return id;
            }
        }
        public bool Get(string key, out byte[] value)
        {
            ensureOpened();

            lock (m_index)
            {
                value = null;

                // ** Search the index for the key
                int id;
                if (!m_index.Get(key, out id))
                    return false;

                // ** Read the record from the database
                var record = m_database.ReadRecord(id);
                value = record.Data;

                return true;
            }
        }
        public void Update(string key, byte[] value)
        {
            ensureOpened();

            int id;
            bool exists;
            lock (m_index)
            {
                exists = m_index.Get(key, out id);

                if (!exists)
                    // Set the stat for the first time
                    Set(key, value);
                else
                {
                    // Set the existing currentValue (in-place)
                    m_database.Update(id, value);
                }
            }
        }
        #endregion

        #region int/long
        public long Set(string key, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            return Set(key, bytes);
        }
        public long Set(string key, long value)
        {            
            var bytes = BitConverter.GetBytes(value);            
            return Set(key, bytes);
        }
        public bool Get(string key, out int value)
        {
            value = 0;

            byte[] bytes;
            if (Get(key, out bytes) == false)
                return false;

            value = BitConverter.ToInt32(bytes, 0);

            return true;
        }
        public bool Get(string key, out long value)
        {
            value = 0L;

            byte[] bytes;
            if (Get(key, out bytes) == false)
                return false;

            value = BitConverter.ToInt64(bytes, 0);
            
            return true;
        }
        public void Update(string key, long value)
        {
            var bytes = BitConverter.GetBytes(value);
            Update(key, bytes);
        }
        public void Update(string key, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Update(key, bytes);
        }
        #endregion

        #region Strings
        /// <summary>
        /// Stores a string as an encoded set of bytes
        /// using the SystemSettings.Encoding
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        public long Set(string key, string value)
        {
            // ** Convert
            var encoding = SystemSettings.Encoding;
            var bytes = encoding.GetBytes(value);

            // ** Store
            return Set(key, bytes);
        }
        /// <summary>
        /// Retuns a string newValue encoded using the 
        /// SystemSettings.Encoding for the specified
        /// key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool Get(string key, out string value)
        {
            // ** Intialize newValue to null
            value = null;

            // ** Fetch binary newValue
            byte[] bytes;
            if (!Get(key, out bytes))
                return false;

            // ** Decode the newValue
            var encoding = SystemSettings.Encoding;
            value = encoding.GetString(bytes);

            return true;
        }
        #endregion

        #region DateTime
        /// <summary>
        /// Stores date time as a long using the Ticks property.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        public long Set(string key, DateTime value)
        {
            var ticks = value.Ticks;
            var bytes = BitConverter.GetBytes(ticks);

            return Set(key, bytes);
        }
        /// <summary>
        /// Returns contents by deserializing the datetime
        /// as a number of ticks then converting it 
        /// to a DateTime object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool Get(string key, out DateTime value)
        {
            value = default(DateTime);
            byte[] bytes = null;
            if (Get(key, out bytes) == false)
                return false;

            var ticks = BitConverter.ToInt64(bytes, 0);
            value = new DateTime(ticks);

            return true;
        }
        #endregion

        #region Generic Objects
        /// <summary>
        /// Sets a serialized newValue of an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        public long Set<T>(string key, T value)
        {
            // ** Serialize the newValue
            var json = value.ToJson(indent: false, includeTypes: true);

            // ** Store the newValue as a JSON string
            return Set(key, json);
        }
        public bool Get<T>(string key, out T value)
        {
            value = default(T);
            string json;
            if (Get(key, out json) == false)
                return false;

            value = json.FromJson<T>();
            return true;
        }
        #endregion
    }
}
