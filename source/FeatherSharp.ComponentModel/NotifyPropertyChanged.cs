using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FeatherSharp.ComponentModel
{
    /// <summary>
    /// Provides a implementation of the <see cref="INotifyPropertyChanged"/> interface
    /// with support for property dependencies.
    /// </summary>
    /// <remarks>
    /// <para>When the getter of property <code>X</code> calls the getter of property
    /// <code>A</code>, on the same object, property <code>X</code> is said to be
    /// dependent upon property <code>A</code>.</para>
    /// <para>So when the value of property <code>A</code> changes, the
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised for
    /// both property <code>A</code> and for property <code>X</code>.</para>
    /// <para>Property dependencies are declared using the attribute
    /// <see cref="DependsUponAttribute"/>.</para>
    /// </remarks>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        readonly IDictionary<string, List<string>> propertyDependencies;


        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChanged"/> class.
        /// </summary>
        protected NotifyPropertyChanged()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var properties = GetType().GetProperties(bindingFlags);
            var dependencies = new Dictionary<string, List<string>>();

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(typeof(DependsUponAttribute), true)
                    .Cast<DependsUponAttribute>();

                foreach (var attribute in attributes)
                {
                    List<string> list;

                    if (dependencies.TryGetValue(attribute.PropertyName, out list) == false)
                    {
                        list = new List<string>();
                        dependencies.Add(attribute.PropertyName, list);
                    }

                    list.Add(property.Name);
                }
            }

            this.propertyDependencies = dependencies;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the property
        /// with the passed <paramref name="propertyName"/> as well as for
        /// all properties that depend on the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            RaisePropertyChanged(propertyName);

            List<string> dependentProperties;

            if (this.propertyDependencies.TryGetValue(propertyName, out dependentProperties))
            {
                foreach (var dependentProperty in dependentProperties)
                    OnPropertyChanged(dependentProperty);
            }
        }


        ///////////////////////////////////////////////////////////////////////

        void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
