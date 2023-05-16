using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace DynamicTypes
{

    /// <summary>
    /// Generates a Dispose method and a _disposed field (when disposing <see cref="IDisposable.Dispose"/> will be called for the source object
    /// </summary>
    public class IDisposableGenerator : MemberGenerator
    {
        #region Properties

        /// <summary>
        /// Contains a list of Fields that are Going to be Disposed
        /// </summary>
        public IEnumerable<FieldGenerator>? ChildrenToDispose { get; }

        /// <summary>
        /// the target for <see cref="Marshal.FinalReleaseComObject"/> will be called
        /// </summary>
        public FieldBuilder Target { get; }

        /// <summary>
        /// The Method builder for the dispose method
        /// </summary>
        public MethodBuilder? MethodBuilder { get; private set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// initializes a new instance of <see cref="IDisposableGenerator"/>
        /// </summary>
        /// <param name="target">the target for that<see cref="IDisposable.Dispose"/> will be called</param>
        public IDisposableGenerator(FieldBuilder target, IEnumerable<FieldGenerator>? childrenToDispose = null) : base(typeof(IDisposable))
        {
            Target = target;
            ChildrenToDispose = childrenToDispose;
        }

        #endregion Constructors

        #region Methods

        /// <inheritdoc/>
        public override void DefineMember(TypeBuilder tb, TypeGenerator tg)
        {
            var fgDisposed = new FieldGenerator("_disposed", typeof(bool));
            fgDisposed.DefineMember(tb, tg);

            MethodBuilder = tb.DefineMethod(nameof(IDisposable.Dispose), MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, null, Type.EmptyTypes);
            var IL = MethodBuilder.GetILGenerator();
            var l1 = IL.DeclareLocal(typeof(bool));
            var l2 = IL.DeclareLocal(typeof(bool));

            var lJump_end = IL.DefineLabel();
            var lJump_setDisposed = IL.DefineLabel();

            IL.DeclareLocal(typeof(bool));

            var _disposed = fgDisposed.internalField;

            IL.Emit(OpCodes.Nop);
            //if!disposed
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldfld, _disposed);
            IL.Emit(OpCodes.Ldc_I4, 0);
            IL.Emit(OpCodes.Ceq);
            IL.Emit(OpCodes.Stloc_0);
            IL.Emit(OpCodes.Ldloc_0);//IF disposed
            IL.Emit(OpCodes.Brfalse, lJump_end);

            PreDispose(IL);

            //if(Source != null)
            IL.Emit(OpCodes.Nop);
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldfld, Target);
            IL.Emit(OpCodes.Ldnull);
            IL.Emit(OpCodes.Cgt_Un);
            IL.Emit(OpCodes.Stloc_1);
            IL.Emit(OpCodes.Ldloc_1);
            IL.Emit(OpCodes.Brfalse, lJump_setDisposed);

            //release
            // Call Dispose for all Disposable Fields
            if (ChildrenToDispose != null)
            {
                foreach (var item in ChildrenToDispose)
                {
                    var lJump_endif = IL.DefineLabel();
                    var lJump_dispose = IL.DefineLabel();
                    //field?.dispose()

                    IL.Emit(OpCodes.Nop);
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(OpCodes.Ldfld, item.internalField);
                    IL.Emit(OpCodes.Dup);
                    IL.Emit(OpCodes.Brtrue, lJump_dispose);
                    IL.Emit(OpCodes.Pop);
                    IL.Emit(OpCodes.Br, lJump_endif);
                    IL.MarkLabel(lJump_dispose);
                    IL.Emit(OpCodes.Call, typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));
                    IL.Emit(OpCodes.Nop);
                    IL.MarkLabel(lJump_endif);
                    IL.Emit(OpCodes.Nop);
                }
            }

            PostDispose(IL);

            //Set _disposed to true
            IL.Emit(OpCodes.Nop);
            IL.MarkLabel(lJump_setDisposed);

            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ldc_I4, 1);
            IL.Emit(OpCodes.Stfld, _disposed);

            IL.MarkLabel(lJump_end);
            IL.Emit(OpCodes.Ret);

            tb.DefineMethodOverride(MethodBuilder, Type.GetMethod(nameof(IDisposable.Dispose)));

            Defined = true;
        }

        /// <summary>
        /// Method called after the Dispose of <see cref="ChildrenToDispose"/> &amp; <see cref="Target"/>
        /// </summary>
        /// <param name="IL">The IL Generator passed as parameter</param>
        public virtual void PostDispose(ILGenerator IL)
        {

        }

        /// <summary>
        /// Method called before the Dispose of <see cref="ChildrenToDispose"/> &amp; <see cref="Target"/>
        /// </summary>
        /// <param name="IL">The IL Generator passed as parameter</param>
        public virtual void PreDispose(ILGenerator IL)
        {

        }

        #endregion Methods
    }
}