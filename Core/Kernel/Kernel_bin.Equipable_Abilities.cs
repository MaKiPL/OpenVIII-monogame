namespace FF8
{
    public partial class Kernel_bin
    {

#region Classes
        /// <summary>
        /// Equipable Abilities that don't go in the 3 command slots.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
        public abstract class Equipable_Ability : Ability
        {
        }

        #endregion Classes
    }
}