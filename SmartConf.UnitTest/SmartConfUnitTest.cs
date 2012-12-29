using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmartConf.UnitTest
{
    [TestClass]
    public class SmartConfUnitTest
    {
        [DebuggerDisplay("Name: {Name}, Age: {Age}, Occupation: {Occupation}")]
        public class Config
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Occupation { get; set; }
            
            public Config()
            {
                Occupation = "Unemployed";
            }
        }

        private class ConfigComparer : IEqualityComparer<Config>
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
                return 3*(obj.Name != null ? obj.Name.GetHashCode() : 0) +
                       5*(obj.Age.GetHashCode());
            }
        }
            
        [TestMethod]
        public void LoadSources_WithSingleSource_EqualsSourceObject()
        {
            // Arrange
            var actual = new Config
                {
                    Name = "Timothy",
                    Age = 20
                };

            var configManager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(actual));
            
            // Act
            var expected = configManager.Out;

            // Assert
            Assert.IsTrue(new ConfigComparer().Equals(expected, actual));
        }

        [TestMethod]
        public void LoadSources_WithMultipleSources_EqualsMergedSource()
        {
            // Arrange
            var source1 = new Config
                {
                    Age = 10
                };
            var source2 = new Config
                {
                    Name = "Fred"
                };

            var expected = new Config
                {
                    Age = 10,
                    Name = "Fred"
                };

            var configManager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(source1),
                new DummyConfigurationSource<Config>(source2));

            // Act 
            var actual = configManager.Out;

            // Assert
            Assert.IsTrue(new ConfigComparer().Equals(expected, actual));
        }

        [TestMethod]
        public void LoadSources_WithPrimarySourceNotLast_MergesObjectsInOrder()
        {
            // Arrange
            var source1 = new Config
                {
                    Age = 10,
                    Name = "Matthew"
                };
            var source2 = new Config
                {
                    Name = "Fred",
                    Occupation = "Homeless"
                };
            var source3 = new Config
                {
                    Occupation = "Awesome"
                };
            var configManager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(source1),
                new DummyConfigurationSource<Config>(source2)
                    {
                        PrimarySource = true
                    },
                new DummyConfigurationSource<Config>(source3));

            var expected = new Config
                {
                    Age = 10,
                    Name = "Fred",
                    Occupation = "Awesome"
                };

            // Act
            var actual = configManager.Out;

            // Assert
            Assert.IsTrue(new ConfigComparer().Equals(expected, actual));
        }

        [TestMethod]
        public void LoadSources_WithNullConfigAndConfigNotRequired_IgnoresConfigAndReturnsDefaultConfig()
        {
            // Arrange
            var configManager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(null));
            var expected = new Config();

            // Act
            var actual = configManager.Out;

            // Assert
            Assert.IsTrue(new ConfigComparer().Equals(expected, actual));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void LoadSources_WithNullConfigFileAndConfigRequired_ThrowsException()
        {
// ReSharper disable ObjectCreationAsStatement
            new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(null)
                    {
                        Required = true
                    });
// ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void ChangedProperties_WhereOutAndBaseAreSame_ReturnsPrimaryChanges()
        {
            // Arrange
            var source1 = new Config
            {
                Age = 10,
                Name = "Matthew"
            };
            var primary = new Config
            {
                Name = "Fred",
                Occupation = "Homeless"
            };
            var source3 = new Config
            {
                Occupation = "Awesome"
            };
            var configManager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(source1),
                new DummyConfigurationSource<Config>(primary)
                {
                    PrimarySource = true
                },
                new DummyConfigurationSource<Config>(source3));

            var expected = new Dictionary<string, object>
                {
                    {"Name", "Fred"},
                    {"Occupation", "Homeless"}
                };

            // Act
            var actual = configManager.GetPropertyChangesByName();

            // Assert
            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestMethod]
        public void ChangedProperties_WhereOutHasUpdatedOccupation_ReturnsUpdatedOccupationOverPrimaryChanges()
        {
            // Arrange
            var source1 = new Config
            {
                Age = 10,
                Name = "Matthew"
            };
            var primary = new Config
            {
                Name = "Fred",
                Occupation = "Homeless"
            };
            var source3 = new Config
            {
                Age = 30,
                Occupation = "Awesome"
            };
            var configManager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(source1),
                new DummyConfigurationSource<Config>(primary)
                {
                    PrimarySource = true
                },
                new DummyConfigurationSource<Config>(source3));

            configManager.Out.Occupation = "Cool";

            var expected = new Dictionary<string, object>
                {
                    {"Name", "Fred"},
                    {"Occupation", "Cool"}
                };

            // Act
            var actual = configManager.GetPropertyChangesByName();

            // Assert
            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestMethod]
        public void SaveChanges_WherePrimarySourceIsNotLast_MergesPrimarySourceIntoOutput()
        {
            // Arrange
            var primary = new Demo
                {
                    Age = 20,
                    Name = "Timothy"
                };
            var secondary = new Demo
                {
                    Name = "Fred"
                };
            var expected = new Demo
                {
                    Age = 88,
                    Name = "Timothy"
                };

            var primarySource = new DummyConfigurationSource<Demo>(primary)
                {
                    PrimarySource = true
                };

            // Act
            var newConf = new ConfigurationManager<Demo>(
                primarySource,
                new DummyConfigurationSource<Demo>(secondary));
            newConf.Out.Age = 88;
            newConf.SaveChanges();

            // Assert
            Assert.AreEqual(expected, primarySource.SavedObject);
        }
    }
}
