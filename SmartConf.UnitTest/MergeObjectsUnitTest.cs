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
    }
}
