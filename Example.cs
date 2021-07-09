using DynamicTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            new example();
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class MyAttribute : Attribute
    {
        readonly string positionalString;

        public MyAttribute(string positionalString, string addition = "test")
        {
            this.positionalString = positionalString + addition;
        }

        public string PositionalString
        {
            get { return positionalString; }
        }
    }

    public interface MyInterface
    {
        string Property1 { get; set; }
    }

    internal class example
    {
        public example()
        {
            PropetyGenerator innerProperty = null;


            //Create a TypeGenerator for the inner Type
            var generator1 = new TypeGenerator("InnerType")
            {
                Members =  {
                    new PropetyGenerator("Property1", typeof(string)),
                },
            };       

            //Create second Type Generator
            var generator2 = new TypeGenerator
            {
                InterfaceImplementations = { typeof(MyInterface) },
                //add DebuggerDisplayAttribute
                Attributes = { new AttributeGenerator(typeof(DebuggerDisplayAttribute), "Generated - P1:{Property1}") },
                //define members
                Members =
                {

                    new PropetyGenerator("Property1", typeof(string)) { OverrideDefinition = typeof(MyInterface) },
                    //Create the Property Containing the type of generator1
                     (innerProperty = new PropetyGenerator("Type1", generator1.Compile())
                        {
                            Attributes = { new AttributeGenerator(typeof(MyAttribute), "DefaultValue") }
                        }),
                },
            };
            generator2.Compile();

            var instance = Activator.CreateInstance(generator2.Type);

            innerProperty.Property.SetValue(instance, Activator.CreateInstance(generator1.Type));
            generator2.Type.GetProperty("Property1").SetValue(instance, "TestValue");
            generator1.Type.GetProperty("Property1").SetValue(innerProperty.Property.GetValue(instance), "Also a TestValue");

            var test = (instance as MyInterface).Property1;
        }
    }
}
