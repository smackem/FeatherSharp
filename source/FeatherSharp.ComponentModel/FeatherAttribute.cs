using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeatherSharp.ComponentModel
{
    /// <summary>
    /// Opts in a class for a feather action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,
                    AllowMultiple = false,
                    Inherited = false)]
    public sealed class FeatherAttribute : Attribute
    {
        readonly FeatherAction action;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatherAttribute"/> class.
        /// </summary>
        /// <param name="action">The action to execute on the decorated code element.</param>
        public FeatherAttribute(FeatherAction action)
        {
            this.action = action;
        }

        /// <summary>
        /// Gets the action to execute on the decorated code element.
        /// </summary>
        public FeatherAction Action
        {
            get { return this.action; }
        }
    }
}
