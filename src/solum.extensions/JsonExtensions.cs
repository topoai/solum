using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Bson;
using System.Diagnostics;
using System.Text;

namespace solum.extensions
{

    public static class JsonExtensions
    {
        static JsonSerializerSettings json_settings;
        static JsonSerializerSettings json_settings_typed;

        static JsonExtensions()
        {
            json_settings = new JsonSerializerSettings();
            json_settings.Converters.Add(new StringEnumConverter());
            //json_settings.Converters.Add(new JsonTypeConverter());

            json_settings_typed = new JsonSerializerSettings();
            json_settings_typed.Converters.Add(new StringEnumConverter());
            //json_settings_typed.Converters.Add(new JsonTypeConverter());
            json_settings_typed.TypeNameHandling = TypeNameHandling.All;

            // TODO: Add support for a resource converter
        }

        /// <summary>
        /// Converts an object to it's Json Representation
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="obj">Object.</param>
        public static string ToJson(this object obj, bool indent = false, bool includeTypes = false)
        {
            var settings = includeTypes ? json_settings_typed : json_settings;

            if (indent)
                return "\n{0}".format(JsonConvert.SerializeObject(obj, Formatting.Indented, settings));

            return JsonConvert.SerializeObject(obj, Formatting.None, settings);
        }

        public static Dictionary<string, object> ToJsonDictionary(this object obj)
        {
            return obj.ToJson().ToJsonDictionary();
        }

        public static Dictionary<string, object> ToJsonDictionary(this string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, json_settings_typed);
        }


        public static object FromJson(this string json)
        {
            return JsonConvert.DeserializeObject(json, json_settings_typed);
        }

        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, json_settings_typed);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, json_settings_typed);
        }

        #region Bson Support
        static JsonSerializer bson_serializer = new JsonSerializer();

        public static byte[] ToBson<T>(this T obj)
        {
            // Serialization
            byte[] bytes;
            using (var ms = new MemoryStream())
            using (var writer = new BsonWriter(ms))
            {
                bson_serializer.Serialize(writer, obj, typeof(string[]));
                // bson_serializer.Serialize(writer, obj);
                bytes = ms.ToArray();
            }

            return bytes;
        }

        public static T FromBson<T>(this byte[] data)
        {
            using (var reader = new BsonReader(new MemoryStream(data)))
                return bson_serializer.Deserialize<T>(reader);
        }

        public static T FromBson<T>(this byte[] data, Type type)
        {
            using (var reader = new BsonReader(new MemoryStream(data)))
                return bson_serializer.Deserialize<T>(reader);
        }
        #endregion
    }
}