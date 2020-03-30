using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Command Abilities
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-abilities"/>
        public sealed class CommandAbilities : Ability
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 19;

            /// <summary>
            /// Icon for this type.
            /// </summary>
            public new const Icons.ID Icon = Icons.ID.Ability_Command;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 12;

            #endregion Fields

            #region Constructors

            private CommandAbilities(FF8String name, FF8String description, byte ap, BattleCommand battleCommand, byte index, byte[] unknown0) :
                base(name, description, ap, Icon) => (BattleCommand,Index,Unknown0) = (battleCommand,index,unknown0);
            

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Battle Command related to this Command Abilities
            /// </summary>
            public BattleCommand BattleCommand { get; }

            /// <summary>
            /// Index to Battle commands
            /// </summary>
            public byte Index { get; }

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

            public static IReadOnlyDictionary<Abilities, CommandAbilities> Read(BinaryReader br)

                => Enumerable.Range(0, Count)
                    .ToDictionary(i => (Abilities)(i + (int)Abilities.Magic), i => CreateInstance(br, i));

            private static CommandAbilities CreateInstance(BinaryReader br, int i)
            {
                //Get related BattleCommand and set a reference to it.
                BattleCommand battleCommand = null;
                if (Memory.Kernel_Bin.BattleCommands != null && Convert.TryGetValue(i, out int convertIndex) && Memory.Kernel_Bin.BattleCommands.Count > convertIndex)
                    battleCommand = Memory.Kernel_Bin.BattleCommands[convertIndex];
                //0x0000	2 bytes Offset to name
                FF8StringReference name = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2);
                //0x0002	2 bytes Offset to description
                FF8StringReference description = Memory.Strings.Read(Strings.FileID.KERNEL, ID, i * 2 + 1);
                //0x0004  1 byte
                byte ap = br.ReadByte();
                //0x0005  1 byte
                byte index = br.ReadByte();
                //0x0006  2 bytes
                byte[] unknown0 = br.ReadBytes(2);
                return new CommandAbilities(name, description, ap, battleCommand, index, unknown0);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}