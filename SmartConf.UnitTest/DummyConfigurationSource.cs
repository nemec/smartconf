using System.Collections.Generic;

namespace SmartConf.UnitTest
{
    public class DummyConfigurationSource<T> : IConfigurationSource<T> where T : class
    {
        public bool PrimarySource { get; set; }

        public bool Required { get; set; }

        public bool PersistentSource { get; set; }

        public DummyConfigurationSource(T config)
        {
            _config = config;
        } 

        private readonly T _config;
        public T Config
        {
            get { return _config; }
        }

        public void Invalidate()
        {
        }

        public void Save(T obj)
        {
            PartialSave(obj, null);
        }

        public T SavedObject { get; private set; }

        public void PartialSave(T obj, IEnumerable<string> propertyNames)
        {
            SavedObject = obj;
        }
    }
}
