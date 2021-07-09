using System.Reflection;

namespace DynamicTypes
{
    /// <summary>
    /// Generates Methods that wrap the Source Method with <see cref="COMWrapper.Wrap"/>
    /// </summary>
    public class DetourMethodWrapper_ComObject : DetourMethodWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DetourMethodWrapper_ComObject"/>
        /// </summary>
        /// <param name="sourceObjcet">Generator that contains a local field that will be used to store the wrapped Property</param>
        /// <param name="source">the Method that will be wrapped</param>
        public DetourMethodWrapper_ComObject(FieldGenerator sourceObjcet, MethodInfo source) 
            : base(sourceObjcet, source, typeof(COMWrapper).GetMethod(nameof(COMWrapper.Wrap)))
        {
        }
    }
}
