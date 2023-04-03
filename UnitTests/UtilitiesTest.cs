using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicTypes.Utilities.Data;

namespace DynamicTypes.UnitTests
{
    public class UtilitiesTest
    {
        [Fact]
        public void DataTableTest()
        {
            int itemsCount = 5;

            var table = new DataTable();
            table.Clear();
            for (int i = 0; i < 12; ++i)
                table.Columns.Add("col" + i);
            for (int rowIndex = 0; rowIndex < itemsCount; ++rowIndex)
            {
                DataRow row = table.NewRow();
                for (int i = 0; i < table.Columns.Count; ++i)
                    row[i] = String.Format("row:{0},col:{1}", rowIndex, i);
                table.Rows.Add(row);
            }

            var items = table.ToObject().ToArray();


            Assert.Equal(itemsCount, items.Length);
            Assert.Equal("row:0,col:0", (items[0] as dynamic).col0);
        }

        [Fact]
        public void JsonTest()
        {
            string jsonString = "{ \"data\": [ { \"col1\": 1, \"col2\": \"value1\" }, { \"col1\": 2, \"col2\": \"value2\" } ] }";
            var item = jsonString.ToObject();


            Assert.Equal(1, (item as dynamic).data[0].col1);
        }
    }
}
