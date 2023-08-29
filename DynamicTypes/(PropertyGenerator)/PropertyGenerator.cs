using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace DynamicTypes
{

    /// <summary>
    /// A Simple generator for Flat Properties eg. get; set;
    /// </summary>
    [DebuggerDisplay("PropertyGenerator {Name} {Type.FullName}")]
    public class PropertyGenerator : PropertyGeneratorBase
    {

        #region Properties

        /// <summary>
        /// Defines a Name for the BackingField
        /// </summary>
        public string? BackingFieldName { get; set; }
        /// <summary>
        /// The FieldGenerator of this class
        /// </summary>
        public FieldGenerator? BackingField { get; set; }

        /// <summary>
        /// The Field that is Generated (Only available after Compiling) 
        /// </summary>
        public FieldInfo? Field { get; set; }

        public bool GegenerateField { get; set; } = true;


        #endregion

        #region Constructors

        public PropertyGenerator()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyGenerator"/>
        /// </summary>
        /// <param name="name">Name of the Property</param>
        /// <param name="type">Type of the Property</param>
        public PropertyGenerator(string name, Type? type) : base(name, type)
        {
        }

        #endregion

        #region Methods

        public string GetFieldName() => BackingFieldName ?? "m_" + Name;

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            if (GegenerateField && BackingField == null)
            {
                if (BackingFieldName != null
                    && tg.Members.OfType<FieldGenerator>().FirstOrDefault(x => x.Name == BackingFieldName) is FieldGenerator fg)
                {
                    BackingField = fg;
                }

                if (BackingField == null)
                {

                    BackingField = new FieldGenerator(GetFieldName(), Type ?? typeof(object));
                    BackingField.DefineMember(tb, tg);
                    tg.Members.Add(BackingField);
                }
            }

            base.DefineMember(tb, tg);
        }

        public override Action<ILGenerator> GenerateGetMethod
        {
            get => base.GenerateGetMethod ?? DefaultGetMethod;
            set => base.GenerateGetMethod=value;
        }

        public Action<ILGenerator> DefaultGetMethod => (ILGenerator il) =>
        {
            if (BackingField != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, BackingField.internalField);
                il.Emit(OpCodes.Ret);
            }
        };

        public override Action<ILGenerator> GenerateSetMethod
        {
            get => base.GenerateSetMethod ?? DefaultSetMethod;
            set => base.GenerateSetMethod=value;
        }


        public Action<ILGenerator> DefaultSetMethod => (ILGenerator il) =>
        {
            if (BackingField != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, BackingField.internalField);
                il.Emit(OpCodes.Ret);
            }
        };


        public override void Compiled(TypeGenerator cg)
        {
            base.Compiled(cg);
            if (BackingField != null)
                Field = cg.Type.GetField(BackingField.Name);
        }
        #endregion

    }
}
