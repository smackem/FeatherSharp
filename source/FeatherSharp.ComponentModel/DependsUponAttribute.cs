using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeatherSharp.ComponentModel
{
    /// <summary>
    /// Marks a property as being dependent on the property with
    /// the specified name.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>When the value of dependency property changes, a
    /// <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event is raised both for the dependency property as well as the dependent
    /// property (which is the one decorated with this attribute).</para>
    /// 
    /// <para>The property listed as dependency may be defined in the same
    /// class as the decorated property or in a base class.</para>
    /// 
    /// <para>Apply multiple attributes of this type to add multiple dependencies.</para>
    /// 
    /// <para>The class that defines the decorated property must be derived from
    /// <see cref="NotifyPropertyChanged"/>.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property,
                    AllowMultiple = true,
                    Inherited = true)]
    public sealed class DependsUponAttribute : Attribute
    {
        readonly string propertyName;


        /// <summary>
        /// Initializes a new instance of the <see cref="DependsUponAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the dependency property.</param>
        public DependsUponAttribute(string propertyName)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Gets the name of the dependency property.
        /// </summary>
        public string PropertyName
        {
            get { return this.propertyName; }
        }
    }
}
