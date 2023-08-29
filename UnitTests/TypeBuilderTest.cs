using System.Diagnostics;
using DynamicTypes.Utilities.Data;

namespace DynamicTypes.UnitTests
{
    public class TypeBuilderTest
    {

        [Fact]

        public void GeneratorTest()
        {
            var g = new TypeGenerator();

            var t = g.Compile();

            Assert.NotNull(t);
            Assert.NotNull(g.CreateInstance());
        }

        [Fact]
        public void EmptyMethodBuilderTest()
        {
            var g = new TypeGenerator
            {
                Members =  {
                new MethodGenerator("Test", typeof(void))
                }
            };

            var t = g.Compile();

            Assert.NotNull(t);

            dynamic instance = g.CreateInstance();
            Assert.NotNull(instance);

            instance.Test();

        }

        [Fact]
        public void MethodBuilderTest()
        {
            var g = new TypeGenerator
            {
                Members =
                {
                    new MethodGenerator("Test", typeof(int), null, (il) =>
                    {
                        il.Emit(OpCodes.Ldc_I4_8);
                        il.Emit(OpCodes.Ret);
                    })
                }
            };

            var t = g.Compile();

            Assert.NotNull(t);

            dynamic instance = g.CreateInstance();
            Assert.NotNull(instance);

            int val = instance.Test();

            Assert.True(val == 8);
        }


        [Fact]

        public void PropertyGeneratorTest()
        {
            var g = new TypeGenerator
            {
                Members =
                {
                  new PropertyGenerator1<TestClass>("testInstance"),

                }
            };
            var t = g.Compile();
            Assert.NotNull(t);
            dynamic instance = g.CreateInstance();
            Assert.NotNull(instance);

            instance.testInstance = TestClass.instance;

            Assert.Equal(TestClass.instance, instance.testInstance);
        }



        [Fact]

        public void IndexPropertyTest()
        {
            var ItemName = "Item";
            var g = new TypeGenerator
            {
                Members = {
                  new iPropertyGenerator<IPivotRowInternals>(ItemName)
                      {
                            UseSingleBackingField = false,
                            GenerateGetMethod =  (il) =>
                            {
                                il.Emit(OpCodes.Ldarg_1);
                                il.Emit(OpCodes.Ret);
                            },
                            GenerateSetMethod = (il) =>
                            {
                                il.Emit(OpCodes.Ret);
                            }
                      }

                },
                Attributes = {
                    new AttributeGenerator<DefaultMemberAttribute>(ItemName)
                }
            };
            var t = g.Compile();
            Assert.NotNull(t);

            dynamic instance = g.CreateInstance();
            Assert.NotNull(instance);


            var sttstst = (instance as IPivotRowInternals)[ItemName];

            Assert.Equal(sttstst, ItemName);
        }

        [Fact]

        public void InterfacePropertyGeneratorTest()
        {
            var g = new TypeGenerator
            {
                Members =
                {
                  new iPropertyGenerator<TestInterface>(nameof(TestInterface.P1)) { UseSingleBackingField = false },
                  new iPropertyGenerator<TestInterface2>(nameof(TestInterface2.P1)) { UseSingleBackingField = false },
                }
            };
            g.InterfaceImplementations.Add(typeof(TestInterface));
            g.InterfaceImplementations.Add(typeof(TestInterface2));

            var t = g.Compile();
            Assert.NotNull(t);
            TestInterface instance = (TestInterface)g.CreateInstance();
            TestInterface2 instance2 = (TestInterface2)instance;
            Assert.NotNull(instance);

            instance.P1 = 66;
            instance2.P1 = 88;

            Assert.NotEqual(instance.P1, instance2.P1);
        }

        [Fact]

        public void InterfacePropertyGeneratorTest2()
        {
            var g = new TypeGenerator
            {
                Members =
                {
                  new iPropertyGenerator<TestInterface>(nameof(TestInterface.P1)) ,
                  new iPropertyGenerator<TestInterface2>(nameof(TestInterface2.P1)) ,
                }
            };
            g.InterfaceImplementations.Add(typeof(TestInterface));
            g.InterfaceImplementations.Add(typeof(TestInterface2));

            var t = g.Compile();
            Assert.NotNull(t);
            TestInterface instance = (TestInterface)g.CreateInstance();
            TestInterface2 instance2 = (TestInterface2)instance;
            Assert.NotNull(instance);

            instance.P1 = 66;
            instance2.P1 = 88;

            Assert.Equal(instance.P1, instance2.P1);
        }


        [Fact]
        public void FieldGeneratorTest()
        {
            var g = new TypeGenerator
            {
                Members =
                {
                  new FieldGenerator<TestClass>("testInstance"){ FieldAttributes = FieldAttributes.Public },

                }
            };
            var t = g.Compile();
            Assert.NotNull(t);
            dynamic instance = g.CreateInstance();
            Assert.NotNull(instance);

            instance.testInstance = TestClass.instance;

            Assert.Equal(TestClass.instance, instance.testInstance);
        }



        [Fact]

        public void DetourMethodBuilderTest()
        {
            PropertyGenerator pg;
            var g = new TypeGenerator
            {
                Members =
                {
                   (pg =  new PropertyGenerator1<TestClass>("testInstance")),
                    new DetourMethodGenerator(pg.BackingField, typeof(TestClass).GetMethod(nameof(TestClass.testMethod)))
                }
            };
            var t = g.Compile();
            Assert.NotNull(t);
            dynamic? instance = g.CreateInstance();
            Assert.NotNull(instance);

            instance.testInstance = TestClass.instance;

            int val = instance.testMethod();

            Assert.True(val == 8);

        }

        public class TestClass
        {

            public static TestClass instance = new TestClass();
            public int testMethod()
            {
                return 8;
            }
        }

        public interface TestInterface
        {
            int P1 { get; set; }
        }

        public interface TestInterface2
        {
            int P1 { get; set; }
        }
    }
}
