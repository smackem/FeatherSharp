using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeatherSharp.ComponentModel
{
    /// <summary>
    /// Opts out a code element from a feather action.
    /// </summary>
    /// <remarks>
    /// Feathering is an opt-in operation on class level. Members of
    /// a class that opts in can opt out using this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.All,
                    AllowMultiple = false,
                    Inherited = false)]
    public sealed class FeatherIgnoreAttribute : Attribute
    {
        readonly FeatherAction action;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatherIgnoreAttribute"/> class.
        /// </summary>
        /// <param name="action">The action to skip for the decorated code element.</param>
        public FeatherIgnoreAttribute(FeatherAction action)
        {
            this.action = action;
        }

        /// <summary>
        /// Gets the action to skip for the decorated code element.
        /// </summary>
        public FeatherAction Action
        {
            get { return this.action; }
        }
    }
}
