using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTypes
{
    public static class TypeList 
    {
        public static void Add<T>(this List<Type> source)
        {
            source.Add(typeof(T));
        }
    }
}
