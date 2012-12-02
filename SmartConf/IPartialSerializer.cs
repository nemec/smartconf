using System.Collections.Generic;
using System.IO;

namespace SmartConf
{
    /// <summary>
    /// Serializer that allows partial serialization of an object.
    /// </summary>
    /// <typeparam name="T">Object type being serialized/deserialized.</typeparam>
    public interface IPartialSerializer<T>
    {
        /// <summary>
        /// Serialize the full object to the stream.
        /// </summary>
        /// <param name="stream">Stream the object is serialized to.</param>
        /// <param name="obj">Object to serialize. All properties will be serialized.</param>
        void Serialize(Stream stream, T obj);

        /// <summary>
        /// Serialize only the given properties of the object to the stream.
        /// </summary>
        /// <param name="stream">Stream where the object is serialized.</param>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="properties">List of properties to serialize.</param>
        void PartialSerialize(Stream stream, T obj, IEnumerable<string> properties);

        /// <summary>
        /// Deserialize the stream into an object of type T.
        /// </summary>
        /// <param name="stream">Data is read from this stream.</param>
        /// <typeparamref name="T"></typeparamref>
        /// <returns>A deserialized object.</returns>
        T Deserialize(Stream stream);
    }

    /// <summary>
    /// PartialSerializer for <see cref="object"/> types.
    /// </summary>
    public interface IPartialSerializer : IPartialSerializer<object>
    {
    }
}
