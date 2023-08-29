using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DynamicTypes.Utilities.Data
{

    public static class PivotExtension
    {
        public static IEnumerable<IPivotRow> Pivot<T, OutT>(this IEnumerable<T> source, string keyX, string keyY, string getValue, string? setValue = null) where OutT : IPivotRow, new()
        {
            return Pivot(source, keyX, keyY, getValue, setValue, typeof(OutT));
        }

        public static IEnumerable<IPivotRow> Pivot<T, OutT>(this IEnumerable<T> source, Func<T, string> keyX, Func<T, object> keyY, Expression<Func<T, object>> value) where OutT : IPivotRow, new()
        {
            return Pivot(source, keyX, keyY, value, typeof(OutT));
        }

        public static IEnumerable<IPivotRow> Pivot<T>(this IEnumerable<T> source, string keyX, string keyY, string getValue, string? setValue = null, Type? baseType = null)
        {
            var keyXMember = typeof(T).GetMember(keyX).First();
            var keyYMember = typeof(T).GetMember(keyY).First();
            var getValueMember = typeof(T).GetMember(getValue).First();
            var setValueMember = string.IsNullOrEmpty(setValue) ? getValueMember : typeof(T).GetMember(setValue).FirstOrDefault();

            return Pivot(source, (x) => keyXMember.InvokeGet(x).ToString(), x => keyYMember.InvokeGet(x), x => getValueMember.InvokeGet(x), setValueMember, baseType);
        }

        public static IEnumerable<IPivotRow> Pivot<T>(this IEnumerable<T> source, Func<T, string> keyX, Func<T, object> keyY, Expression<Func<T, object>> value, Type? baseType = null)
        {
            var getValue = value.Compile();

            return Pivot(source, keyX, keyY, getValue, (value.Body as MemberExpression).Member, baseType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="keyX"></param>
        /// <param name="keyY"></param>
        /// <param name="value"></param>
        /// <param name="getValueMember"></param>
        /// <param name="baseType">U can use this to add a INotifyPropertyChanged handler for Datagrids or something else</param>
        /// <returns></returns>
        public static IEnumerable<IPivotRow> Pivot<T>(this IEnumerable<T> source, Func<T, string> keyX, Func<T, object> keyY, Func<T, object> value, MemberInfo getValueMember, MemberInfo? setValueMember = null, Type? baseType = null)
        {
            setValueMember = setValueMember ?? getValueMember;
            var keyValueList = source.Select(x => new { Item = x, KeyX = keyX.Invoke(x), KeyY = keyY.Invoke(x), Value = value.Invoke(x) });
            var columns = keyValueList.Select(x => x.KeyX).Distinct().ToList();
            var typename = $"Pivot<{typeof(T).Name}>";

            var ItemName = "Item";

            var tg = new TypeGenerator(typename, baseType ?? typeof(PivotRowBase))
            {
                Attributes =
                {
                    new AttributeGenerator<DebuggerDisplayAttribute>(typename + " {ToString()}"),
                    new AttributeGenerator<DefaultMemberAttribute>(ItemName),
                }
            };

            var setValueMethod = typeof(IPivotRow).GetMethod(nameof(IPivotRow.Set));
            var getValueMethod = typeof(IPivotRow).GetMethod(nameof(IPivotRow.Get));

            foreach (var column in columns)
            {
                void generatePivotSet(ILGenerator il)
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
                    GenerateSetMethod = generatePivotSet
                });
            }

            tg.Members.Add(new iPropertyGenerator<IPivotRowInternals>(ItemName)
            {
                ParameterTypes = new[] { typeof(string) },
                GenerateGetMethod =  (il) =>
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Callvirt, getValueMethod);
                    il.Emit(OpCodes.Ret);
                },
                GenerateSetMethod = (il) =>
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Callvirt, setValueMethod);
                    il.Emit(OpCodes.Ret);
                },
            });

            tg.Members.Add(new IMethodGenerator<IPivotRow>(nameof(IPivotRow.ToString))
            {
                Generator = (il) =>
                {
                    var sbAppend = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new[] { typeof(object) });
                    void AppendString(string item)
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldstr, item);
                        il.Emit(OpCodes.Callvirt, sbAppend);
                        il.Emit(OpCodes.Pop);
                    }

                    AppendString("(");
                    for (int i = 0; i < columns.Count; i++)
                    {
                        //Ldarg_1 StringBuilder
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldstr, columns[i]);
                        il.Emit(OpCodes.Callvirt, getValueMethod);
                        il.Emit(OpCodes.Callvirt, sbAppend);
                        il.Emit(OpCodes.Pop);

                        if (i < columns.Count - 1)
                        {
                            AppendString(", ");
                        }
                    }

                    AppendString(")");

                    il.Emit(OpCodes.Ldarg_1);
                    var fum = typeof(StringBuilder).GetMethod(nameof(object.ToString), new Type[0]);
                    il.Emit(OpCodes.Callvirt, fum);

                    il.Emit(OpCodes.Ret);
                }
            });

            var resType = tg.Compile();
            foreach (var item in keyValueList.GroupBy(x => x.KeyY))
            {
                IPivotRow row = tg.CreateInstance<PivotRowBase>();
                row.RowDetails.GetValueMember = getValueMember;
                row.RowDetails.SetValueMember = setValueMember;
                row.RowDetails.GeneratorSource = tg;
                row.RowDetails.SourceItems = item.ToDictionary(x => x.KeyX, x => (object?)x.Item);
                yield return row;
            }
        }

    }
}
