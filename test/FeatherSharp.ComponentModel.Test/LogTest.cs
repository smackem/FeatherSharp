using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.ComponentModel.Test
{
    [TestClass]
    public class LogTest
    {
        [TestMethod]
        public void Test()
        {
            LogLevel? level = null;
            string text = null;

            Log.MessageRaised +=
                (sender, e) =>
                {
                    level = e.Level;
                    text = e.Text;
                };

            Log.Debug("{0}", "whatever");
            Assert.AreEqual(LogLevel.Debug, level);
            Assert.AreEqual("whatever", text);

            Log.Info("");
            Assert.AreEqual(LogLevel.Info, level);

            Log.Warn("");
            Assert.AreEqual(LogLevel.Warning, level);

            Log.Error("");
            Assert.AreEqual(LogLevel.Error, level);
        }
    }
}
