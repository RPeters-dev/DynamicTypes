using System.Reflection;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// An Default Assembly generator, it don't do much
    /// </summary>
    public class DefaultAssemblyGenerator : AssemblyGenerator
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultAssemblyGenerator"/>
        /// </summary>
        public DefaultAssemblyGenerator()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the Assembly
        /// </summary>
        /// <returns></returns>
        public override void Initialize()
        {
            if (AssemblyBuilder != null)
            {
                return;
            }

            AssemblyName aName = new AssemblyName(AssemblyName);
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect);

            Module = AssemblyBuilder.DefineDynamicModule(AssemblyBuilder.FullName + ".dll");
        }

        #endregion

    }
}
