using System.Collections.Generic;
using System.Linq;

namespace SmartConf.UnitTest.Mocks
{
    public class DummyConfigurationSource<T> : IConfigurationSource<T> where T : class, new()
    {
        public bool PrimarySource { get; set; }

        public bool ReadOnly { get { return false; } }

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
            var mrg = new T();
            foreach (var prop in propertyNames
                .Select(propertyName => typeof (T).GetProperty(propertyName)))
            {
                prop.SetValue(mrg, prop.GetValue(obj, null), null);
            }
            SavedObject = mrg;
        }
    }
}
