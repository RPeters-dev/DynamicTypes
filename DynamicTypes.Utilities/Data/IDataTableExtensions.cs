using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTypes.Utilities.Data
{
    public static class IDataTableExtensions
    {

        public static TypeGenerator ToTypeGenerator(this DataTable table)
        {
            PropertyGenerator[] columns = new PropertyGenerator[table.Columns.Count];
            DataColumn column = null;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                column = table.Columns[i];
                columns[i] = new PropertyGenerator(column.ColumnName, column.DataType);
            }

            TypeGenerator tg = new TypeGenerator(string.IsNullOrEmpty(table.TableName) ? "DynamicTypes.DataTableObject" : table.TableName) 
            { 
                Members = columns.ToList<MemberGenerator>(),
                EnshureUniqueName  = !string.IsNullOrEmpty(table.TableName)
            };


            return tg;
        }

        public static IEnumerable<object> ToObject(this DataTable table)
        {
            var tg = table.ToTypeGenerator();
            tg.Compile();

            PropertyGenerator[] columns = tg.Members.OfType<PropertyGenerator>().ToArray();

            object instance = null;
            foreach (var item in table.AsEnumerable())
            {
                instance = tg.CreateInstance();

                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i].SetValue(instance, item[i]);
                }

                yield return instance;
            }
        }
    }
}
