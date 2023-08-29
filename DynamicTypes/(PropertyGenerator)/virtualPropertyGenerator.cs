using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// Generate an oberride for a virtual/abstract Property & calls base.
    /// </summary>
    public class virtualPropertyGenerator : PropertyGeneratorBase
    {
        public PropertyInfo TargetProperty { get; }

        public virtualPropertyGenerator(Type T, string name) : base(name, null)
        {
            OverrideDefinition = T;
            TargetProperty = T.GetProperty(name);

            Type = TargetProperty.PropertyType;

            Get = TargetProperty.GetMethod != null;
            Set = TargetProperty.SetMethod != null;
        }

        public override Action<ILGenerator> GenerateGetMethod
        {
            get => (ILGenerator il) =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, TargetProperty.GetMethod);
                il.Emit(OpCodes.Ret);
            }; set => base.GenerateGetMethod=value;
        }

        public override Action<ILGenerator> GenerateSetMethod
        {
            get => (ILGenerator il) =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, TargetProperty.SetMethod);
                il.Emit(OpCodes.Ret);
            }; set => base.GenerateSetMethod=value;
        }
    }
}
