using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Feathers
{
    /// <summary>
    /// Defines the methods common to all feathers.
    /// </summary>
    [ContractClass(typeof(FeatherContract))]
    public interface IFeather
    {
        /// <summary>
        /// Executes the feather on the specified module.
        /// </summary>
        /// <param name="module">The <see cref="ModuleDefinition"/> to feather.</param>
        /// <param name="assemblyFilePath">The path to the assembly that contains <paramref name="module"/>.</param>
        void Execute(ModuleDefinition module, string assemblyFilePath);
    }

    [ContractClassFor(typeof(IFeather))]
    abstract class FeatherContract : IFeather
    {
        void IFeather.Execute(ModuleDefinition module, string assemblyFilePath)
        {
            Contract.Requires(module != null);
            Contract.Requires(assemblyFilePath != null);
        }
    }
}
