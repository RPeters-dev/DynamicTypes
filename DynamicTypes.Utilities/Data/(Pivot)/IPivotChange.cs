using System.Reflection;

namespace DynamicTypes.Utilities.Data
{
    public interface IPivotChange
    {
        TypeGenerator GeneratorSource { get; set; }

        MemberInfo ValueMember { get; set; }

        Dictionary<string, object> SourceItems { get; set; }

        void Set(string keyX, object value);

        object? Get(string keyX);
    }
}
