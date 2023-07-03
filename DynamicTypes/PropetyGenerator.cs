using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace DynamicTypes
{
    /// <summary>
    /// Generate an oberride for a virtual/abstract Property & calls base.
    /// </summary>
    public class virtualPropertyGenerator : PropertyGeneratorBase
    {
        public PropertyInfo TargetProperty { get; }

        public virtualPropertyGenerator(Type T, string name) : base(name, null)
        {
            OverrideDefinition = T;
            TargetProperty = T.GetProperty(name);

            Type = TargetProperty.PropertyType;

            Get = TargetProperty.GetMethod != null;
            Set = TargetProperty.SetMethod != null;
        }

        public override Action<ILGenerator> GenerateGetMethod
        {
            get => (ILGenerator il) =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, TargetProperty.GetMethod);
                il.Emit(OpCodes.Ret);
            }; set => base.GenerateGetMethod=value;
        }

        public override Action<ILGenerator> GenerateSetMethod
        {
            get => (ILGenerator il) =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, TargetProperty.SetMethod);
                il.Emit(OpCodes.Ret);
            }; set => base.GenerateSetMethod=value;
        }
    }

    /// <summary>
    /// Generate an oberride for a Interface Property
    /// </summary>
    public class iPropertyGenerator : PropertyGenerator
    {
        /// <summary>
        /// i dont like this approach, but it works, for now
        /// </summary>
        public bool UseSingleBackingField { get; set; }

        public PropertyInfo TargetProperty { get; }

        public iPropertyGenerator(Type T, string name) : base(name, null)
        {
            OverrideDefinition = T;
            TargetProperty = T.GetProperty(name);

            Type = TargetProperty.PropertyType;

            Get = TargetProperty.GetMethod != null;
            Set = TargetProperty.SetMethod != null;

            UseSingleBackingField = true;
        }

        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            BackingFieldName = UseSingleBackingField ? null : TargetProperty.DeclaringType.Name + "_" + TargetProperty.Name;

            base.DefineMember(tb, tg);
        }
    }
    public class iPropertyGenerator<T> : iPropertyGenerator
    {
        public iPropertyGenerator(string name) : base(typeof(T), name)
        {

        }
    }
    public class PropertyGenerator<T> : PropertyGenerator
    {
        public PropertyGenerator(string name) : base(name, typeof(T))
        {
        }
    }

    /// <summary>
    /// A Simple generator for Flat Properties eg. get; set;
    /// </summary>
    [DebuggerDisplay("PropertyGenerator {Name} {Type.FullName}")]
    public class PropertyGenerator : PropertyGeneratorBase
    {

        #region Properties

        /// <summary>
        /// Defines a Name for the BackingField
        /// </summary>
        public string? BackingFieldName { get; set; }
        /// <summary>
        /// The FieldGenerator of this class
        /// </summary>
        public FieldGenerator? BackingField { get; set; }

        /// <summary>
        /// The Field that is Generated (Only available after Compiling) 
        /// </summary>
        public FieldInfo? Field { get; set; }


        #endregion

        #region Constructors

        public PropertyGenerator()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyGenerator"/>
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <param name="type">Type of the Property</param>
        public PropertyGenerator(string name, Type? type) : base(name, type)
        {
        }

        #endregion

        #region Methods

        public string GetFieldName() => BackingFieldName ?? "m_" + Name;

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            if (BackingField != null)
                return;

            if (BackingFieldName != null
                && tg.Members.OfType<FieldGenerator>().FirstOrDefault(x => x.Name == BackingFieldName) is FieldGenerator fg)
            {
                BackingField = fg;
            }

            if (BackingField == null)
            {

                BackingField = new FieldGenerator(GetFieldName(), Type ?? typeof(object));
                BackingField.DefineMember(tb, tg);
                tg.Members.Add(BackingField);
            }

            base.DefineMember(tb, tg);
        }

        public override Action<ILGenerator> GenerateGetMethod
        {
            get => (ILGenerator il) =>
        {
            if (BackingField != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, BackingField.internalField);
                il.Emit(OpCodes.Ret);
            }
        }; set => base.GenerateGetMethod=value; }

        public override Action<ILGenerator> GenerateSetMethod
        {
            get => (ILGenerator il) =>
            {
            if (BackingField != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, BackingField.internalField);
                il.Emit(OpCodes.Ret);
            }
        }; set => base.GenerateSetMethod=value; }

        public override void Compiled(TypeGenerator cg)
        {
            base.Compiled(cg);
            Field = cg.Type.GetField(BackingField.Name);
        }
        #endregion

    }


    public class PropertyGeneratorBase : MemberGenerator
    {
        #region Properties

        /// <summary>
        /// The PropertyBuilder of this class
        /// </summary>
        public PropertyBuilder internalProperty { get; set; }

        /// <summary>
        /// The Property that is Generated (Only available after Compiling) 
        /// </summary>
        public PropertyInfo Property { get; set; }



        /// <summary>
        /// Defines if a getProperty will be defined
        /// </summary>
        public bool Get { get; set; } = true;
        /// <summary>
        /// Defines if a setProperty will be defined
        /// </summary>
        public bool Set { get; set; } = true;
        public MethodBuilder GetMethod { get; private set; }
        public MethodBuilder SetMethod { get; private set; }


        #endregion

        #region Constructors

        public PropertyGeneratorBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyGenerator"/>
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <param name="type">Type of the Property</param>
        public PropertyGeneratorBase(string name, Type? type) : base(type)
        {
            Name = name;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            internalProperty = tb.DefineProperty(Name, PropertyAttributes.HasDefault, Type, null);

            foreach (var item in Attributes)
            {
                internalProperty.SetCustomAttribute(item.AttributeBuilder);
            }

            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            if (OverrideDefinition != null)
            {
                getSetAttr = getSetAttr | MethodAttributes.Virtual;
            }
            if (Get)
            {
                GetMethod = tb.DefineMethod("get_" + Name, getSetAttr, Type, Type.EmptyTypes);
                var getIL = GetMethod.GetILGenerator();
                GenerateGetMethod(getIL);

                internalProperty.SetGetMethod(GetMethod);
                if (OverrideDefinition != null)
                {
                    foreach (var item in OverrideDefinitions)
                    {
                        tb.DefineMethodOverride(GetMethod, item.GetMethod("get_" + Name));
                    }
                }
            }
            if (Set)
            {
                SetMethod = tb.DefineMethod("set_" + Name, getSetAttr, null, new Type[] { Type });
                var setIL = SetMethod.GetILGenerator();
                GenerateSetMethod(setIL);

                internalProperty.SetSetMethod(SetMethod);
                if (OverrideDefinition != null)
                {
                    foreach (var item in OverrideDefinitions)
                    {
                        tb.DefineMethodOverride(SetMethod, item.GetMethod("set_" + Name));
                    }
                }
            }

        }

        public virtual Action<ILGenerator> GenerateGetMethod { get; set; }
        public virtual Action<ILGenerator> GenerateSetMethod { get; set; }

        public override void Compiled(TypeGenerator cg)
        {
            Property = cg.Type.GetProperty(Name);
            base.Compiled(cg);
        }
        #endregion
    }
}
