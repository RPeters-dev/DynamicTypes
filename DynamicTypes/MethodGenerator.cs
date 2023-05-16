using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    public class IMethodGenerator : MethodGenerator
    {
        public IMethodGenerator(Type T, string name) : this(T, T.GetMethod(name))
        {

        }

        public IMethodGenerator(MethodInfo target) : this(target.DeclaringType, target)
        {
        }

        public IMethodGenerator(Type T, MethodInfo target)
        {
            Name = target.Name;
            Type = target.ReturnType;
            ParmeterDecriptors = PaarmeterDecriptor.get(target.GetParameters());
            OverrideDefinition = T;
        }
    }

    public class IMethodGenerator<T> : IMethodGenerator
    {
        public IMethodGenerator( string name) : base(typeof(T), name)
        {
        }

        public IMethodGenerator(MethodInfo target) : base(typeof(T), target)
        {
        }
    }


    public class MethodGenerator : MemberGenerator
    {
        #region Properties

        /// <summary>
        /// The builder of the Detouring Method
        /// </summary>
        protected MethodBuilder MethodBuilder { get; set; }
        /// <summary>
        /// The Method that is Generated (Only available after Compiling) 
        /// </summary>
        public MethodInfo Method { get; private set; }
        /// <summary>
        /// Name of the Method
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Method Parameters
        /// </summary>
        public PaarmeterDecriptor[] ParmeterDecriptors { get; set; }

        /// <summary>
        /// IL generator for the method
        /// </summary>
        public Action<ILGenerator> Generator { get; set; }

        #endregion
        public MethodGenerator()
        {

        }
        public MethodGenerator(string name, Type returnType, PaarmeterDecriptor[] parameter = null, Action<ILGenerator> generator = null) : base(returnType)
        {
            Name = name;
            ParmeterDecriptors = parameter ?? Array.Empty<PaarmeterDecriptor>();
            Generator = generator;
        }

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            var attributes = MethodAttributes.Public;

            if (OverrideDefinition != null)
            {
                attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            }
            MethodBuilder = tb.DefineMethod(Name, attributes, Type, ParmeterDecriptors.Select(x => x.ParameterType).ToArray());
            int index = 0;
            foreach (var item in ParmeterDecriptors)
            {
                var parameterBuilder = MethodBuilder.DefineParameter(index++, item.Attributes, item.Name);
                if (item.DefaultValue != PaarmeterDecriptor.NoDefaultValue)
                {
                    parameterBuilder.SetConstant(item.DefaultValue);
                }
            }

            var IL = MethodBuilder.GetILGenerator();

            Generator?.Invoke(IL);
            //return void
            if (Generator == null)
            {
                IL.Emit(OpCodes.Ret);
            }

            if (OverrideDefinition != null)
            {
                MethodInfo mto = null;

                if (ParmeterDecriptors.Any())
                    OverrideDefinition.GetMethod(Name, ParmeterDecriptors.Select(x => x.ParameterType).ToArray());

                if (mto == null)
                    mto = OverrideDefinition.GetMethod(Name);

                tb.DefineMethodOverride(MethodBuilder, mto);
            }
        }
    }
}
