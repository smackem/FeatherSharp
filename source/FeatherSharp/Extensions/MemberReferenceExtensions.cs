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

            return member.DeclaringType.FullName + "::" + member.Name;
        }
    }
}
