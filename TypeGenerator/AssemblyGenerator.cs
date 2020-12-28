using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// BaseClass for Assembly Generators 
    /// </summary>
    public abstract class AssemblyGenerator
    {

        #region Properties

        /// <summary>
        /// Name of the Assembly
        /// </summary>
        public string AssemblyName { get; set; } = "System.DynamicAssembly";

        /// <summary>
        /// The Module used to Define Types
        /// </summary>
        public ModuleBuilder Module { get; protected set; }

        /// <summary>
        /// The Assembly builder
        /// </summary>
        public AssemblyBuilder AssemblyBuilder { get; protected set; }

        #endregion

        #region Constructors

        protected AssemblyGenerator()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the Assembly
        /// </summary>
        /// <returns></returns>
        public abstract void Initialize();

        #endregion

    }
}
