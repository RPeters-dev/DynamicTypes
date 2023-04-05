using System.Reflection;

namespace DynamicTypes
{
    public class PaarmeterDecriptor<T> : PaarmeterDecriptor
    {
        public PaarmeterDecriptor() : base(typeof(T))
        {
            
        }


        public PaarmeterDecriptor(String name) : base(typeof(T), name)
        {
            
        }
    }

    public class PaarmeterDecriptor
    {
        /// <summary>
        /// Defines the attributes that can be associated with a parameter.
        /// </summary>
        public virtual ParameterAttributes Attributes { get; set; }
        /// <summary>
        /// Defines the Taype of the Parameter
        /// </summary>
        public virtual Type ParameterType { get; set; }
        /// <summary>
        /// Defines the Name of the Parameter
        /// </summary>
        public virtual string? Name { get; set; }

        /// <summary>
        /// Defines the DefaultValue of the Parameter 
        /// Attention: dont touch it if u dont want a default value
        /// </summary>
        public object? DefaultValue { get; set; } = NoDefaultValue;

        public PaarmeterDecriptor()
        {
            
        }

        public PaarmeterDecriptor(Type t, string name = null)
        {
            ParameterType = t;
            Name = name;
        }

        public static implicit operator PaarmeterDecriptor(Type t)
        {
            return new PaarmeterDecriptor() { ParameterType = t };
        }

        public static PaarmeterDecriptor[] get(params Type[] source) => get(source as IEnumerable<Type>);

        public static PaarmeterDecriptor[] get(IEnumerable<Type> source)
        {
            return source.Select(x => new PaarmeterDecriptor { ParameterType = x }).ToArray();
        }

        public static PaarmeterDecriptor[] get(IEnumerable<ParameterInfo> source)
        {
            return source.Select(x => new PaarmeterDecriptor
            { ParameterType = x.ParameterType, Name = x.Name, DefaultValue = x.IsOptional ? x.DefaultValue : NoDefaultValue }).ToArray();
        }

        public static PaarmeterDecriptor[] get(params ParameterInfo[] source) => get(source as IEnumerable<ParameterInfo>);



        /// <summary>
        /// used to identify no default values, since null is a default value too
        /// </summary>
        internal static object NoDefaultValue = new object();
    }
}
