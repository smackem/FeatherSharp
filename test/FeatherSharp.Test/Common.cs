using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Test
{
    /// <summary>
    /// Provides static utility methods used in test classes.
    /// </summary>
    static class Common
    {
        /// <summary>
        /// Tests whether the assembly with the given <paramref name="fileName"/>
        /// can be verified using the peverify utility.
        /// </summary>
        /// <param name="fileName">File name of the assembly to test.</param>
        /// <remarks>
        /// Uses the <see cref="Assert"/> type, so calling this method is only
        /// valid from test methods.
        /// </remarks>
        public static void TestWithPeVerify(string fileName)
        {
            var programFilesPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%");
            var peverifyPath = Path.Combine(programFilesPath, @"Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\PEVerify.exe");

            if (File.Exists(peverifyPath))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = peverifyPath,
                        Arguments = fileName + " /md /il",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                    },
                };

                process.Start();

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(output);
                Assert.IsTrue(output.EndsWith(@"All Classes and Methods in " + fileName + " Verified." + Environment.NewLine));
            }
            else
            {
                Assert.Inconclusive("PEVerify.exe tool could not be located!");
            }
        }
    }
}
