using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Command Abilities
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-abilities"/>
        public class Command_abilities : Ability
        {
            public new const int count = 19;
            public new const int id = 12;

            public byte Index { get; private set; }
            public byte[] Unknown0 { get; private set; }
            public Battle_Commands BattleCommand { get; private set; }
            private IReadOnlyDictionary<int, int> convert { get; } = new Dictionary<int, int>
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
            public override void Read(BinaryReader br, int i)
            {
                Icon = Icons.ID.Ability_Command;
                if (convert.ContainsKey(i))
                    BattleCommand = BattleCommands[convert[i]];
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description

                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                Index = br.ReadByte();
                //0x0005  1 byte Index to Battle commands
                Unknown0 = br.ReadBytes(2);
                //0x0006  2 bytes Unknown/ Unused
            }
            public static Dictionary<Abilities,Command_abilities> Read(BinaryReader br)
            {
                Dictionary<Abilities, Command_abilities> ret = new Dictionary<Abilities, Command_abilities>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Command_abilities();
                    tmp.Read(br, i);
                    ret[(Abilities)(i+ (int)Abilities.Magic)] = tmp;
                }
                return ret;
            }
        }
    }
}