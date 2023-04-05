using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTypes
{
    public class ConstructorGenerator : MemberGenerator
    {
        #region Properties

        /// <summary>
        /// The builder of the Detouring Method
        /// </summary>
        protected ConstructorBuilder ConstructorBuilder { get; set; }
        /// <summary>
        /// The Constructor that is Generated (Only available after Compiling) 
        /// </summary>
        public ConstructorInfo Method { get; private set; }

        /// <summary>
        /// Method Parameters
        /// </summary>
        public PaarmeterDecriptor[] ParmeterDecriptors { get; set; } = new PaarmeterDecriptor[0];

        /// <summary>
        /// IL generator for the method
        /// </summary>
        public Action<ILGenerator> Generator { get; set; }

        /// <summary>
        /// Defines if the Constructor is the static Constructor
        /// </summary>
        public bool IsStatic { get; set; }  

        #endregion

        public ConstructorGenerator(bool isStatic = false)
        {
        }

        public ConstructorGenerator(params PaarmeterDecriptor[] parameter) : this(parameter, null)
        {
            
        }

        public ConstructorGenerator(PaarmeterDecriptor[] parameter, Action<ILGenerator> generator) : base(null)
        {

            ParmeterDecriptors = parameter ?? Array.Empty<PaarmeterDecriptor>();
            Generator = generator;
        }

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb)
        {
            var attributes = MethodAttributes.Public;
            if (IsStatic && ParmeterDecriptors.Any())
                throw new Exception("a static Constructor must be parameterless");

            ConstructorBuilder = tb.DefineConstructor(attributes, IsStatic ? CallingConventions.Standard : CallingConventions.HasThis, 
                ParmeterDecriptors.Select(x => x.ParameterType).ToArray());
            int index = 0;
            foreach (var item in ParmeterDecriptors)
            {
                var parameterBuilder = ConstructorBuilder.DefineParameter(index++, item.Attributes, item.Name);
                if (item.DefaultValue != PaarmeterDecriptor.NoDefaultValue)
                {
                    parameterBuilder.SetConstant(item.DefaultValue);
                }
            }

            var IL = ConstructorBuilder.GetILGenerator();

            Generator?.Invoke(IL);
            //return void
            if (Generator == null)
            {
                IL.Emit(OpCodes.Ret);
            }
        }
    }
}
