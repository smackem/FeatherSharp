using FeatherSharp.Feathers.LogInjection;
using FeatherSharp.Feathers.Merge;
using FeatherSharp.Feathers.NpcInjection;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp
{
    class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            Feathers.IFeather[] feathers;
            var fileName = ParseCommandLineArgs(args, out feathers);

            if (feathers.Any(f => f == null))
            {
                Console.WriteLine("Invalid Feather!");
                Console.WriteLine();
                PrintUsage();
                return;
            }

            var assembly = ReadAssembly(fileName);
            if (assembly == null)
                return;

            foreach (var feather in feathers)
                feather.Execute(assembly.MainModule, fileName);

            WriteAssembly(assembly, fileName);
        }

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="feathers">Receives the feathers to execute.</param>
        /// <returns>The name of the file that contains the assembly feather.</returns>
        /// <exception cref="System.ArgumentException">Less than two command line arguments passed!</exception>
        /// <remarks>
        /// <para>Expected command line format:
        /// <code>FeatherSharp.exe [-npc] [-merge] [-log] FileName</code></para>
        /// <para>Internal for unit testing.</para>
        /// </remarks>
        internal static string ParseCommandLineArgs(string[] args,
            out Feathers.IFeather[] feathers)
        {
            if (args.Length < 2)
                throw new ArgumentException("Less than two command line arguments passed!");

            var factory = CreateFeatherFactory();

            Func<string, Feathers.IFeather> getFeather = arg =>
            {
                Func<Feathers.IFeather> factoryMethod;

                return factory.TryGetValue(arg, out factoryMethod)
                       ? factoryMethod()
                       : null;
            };

            feathers = args.Take(args.Length - 1)
                .Select(arg => getFeather(arg))
                .ToArray();

            return args.Last();
        }

        static AssemblyDefinition ReadAssembly(string fileName)
        {
            var readerParameters = new ReaderParameters { ReadSymbols = true };

            try
            {
                return AssemblyDefinition.ReadAssembly(fileName, readerParameters);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading assembly: {0}", ex.Message);
            }

            return null;
        }

        static void WriteAssembly(AssemblyDefinition assembly, string fileName)
        {
            var writerParameters = new WriterParameters { WriteSymbols = true };

            try
            {
                assembly.Write(fileName, writerParameters);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error writing assembly: {0}", ex.Message);
            }
        }

        static void PrintUsage()
        {
            var br = Environment.NewLine;

            Console.WriteLine("USAGE: FeatherSharp.exe <Feathers> <FileName>" + br
                            + "Feathers:" + br
                            + "  -npc : Inject NotifyPropertyChanged" + br
                            + "  -merge : Merge dependencies into <FileName>" + br
                            + "  -log : Inject augmented log method calls");
        }

        static IDictionary<string, Func<Feathers.IFeather>> CreateFeatherFactory()
        {
            var dict = new Dictionary<string, Func<Feathers.IFeather>>();

            dict.Add("-npc", () =>
                new NpcInjectionFeather(new NpcInjectionCallback()));
            dict.Add("-merge", () =>
                new MergeFeather(new MergeCallback()));
            dict.Add("-log", () =>
                new LogInjectionFeather(new LogInjectionCallback()));

            return dict;
        }
    }

    class NpcInjectionCallback : INpcInjectionCallback
    {
        public void NotifyProperty(string qualifiedPropertyName, NpcInjectionPropertyInfo info)
        {
            Console.WriteLine("Property {0}: {1}", qualifiedPropertyName, info);
        }

        public void NotifyPropertyDependencies(string qualifiedPropertyName, IEnumerable<string> dependencies)
        {
            Console.WriteLine("Property {0} Dependencies: {1}",
                qualifiedPropertyName, String.Join(", ", dependencies));
        }

        public void NotifyType(string qualifiedTypeName, NpcInjectionTypeInfo info)
        {
            Console.WriteLine("Type {0}: {1}", qualifiedTypeName, info);
        }
    }

    class MergeCallback : IMergeCallback
    {
        public void NotifyFile(string fileName, MergeFileInfo info)
        {
            Console.WriteLine("File {0}: {1}", fileName, info);
        }
    }

    class LogInjectionCallback : ILogInjectionCallback
    {
        public void NotifyType(string qualifiedTypeName, LogInjectionTypeInfo info)
        {
            Console.WriteLine("Type {0}: {1}", qualifiedTypeName, info);
        }

        public void NotifyMethod(string qualifiedMethodName, LogInjectionMethodInfo info)
        {
            Console.WriteLine("Method {0}: {1}", qualifiedMethodName, info);
        }
    }
}
