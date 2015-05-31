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
            var args = new[] { "-npc", "-merge", "-log", "FileName" };
            var fileName = Program.ParseCommandLineArgs(args, out feathers);

            Assert.AreEqual("FileName", fileName);
            Assert.AreEqual(feathers.Length, 3);
            Assert.IsInstanceOf(typeof(NpcInjectionFeather), feathers[0]);
            Assert.IsInstanceOf(typeof(MergeFeather), feathers[1]);
            Assert.IsInstanceOf(typeof(LogInjectionFeather), feathers[2]);
        }

        /// <summary>
        /// Tests <see cref="Program.ParseCommandLine"/>
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCommandLineParsingException()
        {
            IFeather[] dummy;

            Program.ParseCommandLineArgs(new[] { "theOnlyArgument" }, out dummy);
        }
    }
}
