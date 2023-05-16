using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    public partial class COMWrapper
    {
        #region Inline Types
        /// <summary>
        /// Implements a Finalize Method (Calls  Dispose())
        /// </summary>
        private class FinalizeDispose : MemberGenerator
        {

            #region Properties

            public IDisposableGenerator Dispose { get; }

            public MethodBuilder MethodBuilder { get; private set; }

            #endregion

            #region Constructors

            public FinalizeDispose(IDisposableGenerator dispose) : base(null)
            {
                Dispose = dispose;
            }

            #endregion

            #region Methods

            public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
            {
                MethodBuilder = tb.DefineMethod(nameof(IDisposable.Dispose), MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, null, Type.EmptyTypes);
                var IL = MethodBuilder.GetILGenerator();

                IL.BeginExceptionBlock();

                IL.Emit(OpCodes.Nop);
                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Call, Dispose.MethodBuilder);
                IL.Emit(OpCodes.Nop);

                IL.BeginFinallyBlock();

                IL.Emit(OpCodes.Ldarg_0);
                IL.Emit(OpCodes.Call, Dispose.MethodBuilder);
                IL.Emit(OpCodes.Nop);

                IL.EndExceptionBlock();

                Defined = true;
            }

            #endregion

        }

        #endregion

    }
}
