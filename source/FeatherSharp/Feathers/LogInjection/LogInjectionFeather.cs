using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatherSharp.Extensions;
using FeatherSharp.ComponentModel;

namespace FeatherSharp.Feathers.LogInjection
{
    /// <summary>
    /// Implements a feather that augments logging calls with type and method names.
    /// </summary>
    /// 
    /// <remarks>
    /// It searches for invocations of one of the logging methods declared by
    /// <see cref="Log"/> (<see cref="Log.Debug"/>, <see cref="Log.Info"/>, <see cref="Log.Warn"/>
    /// and <see cref="Log.Error"/>). It then replaces these calls with invocations
    /// of an internal logging method that also takes type and method name of the
    /// site that invokes the log method.
    /// </remarks>
    public class LogInjectionFeather : IFeather
    {
        readonly ILogInjectionCallback callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInjectionFeather"/> class.
        /// </summary>
        /// <param name="callback">An implementation of <see cref="ILogInjectionCallback"/> that is
        /// used to notify the caller about feathered types and properties.</param>
        public LogInjectionFeather(ILogInjectionCallback callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// Executes the feather on the specified module.
        /// </summary>
        /// <param name="module">The <see cref="ModuleDefinition" /> to feather.</param>
        /// <param name="assemblyFilePath">The path to the assembly that contains <paramref name="module" />.</param>
        public void Execute(ModuleDefinition module, string assemblyFilePath)
        {
            var runner = new LogInjectionRunner(module, this.callback);

            var types = from type in module.EnumerateTypes().AsParallel()
                        where type.IsClass && type.HasFeatherAttribute(FeatherAction.Log)
                        select type;

            foreach (var type in types)
                runner.ProcessType(type);
        }
    }
}
