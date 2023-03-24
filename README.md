# DynamicTypes

The DynamicTypes package is a useful tool for C# developers who want to work with dynamic types. This package provides a set of extension methods that make it easy to work with dynamic objects, such as parsing JSON or accessing properties dynamically.

One of the benefits of using DynamicTypes is that it eliminates the need for casting and type checking, which can save time and reduce code complexity. Additionally, this package can help with serialization and deserialization of dynamic objects, making it easier to work with data in different formats.

Overall, the DynamicTypes package is a valuable tool for any C# developer who wants to simplify their code and work more efficiently with dynamic types.

# Howto Use

## Example
```cs
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
```

# COMWrapper

The COMWrapper is an Example to see how to use this Libary.

1. The COM Wrapper class wrappers COM interface classes and Provides IDisposable and Finalize implementation.
2. The COM Resource will be freed in on Dispose 
