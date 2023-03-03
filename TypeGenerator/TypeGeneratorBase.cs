using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace DynamicTypes
{
    /// <summary>
    /// Used to Compile Types
    /// </summary>
    public class TypeGenerator
    {

        #region Properties

        /// <summary>
        /// the Assembly generator
        /// </summary>
        public AssemblyGenerator AssemblyBuilder { get; set; } = new DefaultAssemblyGenerator();
        /// <summary>
        /// The Interface implementations of the Type
        /// </summary>
        public List<Type> InterfaceImplementations { get; } = new List<Type>();

        /// <summary>
        /// Members that will be Defined during the Compile call
        /// </summary>
        public List<MemberGenerator> Members { get; set; } = new List<MemberGenerator>();
        /// <summary>
        /// Attributes that will be attached to the Type
        /// </summary>
        public List<AttributeGenerator> Attributes { get; } = new List<AttributeGenerator>();
        /// <summary>
        /// Superclass of the new type
        /// </summary>
        public Type BaseType { get; }
        /// <summary>
        /// The Type, that will be generated from this Generator
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// The name of the Type that will be generated
        /// </summary>
        public string TypeName { get; }

        #endregion

        /// <summary>
        /// initializes a new instance of <see cref="TypeGeneratorBase"/>
        /// </summary>
        /// <param name="typeName">Name of the Type</param>
        /// <param name="baseType">Superclass of the Type</param>

        #region Constructors

        public TypeGenerator(string typeName = "MyDynamicType", Type baseType = null)
        {
            TypeName = typeName;
            BaseType = baseType;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compiles the Type
        /// </summary>
        public Type Compile()
        {
            AssemblyBuilder.Initialize();

            var tb = AssemblyBuilder.Module.DefineType(TypeName, TypeAttributes.Public, BaseType);

            foreach (var item in Attributes)
            {
                tb.SetCustomAttribute(item.AttributeBuilder);
            }

            foreach (var item in InterfaceImplementations)
            {
                tb.AddInterfaceImplementation(item);
            }

            foreach (var item in Members.Where(x => !x.Defined))
            {
                item.DefineMember(tb);
            }

            Type = tb.CreateType();

            foreach (var item in Members)
            {
                item.Compiled(this);
            }

            return Type;
        }

        #endregion

    }
}
