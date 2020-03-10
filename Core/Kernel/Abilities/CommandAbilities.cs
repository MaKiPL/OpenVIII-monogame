using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII.Kernel
{
    /// <summary>
    /// Command Abilities
    /// </summary>
    /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-abilities"/>
    public sealed class CommandAbilities : IAbility
    {
        #region Fields

        /// <summary>
        /// Section Count
        /// </summary>
        public const int Count = 19;

        /// <summary>
        /// Section ID
        /// </summary>
        public const int ID = 12;

        #endregion Fields

        #region Constructors

        private CommandAbilities(BinaryReader br, int i, BattleCommand battleCommand)
        {
            //set a reference to it.
            BattleCommand = battleCommand;
            //0x0000	2 bytes Offset to name
            Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
            //0x0002	2 bytes Offset to description
            Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
            //0x0004  1 byte
            AP = br.ReadByte();
            //0x0005  1 byte
            Index = br.ReadByte();
            //0x0006  2 bytes
            Unknown0 = br.ReadBytes(2);
        }

        #endregion Constructors

        #region Properties

        public byte AP { get; }

        /// <summary>
        /// Battle Command related to this Command Abilities
        /// </summary>
        public BattleCommand BattleCommand { get; }

        public FF8String Description { get; }

        public Icons.ID Icon { get; } = Icons.ID.Ability_Command;

        /// <summary>
        /// Index to Battle commands
        /// </summary>
        public byte Index { get; }

        public FF8String Name { get; }

        public byte Palette { get; } = Ability.DefaultPalette;

        /// <summary>
        /// Unknown Bytes
        /// </summary>
        public byte[] Unknown0 { get; }

        /// <summary>
        /// Convert Command to Battle
        /// </summary>
        private static IReadOnlyDictionary<int, int> Convert { get; } = new Dictionary<int, int>
            {
                {0,2},
                {1,3},
                {2,6},
                {3,4},
                {4,0},
                {5,29},
                {6,30},
                {7,24},
                {8,25},
                {9,23},
                {10,28},
                {11,26},
                {12,32},
                {13,27},
                {14,33},
                {15,34},
                {16,31},
                {17,7},
                {18,38},
            };

        #endregion Properties

        #region Methods

        public static IReadOnlyDictionary<Abilities, CommandAbilities> Read(BinaryReader br,
                IEnumerable<BattleCommand> battleCommands)
        {
            Debug.Assert(Convert.Count == Count);
            return Convert
                .ToDictionary(i => (Abilities)(i.Key + (int)Abilities.Magic),
                    i => CreateInstance(br, i.Key, battleCommands.ElementAtOrDefault(i.Value)));
        }

        private static CommandAbilities CreateInstance(BinaryReader br, int i, BattleCommand battleCommand)
        => new CommandAbilities(br, i, battleCommand);

        #endregion Methods
    }
}