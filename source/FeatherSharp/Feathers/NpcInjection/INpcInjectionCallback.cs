using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Feathers.NpcInjection
{
    /// <summary>
    /// Clients that use the <see cref="NpcInjectionFeather"/> must provide an implementation
    /// of this interface to receive notifications about the feathering process.
    /// </summary>
    /// 
    /// <remarks>
    /// The notification methods are not guaranteed to be invoked in any specific order.
    /// </remarks>
    public interface INpcInjectionCallback
    {
        /// <summary>
        /// Notifies the client that a property has been processed.
        /// </summary>
        /// <param name="qualifiedPropertyName">The name of the property, prefixed with
        /// the full name of the class that defines the property.</param>
        /// <param name="info">Flag that indicates the processing result.</param>
        /// <remarks>
        /// The format of a qualified member name is <code>Namespace.Type::Member</code>.
        /// </remarks>
        void NotifyProperty(string qualifiedPropertyName, NpcInjectionPropertyInfo info);

        /// <summary>
        /// Notifies the client that property dependencies have been discovered.
        /// </summary>
        /// <param name="qualifiedPropertyName">The name of the property, prefixed with
        /// the full name of the class that defines the property.</param>
        /// <param name="dependencies">The name of the properties that <paramref name="qualifiedPropertyName"/>
        /// depends upon.</param>
        /// <remarks>
        /// The format of a qualified member name is <code>Namespace.Type::Member</code>.
        /// </remarks>
        void NotifyPropertyDependencies(string qualifiedPropertyName, IEnumerable<string> dependencies);

        /// <summary>
        /// Notifies the client that a type is getting processed.
        /// </summary>
        /// <param name="qualifiedTypeName">Full name of processed type.</param>
        /// <param name="info">Flag that indicates the processing result.</param>
        void NotifyType(string qualifiedTypeName, NpcInjectionTypeInfo info);
    }

    /// <summary>
    /// Defines the property-specific information flags for the
    /// NotifyPropertyChanged injection.
    /// </summary>
    public enum NpcInjectionPropertyInfo
    {
        /// <summary>
        /// The <code>OnPropertyChanged</code> invocation was injected into the
        /// property setter.
        /// </summary>
        Ok,

        /// <summary>
        /// The property was ignored because it is decorated with a
        /// <see cref="FeatherSharp.ComponentModel.FeatherIgnoreAttribute"/>.
        /// </summary>
        Ignored,

        /// <summary>
        /// The property setter already calls the method to be injected and is
        /// therefore skipped.
        /// </summary>
        AlreadyCallsRaiseMethod,

        /// <summary>
        /// The <code>OnPropertyChanged</code> invocation was injected into the
        /// property setter, but was not made conditional with an equality check
        /// because the setter was too complex.
        /// </summary>
        NoEqualityCheckInjected,
    }

    /// <summary>
    /// Defines the type-specific information flags for the
    /// NotifyPropertyChanged injection.
    /// </summary>
    public enum NpcInjectionTypeInfo
    {
        /// <summary>
        /// Type was processed.
        /// </summary>
        Ok,

        /// <summary>
        /// Type was processed and property dependencies have been discovered.
        /// </summary>
        OkWithDependencies,

        /// <summary>
        /// Neither the type nor any of its base types implements the
        /// <see cref="System.ComponentModel.INotifyPropertyChanged"/> interface.
        /// </summary>
        InpcNotImplemented,

        /// <summary>
        /// The base type that implements <see cref="System.ComponentModel.INotifyPropertyChanged"/>
        /// does not define a method with the signature <code>void OnPropertyChanged(string)</code>.
        /// </summary>
        MissingRaiseMethod,

        /// <summary>
        /// The base type that implements <see cref="System.ComponentModel.INotifyPropertyChanged"/>
        /// defines a method with the signature <code>void OnPropertyChanged(string)</code>,
        /// but the method is not visible from the processed type.
        /// </summary>
        InaccessibleRaiseMethod,
    }
}
