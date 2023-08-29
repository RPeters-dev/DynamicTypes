namespace DynamicTypes
{
    public class iPropertyGenerator<T> : iPropertyGenerator
    {
        public iPropertyGenerator(string name) : base(typeof(T), name)
        {

        }
    }
}
