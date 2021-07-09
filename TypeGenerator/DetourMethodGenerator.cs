using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// A Simple Generator for Detouring members 
    /// </summary>
    public class DetourMethodGenerator : MemberGenerator
    {

        #region Properties

        /// <summary>
        /// a Field that contains the target of the Detour
        /// </summary>
        protected FieldGenerator SourceObjcet { get; }
        /// <summary>
        /// The MEthod that will be Detoured
        /// </summary>
        protected MethodInfo Source { get; }
        /// <summary>
        /// The builder of the Detouring Method
        /// </summary>
        protected MethodBuilder MethodBuilder { get; set; }
        /// <summary>
        /// The Method that is Generated (Only available after Compiling) 
        /// </summary>
        public MethodInfo Method { get; private set; }
   

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DetourMethodGenerator"/>
        /// </summary>
        /// <param name="sourceObjcet">Field containing theTarget object of the Detour</param>
        /// <param name="source">the Method that will be Detoured</param>
        public DetourMethodGenerator(FieldGenerator sourceObjcet, MethodInfo source) : base(source.ReturnType)
        {
            SourceObjcet = sourceObjcet;
            Source = source;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb)
        {
            var attributes = MethodAttributes.Public;

            if (OverrideDefinition != null)
            {
                attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            }
            MethodBuilder = tb.DefineMethod(Source.Name, attributes, Type, Source.GetParameters().Select(x => x.ParameterType).ToArray());
            var IL = MethodBuilder.GetILGenerator();
            var local = IL.DeclareLocal(typeof(object));

            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldfld, SourceObjcet.internalField);
            int i = 1;
            foreach (var item in Source.GetParameters())
            {
                IL.Emit(OpCodes.Ldarg_S, i++);
            }

            IL.Emit(OpCodes.Callvirt, Source);
            if (Source.ReturnType.FullName != "System.Void")
            {
                IL.Emit(OpCodes.Stloc_0);
                IL.Emit(OpCodes.Ldloc_0);
            }

            IL.Emit(OpCodes.Ret);

            if (OverrideDefinition != null)
            {
                tb.DefineMethodOverride(MethodBuilder, OverrideDefinition.GetMethod(Source.Name));
            }
        }

        #endregion

    }
}
