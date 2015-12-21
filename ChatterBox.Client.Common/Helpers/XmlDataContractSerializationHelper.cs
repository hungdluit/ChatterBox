using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace ChatterBox.Client.Common.Helpers
{
    internal class XmlDataContractSerializationHelper
    {
        /// <summary>
        /// Serialize to xml.
        /// </summary>
        public static string ToXml<T>(T value, IEnumerable<Type> knownTypes = null)
        {
            MemoryStream stream = new MemoryStream();
            DataContractSerializer serializer;
            if (knownTypes != null)
            {
                serializer = new DataContractSerializer(typeof(T), knownTypes);
            }
            else
            {
                serializer = new DataContractSerializer(typeof(T));
            }
            serializer.WriteObject(stream, value);
            stream.Position = 0;
            string result = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
            }
            return result;
        }

        /// <summary>
        /// Deserialize from xml.
        /// </summary>
        public static T FromXml<T>(string xml, IEnumerable<Type> knownTypes = null)
        {
            T value;
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer;
                if (knownTypes != null)
                {
                    deserializer = new DataContractSerializer(typeof(T), knownTypes);
                }
                else
                {
                    deserializer = new DataContractSerializer(typeof(T));
                }
                value = (T)deserializer.ReadObject(stream);
            }
            return value;
        }
    }
}
