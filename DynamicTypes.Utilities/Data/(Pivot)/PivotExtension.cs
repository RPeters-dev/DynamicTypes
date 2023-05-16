using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes.Utilities.Data
{

    public static class PivotExtension
    {
        public static IEnumerable<IPivotChange> Pivot<T, OutT>(this IEnumerable<T> source, string keyX, string keyY, string value) where OutT : IPivotChange, new()
        {
            return Pivot(source, keyX, keyY, value, typeof(OutT));
        }

        public static IEnumerable<IPivotChange> Pivot<T, OutT>(this IEnumerable<T> source, Func<T, string> keyX, Func<T, object> keyY, Expression<Func<T, object>> value) where OutT : IPivotChange, new()
        {
            return Pivot(source, keyX, keyY, value, typeof(OutT));
        }

        public static IEnumerable<IPivotChange> Pivot<T>(this IEnumerable<T> source, string keyX, string keyY, string value, Type? baseType = null)
        {
            var keyXMember = typeof(T).GetMember(keyX).First();
            var keyYMember = typeof(T).GetMember(keyY).First();
            var valueMember = typeof(T).GetMember(value).First();

            return Pivot(source, (x) => keyXMember.InvokeGet(x).ToString(), x => keyYMember.InvokeGet(x), x => valueMember.InvokeGet(x), valueMember, baseType);
        }

        public static IEnumerable<IPivotChange> Pivot<T>(this IEnumerable<T> source, Func<T, string> keyX, Func<T, object> keyY, Expression<Func<T, object>> value, Type? baseType = null)
        {
            var getValue = value.Compile();

            return Pivot(source, keyX, keyY, getValue, (value.Body as MemberExpression).Member, baseType);
        }

        public static IEnumerable<IPivotChange> Pivot<T>(this IEnumerable<T> source, Func<T, string> keyX, Func<T, object> keyY, Func<T, object> value, MemberInfo valueMember, Type? baseType = null)
        {
            var keyValueList = source.Select(x => new { Item = x, KeyX = keyX.Invoke(x), KeyY = keyY.Invoke(x), Value = value.Invoke(x) });
            var columns = keyValueList.Select(x => x.KeyX).Distinct().ToList();

            var tg = new TypeGenerator($"Pivot<{typeof(T).Name}>", baseType ?? typeof(PivotRowBase))
            {

            };

            var setValueMethod = typeof(IPivotChange).GetMethod(nameof(IPivotChange.Set));
            var getValueMethod = typeof(IPivotChange).GetMethod(nameof(IPivotChange.Get));

            foreach (var column in columns)
            {
                void generatepivotSet(ILGenerator il)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, column);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Callvirt, setValueMethod);
                    il.Emit(OpCodes.Ret);

                }
                void generatePivotGet(ILGenerator il)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, column);
                    il.Emit(OpCodes.Callvirt, getValueMethod);
                    il.Emit(OpCodes.Ret);
                }

                tg.Members.Add(new PropertyGeneratorBase(column, typeof(object))
                {
                    GenerateGetMethod = generatePivotGet,
                    GenerateSetMethod = generatepivotSet
                });
            }

            var resType = tg.Compile();


            var result = new List<IPivotChange>();
            IPivotChange row;
            foreach (var item in keyValueList.GroupBy(x => x.KeyY))
            {
                result.Add(row = tg.CreateInstance<PivotRowBase>());
                row.ValueMember = valueMember;
                row.GeneratorSource = tg;
                row.SourceItems = item.ToDictionary(x => x.KeyX, x => (object)x.Item);
                yield return row;
            }
        }

    }
}
