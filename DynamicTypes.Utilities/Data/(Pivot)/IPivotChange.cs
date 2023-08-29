using System.Reflection;
using System.Text;

namespace DynamicTypes.Utilities.Data
{
    public interface IPivotRowInfo
    {
        /// <summary>
        /// The Type Generator used to create the instance
        /// </summary>
        public TypeGenerator GeneratorSource { get; set; }

        /// <summary>
        /// The Member containing the Value
        /// </summary>
        public MemberInfo GetValueMember { get; set; }

        /// <summary>
        /// The Member used to set the Value
        /// </summary>
        public MemberInfo SetValueMember { get; set; }

        /// <summary>
        /// A List of Items used as Source
        /// </summary>
        public IDictionary<string, object> SourceItems { get; set; }
    }

    public interface IPivotRowInternals
    {
        public object this[string keyx] { get; set; }
    }

    /// <summary>
    /// Provides an Inteface to change A Pivoted Value
    /// </summary>
    public interface IPivotRow
    {
        /// <summary>
        /// Contains information about the Pivot Row
        /// </summary>
        public IPivotRowInfo RowDetails { get; }

        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="keyX">The Key for X (Key of <see cref="SourceItems"/>)</param>
        /// <param name="value">The New Value</param>
        void Set(string keyX, object value);

        /// <summary>
        /// Get Value
        /// </summary>
        /// <param name="keyX">The Key for X (Key of <see cref="SourceItems"/>)</param>
        /// <returns>the value of the item</returns>
        object? Get(string keyX);
        /// <summary>
        /// will be generated to add a String Output
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        string ToString(StringBuilder sb);

    }
}
