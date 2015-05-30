using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Feathers.Merge
{
    /// <summary>
    /// Implements a feather that merges the library assemblies (DLLs) into
    /// the assembly that references them, enabling a single-file deployment
    /// of the application.
    /// </summary>
    /// <remarks>
    /// The method used was outlined by Jeffrey Richter in his book
    /// "CLR via C#".
    /// </remarks>
    public class MergeFeather : IFeather
    {
        readonly IMergeCallback callback;

        /// <summary>
        /// Initializes a new instance of <see cref="MergeFeather"/>.
        /// </summary>
        /// <param name="callback">An implementation of <see cref="IMergeCallback"/> that is
        /// used to notify the caller about processed files.</param>
        public MergeFeather(IMergeCallback callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// Executes the feather on the specified module.
        /// </summary>
        /// <param name="module">The <see cref="ModuleDefinition" /> to feather.</param>
        /// <param name="assemblyFilePath">The path to the assembly that contains <paramref name="module"/>.</param>
        public void Execute(ModuleDefinition module, string assemblyFilePath)
        {
            new MergeRunner(module, this.callback, assemblyFilePath).Execute();
        }
    }
}
