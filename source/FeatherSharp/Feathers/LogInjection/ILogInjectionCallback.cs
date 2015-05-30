using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatherSharp.Feathers.LogInjection
{
    /// <summary>
    /// Clients that use the <see cref="LogInjectionFeather"/> must provide an implementation
    /// of this interface to receive notifications about the feathering process.
    /// </summary>
    /// 
    /// <remarks>
    /// The notification methods are not guaranteed to be invoked in any specific order.
    /// </remarks>
    public interface ILogInjectionCallback
    {
        /// <summary>
        /// Notifies the client that a type is getting processed.
        /// </summary>
        /// <param name="qualifiedTypeName">Full name of processed type.</param>
        /// <param name="info">Flag that indicates the processing result.</param>
        void NotifyType(string qualifiedTypeName, LogInjectionTypeInfo info);

        /// <summary>
        /// Notifies the client that a method has been processed.
        /// </summary>
        /// <param name="qualifiedMethodName">The name of the method, prefixed with
        /// the full name of the class that defines the method.</param>
        /// <param name="info">Flag that indicates the processing result.</param>
        /// <remarks>
        /// The format of a qualified member name is <code>Namespace.Type::Member</code>.
        /// </remarks>
        void NotifyMethod(string qualifiedMethodName, LogInjectionMethodInfo info);
    }

    /// <summary>
    /// Defines the type-specific information flags for Log injection.
    /// </summary>
    public enum LogInjectionTypeInfo
    {
        /// <summary>
        /// Signals that the type has been processed.
        /// </summary>
        Ok,
    }

    /// <summary>
    /// Defines the method-specific information flags for Log injection.
    /// </summary>
    public enum LogInjectionMethodInfo
    {
        /// <summary>
        /// Signals that the method has been processed and log method calls
        /// have been injected.
        /// </summary>
        Ok,

        /// <summary>
        /// Signals that the method has been ignored.
        /// </summary>
        Ignored,
    }
}
