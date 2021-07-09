using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{

    /// <summary>
    /// Generates Methods that wrap the Source Method
    /// </summary>
    public class DetourMethodWrapper : DetourMethodGenerator
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DetourMethodWrapper"/>
        /// </summary>
        /// <param name="sourceObjcet">Generator that contains a local field that will be used to store the wrapped Property</param>
        /// <param name="source">the Method that will be wrapped</param>
        /// <param name="wrapper">the method that returns the wrapped instance 'public static T Wrap&lt;T&gt;(T source)'</param>
        public DetourMethodWrapper(FieldGenerator sourceObjcet, MethodInfo source, MethodInfo wrapper) : base(sourceObjcet, source)
        {
            Wrapper = wrapper;
        }

        /// <summary>
        /// Method that Wraps the instance
        /// </summary>
        public MethodInfo Wrapper { get; }

        #endregion

        #region Methods

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
            IL.Emit(OpCodes.Stloc_0);
            IL.Emit(OpCodes.Ldloc_0);
            IL.Emit(OpCodes.Call, Wrapper.MakeGenericMethod(Type));
            IL.Emit(OpCodes.Castclass, Type);
            IL.Emit(OpCodes.Stloc_0);
            IL.Emit(OpCodes.Ldloc_0);
            IL.Emit(OpCodes.Ret);

            if (OverrideDefinition != null)
            {
                tb.DefineMethodOverride(MethodBuilder, OverrideDefinition.GetMethod(Source.Name));
            }
        }

        #endregion

    }
}
