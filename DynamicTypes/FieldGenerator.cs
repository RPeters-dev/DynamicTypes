using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    public class FieldGenerator<T> : FieldGenerator
    {
        public FieldGenerator(string name) : base(name, typeof(T))
        {
        }
    }

    /// <summary>
    /// A Simple Generator for a Field
    /// </summary>
    public class FieldGenerator : MemberGenerator
    {

        #region Properties

        /// <summary>
        /// The Builder of the Field
        /// </summary>
        public FieldBuilder internalField { get; set; }
        /// <summary>
        /// The Field that is Generated (Only available after Compiling) 
        /// </summary>
        public FieldInfo Field { get; private set; }

        public FieldAttributes FieldAttributes { get; set; } = FieldAttributes.Private;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="FieldGenerator"/>
        /// </summary>
        /// <param name="name">Name of the Field</param>
        /// <param name="type">Type of the Field</param>
        public FieldGenerator(string name, Type type) : base(type)
        {
            Name = name;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Compiled(TypeGenerator cg)
        {
            Field = cg.Type.GetField(Name, BindingFlags.NonPublic | BindingFlags.Instance);

            base.Compiled(cg);
        }
        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            internalField = tb.DefineField(Name, Type, FieldAttributes);

            foreach (var item in Attributes)
            {
                internalField.SetCustomAttribute(item.AttributeBuilder);
            }
        }

        #endregion

    }
}
