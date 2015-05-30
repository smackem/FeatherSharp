using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.TestExecutable2
{
    class Program
    {
        static void Main(string[] args)
        {
            FeatherSharp.TestLibrary2.Class1.PrintOutput();
            Console.WriteLine("Output from executable");
        }

        static Assembly AppDomain_AssemblyResolve_ReferenceImpl(object obj, ResolveEventArgs resolveEventArgs)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var name = String.Format("{0}.{1}.dll", "<Merged>", new AssemblyName(resolveEventArgs.Name).Name);
            Console.WriteLine("AppDomain_AssemblyResolve: {0}", name);
            var manifestResourceStream = entryAssembly.GetManifestResourceStream(name);

            if (manifestResourceStream == null)
                return null;

            var array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);
            manifestResourceStream.Dispose();
            return Assembly.Load(array);
        }
    }
}
