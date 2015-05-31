using NUnit.Framework;
using System;
using System.Collections.Generic;
using FeatherSharp.ComponentModel;

namespace Test.FeatherSharp.ComponentModel
{
    /// <summary>
    /// Defines tests for the class <see cref="NotifyPropertyChanged"/>.
    /// </summary>
    [TestFixture]
    public class NotifyPropertyChangedTest
    {
        /// <summary>
        /// Tests whether property changes for dependent properties are signalled
        /// correctly.
        /// </summary>
        [Test]
        public void TestPropertyDependencies()
        {
            var testObj = new TestClassWithDependenciesBase();
            var changedProperties = new List<string>();

            testObj.PropertyChanged += (_, e) =>
                changedProperties.Add(e.PropertyName);

            testObj.A = 100;
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, changedProperties);
        }

        /// <summary>
        /// Tests whether property changes for dependent properties are signalled
        /// correctly, even across base type boundaries.
        /// </summary>
        [Test]
        public void TestPropertyDependenciesWithBaseType()
        {
            var testObj = new TestClassWithDependencies();
            var changedProperties = new List<string>();

            testObj.PropertyChanged += (_, e) =>
                changedProperties.Add(e.PropertyName);

            // "E" will be signalled twice when "A" is signalled because of dependency on "B"
            // which in turn also depends on "A"
            testObj.A = 100;
            CollectionAssert.AreEquivalent(new[] { "A", "B", "D", "E", "E" }, changedProperties);

            changedProperties.Clear();
            testObj.C = 123;
            CollectionAssert.AreEquivalent(new[] { "C", "D", "E" }, changedProperties);
        }
    }

    class TestClassWithDependenciesBase : NotifyPropertyChanged
    {
        int _a;

        public int A
        {
            get { return _a; }
            set
            {
                _a = value;
                OnPropertyChanged("A");
            }
        }

        [DependsUpon("A")]
        public int B
        {
            get { return A + 10; }
        }
    }

    class TestClassWithDependencies : TestClassWithDependenciesBase
    {
        int _c;

        public int C
        {
            get { return _c; }
            set
            {
                _c = value;
                OnPropertyChanged("C");
            }
        }

        [DependsUpon("A")]
        [DependsUpon("C")]
        public int D
        {
            get { return A + C; }
        }

        [DependsUpon("D")]
        [DependsUpon("B")]
        public int E
        {
            get { return D + B; }
        }
    }
}
