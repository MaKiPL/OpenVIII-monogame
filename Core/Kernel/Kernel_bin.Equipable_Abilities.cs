namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Equippable Abilities that don't go in the 3 command slots.
        /// </summary>
        /// <remarks>I am using this to group up the abilities</remarks>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
        public abstract class EquippableAbility : Ability
        {
            #region Constructors

            protected EquippableAbility(FF8String name, FF8String description, byte ap, Icons.ID icon) : base(name, description, ap, icon)
            {
            }

            #endregion Constructors
        }

        #endregion Classes
    }
}