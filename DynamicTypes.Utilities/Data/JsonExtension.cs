using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DynamicTypes.Utilities.Data
{
    public static class JsonExtension
    {


        public static TypeGenerator ToTypeGenerator(this JsonElement rootElement, string name = "DynamicTypes.JsonObject")
        {
            TypeGenerator typeGenerator = new TypeGenerator(name);

            if (rootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in rootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        Type itemType = typeof(object).MakeArrayType();
                        if (property.Value.GetArrayLength() > 0)
                        {
                            //maybe collect the props of all rows.
                            var itemGen = property.Value[0].ToTypeGenerator();

                            itemType =  itemGen.Compile().MakeArrayType();
                        }

                        typeGenerator.Members.Add(new PropertyGenerator(property.Name, itemType));
                    }
                    else
                    {
                        var itemType = property.Value.ValueKind switch
                        {
                            JsonValueKind.Undefined => typeof(object),
                            JsonValueKind.Object => typeof(object),
                            JsonValueKind.Array => typeof(object),
                            JsonValueKind.String => typeof(string),
                            JsonValueKind.Number => typeof(decimal),
                            JsonValueKind.True => typeof(bool),
                            JsonValueKind.False => typeof(bool),
                            JsonValueKind.Null => typeof(object),
                            _ => typeof(object)
                        };


                        typeGenerator.Members.Add(new PropertyGenerator(property.Name, itemType));

                    }

                }
            }


            return typeGenerator;
        }

        public static TypeGenerator ToTypeGenerator(this string jsonString)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
            JsonElement rootElement = jsonDocument.RootElement;

            return rootElement.ToTypeGenerator();
        }

        public static object? ToObject(this string jsonString)
        {
            var tg = jsonString.ToTypeGenerator();
            tg.Compile();

            return JsonSerializer.Deserialize(JsonDocument.Parse(jsonString), tg.Type);

        }
    }
}
