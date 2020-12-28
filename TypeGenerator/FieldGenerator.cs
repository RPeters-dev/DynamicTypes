using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// A Simple Generator for a Field
    /// </summary>
    public class FieldGenerator : MemberGenerator
    {

        #region Properties

        /// <summary>
        /// The Builder of the Field
        /// </summary>
        internal FieldBuilder FieldBuilder { get; set; }
        /// <summary>
        /// Name of the Field
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// The Field that is Generated (Only available after Compiling) 
        /// </summary>
        public FieldInfo Field { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="FieldGenerator"/>
        /// </summary>
        /// <param name="name">Name of the Field</param>
        /// <param name="type">Type of the Field</param>
        public FieldGenerator(string name, Type type) : base(type)
        {
            FieldName = name;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Compiled(TypeGenerator cg)
        {
            Field = cg.Type.GetField(FieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            base.Compiled(cg);
        }
        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb)
        {
            FieldBuilder = tb.DefineField(FieldName, Type, FieldAttributes.Private);
        }

        #endregion

    }
}
