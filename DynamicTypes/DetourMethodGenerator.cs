using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml;

namespace DynamicTypes
{

    public class DetourMethodGenerator : MethodGenerator
    {
        /// <summary>
        /// a Field that contains the target of the Detour
        /// </summary>
        protected FieldGenerator SourceObjcet { get; }
        /// <summary>
        /// The MEthod that will be Detoured
        /// </summary>
        protected MethodInfo Source { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="DetourMethodGenerator"/>
        /// </summary>
        /// <param name="sourceObjcet">Field containing theTarget object of the Detour</param>
        /// <param name="source">the Method that will be Detoured</param>
        public DetourMethodGenerator(FieldGenerator sourceObjcet, MethodInfo source) : base(source.Name, source.ReturnType,
            PaarmeterDecriptor.get(source.GetParameters().Select(x => x.ParameterType)))
        {
            SourceObjcet = sourceObjcet;
            Source = source;
            Generator = Generate;
        }

        private void Generate(ILGenerator IL)
        {
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldfld, SourceObjcet.internalField);
            int i = 1;
            foreach (var item in Source.GetParameters())
            {
                IL.Emit(OpCodes.Ldarg_S, i++);
            }

            IL.Emit(OpCodes.Callvirt, Source);

            IL.Emit(OpCodes.Ret);
        }
    }
}
