namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Equippable Abilities that don't go in the 3 command slots.
        /// </summary>
        /// <remarks>I am using this to group up the abilities</remarks>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>

        public interface IEquippableAbility : IAbility

        {
        }
    }
}
