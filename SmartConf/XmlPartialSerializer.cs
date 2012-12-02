using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SmartConf
{
    /// <summary>
    ///     PartialSerializer implementation for the built-in XML serializer.
    /// </summary>
    /// <typeparam name="T">Object type to serialize.</typeparam>
    public class XmlPartialSerializer<T> : IPartialSerializer<T>
    {
        /// <summary>
        ///     Create a new serializer.
        /// </summary>
        public XmlPartialSerializer()
        {
            Deserializer = new XmlSerializer(typeof (T));
        }

        private XmlSerializer Deserializer { get; set; }

        /// <summary>
        ///     Object to serialize.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public void Serialize(Stream stream, T obj)
        {
            PartialSerialize(stream, obj, null);
        }

        /// <summary>
        ///     Stream to deserialize from.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public T Deserialize(Stream stream)
        {
            return (T) Deserializer.Deserialize(stream);
        }

        /// <summary>
        ///     Partially serialize
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="properties"></param>
        public void PartialSerialize(Stream stream, T obj, IEnumerable<string> properties)
        {
            var attributeOverrides = new XmlAttributeOverrides();

            if (properties != null)
            {
                foreach (string prop in properties)
                {
                    attributeOverrides.Add(
                        typeof (T), prop, new XmlAttributes {XmlIgnore = true});
                }
            }

            var serializer = new XmlSerializer(typeof (T), attributeOverrides);
            serializer.Serialize(stream, obj);
        }
    }
}