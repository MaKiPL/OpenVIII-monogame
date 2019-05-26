namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Equipable Abilities that don't go in the 3 command slots.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
        public abstract class Equipable_Abilities
        {
            public const int count=0;
            public const int id=0;

            public override string ToString() => Name;

            public FF8String Name { get; protected set; }
            public FF8String Description { get; protected set; }
            public byte AP { get; protected set; }
        }
    }

}