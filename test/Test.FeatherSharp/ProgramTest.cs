using System;
using FeatherSharp.Feathers;
using FeatherSharp.Feathers.NpcInjection;
using FeatherSharp.Feathers.Merge;
using FeatherSharp.Feathers.LogInjection;
using NUnit.Framework;

namespace FeatherSharp.Test
{
    /// <summary>
    /// Defines test for the <see cref="Program"/> class.
    /// </summary>
    [TestFixture]
    public class ProgramTest
    {
        /// <summary>
        /// Tests <see cref="Program.ParseCommandLine"/>
        /// </summary>
        [Test]
        public void TestCommandLineParsing()
        {
            IFeather[] feathers;
            Options options;
            var args = new[] { "-npc", "-merge", "--sn=StrongFile", "-log", "FileName" };
            var fileName = Program.ParseCommandLineArgs(args, out feathers, out options);

            Assert.AreEqual("FileName", fileName);
            Assert.AreEqual(feathers.Length, 3);
            Assert.IsInstanceOf(typeof(NpcInjectionFeather), feathers[0]);
            Assert.IsInstanceOf(typeof(MergeFeather), feathers[1]);
            Assert.IsInstanceOf(typeof(LogInjectionFeather), feathers[2]);
            Assert.AreEqual("StrongFile", options.StrongNameFile);
        }

        /// <summary>
        /// Tests <see cref="Program.ParseCommandLine"/>
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCommandLineParsingException()
        {
            IFeather[] dummy;
            Options options;

            Program.ParseCommandLineArgs(new[] { "theOnlyArgument" }, out dummy, out options);
        }
    }
}
