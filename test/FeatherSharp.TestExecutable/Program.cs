using FeatherSharp.ComponentModel;
using FeatherSharp.TestLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.TestExecutable
{
    [Feather(FeatherAction.NotifyPropertyChanged)]
    class TestClass1 : NotifyPropertyChanged
    {
        public int MyPropertyA { get; set; }
        public string MyPropertyB { get; set; }

        public string Combined
        {
            get { return MyPropertyA.ToString() + MyPropertyB; }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AppDomain_AssemblyResolve);

            InnerMain();
        }

        static void InnerMain()
        {
            var test1 = new TestClass1();
            test1.PropertyChanged += HandlePropertyChanged;
            test1.MyPropertyA = 12;
            test1.MyPropertyB = "abc";

            var lib1 = new Class1();
            lib1.PropertyChanged += HandlePropertyChanged;
            lib1.MyProperty1 = 5;
            lib1.MyProperty2 = 6;
        }

        static void HandlePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine("{0}: {1} changed.", sender.GetType(), e.PropertyName);
        }

        static Assembly AppDomain_AssemblyResolve(object obj, ResolveEventArgs resolveEventArgs)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var name = String.Format("{0}.{1}.dll", "<Merged>", new AssemblyName(resolveEventArgs.Name).Name);
            Console.WriteLine("AppDomain_AssemblyResolve: {0}", name);
            var manifestResourceStream = entryAssembly.GetManifestResourceStream(name);

            if (manifestResourceStream == null)
                return null;

            using (manifestResourceStream)
            {
                var array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, array.Length);
                return Assembly.Load(array);
            }
        }
    }
}
