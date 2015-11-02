using FeatherSharp.ComponentModel;
using FeatherSharp.Feathers.LogInjection;
using Microsoft.CSharp;
using Mono.Cecil;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FeatherSharp.Test
{
    /// <summary>
    /// Defines tests for the classes <see cref="LogInjectionFeather"/> and <see cref="LogInjectionRunner"/>.
    /// </summary>
    [TestFixture]
    public class LogInjectionFeatherTest
    {
        static readonly string OutputFileName = "LogInjectionTest.dll";
        static readonly string ModifiedOutputFileName = "LogInjectionTest.mod.dll";
        AssemblyDefinition assembly;

        /// <summary>
        /// Compiles the embedded source file into an on-disk assembly which can then
        /// be feathered by the test methods.
        /// </summary>
        [SetUp]
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
        [TearDown]
        public void Cleanup()
        {
            //if (File.Exists(OutputFileName))
            //    File.Delete(OutputFileName);

            //if (File.Exists(ModifiedOutputFileName))
            //    File.Delete(ModifiedOutputFileName);
        }

        /// <summary>
        /// Tests the basic log injection.
        /// </summary>
        [Test]
        public void TestBasicLogInjection()
        {
            var callback = new LogFeatherCallback();
            var featherImpl = new LogInjectionRunner(this.assembly.MainModule, callback);

            var type = this.assembly.MainModule.GetType("FeatherSharp.Test.Resources.TestClass1");

            featherImpl.ProcessType(type);

            CollectionAssert.AreEqual(
                Tuple.Create("FeatherSharp.Test.Resources.TestClass1",
                    LogInjectionTypeInfo.Ok).Singleton(),
                callback.TypeInfos);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::LogAllLevels",
                        LogInjectionMethodInfo.Ok),
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::TestMethodAsync",
                        LogInjectionMethodInfo.Ok),
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::DontFeatherBecauseIgnored",
                        LogInjectionMethodInfo.Ignored),
                },
                callback.MethodInfos);
        }

        /// <summary>
        /// Runs the peverify utility against the feathered assembly. Looks for the utility
        /// in the .net 4.5.1 tools directory.
        /// If peverify.exe is not found, the test is marked inconclusive.
        /// </summary>
        [Test]
        public void TestWithPeVerify()
        {
            FeatherAndWriteAssembly();

            Common.TestWithPeVerify(ModifiedOutputFileName);
        }

        /// <summary>
        /// Tests behavior of the feathered assembly.
        /// </summary>
        [Test]
        public void TestLive()
        {
            FeatherAndWriteAssembly();

            var obj = CreateObjectFromModifiedAssembly("TestClass1");
            var type = obj.GetType();

            var messages = new List<Tuple<string, LogLevel, string>>();

            Log.MessageRaised += (sender, e) =>
            {
                messages.Add(Tuple.Create(
                        e.TypeName + "::" + e.MethodName,
                        e.Level,
                        e.Text));
            };

            type.GetMethod("LogAllLevels").Invoke(obj, null);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::LogAllLevels",
                        LogLevel.Trace, "trace message"),
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::LogAllLevels",
                        LogLevel.Debug, "debug message"),
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::LogAllLevels",
                        LogLevel.Info, "info message"),
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::LogAllLevels",
                        LogLevel.Warning, "warn message"),
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::LogAllLevels",
                        LogLevel.Error, "error message"),
                },
                messages);

            messages.Clear();

            var task = type.GetMethod("TestMethodAsync").Invoke(obj, null) as Task;
            task.Wait();

            CollectionAssert.AreEquivalent(
                new[]
                {
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::TestMethodAsync",
                        LogLevel.Trace, "trace message"),
                    Tuple.Create("FeatherSharp.Test.Resources.TestClass1::TestMethodAsync",
                        LogLevel.Debug, "debug message"),
                },
                messages);
        }

        ///////////////////////////////////////////////////////////////////////

        void FeatherAndWriteAssembly()
        {
            var callback = new LogFeatherCallback();
            var feather = new LogInjectionFeather(callback);

            feather.Execute(this.assembly.MainModule, OutputFileName);
            this.assembly.Write(ModifiedOutputFileName, new WriterParameters());
        }

        object CreateObjectFromModifiedAssembly(string typeName)
        {
            // loading the assembly locks the assembly file for the lifetime of the test process, so copy it
            var fileName = Path.GetTempFileName();
            File.Copy(ModifiedOutputFileName, fileName, overwrite: true);
            var assembly = System.Reflection.Assembly.LoadFrom(fileName);
            var type = assembly.GetType("FeatherSharp.Test.Resources." + typeName, throwOnError: true);
            return Activator.CreateInstance(type);
        }

        string ReadSource()
        {
            var assembly = GetType().Assembly;
            var resourceStream = assembly.GetManifestResourceStream("Test.FeatherSharp.Resources.LogInjectionTestProgram.cs");

            using (var reader = new StreamReader(resourceStream))
                return reader.ReadToEnd();
        }
    }

    class LogFeatherCallback : ILogInjectionCallback
    {
        public readonly List<Tuple<string, LogInjectionTypeInfo>> TypeInfos =
            new List<Tuple<string, LogInjectionTypeInfo>>();
        public readonly List<Tuple<string, LogInjectionMethodInfo>> MethodInfos =
            new List<Tuple<string, LogInjectionMethodInfo>>();

        public void NotifyType(string qualifiedTypeName, LogInjectionTypeInfo info)
        {
            TypeInfos.Add(Tuple.Create(qualifiedTypeName, info));
        }

        public void NotifyMethod(string qualifiedMethodName, LogInjectionMethodInfo info)
        {
            MethodInfos.Add(Tuple.Create(qualifiedMethodName, info));
        }
    }

}
