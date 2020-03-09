namespace OpenVIII
{
    namespace Kernel
    {
<<<<<<< Updated upstream:Core/Kernel/Kernel_bin.Equipable_Abilities.cs
        #region Classes

=======
>>>>>>> Stashed changes:Core/Kernel/IEquippableAbility.cs
        /// <summary>
        /// Equippable Abilities that don't go in the 3 command slots.
        /// </summary>
        /// <remarks>I am using this to group up the abilities</remarks>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
<<<<<<< Updated upstream:Core/Kernel/Kernel_bin.Equipable_Abilities.cs
        public abstract class EquippableAbility : Ability
=======
        public interface IEquippableAbility : IAbility
>>>>>>> Stashed changes:Core/Kernel/IEquippableAbility.cs
        {
            #region Constructors

            protected EquippableAbility(FF8String name, FF8String description, byte ap, Icons.ID icon) : base(name, description, ap, icon)
            {
            }

            #endregion Constructors
        }
    }
}
