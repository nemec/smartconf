using System.Collections.Generic;
using System.Diagnostics;
using SmartConf.Validation;

namespace SmartConf.UnitTest.Mocks
{
    [DebuggerDisplay("Name: {Name}, Age: {Age}, Occupation: {Occupation}")]
    internal class Config
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Occupation { get; set; }

        public Config()
        {
            Occupation = "Unemployed";
        }
    }

    internal class ConfigComparer : IEqualityComparer<Config>
    {

        public bool Equals(Config x, Config y)
        {
            if (x == null || y == null)
            {
                return x != y;
            }

            return x.Age == y.Age &&
                x.Name == y.Name &&
                x.Occupation == y.Occupation;
        }

        public int GetHashCode(Config obj)
        {
            return 3 * (obj.Name != null ? obj.Name.GetHashCode() : 0) +
                   5 * (obj.Age.GetHashCode());
        }
    }

    internal class ConfigValidator : IValidator<Config>
    {
        public void Validate(Config obj)
        {
            if (obj.Age < 18)
            {
                throw new ValidationException("Minors not allowed.");
            }
        }
    }
}
