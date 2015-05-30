using FeatherSharp.ComponentModel;
using FeatherSharp.Feathers.NpcInjection;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FeatherSharp.Test
{
    /// <summary>
    /// Defines tests for the classes <see cref="NpcInjectionFeather"/> and <see cref="NpcInjectionRunner"/>.
    /// </summary>
    [TestClass]
    public class NpcInjectionFeatherTest
    {
        static readonly string OutputFileName = "NpcInjectionTest.dll";
        static readonly string ModifiedOutputFileName = "NpcInjectionTest.mod.dll";
        AssemblyDefinition assembly;


        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Compiles the embedded source file into an on-disk assembly which can then
        /// be feathered by the test methods.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            var codeProvider = new CSharpCodeProvider();
            var source = ReadSource();
            var references = new[]
            {
                typeof(System.Linq.Enumerable).Assembly.GetName().Name + ".dll",
                typeof(INotifyPropertyChanged).Assembly.GetName().Name + ".dll",
                typeof(FeatherAttribute).Assembly.GetName().Name + ".dll",
            };

            var parameters = new CompilerParameters(references)
            {
                GenerateInMemory = false,
                OutputAssembly = OutputFileName,
            };
            var results = codeProvider.CompileAssemblyFromSource(parameters, source);

            if (results.Errors.HasErrors)
                throw new Exception("Compilation of embedded source code failed.");

            this.assembly = AssemblyDefinition.ReadAssembly(results.PathToAssembly);

            var oldAssemblyName = this.assembly.Name;
            var modifiedAssemblyName = Path.ChangeExtension(ModifiedOutputFileName, null);
            this.assembly.Name = new AssemblyNameDefinition(modifiedAssemblyName, oldAssemblyName.Version);
        }

        /// <summary>
        /// Removes the files created by <see cref="Initialize()"/>.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(OutputFileName))
                File.Delete(OutputFileName);

            if (File.Exists(ModifiedOutputFileName))
                File.Delete(ModifiedOutputFileName);
        }

        /// <summary>
        /// Test whether the feathering process yields the right notifications about properties.
        /// </summary>
        [TestMethod]
        public void TestBasicNpcInjection()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(this.assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClass1");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClass1",
                    NpcInjectionTypeInfo.Ok).Singleton(),
                callback.TypeInfos);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    Tuple.Create("TestProperty_Int", NpcInjectionPropertyInfo.Ok),
                    Tuple.Create("TestProperty_Object", NpcInjectionPropertyInfo.Ok),
                    Tuple.Create("TestProperty_Struct", NpcInjectionPropertyInfo.Ok),
                    Tuple.Create("TestProperty_String", NpcInjectionPropertyInfo.Ok),
                    Tuple.Create("TestProperty_Complex", NpcInjectionPropertyInfo.NoEqualityCheckInjected),
                    Tuple.Create("TestProperty_WithBodies", NpcInjectionPropertyInfo.Ok),
                    Tuple.Create("TestProperty_WithRaise", NpcInjectionPropertyInfo.AlreadyCallsRaiseMethod),
                },
                callback.PropertyInfos);
        }

        /// <summary>
        /// Test whether the feathering process correctly evaluates the
        /// <see cref="FeatherAttribute"/> and only
        /// processes types decorated with this attribute.
        /// </summary>
        [TestMethod]
        public void TestTypeFiltering()
        {
            var callback = new NpcFeatherCallback();
            var feather = new NpcInjectionFeather(callback);

            feather.Execute(this.assembly.MainModule, OutputFileName);

            Action<string> assertContains = typeName =>
            {
                Assert.IsTrue(callback.TypeInfos.Any(typeInfo =>
                    typeInfo.Item1 == "FeatherSharp.Test.Resources." + typeName),
                    "Type " + typeName + " was not processed");
            };

            Action<string> assertNotContains = typeName =>
            {
                Assert.IsFalse(callback.TypeInfos.Any(typeInfo =>
                    typeInfo.Item1 == "FeatherSharp.Test.Resources." + typeName),
                    "Type " + typeName + " was processed by mistake");
            };

            assertContains("TestClass1");
            assertContains("TestClassWithBase");
            assertContains("TestClassBase");
            assertContains("TestClassWithouthInpc");
            assertContains("TestClassWithouthRaiseMethod");
            assertContains("TestClassWithOverride");
            assertContains("TestClassWithInaccessibleRaiseMethod");
            assertContains("TestClassWithInaccessibleRaiseMethodBase");
            assertContains("TestClassWithDependenciesBase");
            assertContains("TestClassWithDependencies");
            assertContains("TestClassWithDependencies");
            assertContains("TestClassContainer");
            assertContains("TestClassContainer/Nested");

            assertNotContains("TestClassWithOverrideBase");
        }

        /// <summary>
        /// Loads the feathered assembly and uses reflection to test whether the
        /// property-changed event is raised correctly.
        /// </summary>
        [TestMethod]
        public void TestFeatheredAssemblyLive()
        {
            FeatherAndWriteAssembly();

            var obj = CreateObjectFromModifiedAssembly("TestClass1");
            var type = obj.GetType();
            var changedProperty = null as string;
            obj.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            Action<string, object, bool> setProperty = (propertyName, value, interceptsEquality) =>
            {
                type.GetProperty(propertyName).SetValue(obj, value);
                Assert.AreEqual(propertyName, changedProperty);

                changedProperty = null;
                type.GetProperty(propertyName).SetValue(obj, value);

                if (interceptsEquality)
                    Assert.AreNotEqual(propertyName, changedProperty);
                else
                    Assert.AreEqual(propertyName, changedProperty);
            };

            setProperty("TestProperty_Int", 100, true);
            setProperty("TestProperty_Object", new Object(), true);
            setProperty("TestProperty_Struct", DateTime.Now, true);
            setProperty("TestProperty_String", "hello", true);
            setProperty("TestProperty_Complex", 123.5, false);
            setProperty("TestProperty_WithBodies", true, true);
            setProperty("TestProperty_WithRaise", 12.5f, false);
        }

        /// <summary>
        /// Runs the peverify utility against the feathered assembly. Looks for the utility
        /// in the .net 4.5.1 tools directory.
        /// If peverify.exe is not found, the test is marked inconclusive.
        /// </summary>
        [TestMethod]
        public void TestWithPeVerify()
        {
            FeatherAndWriteAssembly();

            Common.TestWithPeVerify(ModifiedOutputFileName);
        }

        /// <summary>
        /// Tests whether the base class implementing <see cref="INotifyPropertyChanged"/> is found.
        /// </summary>
        [TestMethod]
        public void TestTypeHierarchy()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(this.assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClassWithBase");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClassWithBase",
                    NpcInjectionTypeInfo.Ok).Singleton(),
                callback.TypeInfos);

            CollectionAssert.AreEqual(
                Tuple.Create("MyProperty", NpcInjectionPropertyInfo.Ok).Singleton(),
                callback.PropertyInfos);
        }

        /// <summary>
        /// Tests whether processing a class that does not implement <see cref="INotifyPropertyChanged"/>
        /// yields the right notification.
        /// </summary>
        [TestMethod]
        public void TestTypeWithoutInpc()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClassWithouthInpc");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClassWithouthInpc",
                    NpcInjectionTypeInfo.InpcNotImplemented).Singleton(),
                callback.TypeInfos);
        }

        /// <summary>
        /// Tests whether processing a class that does not define an <code>OnPropertyChanged</code>
        /// method yields the right notification.
        /// </summary>
        [TestMethod]
        public void TestTypeWithoutRaiseMethod()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(this.assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClassWithouthRaiseMethod");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClassWithouthRaiseMethod",
                    NpcInjectionTypeInfo.MissingRaiseMethod).Singleton(),
                callback.TypeInfos);
        }

        /// <summary>
        /// Tests whether processing a class with an inaccessible <code>OnPropertyChanged</code>
        /// method yields the right notification.
        /// </summary>
        [TestMethod]
        public void TestInaccessibleRaiseMethod()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(this.assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClassWithInaccessibleRaiseMethod");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClassWithInaccessibleRaiseMethod",
                    NpcInjectionTypeInfo.InaccessibleRaiseMethod).Singleton(),
                callback.TypeInfos);
        }

        /// <summary>
        /// Loads the feathered assembly and uses reflection to test whether overrides
        /// of the <code>OnPropertyChanged</code> method are called correctly.
        /// </summary>
        [TestMethod]
        public void TestOverriddenRaiseMethod()
        {
            FeatherAndWriteAssembly();

            var obj = CreateObjectFromModifiedAssembly("TestClassWithOverride");
            var type = obj.GetType();
            var changedProperty = null as string;
            obj.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            type.GetProperty("MyProperty").SetValue(obj, 123);
            Assert.AreEqual("MyProperty", changedProperty);
            Assert.IsTrue((bool)type.GetField("OverrideMethodCalled").GetValue(obj));
        }

        /// <summary>
        /// Tests whether property dependencies get recognized correctly.
        /// </summary>
        [TestMethod]
        public void TestTypeWithDependenciesBase()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(this.assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClassWithDependenciesBase");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClassWithDependenciesBase",
                    NpcInjectionTypeInfo.OkWithDependencies).Singleton(),
                callback.TypeInfos);

            CollectionAssert.AreEqual(
                Tuple.Create("A", NpcInjectionPropertyInfo.Ok).Singleton(),
                callback.PropertyInfos);

            CollectionAssert.AreEqual(
                Tuple.Create("B", "A").Singleton(),
                callback.PropertyDependencies);
        }

        /// <summary>
        /// Tests whether property dependencies get recognized correctly across
        /// base type boundaries.
        /// </summary>
        [TestMethod]
        public void TestTypeWithDependencies()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(this.assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClassWithDependencies");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClassWithDependencies",
                    NpcInjectionTypeInfo.OkWithDependencies).Singleton(),
                callback.TypeInfos);

            CollectionAssert.AreEquivalent(
                Tuple.Create("C", NpcInjectionPropertyInfo.Ok).Singleton(),
                callback.PropertyInfos);

            CollectionAssert.AreEqual(
                new[]
                {
                    Tuple.Create("D", "A,C"),
                    Tuple.Create("E", "D,B"),
                },
                callback.PropertyDependencies);
        }

        /// <summary>
        /// Loads the feathered assembly and uses reflection to test whether property
        /// dependencies are signalled correctly.
        /// </summary>
        [TestMethod]
        public void TestDependenciesLive()
        {
            FeatherAndWriteAssembly();

            var obj = CreateObjectFromModifiedAssembly("TestClassWithDependenciesBase");
            var type = obj.GetType();
            var changedProperties = new List<string>();

            obj.PropertyChanged += (_, e) =>
                changedProperties.Add(e.PropertyName);

            type.GetProperty("A").SetValue(obj, 123);
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, changedProperties);
        }

        /// <summary>
        /// Loads the feathered assembly and uses reflection to test whether property
        /// dependencies are signalled correctly across base type boundaries.
        /// </summary>
        [TestMethod]
        public void TestDependenciesWithBaseTypeLive()
        {
            FeatherAndWriteAssembly();

            // loading the assembly locks the assembly file for the lifetime of the test process, so copy it
            var obj = CreateObjectFromModifiedAssembly("TestClassWithDependencies");
            var type = obj.GetType();
            var changedProperties = new List<string>();

            obj.PropertyChanged += (_, e) =>
                changedProperties.Add(e.PropertyName);

            type.GetProperty("C").SetValue(obj, 123);
            CollectionAssert.AreEquivalent(new[] { "C", "D", "E" }, changedProperties);

            changedProperties.Clear();
            type.GetProperty("A").SetValue(obj, 123);
            CollectionAssert.AreEquivalent(new[] { "A", "B", "D", "E", "E" }, changedProperties);
        }

        [TestMethod]
        public void TestIgnoreProperty()
        {
            var callback = new NpcFeatherCallback();
            var featherImpl = new NpcInjectionRunner(this.assembly.MainModule, callback);
            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClassWithIgnoredProperty");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                new[]
                {
                    Tuple.Create("MyProperty", NpcInjectionPropertyInfo.Ok),
                    Tuple.Create("MyIgnoredProperty", NpcInjectionPropertyInfo.Ignored),
                },
                callback.PropertyInfos);
        }


        ///////////////////////////////////////////////////////////////////////

        void FeatherAndWriteAssembly()
        {
            var callback = new NpcFeatherCallback();
            var feather = new NpcInjectionFeather(callback);

            feather.Execute(this.assembly.MainModule, OutputFileName);
            this.assembly.Write(ModifiedOutputFileName, new WriterParameters());
        }

        INotifyPropertyChanged CreateObjectFromModifiedAssembly(string typeName)
        {
            // loading the assembly locks the assembly file for the lifetime of the test process, so copy it
            var fileName = Path.GetTempFileName();
            File.Copy(ModifiedOutputFileName, fileName, overwrite: true);
            var assembly = System.Reflection.Assembly.LoadFrom(fileName);
            var type = assembly.GetType("FeatherSharp.Test.Resources." + typeName, throwOnError: true);
            return (INotifyPropertyChanged)Activator.CreateInstance(type);
        }

        string ReadSource()
        {
            var assembly = GetType().Assembly;
            var resourceStream = assembly.GetManifestResourceStream("FeatherSharp.Test.Resources.NpcInjectionTestProgram.cs");

            using (var reader = new StreamReader(resourceStream))
                return reader.ReadToEnd();
        }
    }

    class NpcFeatherCallback : INpcInjectionCallback
    {
        public readonly List<Tuple<string, NpcInjectionPropertyInfo>> PropertyInfos =
            new List<Tuple<string, NpcInjectionPropertyInfo>>();

        public readonly List<Tuple<string, string>> PropertyDependencies =
            new List<Tuple<string, string>>();

        public readonly List<Tuple<string, NpcInjectionTypeInfo>> TypeInfos =
            new List<Tuple<string, NpcInjectionTypeInfo>>();

        public void NotifyProperty(string qualifiedPropertyName, NpcInjectionPropertyInfo info)
        {
            var lastDotIndex = qualifiedPropertyName.LastIndexOf(':');
            var propertyName = qualifiedPropertyName.Substring(lastDotIndex + 1);

            PropertyInfos.Add(Tuple.Create(propertyName, info));
        }

        public void NotifyPropertyDependencies(string qualifiedPropertyName, IEnumerable<string> dependencies)
        {
            var dependenciesStr = String.Join(",", dependencies);
            var lastDotIndex = qualifiedPropertyName.LastIndexOf(':');
            var propertyName = qualifiedPropertyName.Substring(lastDotIndex + 1);

            PropertyDependencies.Add(Tuple.Create(propertyName, dependenciesStr));
        }

        public void NotifyType(string qualifiedTypeName, NpcInjectionTypeInfo info)
        {
            TypeInfos.Add(Tuple.Create(qualifiedTypeName, info));
        }
    }
}
