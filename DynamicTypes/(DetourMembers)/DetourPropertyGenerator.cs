using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// A Simple Generator for Detouring properties 
    /// </summary>
    public class DetourPropertyGenerator : MemberGenerator
    {

        #region Properties

        /// <summary>
        /// a Field that contains the target of the Detour
        /// </summary>
        public FieldGenerator SourceObjcet { get; }
        /// <summary>
        /// The Property that will be Detoured
        /// </summary>
        protected PropertyInfo Source { get; }

        /// <summary>
        /// the new Property that calls the original Property
        /// </summary>
        protected PropertyBuilder PropertyBuilder { get; set; }

        /// <summary>
        /// Array of the Property's parameters (Only used for Indexes)
        /// </summary>
        public Type[] IndexParameter { get; }

        /// <summary>
        /// The Property that is Generated (Only available after Compiling)
        /// </summary>
        public PropertyInfo Property { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new Instance of <see cref="DetourPropertyGenerator"/>
        /// </summary>
        /// <param name="sourceOField">Field that contains the target of the Detour </param>
        /// <param name="source">Property that will be Detoured</param>
        public DetourPropertyGenerator(FieldGenerator sourceOField, PropertyInfo source) : base(source.PropertyType)
        {
            SourceObjcet = sourceOField;
            Source = source;
            IndexParameter = Source.GetIndexParameters().Select(x => x.ParameterType).ToArray();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Compiled(TypeGenerator cg)
        {
            Property = cg.Type.GetProperty(Source.Name);

            base.Compiled(cg);
        }
        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            PropertyBuilder = tb.DefineProperty(Source.Name, PropertyAttributes.HasDefault | PropertyAttributes.SpecialName, Source.PropertyType, IndexParameter);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var ccon = CallingConventions.Standard;
            if (OverrideDefinition != null)
            {
                getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
                ccon = CallingConventions.HasThis;
            }
            if (Source.GetMethod != null)
            {
                var sepParameter = Type.EmptyTypes;
                var mbGet = tb.DefineMethod("get_" + Source.Name, getSetAttr, ccon, Type, Source.GetGetMethod().GetParameters().Select(x => x.ParameterType).ToArray());

                var getIL = mbGet.GetILGenerator();

                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, SourceObjcet.internalField);
                for (int i = 0; i < IndexParameter.Length; i++)
                {
                    getIL.Emit(OpCodes.Ldarg_S, i + 1);
                }
                getIL.Emit(OpCodes.Callvirt, Source.GetGetMethod());
                getIL.Emit(OpCodes.Ret);
                PropertyBuilder.SetGetMethod(mbGet);

                if (OverrideDefinition != null)
                {
                    tb.DefineMethodOverride(mbGet, OverrideDefinition.GetMethod("get_" + Source.Name));
                }
            }
            if (Source.SetMethod != null)
            {
                var mbSet = tb.DefineMethod("set_" + Source.Name, getSetAttr, ccon, null, Source.GetSetMethod().GetParameters().Select(x => x.ParameterType).ToArray());

                var setIL = mbSet.GetILGenerator();

                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, SourceObjcet.internalField);
                for (int i = 0; i < IndexParameter.Length + 1; i++)
                {
                    setIL.Emit(OpCodes.Ldarg_S, i + 1);
                }
                setIL.Emit(OpCodes.Callvirt, Source.GetSetMethod());
                setIL.Emit(OpCodes.Nop);
                setIL.Emit(OpCodes.Ret);
                PropertyBuilder.SetSetMethod(mbSet);

                if (OverrideDefinition != null)
                {
                    tb.DefineMethodOverride(mbSet, OverrideDefinition.GetMethod("set_" + Source.Name));
                }
            }
        }

        #endregion

    }
}
