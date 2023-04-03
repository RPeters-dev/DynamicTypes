using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32.SafeHandles;

namespace DynamicTypes.Utilities.Data
{
    public static class IDataReaderExtension
    {
        [DebuggerDisplay("{Name} - {Value}")]
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        public sealed class ColumnInfoAttribute : Attribute
        {
            public ColumnInfoAttribute(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public string Value { get; }
        }


        public interface ILoadIDataReaderRow
        {
            public void Load(IDataReader source);
        }

        public static TypeGenerator ToTypeGenerator(this IDataReader reader)
        {
            var dt = reader.GetSchemaTable();
            string[] columnNames = dt?.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
            .ToArray() ?? Array.Empty<string>();

            PropertyGenerator[] columns = new PropertyGenerator[reader.FieldCount];
            Type fieldType = null;
            for (int i = 0; i<reader.FieldCount; i++)
            {
                fieldType = reader.GetFieldType(i);

                if (fieldType.IsValueType)
                    fieldType = dt.Rows?[i]?["AllowDbNull"] as bool? == false
                    ? fieldType : typeof(Nullable<>).MakeGenericType(reader.GetFieldType(i));

                columns[i] = new PropertyGenerator(reader.GetName(i), reader.GetFieldType(i))
                {
                    Attributes = columnNames.Select(x =>
                    {
                        return new AttributeGenerator<ColumnInfoAttribute>(x, dt?.Rows[i]?[x]?.ToString());
                    }).ToList<AttributeGenerator>()
                };
            }

            List<string> sourcetables = new List<string>();
            TypeGenerator tg = new TypeGenerator("DynamicTypes.IDataReaderObject") { 
                Members = columns.ToList<MemberGenerator>(),
                InterfaceImplementations = { typeof(ILoadIDataReaderRow) } 
            };

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sourcetables.Add(dt.Rows[i]["BaseSchemaName"] + "." + dt.Rows[i]["BaseTableName"]);
            }
            if (sourcetables.Distinct().Count() == 1 && sourcetables[0] != ".")
            {
                tg.TypeName = sourcetables[0];
                tg.EnshureUniqueName = false;
            }

            var GetValue = typeof(IDataReaderExtension).GetMethod(nameof(IDataReader.GetValue), BindingFlags.Static | BindingFlags.Public);

            tg.Members.Add(new IMethodGenerator<ILoadIDataReaderRow>(nameof(ILoadIDataReaderRow.Load))
            {
                Generator  = (il) =>
                    {
                        for (int i = 0; columns.Length > i; i++)
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Ldc_I4, i);
                            il.Emit(OpCodes.Call, GetValue.MakeGenericMethod(columns[i].Type));
                            il.Emit(OpCodes.Stfld, columns[i].BackingField.internalField);
                        }
                        il.Emit(OpCodes.Ret);
                    }
            });

            return tg;
        }

        public static T GetValue<T>(IDataReader reader, int column) => reader.IsDBNull(column) ? default : (T)reader.GetValue(column);

        public static IEnumerable<object> ToObject(this IDataReader reader)
        {
            var tg = reader.ToTypeGenerator();
            tg.Compile();
            while (reader.Read())
            {
                ILoadIDataReaderRow instance = tg.CreateInstance<ILoadIDataReaderRow>();
                instance.Load(reader);

                yield return instance;
            }
        }
    }
}
