using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTypes
{
    public static class Tools
    {
        /// <summary>
        /// i dont want to add a refererence to management here, just use thenum or the name
        /// </summary>
        /// <param name="cimType"></param>
        /// <returns></returns>
        public static Type chimMToSystemType(object cimType)
        {
            switch (cimType.ToString())
            {
                case "Boolean": return typeof(bool);
                case "Char16": return typeof(char);
                case "DateTime": return typeof(string);
                case "None":
                case "Object": return typeof(object);
                case "Real32": return typeof(float);
                case "Real64": return typeof(double);
                case "Reference": return typeof(short);
                case "SInt16": return typeof(short);
                case "SInt32": return typeof(int);
                case "SInt64": return typeof(long);
                case "SInt8": return typeof(sbyte);
                case "String": return typeof(string);
                case "UInt8": return typeof(byte);
                case "UInt16": return typeof(ushort);
                case "UInt32": return typeof(uint);
                case "UInt64": return typeof(ulong);
                case "": return typeof(double);
                default: return typeof(object);
            }
        }
    }
}
