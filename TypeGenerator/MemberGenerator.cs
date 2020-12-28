using System;
using System.Reflection.Emit;

namespace DynamicTypes
{
    /// <summary>
    /// BaseClasss for Member Generators, Can be used to define Member Groups or Single Members
    /// </summary>
    public abstract class MemberGenerator
    {

        #region Properties

        /// <summary>
        /// A type is always helpful for all Actions u Plan
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Some members have to be defined Early. Please use this to Check for Redundancies 
        /// </summary>
        public bool Defined { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="MemberGenerator"/>
        /// </summary>
        /// <param name="type">A type is always helpful for all Actions u Plan, can be null</param>
        protected MemberGenerator(Type type)
        {
            Type = type;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Defines a Member (Creates Field, Properties and Methods), please note, Definition of a member can be done anytime if necessary.
        /// </summary>
        /// <param name="tb"></param>
        public abstract void DefineMember(TypeBuilder tb);

        /// <summary>
        /// Will be when the Final Type is compiled
        /// </summary>
        /// <param name="cg"></param>
        public virtual void Compiled(TypeGenerator cg) { }

        #endregion

    }
}
