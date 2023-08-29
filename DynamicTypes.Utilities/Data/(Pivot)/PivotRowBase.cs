using System.Reflection;
using System.Text;

namespace DynamicTypes.Utilities.Data
{
    public abstract class PivotRowBase : IPivotRow, IPivotRowInfo
    {
        public IPivotRowInfo RowDetails => this;

        public TypeGenerator GeneratorSource { get; set; }
        public MemberInfo GetValueMember { get; set; }
        public MemberInfo SetValueMember { get; set; }
        public IDictionary<string, object> SourceItems { get; set; }

        /// <inheritdoc/>
        public void Set(string keyX, object value)
        {
            RowDetails.SetValueMember.InvokeSet(RowDetails.SourceItems[keyX], value);
        }

        /// <inheritdoc/>
        public object? Get(string keyX)
        {
            return RowDetails.GetValueMember.InvokeGet(RowDetails.SourceItems[keyX]);
        }

        public override string ToString()
        {
            return ((IPivotRow)this).ToString(new StringBuilder());
        }

        public virtual string ToString(StringBuilder sb)
        {
            return GetType().ToString();
        }
    }
}
