using System.Reflection;

namespace DynamicTypes
{
    /// <summary>
    /// Wrapps ComObjects with the <see cref="COMWrapper"/>
    /// </summary>
    internal class DetourPeoprtyWrapper_ComObject : DetourPropertyWrapper
    {
        public DetourPeoprtyWrapper_ComObject(FieldGenerator localObject, FieldGenerator sourceObjcet, PropertyInfo source) 
            : base(localObject, sourceObjcet, source, typeof(COMWrapper).GetMethod(nameof(COMWrapper.Wrap)))
        {
        }
    }
}
