using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatherSharp.ComponentModel;
using FeatherSharp.Extensions;
using Mono.Cecil.Cil;
using System.Diagnostics.Contracts;

namespace FeatherSharp.Feathers.NpcInjection
{
    /// <summary>
    /// This class implements the core logic published by the <see cref="NpcInjectionFeather"/>.
    /// </summary>
    class NpcInjectionRunner
    {
        readonly ModuleDefinition module;
        readonly INpcInjectionCallback callback;


        /// <summary>
        /// Initializes a new instance of the <see cref="NpcInjectionRunner"/> class.
        /// </summary>
        /// <param name="module">The module to feather.</param>
        /// <param name="callback">The callback object to use for progress notifications.</param>
        public NpcInjectionRunner(ModuleDefinition module, INpcInjectionCallback callback)
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
        public INpcInjectionCallback Callback
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

            var raiseMethod = FindRaiseMethod(type);

            if (raiseMethod == null)
                return;

            var isDependencyAware = type.IsDerivedFrom<NotifyPropertyChanged>();

            this.callback.NotifyType(type.FullName,
                isDependencyAware
                ? NpcInjectionTypeInfo.OkWithDependencies
                : NpcInjectionTypeInfo.Ok);

            foreach (var property in type.Properties)
                ProcessProperty(property, raiseMethod, isDependencyAware);
        }


        ///////////////////////////////////////////////////////////////////////

        void ProcessProperty(PropertyDefinition property, MethodDefinition raiseMethod, bool isDependencyAware)
        {
            if (HasIgnoreNpcAttribute(property))
            {
                this.callback.NotifyProperty(property.GetQualifiedName(),
                    NpcInjectionPropertyInfo.Ignored);

                return;
            }

            if (property.SetMethod != null)
                InjectRaiseMethodCall(property, raiseMethod);

            if (property.GetMethod != null && isDependencyAware)
                InjectDependsUponAttributes(property);
        }

        void InjectDependsUponAttributes(PropertyDefinition property)
        {
            var getter = property.GetMethod;
            var body = getter.Body;
            var instructions = body.Instructions;

            // look for a call to a property getter on this instance ("this.Property"):
            // Ldarg_0
            // Call get_Property
            var calledProperties = (from instr in instructions.Skip(1)
                                    where instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt
                                    let methodRef = instr.Operand as MethodReference
                                    where methodRef != null && methodRef.Name.StartsWith("get_", StringComparison.Ordinal)
                                    let method = methodRef.Resolve()
                                    where method.IsGetter && method.IsStatic == false
                                    let prev = instr.Previous
                                    where prev.OpCode == OpCodes.Ldarg_0
                                       || prev.OpCode == OpCodes.Ldarg && Object.Equals(prev.Operand, 0)
                                    select method.Name.Substring(4))
                                    .ToList();

            if (calledProperties.Count > 0)
            {
                foreach (var calledProperty in calledProperties)
                {
                    var customAttr = new CustomAttribute(
                        this.module.ImportCtor<DependsUponAttribute>(typeof(string)));

                    customAttr.ConstructorArguments.Add(
                        new CustomAttributeArgument(this.module.TypeSystem.String, calledProperty));

                    property.CustomAttributes.Add(customAttr);
                }

                this.callback.NotifyPropertyDependencies(property.GetQualifiedName(), calledProperties);
            }
        }

        void InjectRaiseMethodCall(PropertyDefinition property, MethodDefinition raiseMethod)
        {
            var setter = property.SetMethod;
            var body = setter.Body;
            var firstInstr = body.Instructions.First();
            var retInstr = body.Instructions.Last();
            var il = body.GetILProcessor();

            var info = CheckSetter(body, raiseMethod);

            switch (info)
            {
                case NpcInjectionPropertyInfo.Ok:
                {
                    var fieldRef = (FieldReference)body.Instructions[2].Operand;
                    var propertyType = setter.Parameters[0].ParameterType;

                    if (propertyType.IsPrimitive)
                    {
                        EmitPrimitiveEqualityCheck(il, firstInstr, fieldRef, retInstr);
                    }
                    else if (propertyType.IsValueType)
                    {
                        EmitValueTypeEqualityCheck(il, firstInstr, fieldRef, retInstr, propertyType);
                    }
                    else
                    {
                        EmitReferenceTypeEqualityCheck(il, firstInstr, fieldRef, retInstr);
                    }

                    goto case NpcInjectionPropertyInfo.NoEqualityCheckInjected;
                }

                case NpcInjectionPropertyInfo.NoEqualityCheckInjected:
                    il.InsertBefore(retInstr, Instruction.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(retInstr, Instruction.Create(OpCodes.Ldstr, property.Name));
                    il.InsertBefore(retInstr, Instruction.Create(OpCodes.Callvirt, this.module.Import(raiseMethod)));
                    break;
            }

            this.callback.NotifyProperty(property.GetQualifiedName(), info);
        }

        void EmitPrimitiveEqualityCheck(ILProcessor il, Instruction position, FieldReference fieldRef, Instruction retInstr)
        {
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldfld, fieldRef));
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(position, Instruction.Create(OpCodes.Beq_S, retInstr));
        }

        void EmitValueTypeEqualityCheck(ILProcessor il, Instruction position, FieldReference fieldRef,
            Instruction retInstr, TypeReference propertyType)
        {
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldfld, fieldRef));
            il.InsertBefore(position, Instruction.Create(OpCodes.Box, propertyType));
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(position, Instruction.Create(OpCodes.Box, propertyType));
            il.InsertBefore(position, Instruction.Create(OpCodes.Call,
                this.module.ImportMethod<object>("Equals", typeof(object), typeof(object))));
            il.InsertBefore(position, Instruction.Create(OpCodes.Brtrue_S, retInstr));
        }

        void EmitReferenceTypeEqualityCheck(ILProcessor il, Instruction position, FieldReference fieldRef, Instruction retInstr)
        {
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldfld, fieldRef));
            il.InsertBefore(position, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(position, Instruction.Create(OpCodes.Call,
                this.module.ImportMethod<object>("Equals", typeof(object), typeof(object))));
            il.InsertBefore(position, Instruction.Create(OpCodes.Brtrue_S, retInstr));
        }

        NpcInjectionPropertyInfo CheckSetter(MethodBody setterBody, MethodDefinition raiseMethod)
        {
            var instructions = (from instr in setterBody.Instructions
                                where instr.OpCode != OpCodes.Nop
                                select instr)
                                .ToArray();

            // first two instructions are Ldarg_0 and Ldarg_1 or something equivalent
            if (instructions.Length == 4
            &&  instructions[2].OpCode == OpCodes.Stfld
            &&  instructions[3].OpCode == OpCodes.Ret)
                return NpcInjectionPropertyInfo.Ok;

            var callRaiseMethodInstrs = from instr in instructions
                                        where instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt
                                        where instr.Operand == raiseMethod
                                        select instr;

            if (callRaiseMethodInstrs.Any())
                return NpcInjectionPropertyInfo.AlreadyCallsRaiseMethod;

            return NpcInjectionPropertyInfo.NoEqualityCheckInjected;
        }

        MethodDefinition FindRaiseMethod(TypeDefinition type)
        {
            var inpcType = type.GetBaseThatImplements<INotifyPropertyChanged>();

            if (inpcType == null)
            {
                this.callback.NotifyType(type.FullName, NpcInjectionTypeInfo.InpcNotImplemented);
                return null;
            }

            return GetRaiseMethod(inpcType, type);
        }

        MethodDefinition GetRaiseMethod(TypeDefinition type, TypeDefinition referencingType)
        {
            var raiseMethod = (from method in type.Methods
                               where method.Name == "OnPropertyChanged"
                                  && method.ReturnType.Is(typeof(void))
                                  && method.Parameters.Count == 1
                                  && method.Parameters.First().ParameterType.Is<string>()
                               select method)
                               .FirstOrDefault();

            if (raiseMethod == null)
            {
                this.callback.NotifyType(referencingType.FullName, NpcInjectionTypeInfo.MissingRaiseMethod);
                return null;
            }

            // fail raiseMethod is not visible from referencingType
            if (raiseMethod.IsPrivate && referencingType != type
            ||  raiseMethod.IsAssembly && referencingType.Module.Assembly != type.Module.Assembly)
            {
                this.callback.NotifyType(referencingType.FullName, NpcInjectionTypeInfo.InaccessibleRaiseMethod);
                return null;
            }

            return raiseMethod;
        }

        static bool HasIgnoreNpcAttribute(PropertyDefinition property)
        {
            return (from attr in property.CustomAttributes
                    where attr.AttributeType.Is<FeatherIgnoreAttribute>()
                       && Object.Equals(attr.ConstructorArguments.First().Value, (int)FeatherAction.NotifyPropertyChanged)
                    select attr)
                    .Any();
        }
    }
}
