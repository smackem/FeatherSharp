using FeatherSharp.Feathers.LogInjection;
using FeatherSharp.Feathers.Merge;
using FeatherSharp.Feathers.NpcInjection;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            Console.WriteLine("Feather# v{0}", typeof(Program).Assembly.GetName().Version);

            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            Feathers.IFeather[] feathers;
            Options options;
            var fileName = ParseCommandLineArgs(args, out feathers, out options);

            if (fileName == null)
            {
                PrintUsage();
                return;
            }

            var assembly = ReadAssembly(fileName);
            if (assembly == null)
                return;

            foreach (var feather in feathers)
                feather.Execute(assembly.MainModule, fileName);

            WriteAssembly(assembly, fileName, options.StrongNameFile);
        }

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="feathers">Receives the feathers to execute.</param>
        /// <param name="options">Receives the options to use.</param>
        /// <returns>The name of the file that contains the assembly feather.</returns>
        /// <exception cref="System.ArgumentException">Less than two command line arguments passed!</exception>
        /// <remarks>
        /// <para>Expected command line format:
        /// <code>FeatherSharp.exe [-npc] [-merge] [-log] FileName</code></para>
        /// <para>Internal for unit testing.</para>
        /// </remarks>
        internal static string ParseCommandLineArgs(string[] args,
            out Feathers.IFeather[] feathers, out Options options)
        {
            if (args.Length < 2)
                throw new ArgumentException("Less than two command line arguments passed!");

            feathers = null;
            options = null;

            var factory = CreateFeatherFactory();

            Func<string, Feathers.IFeather> getFeather = arg =>
            {
                Func<Feathers.IFeather> factoryMethod;

                return factory.TryGetValue(arg, out factoryMethod)
                       ? factoryMethod()
                       : null;
            };

            var localFeathers = new List<Feathers.IFeather>();
            var localOptions = new Options();

            foreach (var arg in args.Take(args.Length - 1))
            {
                if (arg.StartsWith("--"))
                {
                    if (ParseOption(arg, localOptions) == false)
                    {
                        Console.WriteLine("Invalid option: '{0}'", arg);
                        return null;
                    }
                }
                else if (arg.StartsWith("-"))
                {
                    var feather = getFeather(arg);

                    if (feather == null)
                    {
                        Console.WriteLine("Invalid feather: '{0}'", arg);
                        return null;
                    }
                    else
                    {
                        localFeathers.Add(feather);
                    }
                }
            }

            feathers = localFeathers.ToArray();
            options = localOptions;
            return args.Last();
        }

        static bool ParseOption(string arg, Options options)
        {
            var tokens = arg.Split('=');

            switch (tokens[0].ToLower())
            {
                case "--sn":
                    if (tokens.Length == 2)
                        options.StrongNameFile = tokens[1];
                    break;

                default:
                    return false;
            }

            return true;
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

        static void WriteAssembly(AssemblyDefinition assembly, string fileName, string strongNameFile)
        {
            try
            {
                var strongName = strongNameFile != null
                                 ? new StrongNameKeyPair(File.ReadAllBytes(strongNameFile))
                                 : null;

                var writerParameters = new WriterParameters
                {
                    WriteSymbols = true,
                    StrongNameKeyPair = strongName,
                };

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

            Console.WriteLine("USAGE: FeatherSharp.exe <Feathers> <Options> <FileName>" + br
                            + "Feathers:" + br
                            + "    -npc : Inject NotifyPropertyChanged" + br
                            + "    -merge : Merge dependencies into <FileName>" + br
                            + "    -log : Inject augmented log method calls" + br
                            + "Options:" + br
                            + "    --sn=<StrongNameFile> : Re-sign with strong name");
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

    class Options
    {
        public string StrongNameFile { get; set; }
    }
}
