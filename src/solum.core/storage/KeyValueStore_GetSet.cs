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
        #region Strings
        /// <summary>
        /// Stores a string as an encoded set of bytes
        /// using the SystemSettings.Encoding
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            // ** Convert
            var encoding = SystemSettings.Encoding;
            var bytes = encoding.GetBytes(value);

            // ** Store
            Set(key, bytes);
        }
        /// <summary>
        /// Retuns a string value encoded using the 
        /// SystemSettings.Encoding for the specified
        /// key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Get(string key, out string value)
        {
            // ** Intialize value to null
            value = null;

            // ** Fetch binary value
            byte[] bytes;
            if (!Get(key, out bytes))
                return false;

            // ** Decode the value
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
        /// <param name="value"></param>
        public void Set(string key, DateTime value)
        {
            var ticks = value.Ticks;
            var bytes = BitConverter.GetBytes(ticks);

            Set(key, bytes);
        }
        /// <summary>
        /// Returns contents by deserializing the datetime
        /// as a number of ticks then converting it 
        /// to a DateTime object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
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
        /// Sets a serialized value of an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value)
        {
            // ** Serialize the value
            var json = value.ToJson(indent: false, includeTypes: true);

            // ** Store the value as a JSON string
            Set(key, json);
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
