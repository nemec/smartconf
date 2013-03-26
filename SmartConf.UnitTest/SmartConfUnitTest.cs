using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartConf.UnitTest.Mocks;
using SmartConf.Validation;

namespace SmartConf.UnitTest
{
    [TestClass]
    public class SmartConfUnitTest
    {
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
                new DummyConfigurationSource<Config>(source3))
                {
                    Out = {Occupation = "Cool"}
                };

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
            var primary = new Config
                {
                    Age = 20,
                    Name = "Timothy"
                };
            var secondary = new Config
                {
                    Name = "Fred"
                };
            var expected = new Config
                {
                    Age = 88,
                    Name = "Timothy"
                };

            var primarySource = new DummyConfigurationSource<Config>(primary)
                {
                    PrimarySource = true
                };

            // Act
            var newConf = new ConfigurationManager<Config>(
                primarySource,
                new DummyConfigurationSource<Config>(secondary))
                {
                    Out = {Age = 88}
                };
            newConf.SaveChanges();

            // Assert
            Assert.IsTrue(new ConfigComparer().Equals(expected, primarySource.SavedObject));
        }

        [TestMethod]
        public void Validation_WhereValidationIsPassing_DoesNotThrowException()
        {
            // Arrange
            var primary = new Config
            {
                Age = 20,
                Name = "Timothy"
            };

            var primarySource = new DummyConfigurationSource<Config>(primary);

            // Act
// ReSharper disable ObjectCreationAsStatement
            new ConfigurationManager<Config>(
                new ConfigValidator(), primarySource);
// ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void Validation_WhereValidationIsFailingThrowsException()
        {
            // Arrange
            var primary = new Config
            {
                Age = 12,
                Name = "Timothy"
            };

            var primarySource = new DummyConfigurationSource<Config>(primary);

            // Act
            // ReSharper disable ObjectCreationAsStatement
            new ConfigurationManager<Config>(
                new ConfigValidator(), primarySource);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void Validation_WithMultipleSourcesWhereValidationIsFailingInFirstButOverriden_DoesNotThrowException()
        {
            // Arrange
            var primary = new Config
            {
                Age = 12,
                Name = "Timothy"
            };

            var secondary = new Config
            {
                Age = 20,
            };

            var primarySource = new DummyConfigurationSource<Config>(primary);
            var secondarySource = new DummyConfigurationSource<Config>(secondary);

            // Act
            // ReSharper disable ObjectCreationAsStatement
            new ConfigurationManager<Config>(
                new ConfigValidator(), primarySource, secondarySource);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void AlwaysSerialize_WithUnchangedProperty_SerializesPropertyAnyway()
        {
            // Arrange
            var secondary = new Config
                {
                    Age = 12,
                    Name = "Timothy"
                };

            var primary = new Config();

            var expected = new Config
                {
                    Name = "Timothy"
                };

            var configManager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(secondary),
                new DummyConfigurationSource<Config>(primary));

            configManager.AlwaysSerialize(t => t.Name);

            var actualManager = new DummyConfigurationSource<Config>(new Config());
            configManager.SaveChanges(actualManager);

            Assert.IsTrue(new ConfigComparer().Equals(
                expected, actualManager.SavedObject));
        }

        [TestMethod]
        public void NeverSerialize_WithChangedProperty_DoesNotSerializeProperty()
        {
            var config = new Config
                {
                    Name = "Timothy"
                };

            var manager = new ConfigurationManager<Config>(
                new DummyConfigurationSource<Config>(config));

            manager.NeverSerialize(p => p.Occupation);

            manager.Out.Occupation = "Top Secret";

            var actualManager = new DummyConfigurationSource<Config>(new Config());
            manager.SaveChanges(actualManager);

            Assert.IsTrue(new ConfigComparer().Equals(
                config, actualManager.SavedObject));
        }
    }
}
