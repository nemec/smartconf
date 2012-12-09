using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace SmartConf.XmlConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XmlConfigurationSource<T> : IConfigurationSource<T> where T : class
    {
        private readonly XmlSerializer _serializer;

        private string Filename { get; set; }

        public bool PrimarySource { get; set; }

        /// <summary>
        /// Load the configuration object from a file.
        /// Filename is generated from some other source,
        /// such as another IConfigurationSource.
        /// </summary>
        /// <param name="getFilename">Function to generate filename.</param>
        public XmlConfigurationSource(Func<string> getFilename)
            : this(getFilename())
        {
        }

        /// <summary>
        /// Load the configuration object from a file.
        /// </summary>
        /// <param name="filename">Filename to load.</param>
        public XmlConfigurationSource(string filename)
        {
            Filename = filename;
            _serializer = new XmlSerializer(typeof(T));
        }

        private T _config;

        public T Config
        {
            get
            {
                if (_config == null)
                {
                    using (var stream = new FileStream(
                        Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        _config = (T) _serializer.Deserialize(stream);
                    }
                }
                return _config;
            }
        }

        public void Invalidate()
        {
            _config = null;
        }

        public void Save(T obj)
        {
            PartialSave(obj, null);
        }

        public void PartialSave(T obj, IEnumerable<string> propertyNames)
        {
            var attributeOverrides = new XmlAttributeOverrides();

            if (propertyNames != null)
            {
                var properties = new HashSet<string>(
                    typeof (T).GetProperties().Select(p => p.Name))
                    .Except(propertyNames);
                foreach (var prop in properties)
                {
                    attributeOverrides.Add(
                        typeof(T), prop, new XmlAttributes { XmlIgnore = true });
                }
            }

            var serializer = new XmlSerializer(typeof(T), attributeOverrides);
            using (var writer = new FileStream(Filename, FileMode.Create))
            {
                serializer.Serialize(writer, obj);
            }           
        }
    }
}
