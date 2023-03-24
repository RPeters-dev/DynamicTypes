using System.ComponentModel;
using System.Diagnostics;

namespace DynamicTypes
{
    /// <summary>
    /// there is no use exept for looking at it so its hidden
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Example
    {
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        public class MyAttribute : Attribute
        {
            readonly string positionalString;

            public MyAttribute(string positionalString, string addition = "value")
            {
                this.positionalString = positionalString + addition;
            }

            public string PositionalString
            {
                get { return positionalString; }
            }
        }

        public interface IExample
        {
            string Property1 { get; set; }
        }


        public void example()
        {
            PropertyGenerator? innerProperty = null;

            //Create a TypeGenerator for the inner Type
            var generator1 = new TypeGenerator("InnerType")
            {
                InterfaceImplementations = { typeof(IExample) },
                Members =  {
                    new iPropertyGenerator<IExample>(nameof(IExample.Property1)),
                },
            };

            //Create second Type Generator
            var generator2 = new TypeGenerator
            {
                InterfaceImplementations = { typeof(IExample) },
                //add DebuggerDisplayAttribute
                Attributes = { new AttributeGenerator<DebuggerDisplayAttribute>("Generated - IExample.Property1:{Property1}") },
                //define members
                Members =
                {
                    new PropertyGenerator(nameof(IExample.Property1), typeof(string)) { OverrideDefinition = typeof(IExample) },
                    //Create the Property Containing the type of generator1
                     (innerProperty = new PropertyGenerator("Type1", generator1.Compile())
                        {
                            Attributes = { new AttributeGenerator<MyAttribute>("Default") }
                        }),
                },
            };
            generator2.Compile();

            var instance = generator2.CreateInstance();



            //set the value via PropertyGenerator extension
            innerProperty.SetValue(instance, generator1.CreateInstance());

            //set the value via interface
            ((IExample)instance).Property1 = "A value";

            //set the value a bit more complicated
            generator1.Type.GetProperty(nameof(IExample.Property1)).SetValue(innerProperty.GetValue(instance), "Also a value");
        }
    }
}
