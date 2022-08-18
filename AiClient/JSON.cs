using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AiClient
{
    public class JSON
    {
        public static T Parse<T>(Stream stream)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"parse failure: {e.Message}");
                throw;
            }
        }

        public static T Parse<T>(string str)
        {
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(str);
            return Parse<T>(buf, buf.Length);
        }
        public static T Parse<T>(byte[] buf, int count) {
            try
            {
                using (var ms = new MemoryStream(buf, 0, count))
                {
                    return Parse<T>(ms);
                }
            }
            catch (Exception e)
            {
                var str = Encoding.UTF8.GetString(buf);
                Debug.WriteLine("parse failure json: " + str);
                throw;
            }
        }

        public static string ToString<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}