using FeatherSharp.ComponentModel;
using FeatherSharp.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SR = System.Reflection;

namespace FeatherSharp.Feathers.LogInjection
{
    /// <summary>
    /// This class implements the core logic published by the <see cref="LogInjectionFeather"/>.
    /// </summary>
    class LogInjectionRunner
    {
        static readonly SR.MethodInfo DebugMethod;
        static readonly SR.MethodInfo InfoMethod;
        static readonly SR.MethodInfo WarnMethod;
        static readonly SR.MethodInfo ErrorMethod;
        static readonly SR.MethodInfo InternalLogMethod;

        readonly ModuleDefinition module;
        readonly ILogInjectionCallback callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInjectionRunner"/> class.
        /// </summary>
        /// <param name="module">The module to feather.</param>
        /// <param name="callback">The callback object to use for progress notifications.</param>
        public LogInjectionRunner(ModuleDefinition module, ILogInjectionCallback callback)
        {
            Contract.Requires(module != null);
            Contract.Requires(callback != null);

            this.module = module;
            this.callback = callback;
        }

        /// <summary>
        /// Gets the module to feather.
        /// </summary>
        public ModuleDefinition Module
        {
            get { return this.module; }
        }

        /// <summary>
        /// Gets the callback object to use for progress notifications.
        /// </summary>
        public ILogInjectionCallback Callback
        {
            get { return this.callback; }
        }

        /// <summary>
        /// Processes the type.
        /// </summary>
        /// <param name="type">The type.</param>
        public void ProcessType(TypeDefinition type)
        {
            Contract.Requires(type != null);
            Contract.Requires(type.Module == Module);

            this.callback.NotifyType(type.FullName, LogInjectionTypeInfo.Ok);

            foreach (var method in type.Methods)
            {
                if (HasIgnoreLogAttribute(method))
                {
                    this.callback.NotifyMethod(method.GetQualifiedName(), LogInjectionMethodInfo.Ignored);
                }
                else
                {
                    var injectionCount = ProcessMethod(method);

                    if (injectionCount > 0)
                        this.callback.NotifyMethod(method.GetQualifiedName(), LogInjectionMethodInfo.Ok);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        static LogInjectionRunner()
        {
            var logParams = new[] { typeof(string), typeof(object[]) };

            DebugMethod = typeof(Log).GetMethod("Debug", logParams);
            InfoMethod = typeof(Log).GetMethod("Info", logParams);
            WarnMethod = typeof(Log).GetMethod("Warn", logParams);
            ErrorMethod = typeof(Log).GetMethod("Error", logParams);

            var internalLogParams = new[] { typeof(string), typeof(object[]),
                typeof(string), typeof(string), typeof(LogLevel) };

            InternalLogMethod = typeof(Log).GetMethod("InternalLog", internalLogParams);
        }

        int ProcessMethod(MethodDefinition method)
        {
            var body = method.Body;
            var il = body.GetILProcessor();
            var calls = (from instr in body.Instructions
                         where instr.OpCode == OpCodes.Call
                            || instr.OpCode == OpCodes.Callvirt
                         select instr)
                         .ToArray();

            var injectionCount = 0;

            foreach (var instr in calls)
            {
                var calledMethod = (MethodReference)instr.Operand;
                LogLevel level;

                if (IsMethod(calledMethod, DebugMethod))
                {
                    level = LogLevel.Debug;
                }
                else if (IsMethod(calledMethod, InfoMethod))
                {
                    level = LogLevel.Info;
                }
                else if (IsMethod(calledMethod, WarnMethod))
                {
                    level = LogLevel.Warning;
                }
                else if (IsMethod(calledMethod, ErrorMethod))
                {
                    level = LogLevel.Error;
                }
                else
                {
                    continue;
                }

                instr.Operand = module.Import(InternalLogMethod);
                il.InsertBefore(instr, Instruction.Create(OpCodes.Ldstr, method.DeclaringType.FullName));
                il.InsertBefore(instr, Instruction.Create(OpCodes.Ldstr, method.Name));
                il.InsertBefore(instr, Instruction.Create(OpCodes.Ldc_I4, (int)level));
                injectionCount++;
            }

            return injectionCount;
        }

        bool IsMethod(MethodReference method, SR.MethodInfo methodInfo)
        {
            return method.DeclaringType.Is(methodInfo.DeclaringType)
                && method.Name == methodInfo.Name
                && AreParametersEquivalent(method.Parameters, methodInfo.GetParameters());
        }

        bool AreParametersEquivalent(IList<ParameterDefinition> parameters, IList<SR.ParameterInfo> parameterInfos)
        {
            if (parameters.Count != parameterInfos.Count)
                return false;

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].ParameterType.Is(parameterInfos[i].ParameterType) == false)
                    return false;
            }

            return true;
        }

        static bool HasIgnoreLogAttribute(MethodDefinition method)
        {
            return (from attr in method.CustomAttributes
                    where attr.AttributeType.Is<FeatherIgnoreAttribute>()
                       && Object.Equals(attr.ConstructorArguments.First().Value, (int)FeatherAction.Log)
                    select attr)
                    .Any();
        }
    }
}
