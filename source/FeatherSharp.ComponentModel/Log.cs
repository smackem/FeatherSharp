using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FeatherSharp.ComponentModel
{
    /// <summary>
    /// Provides logging facilities built for an aop tool that injects
    /// log calls passing the type and method names.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>To produce log messages, client code must the log methods with the signature
    /// <see cref="Log.Debug(string, object[])"/>. The post-processing tool will then
    /// change these calls to the override accepting a type name and method name.</para>
    /// <para>To actually consume the messages to log, client code must subscribe
    /// to the <see cref="Log.MessageRaised"/> event.</para>
    /// </remarks>
    public sealed class Log
    {
        static readonly Log Instance = new Log();

        /// <summary>
        /// Occurs when a message is to be logged.
        /// </summary>
        public static event EventHandler<LogEventArgs> MessageRaised
        {
            add { Instance.InternalRaiseMessage += value; }
            remove { Instance.InternalRaiseMessage -= value; }
        }

        /// <summary>
        /// Logs a message with <see cref="LogLevel.Trace"/> level.
        /// </summary>
        /// <param name="format">A format string as used by
        /// <see cref="String.Format(string, object[])"/>.</param>
        /// <param name="args">The arguments for the string format.</param>
        public static void Trace(string format, params object[] args)
        {
            InternalLog(format, args, null, null, LogLevel.Trace);
        }

        /// <summary>
        /// Logs a message with <see cref="LogLevel.Debug"/> level.
        /// </summary>
        /// <param name="format">A format string as used by
        /// <see cref="String.Format(string, object[])"/>.</param>
        /// <param name="args">The arguments for the string format.</param>
        public static void Debug(string format, params object[] args)
        {
            InternalLog(format, args, null, null, LogLevel.Debug);
        }

        /// <summary>
        /// Logs a message with <see cref="LogLevel.Info"/> level.
        /// </summary>
        /// <param name="format">A format string as used by
        /// <see cref="String.Format(string, object[])"/>.</param>
        /// <param name="args">The arguments for the string format.</param>
        public static void Info(string format, params object[] args)
        {
            InternalLog(format, args, null, null, LogLevel.Info);
        }

        /// <summary>
        /// Logs a message with <see cref="LogLevel.Warning"/> level.
        /// </summary>
        /// <param name="format">A format string as used by
        /// <see cref="String.Format(string, object[])"/>.</param>
        /// <param name="args">The arguments for the string format.</param>
        public static void Warn(string format, params object[] args)
        {
            InternalLog(format, args, null, null, LogLevel.Warning);
        }

        /// <summary>
        /// Logs a message with <see cref="LogLevel.Error"/> level.
        /// </summary>
        /// <param name="format">A format string as used by
        /// <see cref="String.Format(string, object[])"/>.</param>
        /// <param name="args">The arguments for the string format.</param>
        public static void Error(string format, params object[] args)
        {
            InternalLog(format, args, null, null, LogLevel.Error);
        }

        /// <summary>
        /// Supports the logging and feathering infrastructure. Do not call this method
        /// from client code.
        /// </summary>
        public static void InternalLog(string format, object[] args, string typeName, string methodName, LogLevel level)
        {
            Instance.OnRaiseMessage(new LogEventArgs(typeName, methodName,
                level, String.Format(CultureInfo.InvariantCulture, format, args)));
        }

        ///////////////////////////////////////////////////////////////////////

        // hidden ctor
        Log()
        {
        }

        event EventHandler<LogEventArgs> InternalRaiseMessage;

        void OnRaiseMessage(LogEventArgs e)
        {
            if (InternalRaiseMessage != null)
                InternalRaiseMessage(this, e);
        }
    }

    /// <summary>
    /// The arguments passed with the <see cref="Log.MessageRaised"/> event.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        readonly string typeName;
        readonly string methodName;
        readonly LogLevel level;
        readonly string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventArgs"/> class.
        /// </summary>
        /// <param name="typeName">Name of the call site type.</param>
        /// <param name="methodName">Name of the call site method.</param>
        /// <param name="level">The log level.</param>
        /// <param name="text">The message text.</param>
        public LogEventArgs(string typeName, string methodName, LogLevel level, string text)
        {
            Contract.Requires(text != null);

            this.typeName = typeName;
            this.methodName = methodName;
            this.level = level;
            this.text = text;
        }

        /// <summary>
        /// Gets the name of the call site type.
        /// </summary>
        public string TypeName
        {
            get { return this.typeName; }
        }

        /// <summary>
        /// Gets the name of the call site method.
        /// </summary>
        public string MethodName
        {
            get { return this.methodName; }
        }

        /// <summary>
        /// Gets the log level.
        /// </summary>
        public LogLevel Level
        {
            get { return this.level; }
        }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        public string Text
        {
            get { return this.text; }
        }
    }

    /// <summary>
    /// Defines the possible log levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// The lowest log level, used for trace messages.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// The log level used for debug messages.
        /// </summary>
        Debug   = 1,

        /// <summary>
        /// Used for signalling important messages not related to any faults.
        /// </summary>
        Info    = 10,

        /// <summary>
        /// Used to signal faults that do not impair correct program execution.
        /// </summary>
        Warning = 20,

        /// <summary>
        /// Used to signal faults that impair correct program execution.
        /// </summary>
        Error   = 30,
    }
}
