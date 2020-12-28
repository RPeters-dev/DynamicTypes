using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// A Simple generator for Flat Properties eg. get; set;
    /// </summary>
    public class PropetyGenerator : MemberGenerator
    {

        #region Properties

        /// <summary>
        /// Name of the Property
        /// </summary>
        public string PeoprtyName { get; set; }
        /// <summary>
        /// The Field containing the value
        /// </summary>
        public FieldGenerator Field { get; set; }

        /// <summary>
        /// The Property that is Generated (Only available after Compiling) 
        /// </summary>
        public PropertyBuilder Property { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="PropetyGenerator"/>
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <param name="type">Type of the Property</param>
        public PropetyGenerator(string name, Type type) : base(type)
        {
            PeoprtyName = name;
            Field = new FieldGenerator("m_" + name, type);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb)
        {
            Field.DefineMember(tb);

            Property = tb.DefineProperty(PeoprtyName, PropertyAttributes.HasDefault, Type, null);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            {
                var mbGet = tb.DefineMethod("get_" + PeoprtyName, getSetAttr, Type, Type.EmptyTypes);
                var getIL = mbGet.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, Field.FieldBuilder);
                getIL.Emit(OpCodes.Ret);
                Property.SetGetMethod(mbGet);
            }
            {
                var mbSet = tb.DefineMethod("set_" + PeoprtyName, getSetAttr, null, new Type[] { Type });
                var setIL = mbSet.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, Field.FieldBuilder);
                setIL.Emit(OpCodes.Ret);
                Property.SetSetMethod(mbSet);
            }
        }

        #endregion

    }
}
