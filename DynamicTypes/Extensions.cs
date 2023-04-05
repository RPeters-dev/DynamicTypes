using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTypes
{
    public static class Extensions
    {

        public static object? CreateInstance(this TypeGenerator tg, params object?[]? args) => Activator.CreateInstance(tg.Type, args);
        public static T? CreateInstance<T>(this TypeGenerator tg, params object?[]? args) => (T?)Activator.CreateInstance(tg.Type, args);

        public static void Add<T>(this List<Type> source)
        {
            source.Add(typeof(T));
        }

        public static void SetValue(this TypeGenerator tg, object instance, string name, object value)
        {
            tg.Members.OfType<PropertyGenerator>().FirstOrDefault(x => x.PropertyName == name)?.SetValue(instance, value);
            tg.Members.OfType<FieldGenerator>().FirstOrDefault(x => x.FieldName == name)?.SetValue(instance, value);
        }

        public static void SetValue(this PropertyGenerator pg, object instance, object value) => pg.Property.SetValue(instance, value);

        public static void SetValue(this FieldGenerator fg, object instance, object value) => fg.Field.SetValue(instance, value);

        public static object? GetValue(this PropertyGenerator pg, object instance) => pg.Property.GetValue(instance);

        public static object? GetValue(this FieldGenerator fg, object instance) => fg.Field.GetValue(instance);

        public static T? GetValue<T>(this PropertyGenerator pg, object instance) => (T?)pg.GetValue(instance);

        public static T? GetValue<T>(this FieldGenerator fg, object instance) => (T?)fg.GetValue(instance);


    }
}
