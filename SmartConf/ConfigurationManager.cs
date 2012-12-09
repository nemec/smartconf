using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartConf.XmlConfiguration;

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

        private IConfigurationSource<T>[] ConfigStack { get; set; }

        private readonly IConfigurationSource<T> _primarySource;

        /// <summary>
        /// This object contains the merged configuration settings.
        /// It will track all changes and save them out to a file
        /// when <see ref="SaveChanges"/> is called.
        /// </summary>
        public T Out { get; set; }

        /// <summary>
        /// Create a ConfigurationManager loading its information
        /// from a list of XML configuration files parsed by
        /// <see cref="XmlConfigurationSource{T}"/> sources.
        /// </summary>
        /// <param name="configFiles">Filenames to load.</param>
        public ConfigurationManager(params string[] configFiles)
            : this(configFiles.Select(c =>
                (IConfigurationSource<T>)new XmlConfigurationSource<T>(c))
                .ToArray())
        {
            
        } 

        /// <summary>
        /// Create a ConfigurationManager, loading its information
        /// from an ordered list of sources.
        /// 
        /// If no source is marked as a PrimarySource, the last
        /// source in the sequence will be assumed to be primary.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// More than one source is marked as a PrimarySource.
        /// </exception>
        /// <param name="sources"></param>
        public ConfigurationManager(params IConfigurationSource<T>[] sources)
        {
            ConfigStack = sources;
            var primarySources = sources.Where(s => s.PrimarySource);
            if (primarySources.Count() > 1)
            {
                throw new ArgumentException("Cannot have more than one primary source.");
            }
            _primarySource = primarySources.Any() ?
                primarySources.First() :
                sources.Last();
            Out = sources.Select(s => s.Config).Merge();
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
            if (!ChangeTrackingEnabled)
            {
                ChangeTrackingEnabled = true;
            }
        }

        /// <summary>
        /// Save changes to <see cref="Out"/> to
        /// the PrimarySource.
        /// </summary>
        public void SaveChanges()
        {
            SaveChanges(_primarySource);
        }

        /// <summary>
        /// Saves changes to <see cref="Out"/> to the
        /// given IConfigurationSource.
        /// </summary>
        /// <param name="source">Source to use as the primary source.</param>
        public void SaveChanges(IConfigurationSource<T> source)
        {
            source.PartialSave(Out,
                GetProperties(PropertyStatus.Changed).Select(p => p.Name));
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
            var @base = ConfigStack
                .Select(c => c.Config)
                .Where(c => !ReferenceEquals(c, _primarySource.Config)).Merge();
            foreach (var info in typeof(T).GetProperties())
            {
                var priValue = info.GetGetMethod().Invoke(Out, null);
                var baseValue = info.GetGetMethod().Invoke(@base, null);

                var propertyChanged = (priValue == null || !priValue.Equals(baseValue)) &&
                    (priValue != null || baseValue != null);
                if (status == PropertyStatus.Changed && propertyChanged)
                {
                    yield return info;
                }
                else if(status == PropertyStatus.Unchanged && !propertyChanged)
                {
                    yield return info;
                }
            }
        }
    }
}