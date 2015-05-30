using FeatherSharp.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using SR = System.Reflection;

namespace FeatherSharp.Feathers.Merge
{
    /// <summary>
    /// Implementation of the <see cref="MergeFeather"/>.
    /// Thanks to Sean Blakemore http://sblakemore.com/blog/
    /// </summary>
    class MergeRunner
    {
        static readonly string ResourcePrefix = "<Merged>";
        readonly ModuleDefinition module;
        readonly IMergeCallback callback;
        readonly string assemblyFilePath;

        /// <summary>
        /// Initializes a new instance of <see cref="MergeRunner"/>.
        /// </summary>
        /// <param name="module">The main module of the assembly to process.</param>
        /// <param name="callback">An implementation of <see cref="IMergeCallback"/> that is
        /// used to notify the caller about processed files.</param>
        /// <param name="assemblyFilePath">The path to the assembly that contains <paramref name="module"/>.</param>
        public MergeRunner(ModuleDefinition module, IMergeCallback callback, string assemblyFilePath)
        {
            Contract.Requires(module != null);
            Contract.Requires(callback != null);
            Contract.Requires(assemblyFilePath != null);

            this.module = module;
            this.callback = callback;
            this.assemblyFilePath = assemblyFilePath;
        }

        /// <summary>
        /// Executes the feather implementation.
        /// </summary>
        public void Execute()
        {
            foreach (var filePath in GetFilesToMerge())
            {
                AddFileAsResource(filePath);

                this.callback.NotifyFile(
                    Path.GetFileName(filePath), MergeFileInfo.Merged);
            }

            var assemblyResolve = EmitAssemblyResolveHandler();
            var ctor = EmitModuleCtor(assemblyResolve);

            var moduleType = module.Types.Single(t => t.Name == "<Module>");
            moduleType.Methods.Add(assemblyResolve);
            moduleType.Methods.Add(ctor);
        }

        ///////////////////////////////////////////////////////////////////////

        void AddFileAsResource(string filePath)
        {
            var resourceName = String.Format("{0}.{1}", ResourcePrefix, Path.GetFileName(filePath));
            var bytes = File.ReadAllBytes(filePath);
            var resource = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, bytes);

            this.module.Resources.Add(resource);
        }

        MethodDefinition EmitAssemblyResolveHandler()
        {
            var attrs = MethodAttributes.Private
                      | MethodAttributes.HideBySig
                      | MethodAttributes.Static;
            var method = new MethodDefinition("<AppDomain_AssemblyResolve>",
                attrs, this.module.ImportType<SR.Assembly>());

            new List<ParameterDefinition>
            {
                new ParameterDefinition(this.module.ImportType<object>()),
                new ParameterDefinition(this.module.ImportType<ResolveEventArgs>()),
            }.ForEach(method.Parameters.Add);

            new List<VariableDefinition>
            {
                new VariableDefinition(this.module.ImportType<SR.Assembly>()),
                new VariableDefinition(this.module.ImportType<string>()),
                new VariableDefinition(this.module.ImportType<Stream>()),
                new VariableDefinition(this.module.ImportType<byte[]>())
            }.ForEach(method.Body.Variables.Add);

            method.Body.InitLocals = true;

            var il = method.Body.GetILProcessor();

            il.Emit(OpCodes.Call, this.module.ImportMethod<SR.Assembly>("GetEntryAssembly"));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldstr, "{0}.{1}.dll");
            il.Emit(OpCodes.Ldstr, "<Merged>");
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, this.module.ImportMethod<ResolveEventArgs>("get_Name"));
            il.Emit(OpCodes.Newobj, this.module.ImportCtor<SR.AssemblyName>(typeof(string)));
            il.Emit(OpCodes.Call, this.module.ImportMethod<SR.AssemblyName>("get_Name"));
            il.Emit(OpCodes.Call, this.module.ImportMethod<String>("Format", typeof(string), typeof(object), typeof(object)));
            il.Emit(OpCodes.Stloc_1);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Callvirt, this.module.ImportMethod<SR.Assembly>("GetManifestResourceStream", typeof(string)));
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldloc_2);
            var targetInstr = Instruction.Create(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Brtrue_S, targetInstr);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);

            il.Append(targetInstr);
            il.Emit(OpCodes.Callvirt, this.module.ImportMethod<Stream>("get_Length"));
            il.Emit(OpCodes.Conv_Ovf_I);
            il.Emit(OpCodes.Newarr, this.module.TypeSystem.Byte);
            il.Emit(OpCodes.Stloc_3);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Callvirt, this.module.ImportMethod<Stream>("Read", typeof(byte[]), typeof(int), typeof(int)));
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Callvirt, this.module.ImportMethod<Stream>("Dispose"));
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Call, this.module.ImportMethod<SR.Assembly>("Load", typeof(byte[])));
            il.Emit(OpCodes.Ret);

            return method;
        }

        MethodDefinition EmitModuleCtor(MethodDefinition assemblyResolveHandler)
        {
            var attrs = MethodAttributes.Static
                      | MethodAttributes.SpecialName
                      | MethodAttributes.RTSpecialName;
            var ctor = new MethodDefinition(".cctor", attrs,
                this.module.Import(typeof(void)));

            var il = ctor.Body.GetILProcessor();
            il.Emit(OpCodes.Call, this.module.ImportMethod<AppDomain>("get_CurrentDomain"));
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldftn, assemblyResolveHandler);
            il.Emit(OpCodes.Newobj, this.module.ImportCtor<ResolveEventHandler>(typeof(object), typeof(IntPtr)));
            il.Emit(OpCodes.Callvirt,
                this.module.Import(typeof(AppDomain).GetEvent("AssemblyResolve").GetAddMethod()));
            il.Emit(OpCodes.Ret);

            return ctor;
        }

        IEnumerable<string> GetFilesToMerge()
        {
            var directory = Path.GetDirectoryName(this.assemblyFilePath);

            if (String.IsNullOrWhiteSpace(directory))
                directory = Directory.GetCurrentDirectory();

            return Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(f => f != this.assemblyFilePath);
        }
    }
}
