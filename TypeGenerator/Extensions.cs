using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTypes
{
    public static class Extensions
    {

        public static object CreateInstance(this TypeGenerator tg)
        {
            return Activator.CreateInstance(tg.Type);
        }
        public static void SetValue(this TypeGenerator tg, object instance, string name, object value)
        {
            tg.Members.OfType<PropertyGenerator>().FirstOrDefault(x => x.PropertyName == name)?.SetValue(instance, value);
            tg.Members.OfType<FieldGenerator>().FirstOrDefault(x => x.FieldName == name)?.SetValue(instance, value);
        }
        public static void SetValue(this PropertyGenerator pg, object instance, object value)
        {
            pg.Property.SetValue(instance, value);
        }

        public static void SetValue(this FieldGenerator fg, object instance, object value)
        {
            fg.Field.SetValue(instance, value);
        }

    }
}
