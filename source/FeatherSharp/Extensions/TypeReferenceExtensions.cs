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
    /// Defines extension methods for the <see cref="TypeReference"/> class.
    /// </summary>
    static class TypeReferenceExtensions
    {
        /// <summary>
        /// Determines whether a <see cref="TypeReference" /> refers to the
        /// same runtime type as the specified <see cref="Type" />.
        /// </summary>
        /// <typeparam name="T">The type to check for.</typeparam>
        /// <param name="typeRef">The type reference to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="typeRef" /> refers to the
        /// same runtime type as <typeparamref name="T"/>.
        /// </returns>
        public static bool Is<T>(this TypeReference typeRef)
        {
            Contract.Requires(typeRef != null);

            return typeRef.FullName == typeof(T).FullName;
        }

        /// <summary>
        /// Determines whether a <see cref="TypeReference" /> refers to the
        /// same runtime type as the specified <see cref="Type" />.
        /// </summary>
        /// <param name="typeRef">The type reference to check.</param>
        /// <param name="type">The type to check for.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="typeRef" /> refers to the
        /// same runtime type as <paramref name="type"/>.
        /// </returns>
        public static bool Is(this TypeReference typeRef, Type type)
        {
            Contract.Requires(typeRef != null);

            return typeRef.FullName == type.FullName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetUndecoratedFullName(this TypeReference type)
        {
            return type.Name.StartsWith("<") && type.IsNested
                   ? type.DeclaringType.FullName
                   : type.FullName;
        }
    }
}
