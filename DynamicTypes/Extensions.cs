using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            tg.Members.OfType<PropertyGeneratorBase>().FirstOrDefault(x => x.Name == name)?.SetValue(instance, value);
            tg.Members.OfType<FieldGenerator>().FirstOrDefault(x => x.Name == name)?.SetValue(instance, value);
        }

        public static object? GetValue(this TypeGenerator tg, object instance, string name)
        {
            if (tg.Members.OfType<PropertyGeneratorBase>().FirstOrDefault(x => x.Name == name) is PropertyGeneratorBase pg)
                return pg.GetValue(instance);

            return tg.Members.OfType<FieldGenerator>().FirstOrDefault(x => x.Name == name)?.GetValue(instance);
        }

        public static void SetValue(this PropertyGeneratorBase pg, object instance, object value) => pg.Property.SetValue(instance, value);

        public static void SetValue(this FieldGenerator fg, object instance, object value) => fg.Field.SetValue(instance, value);

        public static object? GetValue(this PropertyGeneratorBase pg, object instance) => pg.Property.GetValue(instance);

        public static object? GetValue(this FieldGenerator fg, object instance) => fg.Field.GetValue(instance);

        public static T? GetValue<T>(this PropertyGeneratorBase pg, object instance) => (T?)pg.GetValue(instance);

        public static T? GetValue<T>(this FieldGenerator fg, object instance) => (T?)fg.GetValue(instance);

        public static void InvokeSet(this MemberInfo member, object? instance, params object?[]? index)
        {
            if (member is FieldInfo fieldInfo)
                fieldInfo.SetValue(instance, index?.FirstOrDefault());
            else if (member is PropertyInfo propertyInfo)
                propertyInfo.SetValue(instance, index?.FirstOrDefault());
            else if (member is MethodInfo methodInfo)
                methodInfo.Invoke(instance, index);
        }
        public static object? InvokeGet(this MemberInfo member, object? instance, params object?[]? index)
        {
            if (member is FieldInfo fieldInfo)
                return fieldInfo.GetValue(instance);
            else if (member is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(instance, index);
            else if (member is MethodInfo methodInfo)
                return methodInfo.Invoke(instance, index);

            return null;
        }
        public static void Throw<T>(this ILGenerator il, params object[] p)
        {
            foreach (var item in p)
            {
                if (item is string s)
                    il.Emit(OpCodes.Ldstr, s);
                else if (item == null)
                    il.Emit(OpCodes.Ldnull);
                else if (il.ldNumber(item))
                {
                }
                else
                    throw new InvalidOperationException("not supported at the moment");
            }

            il.Emit(OpCodes.Newobj, typeof(T));
            il.Emit(OpCodes.Throw, typeof(T));
        }

        public static bool ldNumber(this ILGenerator il, object value)
        {
            switch (value.GetType().Name)
            {
                case nameof(SByte):
                case nameof(Byte):
                    il.Emit(OpCodes.Ldc_I4, (byte)value);
                    break;
                case nameof(Int16):
                case nameof(UInt16):
                    il.Emit(OpCodes.Ldc_I4_S, (short)value);
                    break;
                case nameof(Int32):
                case nameof(UInt32):
                    il.Emit(OpCodes.Ldc_I4, (int)value);
                    break;
                case nameof(Int64):
                case nameof(UInt64):
                    il.Emit(OpCodes.Ldc_I8, (long)value);
                    break;
                case nameof(Single):
                    il.Emit(OpCodes.Ldc_R4, (float)value);
                    break;
                case nameof(Double):
                    il.Emit(OpCodes.Ldc_R8, (double)value);
                    break;
                case nameof(Decimal): return false; //throw new InvalidOperationException("thats an object");
                                                    //instance void [System.Runtime]System.Decimal::.ctor(int32, int32, int32, bool, uint8)
                default: return false;
            }
            return true;
        }

    }
}
