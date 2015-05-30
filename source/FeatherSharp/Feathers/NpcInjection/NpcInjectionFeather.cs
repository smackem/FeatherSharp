using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatherSharp.Extensions;
using FeatherSharp.ComponentModel;

namespace FeatherSharp.Feathers.NpcInjection
{
    /// <summary>
    /// Implements a feather that injects invocations of the
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event into
    /// property setters.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>This feather processes all types that are decorated with a
    /// <see cref="FeatherAttribute"/> attribute with an action of
    /// <see cref="FeatherAction.NotifyPropertyChanged"/>.
    /// It looks for a method (in base types also) with the signature
    /// <code>void OnPropertyChanged(string)</code> and injects conditional invocations
    /// of this methods in the property setters that are only called when the
    /// new value is different from the current value of the backing field.</para>
    /// 
    /// <para>This is an opt-in feather on class level. Individual members of
    /// a class may opt-out using the <see cref="FeatherIgnoreAttribute"/>.</para>
    /// 
    /// <para>To get feedback about the feathered types and properties, a client
    /// must pass an implementation of the <see cref="INpcInjectionCallback"/> interface.</para>
    /// 
    /// <para>It also detects property dependencies in classes that derive from
    /// <see cref="NotifyPropertyChanged"/> and decorates dependent properties
    /// with the <see cref="DependsUponAttribute"/> attribute.</para>
    /// </remarks>
    public class NpcInjectionFeather : IFeather
    {
        readonly INpcInjectionCallback callback;


        /// <summary>
        /// Initializes a new instance of the <see cref="NpcInjectionFeather"/> class.
        /// </summary>
        /// <param name="callback">An implementation of <see cref="INpcInjectionCallback"/> that is
        /// used to notify the caller about feathered types and properties.</param>
        public NpcInjectionFeather(INpcInjectionCallback callback)
        {
            Contract.Requires(callback != null);

            this.callback = callback;
        }

        /// <summary>
        /// Executes the feather on the specified module.
        /// </summary>
        /// <param name="module">The <see cref="ModuleDefinition" /> to feather.</param>
        /// <param name="assemblyFilePath">The path to the assembly that contains <paramref name="module"/>.</param>
        public void Execute(ModuleDefinition module, string assemblyFilePath)
        {
            var runner = new NpcInjectionRunner(module, this.callback);

            var types = from type in module.EnumerateTypes().AsParallel()
                        where type.IsClass && HasNpcAttribute(type)
                        select type;

            foreach (var type in types)
                runner.ProcessType(type);
        }

        ///////////////////////////////////////////////////////////////////////

        static bool HasNpcAttribute(TypeDefinition type)
        {
            return (from attr in type.CustomAttributes
                    where attr.AttributeType.Is<FeatherAttribute>()
                       && Object.Equals(attr.ConstructorArguments.First().Value, (int)FeatherAction.NotifyPropertyChanged)
                    select attr)
                    .Any();
        }
    }
}
