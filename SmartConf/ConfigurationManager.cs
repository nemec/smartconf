using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Xml.Linq;
using System.Collections;
using System.Reflection;

namespace SmartConf
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Merges all properties in the secondary object
        /// with those in the primary.
        /// 
        /// A property value will be set only if the
        /// value in <paramref name="secondary"/> is
        /// different from the value in <paramref name="primary"/>
        /// AND different from the default value (after construction).
        /// 
        /// Note: Dynamic default values (like DateTime.Now) may not
        /// return the same default value for multiple invocations,
        /// thus may not merge accurately.
        /// </summary>
        /// <typeparam name="T">
        /// Type to merge. Must have a default constructor that
        /// sets default property values.
        /// </typeparam>
        /// <param name="primary">Object to overwrite values for.</param>
        /// <param name="secondary">
        /// Object to merge into <paramref name="primary"/>.
        /// </param>
        public static void MergeWith<T>(this T primary, T secondary) where T : new()
        {
            // We need to know whether or not the value is new or from the constructor.
            // Doesn't work on objects that don't implement IEquatable.
            var defaultObject = new T();
            if (secondary == null) return;
            if (primary == null) primary = defaultObject;

            foreach (var pi in typeof(T).GetProperties())
            {
                var priValue = pi.GetGetMethod().Invoke(primary, null);
                var secValue = pi.GetGetMethod().Invoke(secondary, null);
                var defaultValue = typeof(T).GetProperty(pi.Name).GetGetMethod().Invoke(defaultObject, null);

                if (priValue == null && secValue != null ||
                    (priValue != null && !priValue.Equals(secValue) &&
                        (secValue != defaultValue ||
                            secValue != null && !secValue.Equals(defaultValue))))
                {
                    pi.GetSetMethod().Invoke(primary, new object[] { secValue });
                }
            }
        }
    }

    public class ConfigurationManager<T> where T : class, new()
    {
        private string LocalFilepath { get; set; }

        private T Base { get; set; }

        /// <summary>
        /// This object contains the merged configuration settings.
        /// It will track all changes and save them out to a file
        /// when <see cref="SaveChanges"/> is called.
        /// </summary>
        public T Out { get; private set; }

        /// <summary>
        /// Create a new ConfigurationReader pulling in values from
        /// the given <see cref="baseFilepath"/> and
        /// <see cref="localFilepath"/>.
        /// </summary>
        /// <param name="baseFilepath">Main config file.</param>
        /// <param name="localFilepath">Local overrides of configuration settings.</param>
        public ConfigurationManager(string baseFilepath, string localFilepath)
        {
            var Serializer = new XmlSerializer(typeof(T));
            LocalFilepath = localFilepath;

            if (baseFilepath != null)
            {
                using (var reader = new FileStream(baseFilepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Base = (T)Serializer.Deserialize(reader);  // Keep the original around for diffing
                    reader.Seek(0, SeekOrigin.Begin);
                    Out = (T)Serializer.Deserialize(reader);
                }
            }
            else
            {
                Base = new T();
                Out = new T();
            }

            if (localFilepath != null)
            {
                using (var reader = new FileStream(localFilepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Out.MergeWith((T)Serializer.Deserialize(reader));
                }
            }
        }

        public Dictionary<string, object> GetPropertyChangesByName()
        {
            return GetProperties(PropertyStatus.Changed).ToDictionary(
                k => k.Name,
                v => v.GetGetMethod().Invoke(Out, null));
        }

        /// <summary>
        /// Saves changes to <see cref="Out"/> to the
        /// local file path passed in the constructor.
        /// </summary>
        public void SaveChanges()
        {
            if (LocalFilepath == null)
            {
                throw new ArgumentNullException(
                    "LocalFilepath cannot be null when saving changes to default location.");
            }
            SaveChanges(LocalFilepath);
        }

        /// <summary>
        /// Saves changes to <see cref="Out"/> to the
        /// file path provided as an argument.
        /// </summary>
        /// <param name="outputFilepath"></param>
        public void SaveChanges(string outputFilepath)
        {
            if (outputFilepath == null)
            {
                throw new ArgumentNullException("Output filepath cannot be null.");
            }
            var overrides = GetIgnoredProperties();
            var Serializer = new XmlSerializer(typeof(T), overrides);
            
            using (TextWriter writer = new StreamWriter(outputFilepath))
            {
                
                Serializer.Serialize(writer, Out);
            }
        }

        /// <summary>
        /// Ignores all properties where the current value
        /// equals the default value.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        private XmlAttributeOverrides GetIgnoredProperties()
        {
            var attributeOverrides = new XmlAttributeOverrides();

            foreach(var info in GetProperties(PropertyStatus.Unchanged)){
                attributeOverrides.Add(
                    typeof(T), info.Name, new XmlAttributes { XmlIgnore = true });
            }
            
            return attributeOverrides;
        }

        private enum PropertyStatus
        {
            Changed,
            Unchanged
        }

        private IEnumerable<PropertyInfo> GetProperties(PropertyStatus status)
        {
            foreach (var info in typeof(T).GetProperties())
            {
                var priValue = info.GetGetMethod().Invoke(Out, null);
                var secValue = info.GetGetMethod().Invoke(Base, null);

                // True if changed, False if unchanged.
                var actualStatus = (priValue == null || !priValue.Equals(secValue)) &&
                    (priValue != null || secValue != null);
                if (status == PropertyStatus.Changed && actualStatus)
                {
                    yield return info;
                }
                else if(status == PropertyStatus.Unchanged && !actualStatus)
                {
                    yield return info;
                }
            }
        }
    }
}