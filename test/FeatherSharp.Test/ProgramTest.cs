using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FeatherSharp.Feathers;
using FeatherSharp.Feathers.NpcInjection;
using FeatherSharp.Feathers.Merge;
using FeatherSharp.Feathers.LogInjection;

namespace FeatherSharp.Test
{
    /// <summary>
    /// Defines test for the <see cref="Program"/> class.
    /// </summary>
    [TestClass]
    public class ProgramTest
    {
        /// <summary>
        /// Tests <see cref="Program.ParseCommandLine"/>
        /// </summary>
        [TestMethod]
        public void TestCommandLineParsing()
        {
            IFeather[] feathers;
            var args = new[] { "-npc", "-merge", "-log", "FileName" };
            var fileName = Program.ParseCommandLineArgs(args, out feathers);

            Assert.AreEqual("FileName", fileName);
            Assert.AreEqual(feathers.Length, 3);
            Assert.IsInstanceOfType(feathers[0], typeof(NpcInjectionFeather));
            Assert.IsInstanceOfType(feathers[1], typeof(MergeFeather));
            Assert.IsInstanceOfType(feathers[2], typeof(LogInjectionFeather));
        }

        /// <summary>
        /// Tests <see cref="Program.ParseCommandLine"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCommandLineParsingException()
        {
            IFeather[] dummy;

            Program.ParseCommandLineArgs(new[] { "theOnlyArgument" }, out dummy);
        }
    }
}
