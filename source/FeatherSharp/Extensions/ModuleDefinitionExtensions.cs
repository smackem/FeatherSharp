using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Extensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="ModuleDefinition"/> class.
    /// </summary>
    static class ModuleDefinitionExtensions
    {
        /// <summary>
        /// Imports a type into a module.
        /// </summary>
        /// <typeparam name="T">The type to import.</typeparam>
        /// <param name="module">The <see cref="ModuleDefinition"/> to import into.</param>
        /// <returns>A <see cref="TypeReference"/> to the imported type.</returns>
        public static TypeReference ImportType<T>(this ModuleDefinition module)
        {
            Contract.Requires(module != null);

            return module.Import(typeof(T));
        }

        /// <summary>
        /// Imports a method into a module.
        /// </summary>
        /// <typeparam name="T">The type that defines the method to import.</typeparam>
        /// <param name="module">The <see cref="ModuleDefinition"/> to import into.</param>
        /// <param name="methodName">Name of the method to import.</param>
        /// <param name="parameterTypes">The parameter types of the method to import.</param>
        /// <returns>A <see cref="MethodReference"/> to the imported method.</returns>
        public static MethodReference ImportMethod<T>(this ModuleDefinition module, string methodName, params Type[] parameterTypes)
        {
            Contract.Requires(module != null);
            Contract.Requires(String.IsNullOrWhiteSpace(methodName) == false);

            return module.Import(typeof(T).GetMethod(methodName, parameterTypes));
        }

        /// <summary>
        /// Imports a constructor into a module.
        /// </summary>
        /// <typeparam name="T">The type that defines the constructor to import.</typeparam>
        /// <param name="module">The <see cref="ModuleDefinition"/> to import into.</param>
        /// <param name="parameterTypes">The parameter types of the constructor to import.</param>
        /// <returns>A <see cref="MethodReference"/> to the imported constructor.</returns>
        public static MethodReference ImportCtor<T>(this ModuleDefinition module, params Type[] parameterTypes)
        {
            Contract.Requires(module != null);

            return module.Import(typeof(T).GetConstructor(parameterTypes));
        }

        /// <summary>
        /// Lazily enumerates all types defined in a <see cref="ModuleDefinition"/>,
        /// including nested types.
        /// </summary>
        /// <param name="module">The module to process.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that yields all types
        /// defined in the <paramref name="module"/>.</returns>
        public static IEnumerable<TypeDefinition> EnumerateTypes(this ModuleDefinition module)
        {
            Contract.Requires(module != null);

            foreach (var type in module.Types)
            {
                yield return type;

                foreach (var nestedType in type.NestedTypes)
                    yield return nestedType;
            }
        }
    }
}
