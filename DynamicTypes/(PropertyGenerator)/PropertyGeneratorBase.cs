using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
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
        /// Defines the ParamaterTypes of the Property, used for Indexes
        /// </summary>
        public Type[]? ParameterTypes { get; set; }


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
            ParameterTypes = ParameterTypes ?? Type.EmptyTypes;

            internalProperty = tb.DefineProperty(Name, PropertyAttributes.HasDefault, Type, ParameterTypes);

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
                GetMethod = tb.DefineMethod("get_" + Name, getSetAttr, Type, ParameterTypes);
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
                SetMethod = tb.DefineMethod("set_" + Name, getSetAttr, null, ParameterTypes.Concat(new Type[] { Type }).ToArray());
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
