using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// A Simple generator for Flat Properties eg. get; set;
    /// </summary>
    public class PropertyGenerator : MemberGenerator
    {

        #region Properties

        /// <summary>
        /// Name of the Property
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// The FieldGenerator of this class
        /// </summary>
        protected internal FieldGenerator internalField { get; set; }

        /// <summary>
        /// The PropertyBuilder of this class
        /// </summary>
        protected internal PropertyBuilder internalProperty { get; set; }

        /// <summary>
        /// The Property that is Generated (Only available after Compiling) 
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// The Field that is Generated (Only available after Compiling) 
        /// </summary>
        public FieldInfo Field { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyGenerator"/>
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <param name="type">Type of the Property</param>
        public PropertyGenerator(string name, Type type) : base(type)
        {
            PropertyName = name;
            internalField = new FieldGenerator("m_" + name, type);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb)
        {
            internalField.DefineMember(tb);

            internalProperty = tb.DefineProperty(PropertyName, PropertyAttributes.HasDefault, Type, null);

            foreach (var item in Attributes)
            {
                internalProperty.SetCustomAttribute(item.AttributeBuilder);
            }

            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            if(OverrideDefinition != null)
            {
                getSetAttr = getSetAttr | MethodAttributes.Virtual;
            }
            {
                var mbGet = tb.DefineMethod("get_" + PropertyName, getSetAttr, Type, Type.EmptyTypes);
                var getIL = mbGet.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, internalField.internalField);
                getIL.Emit(OpCodes.Ret);
                internalProperty.SetGetMethod(mbGet);

                if (OverrideDefinition != null)
                {
                    tb.DefineMethodOverride(mbGet, OverrideDefinition.GetMethod("get_" + PropertyName));
                }
            }
            {
                var mbSet = tb.DefineMethod("set_" + PropertyName, getSetAttr, null, new Type[] { Type });
                var setIL = mbSet.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, internalField.internalField);
                setIL.Emit(OpCodes.Ret);
                internalProperty.SetSetMethod(mbSet);

                if (OverrideDefinition != null)
                {
                    tb.DefineMethodOverride(mbSet, OverrideDefinition.GetMethod("set_" + PropertyName));
                }
            }

        }


        public override void Compiled(TypeGenerator cg)
        {
            Property = cg.Type.GetProperty(PropertyName);
            Field = cg.Type.GetField(internalField.FieldName);
            base.Compiled(cg);
        }
        #endregion

    }
}
