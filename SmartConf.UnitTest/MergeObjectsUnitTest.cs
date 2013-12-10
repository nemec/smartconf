using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmartConf.UnitTest
{
    [DebuggerDisplay("Name: {Name}, Age: {Age}")]
    public class Demo
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public bool Equals(Demo obj)
        {
            if (obj == null)
            {
                return false;
            }
            return Name == obj.Name && Age == obj.Age;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Demo);
        }

        public override int GetHashCode()
        {
            return 3*(Name != null ? Name.GetHashCode() : 0) +
                   5*Age.GetHashCode();
        }
    }

    public class DefaultName : Demo
    {
        public DefaultName()
        {
            Name = "Matthew";
        }
    }

    [DebuggerDisplay("Comment: {Comment}, Nested: (Name: {Nested.Name}, Age: {Nested.Age})")]
    public class NestingDemo
    {
        public string Comment { get; set; }
        public Demo Nested { get; set; }

        public NestingDemo()
        {
            Nested = new Demo();
        }

        public bool Equals(NestingDemo other)
        {
            return Comment == other.Comment && Equals(Nested, other.Nested);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as NestingDemo;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Comment != null ? Comment.GetHashCode() : 0)*397)
                       ^ (Nested != null ? Nested.GetHashCode() : 0);
            }
        }
    }

    public class NestingDemoWithNullDefaultNestedSection : NestingDemo
    {
        public NestingDemoWithNullDefaultNestedSection()
        {
            Nested = null;
        }
    }

    public class NestingDemoWithCustomizedDefaultAge : NestingDemo
    {
        public NestingDemoWithCustomizedDefaultAge()
        {
            Nested.Age = 50;
        }
    }

    [TestClass]
    public class MergeObjectsUnitTest
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MergeWith_WithNullBase_ThrowsException()
        {
            MergeObjectExtensions.MergeWith(null, new Demo
                {
                    Age = 10
                });
        }

        [TestMethod]
        public void MergeWith_WithCustomName_MergesNameIntoBase()
        {
            var @base = new Demo
                {
                    Age = 20,
                    Name = "Tim"
                };
            var @new = new Demo
                {
                    Name = "Fred"
                };
            var expected = new Demo
                {
                    Age = 20,
                    Name = "Fred"
                };

            // Act
            @base.MergeWith(@new);

            // Assert
            Assert.AreEqual(expected, @base);
        }

        [TestMethod]
        public void Merge_WithDefaultValueInConstructor_IgnoresDefaultValueWhenMerging()
        {
            var @base = new Demo
            {
                Age = 20,
                Name = "Tim"
            };
            var @new = new Demo
            {
                Age = 10
            };
            var expected = new Demo
            {
                Age = 10,
                Name = "Tim"
            };

            // Act
            @base.MergeWith(@new);

            // Assert
            Assert.AreEqual(expected, @base);
        }

        [TestMethod]
        public void Merge_WithEmptyIterable_ReturnsDefaultObject()
        {
            // Arrange
            var expected = new Demo();

            // Act
            var actual = new List<Demo>().Merge();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Merge_WithNestedObject_MergesNestedProperties()
        {
            // Arrange
            var @base = new NestingDemo
            {
                Comment = "base comment",
                Nested = new Demo
                {
                    Age = 20,
                    Name = "Tim"
                }
            };
            var @new = new NestingDemo
            {
                Comment = "new comment",
                Nested = new Demo
                {
                    Age = 10
                }
            };
            var expected = new NestingDemo
            {
                Comment = "new comment",
                Nested = new Demo
                {
                    Age = 10,
                    Name = "Tim"
                }
            };

            // Act
            @base.MergeWith(@new);

            // Assert
            Assert.AreEqual(expected, @base);
        }

        [TestMethod]
        public void Merge_WithNestedObjectNullByDefault_MergesNestedProperties()
        {
            // Arrange
            var @base = new NestingDemoWithNullDefaultNestedSection
            {
                Nested = new Demo
                {
                    Age = 20,
                    Name = "Tim"
                }
            };
            var @new = new NestingDemoWithNullDefaultNestedSection
            {
                Nested = new Demo
                {
                    Age = 10
                }
            };
            var expected = new NestingDemoWithNullDefaultNestedSection
            {
                Nested = new Demo
                {
                    Age = 10,
                    Name = "Tim"
                }
            };

            // Act
            @base.MergeWith(@new);

            // Assert
            Assert.AreEqual(expected, @base);
        }

        [TestMethod]
        public void Merge_WithNestedObject_TakesParentSpecificDefaultsIntoAccount()
        {
            // Arrange
            var @base = new NestingDemoWithCustomizedDefaultAge
            {
                Nested = new Demo
                {
                    Age = 0
                }
            };
            var @new = new NestingDemoWithCustomizedDefaultAge{};
            var expected = new NestingDemoWithCustomizedDefaultAge
            {
                Nested = new Demo
                {
                    Age = 0,
                }
            };

            // Act
            @base.MergeWith(@new);

            // Assert
            Assert.AreEqual(expected, @base);
        }
    }
}
