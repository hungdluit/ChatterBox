using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ChatterBox.Client.Common.Helpers
{
    internal class XmlSerializationHelper
    {
        /// <summary>
        /// Serialize to xml.
        /// </summary>
        public static string ToXml<T>(T value)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true,
            };

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                serializer.Serialize(xmlWriter, value);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Deserialize from xml.
        /// </summary>
        public static T FromXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T value;
            using (StringReader stringReader = new StringReader(xml))
            {
                object deserialized = serializer.Deserialize(stringReader);
                value = (T)deserialized;
            }

            return value;
        }
    }
}
