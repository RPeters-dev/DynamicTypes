
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace DynamicTypes
{
    /// <summary>
    /// Configures a new Type that Wraps a COM object and adds Finalize and Dispose 
    /// </summary>
    public partial class COMWrapper : MemberGenerator
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
        IDisposableGenerator Dispose { get; set; }
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
        List<DetourPropertyGenerator> Properties { get; set; } = new List<DetourPropertyGenerator>();

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
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            //Add SourceField
            Sourcefield = new FieldGenerator("source_" + Type.Name, Type);
            Sourcefield.DefineMember(tb, tg);


            DetourPropertyGenerator dpg = null;
            foreach (var item in IterateInterfaceProperties(Type))
            {
                if (item.PropertyType.IsInterface && IterateInterfaces(item.PropertyType).Any(x => x.GetCustomAttributes(typeof(TypeLibTypeAttribute)).Any()))
                {
                    var InnerSourcefield = new FieldGenerator("source_" + item.Name, item.PropertyType);
                    InnerSourcefield.DefineMember(tb, tg);
                    //Wrap it 
                    dpg = new DetourPeoprtyWrapper_ComObject(InnerSourcefield, Sourcefield, item)
                    {
                        OverrideDefinition = item.DeclaringType
                    };
                }
                else
                {
                    //use a normal Detour Property for normal Types
                    dpg = new DetourPropertyGenerator(Sourcefield, item)
                    {
                        OverrideDefinition = item.DeclaringType
                    };
                }
                Properties.Add(dpg);
                dpg.DefineMember(tb, tg);
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
                    mpg = new DetourMethodWrapper_ComObject(Sourcefield, item)
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

                mpg.DefineMember(tb, tg);
            }

            //Add Release
            Dispose = new IDisposable_ComObject(Properties.OfType<DetourPeoprtyWrapper_ComObject>().Select(x => x.LocalObject), Sourcefield.internalField);
            Dispose.DefineMember(tb, tg);

            //Add Finalize
            Dispose_Finalize = new FinalizeDispose(Dispose);
            Dispose_Finalize.DefineMember(tb, tg);

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

        #endregion

    }
}
