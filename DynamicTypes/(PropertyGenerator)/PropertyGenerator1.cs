namespace DynamicTypes
{
    public class PropertyGenerator1<T> : PropertyGenerator
    {
        public PropertyGenerator1(string name) : base(name, typeof(T))
        {
        }
    }
}
