using FeatherSharp.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Test.Resources
{
    [Feather(FeatherAction.Log)]
    public class TestClass1
    {
        public void LogAllLevels()
        {
            Log.Debug("debug message");
            Log.Info("info message");
            Log.Warn("warn message");
            Log.Error("error message");
        }

        public void DontFeatherBecauseFullMethodsUsed()
        {
            Log.InternalLog("debug message", null, "TestClass1", "DontFeatherBecauseFullMethodsUsed", LogLevel.Debug);
        }

        [FeatherIgnore(FeatherAction.Log)]
        public void DontFeatherBecauseIgnored()
        {
            Log.Error("If this method was feathered something is wrong");
        }

        public void DoesntLog()
        {
            Console.WriteLine("No log call");
        }
    }

    public class UndecoratedThereforeNotLogging
    {
        public void DoLog()
        {
            Log.Error("If this class was feathered something is wrong");
        }
    }
}