using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        #region Classes
        public abstract class Ability
        {
            #region Fields

            public const int count = 0;
            public const int id = 0;

            #endregion Fields 
            #region Properties

            public byte AP { get; protected set; }
            public FF8String Description { get; protected set; }
            public Icons.ID Icon { get; protected set; } = Icons.ID.None;
            public FF8String Name { get; protected set; }

            #endregion Properties

            #region Methods

            public abstract void Read(BinaryReader br, int i);

            public override string ToString() => Name;

            #endregion Methods
        }
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