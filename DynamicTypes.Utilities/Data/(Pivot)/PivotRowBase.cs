using System.Reflection;

namespace DynamicTypes.Utilities.Data
{
    public class PivotRowBase : IPivotChange
    {
        TypeGenerator IPivotChange.GeneratorSource { get; set; }
        MemberInfo IPivotChange.ValueMember { get; set; }

        Dictionary<string, object> IPivotChange.SourceItems { get; set; }

        public void Set(string keyX, object value)
        {
            (this as IPivotChange).ValueMember.InvokeSet((this as IPivotChange).SourceItems[keyX], value);
        }

        public object? Get(string keyX)
        {
            return (this as IPivotChange).ValueMember.InvokeGet((this as IPivotChange).SourceItems[keyX]);
        }
    }
}
