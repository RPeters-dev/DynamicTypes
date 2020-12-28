using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace DynamicTypes
{
    /// <summary>
    /// Configures a new Type that Wraps a COM object and adds Finalize and Dispose 
    /// </summary>
    public class COMWrapper : MemberGenerator
    {

        #region Fields

        /// <summary>
        /// Cache List that contains already defined wrapper Types
        /// </summary>
        static Dictionary<Type, COMWrapper> Cache = new Dictionary<Type, COMWrapper>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="COMWrapper"/>
        /// </summary>
        /// <param name="detourType">Type that will be wrapped</param>
        /// <param name="tg">the generator used to generate the Type</param>
        public COMWrapper(Type detourType, string name) : base(detourType)
        {
            TypeGenerator = new TypeGenerator(name);
            TypeGenerator.InterfaceImplementations.Add(detourType);
            TypeGenerator.InterfaceImplementations.Add(typeof(IDisposable));
            TypeGenerator.Members.Add(this);
        }
        /// <summary>
        /// The Type Generator used to Create the COM Wrapper
        /// </summary>
        public TypeGenerator TypeGenerator { get; }
        /// <summary>
        /// Dispose implementation
        /// </summary>
        IDisposable_FinalReleaseComObject Dispose { get; set; }
        /// <summary>
        /// Finalize implementation
        /// </summary>
        FinalizeDispose Dispose_Finalize { get; set; }
        /// <summary>
        /// List of all Methods
        /// </summary>
        List<DetourMethodGenerator> Methods { get; set; } = new List<DetourMethodGenerator>();
        /// <summary>
        /// List of all Properties
        /// </summary>
        List<DetourPeoprtyGenerator> Properties { get; set; } = new List<DetourPeoprtyGenerator>();

        #endregion

        #region Properties

        /// <summary>
        /// Generator of the Object that is Wrapped
        /// </summary>
        FieldGenerator Sourcefield { get; set; }
        /// <summary>
        /// Iterates through all interfaces of the <paramref name="type"/>
        /// </summary>
        /// <param name="type">source type</param>
        /// <returns>all interfaces</returns>
        public static IEnumerable<Type> IterateInterfaces(Type type)
        {
            yield return type;
            foreach (var item in type.GetInterfaces())
            {
                yield return item;
            }
        }
        /// <summary>
        /// Wraps a ComClass
        /// </summary>
        /// <typeparam name="T">The Type of the COM Interface </typeparam>
        /// <param name="source">the Source COM object</param>
        /// <returns>a Object thats wraps the <paramref name="source"/> <returns>
        public static T Wrap<T>(T source)
        {
            COMWrapper wrapper = null;
            if (!Cache.TryGetValue(typeof(T), out wrapper))
            {
                wrapper = new COMWrapper(typeof(T), $"Managed_{typeof(T).Name}");
                Cache.Add(typeof(T), wrapper);
                wrapper.Compile();
            }

            var instance = Activator.CreateInstance(wrapper.TypeGenerator.Type);
            wrapper.InitializeInstance(instance, source);
            return (T)instance;
        }

        /// <inheritdoc/>
        public override void Compiled(TypeGenerator cg)
        {
            Sourcefield.Compiled(cg);

            foreach (var item in Properties)
            {
                item.Compiled(cg);
            }
            foreach (var item in Methods)
            {
                item.Compiled(cg);
            }

            base.Compiled(cg);
        }

        /// <inheritdoc />
        public override void DefineMember(TypeBuilder tb)
        {
            //Add SourceField
            Sourcefield = new FieldGenerator("source_" + Type.Name, Type);
            Sourcefield.DefineMember(tb);


            DetourPeoprtyGenerator dpg = null;
            foreach (var item in IterateInterfaceProperties(Type))
            {
                if (item.PropertyType.IsInterface && IterateInterfaces(item.PropertyType).Any(x => x.GetCustomAttributes(typeof(TypeLibTypeAttribute)).Any()))
                {
                    var InnerSourcefield = new FieldGenerator("source_" + item.Name, item.PropertyType);
                    InnerSourcefield.DefineMember(tb);
                    dpg = new DetourPeoprtyGenerator_ComObject(InnerSourcefield, Sourcefield, item)
                    {
                        OverrideDefinition = item.DeclaringType
                    };
                }
                else
                {
                    dpg = new DetourPeoprtyGenerator(Sourcefield, item)
                    {
                        OverrideDefinition = item.DeclaringType
                    };
                }
                Properties.Add(dpg);
                dpg.DefineMember(tb);
            }

            DetourMethodGenerator mpg = null;
            foreach (var item in IterateInterfaceMethods(Type))
            {
                //Do not implement getter and setter
                if (item.IsSpecialName)
                {
                    continue;
                }
                if (item.ReturnType.IsInterface && IterateInterfaces(item.ReturnType).Any(x => x.GetCustomAttributes(typeof(TypeLibTypeAttribute)).Any()))
                {
                    mpg = new DetourMethodGenerator_ComObject(Sourcefield, item)
                    {
                        OverrideDefinition = item.DeclaringType
                    };
                }
                else
                {
                    mpg = new DetourMethodGenerator(Sourcefield, item)
                    {
                        OverrideDefinition = item.DeclaringType
                    };
                }
                Methods.Add(mpg);

                mpg.DefineMember(tb);
            }

            //Add Release
            Dispose = new IDisposable_FinalReleaseComObject(Sourcefield.FieldBuilder, Properties.OfType<DetourPeoprtyGenerator_ComObject>().Select(x => x.LocalObject));
            Dispose.DefineMember(tb);

            //Add Finalize
            Dispose_Finalize = new FinalizeDispose(Dispose);
            Dispose_Finalize.DefineMember(tb);

            Defined = true;
        }

        /// <summary>
        /// Initializes a COMinstance Wrapper
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="DetourObject"></param>
        internal void InitializeInstance(object instance, object DetourObject)
        {
            Sourcefield.Field.SetValue(instance, DetourObject);
        }

        /// <summary>
        /// Iterates through all MEthods of the <paramref name="type"/>
        /// </summary>
        /// <param name="type">source type</param>
        /// <returns>all methods</returns>
        private static IEnumerable<MethodInfo> IterateInterfaceMethods(Type type)
        {
            foreach (var face in IterateInterfaces(type))
            {
                foreach (var item in face.GetMethods())
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Iterates through all Properties of the <paramref name="type"/>
        /// </summary>
        /// <param name="type">source type</param>
        /// <returns>all properties</returns>
        private static IEnumerable<PropertyInfo> IterateInterfaceProperties(Type type)
        {
            foreach (var face in IterateInterfaces(type))
            {
                foreach (var item in face.GetProperties())
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// <inheritdoc  cref="DynamicTypes.Compile" />
        /// </summary>
        private void Compile()
        {
            TypeGenerator.Compile();
        }

        private class DetourMethodGenerator_ComObject : DetourMethodGenerator
        {

            #region Constructors

            public DetourMethodGenerator_ComObject(FieldGenerator sourceObjcet, MethodInfo source) : base(sourceObjcet, source)
            {
            }

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
                IL.Emit(OpCodes.Ldfld, SourceObjcet.FieldBuilder);
                int i = 1;
                foreach (var item in Source.GetParameters())
                {
                    IL.Emit(OpCodes.Ldarg_S, i++);
                }

                IL.Emit(OpCodes.Callvirt, Source);
                IL.Emit(OpCodes.Stloc_0);
                IL.Emit(OpCodes.Ldloc_0);
                IL.Emit(OpCodes.Call, typeof(COMWrapper).GetMethod(nameof(COMWrapper.Wrap)).MakeGenericMethod(Type));
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

        #endregion

        #region Inline Types

        /// <summary>
        /// Generates Properties that wrap and initialize the Propertie
        /// </summary>
        private class DetourPeoprtyGenerator_ComObject : DetourPeoprtyGenerator
        {

            #region Properties

            /// <summary>
            /// Generator that contains a reference to the Parents Property that will be wrapped
            /// </summary>
            public FieldGenerator LocalObject { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of <see cref="DetourPeoprtyGenerator_ComObject"/>
            /// </summary>
            /// <param name="localObject">Generator that contains a local field that will be used to store the wrapped Property</param>
            /// <param name="sourceObjcet">The source object that contains the Property that will be wrapped </param>
            /// <param name="source">the Property that will be wrapped</param>
            public DetourPeoprtyGenerator_ComObject(FieldGenerator localObject, FieldGenerator sourceObjcet, PropertyInfo source) : base(sourceObjcet, source)
            {
                LocalObject = localObject;
            }

            #endregion

            #region Methods

            /// <inheritdoc/>
            public override void DefineMember(TypeBuilder tb)
            {
                PropertyBuilder = tb.DefineProperty(Source.Name, PropertyAttributes.HasDefault | PropertyAttributes.SpecialName, Type, IndexParameter);
                var getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final;
                var ccon = CallingConventions.HasThis;

                if (Source.GetMethod != null)
                {
                    var sepParameter = Type.EmptyTypes;
                    var mbGet = tb.DefineMethod("get_" + Source.Name, getSetAttr, ccon, Type, Source.GetGetMethod().GetParameters().Select(x => x.ParameterType).ToArray());

                    var getIL = mbGet.GetILGenerator();

                    getIL.DeclareLocal(typeof(bool));
                    getIL.DeclareLocal(Type);
                    var sJmp_fieldDefined = getIL.DefineLabel();
                    var sJmp_end = getIL.DefineLabel();

                    //if(LocalObject != null)
                    //{
                    getIL.Emit(OpCodes.Nop);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldfld, LocalObject.FieldBuilder);
                    getIL.Emit(OpCodes.Ldnull);
                    getIL.Emit(OpCodes.Ceq);
                    getIL.Emit(OpCodes.Stloc_0);
                    getIL.Emit(OpCodes.Ldloc_0);
                    getIL.Emit(OpCodes.Brfalse_S, sJmp_fieldDefined);
                    //}
                    //Wrap and initialize LocalObject
                    getIL.Emit(OpCodes.Nop);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldfld, SourceObjcet.FieldBuilder);
                    getIL.Emit(OpCodes.Callvirt, OverrideDefinition.GetMethod(mbGet.Name));
                    getIL.Emit(OpCodes.Call, typeof(COMWrapper).GetMethod(nameof(COMWrapper.Wrap)).MakeGenericMethod(Type));
                    getIL.Emit(OpCodes.Castclass, Type);
                    getIL.Emit(OpCodes.Stfld, LocalObject.FieldBuilder);

                    //return LocalObject
                    getIL.MarkLabel(sJmp_fieldDefined);
                    getIL.Emit(OpCodes.Nop);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldfld, LocalObject.FieldBuilder);
                    getIL.Emit(OpCodes.Stloc_1);
                    getIL.Emit(OpCodes.Br_S, sJmp_end);
                    getIL.MarkLabel(sJmp_end);
                    getIL.Emit(OpCodes.Ldloc_1);

                    getIL.Emit(OpCodes.Ret);


                    PropertyBuilder.SetGetMethod(mbGet);

                    if (OverrideDefinition != null)
                    {
                        tb.DefineMethodOverride(mbGet, OverrideDefinition.GetMethod("get_" + Source.Name));
                    }
                }

                Defined = true;
            }

            #endregion

        }
        /// <summary>
        /// Implements a Finalize Method (Calls  Dispose())
        /// </summary>
        private class FinalizeDispose : MemberGenerator
        {

            #region Properties

            public IDisposable_FinalReleaseComObject Dispose { get; }

            public MethodBuilder MethodBuilder { get; private set; }

            #endregion

            #region Constructors

            public FinalizeDispose(IDisposable_FinalReleaseComObject dispose) : base(null)
            {
                Dispose = dispose;
            }

            #endregion

            #region Methods

            public override void DefineMember(TypeBuilder tb)
            {
                MethodBuilder = tb.DefineMethod(nameof(IDisposable.Dispose), MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, null, Type.EmptyTypes);
                var IL = MethodBuilder.GetILGenerator();

                IL.BeginExceptionBlock();

                IL.Emit(OpCodes.Nop);
                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Call, Dispose.MethodBuilder);
                IL.Emit(OpCodes.Nop);

                IL.BeginFinallyBlock();

                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Call, Dispose.MethodBuilder);
                IL.Emit(OpCodes.Nop);

                IL.EndExceptionBlock();

                Defined = true;
            }

            #endregion

        }

        /// <summary>
        /// Generates a Dispose method and a _disposed field (when disposing <see cref="Marshal.FinalReleaseComObject"/> will be called for the source object
        /// </summary>
        private class IDisposable_FinalReleaseComObject : MemberGenerator
        {

            #region Properties

            /// <summary>
            /// The Method builder for the dispose method
            /// </summary>
            public MethodBuilder MethodBuilder { get; private set; }
            /// <summary>
            /// the target for <see cref="Marshal.FinalReleaseComObject"/> will be called
            /// </summary>
            public FieldBuilder FinalizeTarget { get; }
            /// <summary>
            /// Contains a list of Fields that are Going to be Disposed
            /// </summary>
            public IEnumerable<FieldGenerator> ChildrenToDispose { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// initializes a new instance of <see cref="IDisposable_FinalReleaseComObject"/>
            /// </summary>
            /// <param name="finalizeTarget">the target for that<see cref="Marshal.FinalReleaseComObject"/> will be called</param>
            public IDisposable_FinalReleaseComObject(FieldBuilder finalizeTarget, IEnumerable<FieldGenerator> childrenToDispose) : base(typeof(IDisposable))
            {
                FinalizeTarget = finalizeTarget;
                ChildrenToDispose = childrenToDispose;
            }

            #endregion

            #region Methods

            /// <inheritdoc/>
            public override void DefineMember(TypeBuilder tb)
            {
                var fgDisposed = new FieldGenerator("_disposed", typeof(bool));
                fgDisposed.DefineMember(tb);

                MethodBuilder = tb.DefineMethod(nameof(IDisposable.Dispose), MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, null, Type.EmptyTypes);
                var IL = MethodBuilder.GetILGenerator();
                var l1 = IL.DeclareLocal(typeof(bool));
                var l2 = IL.DeclareLocal(typeof(bool));

                var lJump_end = IL.DefineLabel();
                var lJump_setDisposed = IL.DefineLabel();

                IL.DeclareLocal(typeof(bool));

                var _disposed = fgDisposed.FieldBuilder;

                IL.Emit(OpCodes.Nop);
                //if!disposed
                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Ldfld, _disposed);
                IL.Emit(OpCodes.Ldc_I4, 0);
                IL.Emit(OpCodes.Ceq);
                IL.Emit(OpCodes.Stloc_0);
                IL.Emit(OpCodes.Ldloc_0);//IF disposed
                IL.Emit(OpCodes.Brfalse, lJump_end);
                //if(Source != null)
                IL.Emit(OpCodes.Nop);
                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Ldfld, FinalizeTarget);
                IL.Emit(OpCodes.Ldnull);
                IL.Emit(OpCodes.Cgt_Un);
                IL.Emit(OpCodes.Stloc_1);
                IL.Emit(OpCodes.Ldloc_1);
                IL.Emit(OpCodes.Brfalse, lJump_setDisposed);

                //release
                // Call Dispose for all Disposable Fields
                foreach (var item in ChildrenToDispose)
                {

                    var lJump_endif = IL.DefineLabel();
                    var lJump_dispose = IL.DefineLabel();
                    //field?.dispose()

                    IL.Emit(OpCodes.Nop);
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(OpCodes.Ldfld, item.FieldBuilder);
                    IL.Emit(OpCodes.Dup);
                    IL.Emit(OpCodes.Brtrue, lJump_dispose);
                    IL.Emit(OpCodes.Pop);
                    IL.Emit(OpCodes.Br, lJump_endif);
                    IL.MarkLabel(lJump_dispose);
                    IL.Emit(OpCodes.Call, typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));
                    IL.Emit(OpCodes.Nop);
                    IL.MarkLabel(lJump_endif);
                    IL.Emit(OpCodes.Nop);
                }

                IL.Emit(OpCodes.Nop);
                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Ldfld, FinalizeTarget);
                IL.Emit(OpCodes.Call, typeof(Marshal).GetMethod(nameof(Marshal.FinalReleaseComObject)));
                IL.Emit(OpCodes.Pop);

                //Set _disposed to true
                IL.Emit(OpCodes.Nop);
                IL.MarkLabel(lJump_setDisposed);

                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Ldc_I4, 1);
                IL.Emit(OpCodes.Stfld, _disposed);

                IL.MarkLabel(lJump_end);
                IL.Emit(OpCodes.Ret);

                tb.DefineMethodOverride(MethodBuilder, Type.GetMethod(nameof(IDisposable.Dispose)));

                Defined = true;
            }

            #endregion

        }

        #endregion

    }
}
