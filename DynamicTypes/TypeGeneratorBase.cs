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
        public Type? BaseType { get; }
        /// <summary>
        /// The Type, that will be generated from this Generator
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// The name of the Type that will be generated
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Enshures that a unique Type name is used, makes it easyer to debug
        /// </summary>
        public bool EnshureUniqueName { get; set; } = true;

        public Dictionary<string, int> TypeNames = new Dictionary<string, int>();

        #endregion

        /// <summary>
        /// initializes a new instance of <see cref="TypeGeneratorBase"/>
        /// </summary>
        /// <param name="typeName">Name of the Type</param>
        /// <param name="baseType">Superclass of the Type</param>

        #region Constructors

        public TypeGenerator(string typeName = "<>DynamicType", Type? baseType = null)
        {
            TypeName = typeName;
            BaseType = baseType;
        }

        #endregion

        #region Methods

        public string GetTypeName()
        {
            var tnk = string.Format(TypeName, null, Members.Count);

            if (!TypeNames.ContainsKey(tnk))
                TypeNames[tnk] = 0;
            else
                TypeNames[tnk]++;

            return !EnshureUniqueName ? TypeName : string.Format(TypeName + "_{0}`{1}", TypeNames[tnk], Members.Count);
        }

        /// <summary>
        /// Compiles the Type
        /// </summary>
        public Type Compile()
        {
            AssemblyBuilder.Initialize();

            QualityOfLive();

            var tb = AssemblyBuilder.Module.DefineType(GetTypeName(), TypeAttributes.Public, BaseType);

            foreach (var item in Attributes)
            {
                tb.SetCustomAttribute(item.AttributeBuilder);
            }

            var toDefine = Members.Where(x => !x.Defined).ToArray();
            foreach (var item in toDefine)
            {
                item.DefineMember(tb, this);
            }

            foreach (var item in InterfaceImplementations)
            {
                tb.AddInterfaceImplementation(item);
            }
            Type = tb.CreateType();

            foreach (var item in Members)
            {
                item.Compiled(this);
            }

            return Type;
        }

        private void QualityOfLive()
        {
            InterfaceImplementations.AddRange(
                Members.SelectMany(x => x.OverrideDefinitions).Where(x => x != null && x.IsInterface).Distinct().Except(InterfaceImplementations).ToArray());
        }

        #endregion

    }
}
