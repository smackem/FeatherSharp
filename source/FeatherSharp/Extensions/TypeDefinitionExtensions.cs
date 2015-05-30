using FeatherSharp.ComponentModel;
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
    /// Defines extension methods for the <see cref="TypeDefinition"/> class.
    /// </summary>
    static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Determines whether a type is derived from a specified base type.
        /// </summary>
        /// <typeparam name="T">The type that is a potential base type of <paramref name="type"/></typeparam>
        /// <param name="type">The type that is potentially derived from <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is derived from <typeparamref name="T"/>,
        /// otherwise <c>false</c>.</returns>
        public static bool IsDerivedFrom<T>(this TypeDefinition type)
        {
            Contract.Requires(type != null);
            Contract.Requires(type.IsClass);
            Contract.Requires(typeof(T).IsClass);

            return FindMatchingBase(type, t => t.Is<T>()) != null;
        }

        /// <summary>
        /// Searches a type's base types for one that implements a specific interface
        /// and returns the found base type.
        /// </summary>
        /// <typeparam name="T">The interface type to look for.</typeparam>
        /// <param name="type">The type to start searching from.</param>
        /// <returns>A type that implements <typeparamref name="T"/> or <c>null</c>
        /// if none found.</returns>
        public static TypeDefinition GetBaseThatImplements<T>(this TypeDefinition type)
        {
            Contract.Requires(type != null);
            Contract.Requires(type.IsClass);
            Contract.Requires(typeof(T).IsInterface);

            return FindMatchingBase(type, t =>
                t.Interfaces.Any(it => it.Is<T>()));
        }

        /// <summary>
        /// Tests whether a type is decorated with a <see cref="FeatherAttribute"/> for
        /// the given <see cref="FeatherAction"/>.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <param name="action">The action to test for.</param>
        /// <returns><code>true</code> if the passed type is decorated with
        /// a feather attribute for the passed action.</returns>
        public static bool HasFeatherAttribute(this TypeDefinition type, FeatherAction action)
        {
            return (from attr in type.CustomAttributes
                    where attr.AttributeType.Is<FeatherAttribute>()
                       && Object.Equals(attr.ConstructorArguments.First().Value, (int)action)
                    select attr)
                    .Any();
        }

        ///////////////////////////////////////////////////////////////////////

        static TypeDefinition FindMatchingBase(TypeDefinition type, Func<TypeDefinition, bool> predicate)
        {
            for (TypeReference typeRef = type; typeRef != null; )
            {
                type = typeRef as TypeDefinition ?? typeRef.Resolve();

                if (predicate(type))
                    return type;

                typeRef = type.BaseType;
            }

            return null;
        }
    }
}
