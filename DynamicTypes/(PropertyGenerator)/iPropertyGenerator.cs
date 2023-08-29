using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// Generate an override for a Interface Property
    /// </summary>
    public class iPropertyGenerator : PropertyGenerator
    {
        /// <summary>
        /// i dont like this approach, but it works, for now
        /// </summary>
        public bool UseSingleBackingField { get; set; }

        public PropertyInfo TargetProperty { get; }

        public iPropertyGenerator(Type T, string name) : base(name, null)
        {
            OverrideDefinition = T;

            var mems = T.GetMembers();

            TargetProperty = T.GetProperty(name);

            ParameterTypes = TargetProperty.GetIndexParameters().Select(x => x.ParameterType).ToArray();

            //dont generate a field with index poroperties
            GegenerateField = !ParameterTypes.Any();

            Type = TargetProperty.PropertyType;

            Get = TargetProperty.GetMethod != null;
            Set = TargetProperty.SetMethod != null;

            UseSingleBackingField = true;
        }

        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            BackingFieldName = UseSingleBackingField ? null : TargetProperty.DeclaringType.Name + "_" + TargetProperty.Name;

            base.DefineMember(tb, tg);
        }
    }
}
