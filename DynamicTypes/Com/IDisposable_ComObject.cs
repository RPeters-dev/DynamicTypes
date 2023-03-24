using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace DynamicTypes
{
    /// <summary>
    /// initializes a new instance of <see cref="IDisposableGenerator"/>
    /// </summary>
    /// <param name="target">the target for that<see cref="Marshal.FinalReleaseComObject"/> will be called</param>
    internal class IDisposable_ComObject : IDisposableGenerator
    {

        /// <summary>
        /// Initializes a new instance of <see cref="IDisposable_ComObject"/>
        /// </summary>
        /// <param name="childrenToDispose">Children that also will be Disposed</param>
        /// <param name="target">the target for that<see cref="Marshal.FinalReleaseComObject"/> will be called</param>
        public IDisposable_ComObject(IEnumerable<FieldGenerator> childrenToDispose, FieldBuilder target) : base(target, childrenToDispose)
        {
        }

        /// <inheritdoc/>
        public override void PostDispose(ILGenerator IL)
        {
            IL.Emit(OpCodes.Nop);
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldfld, Target);
            IL.Emit(OpCodes.Call, typeof(Marshal).GetMethod(nameof(Marshal.FinalReleaseComObject)));
            IL.Emit(OpCodes.Pop);
        }
    }
}