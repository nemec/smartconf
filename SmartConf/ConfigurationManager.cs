using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SmartConf
{
    /// <summary>
    /// A configuration manager that loads and merges settings from
    /// multiple sources and intelligently tracks changes to the
    /// resulting configuration object.
    /// </summary>
    /// <typeparam name="T">Strongly typed Configuration object</typeparam>
    public class ConfigurationManager<T> where T : class, new()
    {
        /// <summary>
        /// Whether or not change tracking is currently enabled.
        /// </summary>
        public bool ChangeTrackingEnabled { get; private set; }

        private string LocalFilepath { get; set; }

        private IPartialSerializer<T> Serializer { get; set; }

        private T Base { get; set; }

        /// <summary>
        /// This object contains the merged configuration settings.
        /// It will track all changes and save them out to a file
        /// when <see ref="SaveChanges"/> is called.
        /// </summary>
        public T Out { get; set; }

        /// <summary>
        /// Create a ConfigurationManager with the default 
        /// XmlPartialSerializer implementation.
        /// </summary>
        public ConfigurationManager()
        {
            Serializer = new XmlPartialSerializer<T>();
        } 

        /// <summary>
        /// Create a ConfigurationManager with the given
        /// IPartialSerializer.
        /// </summary>
        /// <param name="serializer"></param>
        public ConfigurationManager(IPartialSerializer<T> serializer)
        {
            Serializer = serializer;
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localFilepath">
        /// The path that configuration changes will be saved to.
        /// Contents of this file will be loaded onto the config
        /// object last.
        /// </param>
        /// <param name="baseFilepaths">
        /// Variable number of configuration files to load settings
        /// from before loading the local file. Files will be loaded
        /// in order and any non-default values will overwrite those
        /// of previous configuration files.
        /// 
        /// The configuration state once all base files have been loaded
        /// will be used as the "default" state for the config.
        /// </param>
        public void Load(string localFilepath, params string[] baseFilepaths)
        {
            LocalFilepath = localFilepath;
            FileStream localStream = null;
            Stream[] baseStreams = null;
            try
            {
                localStream = new FileStream(localFilepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                baseStreams = baseFilepaths.Select(b =>
                    (Stream)new FileStream(b, FileMode.Open, FileAccess.Read, FileShare.Read))
                    .ToArray();
                Load(localStream, baseStreams.ToArray());
            }
            finally{
                if (localStream != null)
                {
                    localStream.Dispose();
                }
                if (baseStreams != null)
                {
                    foreach (var baseStream in baseStreams)
                    {
                        baseStream.Dispose();
                    }
                }
            }
        }

        private void Load(Stream localStream, params Stream[] baseStreams)
        {
            Base = new T();
            Out = new T();
            foreach (var stream in baseStreams)
            {
                Base.MergeWith(Serializer.Deserialize(stream)); // Keep the original around for diffing
                stream.Seek(0, SeekOrigin.Begin);
                Out.MergeWith(Serializer.Deserialize(stream));
            }

            if (localStream != null)
            {
                Out.MergeWith(Serializer.Deserialize(localStream));
            }

            EnableChangeTracking();
        }

        /// <summary>
        /// Temporarily disables change tracking.
        /// </summary>
        public void DisableChangeTracking()
        {
            if (!ChangeTrackingEnabled) return;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Enables change tracking after it has
        /// been disabled.
        /// </summary>
        public void EnableChangeTracking()
        {
            if (ChangeTrackingEnabled) return;

            ChangeTrackingEnabled = true;
        }

        /// <summary>
        /// Saves changes to <see cref="Out"/> to the
        /// local file path passed in the constructor.
        /// 
        /// <exception cref="InvalidOperationException">
        /// Thrown if no output file path is known.
        /// </exception>
        /// </summary>
        public void SaveChanges()
        {
            if (LocalFilepath == null)
            {
                throw new InvalidOperationException(
                    "LocalFilepath cannot be null when saving changes to default location.");
            }
            SaveChanges(LocalFilepath);
        }

        /// <summary>
        /// Saves changes to <see cref="Out"/> to the
        /// file path provided as an argument.
        /// 
        /// If there is no known
        /// </summary>
        /// <param name="outputFilepath"></param>
        public void SaveChanges(string outputFilepath)
        {
            if (outputFilepath == null)
            {
                throw new ArgumentNullException("outputFilepath", "Output filepath cannot be null.");
            }
            
            using (var writer = new FileStream(outputFilepath, FileMode.Create))
            {
                SaveChanges(writer);
            }
        }

        /// <summary>
        /// Saves modified properties to the given stream.
        /// </summary>
        /// <param name="outputStream"></param>
        public void SaveChanges(Stream outputStream)
        {
            Serializer.Serialize(outputStream, Out);
        }

        /// <summary>
        /// Retrieves the names of all properties that
        /// have been changed since loading.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetPropertyChangesByName()
        {
            return GetProperties(PropertyStatus.Changed).ToDictionary(
                k => k.Name,
                v => v.GetGetMethod().Invoke(Out, null));
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