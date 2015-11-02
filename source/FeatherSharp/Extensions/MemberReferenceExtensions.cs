using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeatherSharp.Extensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="MemberReference"/> class.
    /// </summary>
    static class MemberReferenceExtensions
    {
        /// <summary>
        /// Gets the qualified name of the member.
        /// </summary>
        /// <param name="member">The member to examine.</param>
        /// <returns>The qualified name of the member.</returns>
        /// <remarks>
        /// This name has the following format: <code>Namespace.Type::Member</code>.
        /// </remarks>
        public static string GetQualifiedName(this MemberReference member)
        {
            Contract.Requires(member != null);

            return member.DeclaringType.GetUndecoratedFullName() + "::" + member.GetUndecoratedName();
        }

        /// <summary>
        /// Gets the name of the member, removing decorations added for compiler-generated
        /// methods.
        /// </summary>
        /// <param name="member">The member to examine.</param>
        /// <returns>The undecorated name of the member.</returns>
        /// <remarks>
        /// Compiler-generated methods (e.g. in the context of async methods) often have decorations
        /// like &lt; or &gt;, or even reside in nested types with decorated names.
        /// This method strips these decorations and returns the original method name as written in the
        /// source file.
        /// </remarks>
        public static string GetUndecoratedName(this MemberReference member)
        {
            Contract.Requires(member != null);

            var declaringType = member.DeclaringType;

            if (member.Name.StartsWith("<"))
            {
                var index = member.Name.LastIndexOf('>');

                if (index < 0)
                    index = member.Name.Length;

                return member.Name.Substring(1, index - 1);
            }
            else if (declaringType.Name.StartsWith("<") && declaringType.IsNested)
            {
                var index = declaringType.Name.LastIndexOf('>');

                if (index < 0)
                    index = declaringType.Name.Length;

                return declaringType.Name.Substring(1, index - 1);
            }
            else
            {
                return member.Name;
            }
        }
    }
}
