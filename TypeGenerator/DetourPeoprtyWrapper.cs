using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{

    /// <summary>
    /// Generates Properties that wrap and initialize the Propertie
    /// </summary>
    public class DetourPeoprtyWrapper : DetourPeoprtyGenerator
    {

        #region Properties

        /// <summary>
        /// Generator that contains a reference to the Parents Property that will be wrapped
        /// </summary>
        public FieldGenerator LocalObject { get; }
        /// <summary>
        /// Method that Wraps the instance
        /// </summary>
        public MethodInfo Wrapper { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DetourPeoprtyWrapper"/>
        /// </summary>
        /// <param name="localObject">Generator that contains a local field that will be used to store the wrapped Property</param>
        /// <param name="sourceObjcet">The source object that contains the Property that will be wrapped </param>
        /// <param name="source">the Property that will be wrapped</param>
        /// <param name="wrapper">the method that returns the wrapped instance 'public static T Wrap&lt;T&gt;(T source)'</param>
        public DetourPeoprtyWrapper(FieldGenerator localObject, FieldGenerator sourceObjcet, PropertyInfo source, MethodInfo wrapper) : base(sourceObjcet, source)
        {
            LocalObject = localObject;
            Wrapper = wrapper;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb)
        {
            PropertyBuilder = tb.DefineProperty(Source.Name, PropertyAttributes.HasDefault | PropertyAttributes.SpecialName, Type, IndexParameter);
            var getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
            var ccon = CallingConventions.HasThis;

            if (Source.GetMethod != null)
            {
                var sepParameter = Type.EmptyTypes;
                var mbGet = tb.DefineMethod("get_" + Source.Name, getSetAttr, ccon, Type, Source.GetGetMethod().GetParameters().Select(x => x.ParameterType).ToArray());

                var getIL = mbGet.GetILGenerator();

                getIL.DeclareLocal(typeof(bool));
                getIL.DeclareLocal(Type);
                var sJmp_fieldDefined = getIL.DefineLabel();
                var sJmp_end = getIL.DefineLabel();

                //if(LocalObject != null)
                //{
                getIL.Emit(OpCodes.Nop);
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, LocalObject.internalField);
                getIL.Emit(OpCodes.Ldnull);
                getIL.Emit(OpCodes.Ceq);
                getIL.Emit(OpCodes.Stloc_0);
                getIL.Emit(OpCodes.Ldloc_0);
                getIL.Emit(OpCodes.Brfalse_S, sJmp_fieldDefined);
                //}
                //Wrap and initialize LocalObject
                getIL.Emit(OpCodes.Nop);
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, SourceObjcet.internalField);
                getIL.Emit(OpCodes.Callvirt, OverrideDefinition.GetMethod(mbGet.Name));
                getIL.Emit(OpCodes.Call, Wrapper.MakeGenericMethod(Type));
                getIL.Emit(OpCodes.Castclass, Type);
                getIL.Emit(OpCodes.Stfld, LocalObject.internalField);

                //return LocalObject
                getIL.MarkLabel(sJmp_fieldDefined);
                getIL.Emit(OpCodes.Nop);
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, LocalObject.internalField);
                getIL.Emit(OpCodes.Stloc_1);
                getIL.Emit(OpCodes.Br_S, sJmp_end);
                getIL.MarkLabel(sJmp_end);
                getIL.Emit(OpCodes.Ldloc_1);

                getIL.Emit(OpCodes.Ret);

                PropertyBuilder.SetGetMethod(mbGet);

                if (OverrideDefinition != null)
                {
                    tb.DefineMethodOverride(mbGet, OverrideDefinition.GetMethod("get_" + Source.Name));
                }
            }

            Defined = true;
        }

        #endregion

    }
}
