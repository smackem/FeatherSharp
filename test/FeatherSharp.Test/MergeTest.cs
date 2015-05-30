using FeatherSharp.Feathers.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Test
{
    [TestClass]
    public class MergeTest
    {
        static readonly string ExecutableFileName = "FeatherSharp.TestExecutable2.exe";
        static readonly string LibraryFileName = "FeatherSharp.TestLibrary2.dll";
        static readonly string TestDirectory = "temp";

        [TestInitialize]
        public void Initialize()
        {
            Directory.CreateDirectory(TestDirectory);
            Directory.SetCurrentDirectory(TestDirectory);
            CopyResourceToFile(ExecutableFileName);
            CopyResourceToFile(LibraryFileName);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + @"\..");

            if (File.Exists(ExecutableFileName))
                File.Delete(ExecutableFileName);

            if (File.Exists(LibraryFileName))
                File.Delete(LibraryFileName);
        }

        [TestMethod]
        public void TestMerge()
        {
            var br = Environment.NewLine;
            var expectedOutput = "Output from library" + br
                               + "Output from executable" + br;

            Assert.AreEqual(expectedOutput, CaptureExecutableOutput());

            var assembly = AssemblyDefinition.ReadAssembly(ExecutableFileName);
            var callback = new MergeCallback();
            new MergeFeather(callback).Execute(assembly.MainModule, ExecutableFileName);
            assembly.Write(ExecutableFileName);

            CollectionAssert.AreEquivalent(
                new[] { Tuple.Create(LibraryFileName, MergeFileInfo.Merged) },
                callback.MergedFileNames);

            File.Delete(LibraryFileName);

            Assert.AreEqual(expectedOutput, CaptureExecutableOutput());
        }

        [TestMethod]
        public void TestWithPeVerify()
        {
            Common.TestWithPeVerify(ExecutableFileName);
        }

        ///////////////////////////////////////////////////////////////////////

        void CopyResourceToFile(string fileName)
        {
            var outStream = File.Create(fileName);

            var assembly = GetType().Assembly;
            var resourceStream = assembly.GetManifestResourceStream(
                "FeatherSharp.Test.Resources." + fileName);

            using (resourceStream)
            using (outStream)
                resourceStream.CopyTo(outStream);
        }

        string CaptureExecutableOutput()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ExecutableFileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                },
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }

    class MergeCallback : IMergeCallback
    {
        public readonly List<Tuple<string, MergeFileInfo>> MergedFileNames =
            new List<Tuple<string, MergeFileInfo>>();

        public void NotifyFile(string fileName, MergeFileInfo info)
        {
            MergedFileNames.Add(Tuple.Create(fileName, info));
        }
    }

}
