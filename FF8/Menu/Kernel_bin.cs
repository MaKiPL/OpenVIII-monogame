using System;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {

        private ArchiveWorker aw;
        private readonly string ArchiveString = Memory.Archives.A_MAIN;
        public static Magic_Data[] MagicData { get; private set; }//0
        public static Junctionable_GFs_Data[] JunctionableGFsData { get; private set; }//1
        public static Enemy_Attacks_Data[] EnemyAttacksData { get; private set; }//2
        public static Battle_Commands[] BattleCommands { get; private set; }//3
        public static Weapons_Data[] WeaponsData { get; private set; }//4

        public static Character_Stats[] CharacterStats { get; private set; }//6

        /// <summary>
        /// Read binary data from into structures and arrays
        /// </summary>
        /// <see cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Kernel.bin"/>
        public Kernel_bin()
        {
            aw = new ArchiveWorker(ArchiveString);
            byte[] buffer = aw.GetBinaryFile(Memory.Strings.Filenames[(int)Strings.FileID.KERNEL]);
            List<Loc> subPositions = Memory.Strings.Files[Strings.FileID.KERNEL].subPositions;


            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                //Battle commands
                BattleCommands = new Battle_Commands[Battle_Commands.count];
                ms.Seek(subPositions[Battle_Commands.id], SeekOrigin.Begin);
                for (int i = 0; i < Battle_Commands.count; i++)
                {
                    BattleCommands[i].Read(br);
                }
                //Magic data
                MagicData = new Magic_Data[Magic_Data.count];
                ms.Seek(subPositions[Magic_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Magic_Data.count; i++)
                {
                    MagicData[i].Read(br);
                }
                //Junctionable GFs data
                JunctionableGFsData = new Junctionable_GFs_Data[Junctionable_GFs_Data.count];
                ms.Seek(subPositions[Junctionable_GFs_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Junctionable_GFs_Data.count; i++)
                {
                    JunctionableGFsData[i].Read(br);
                }

                //Enemy Attacks data
                EnemyAttacksData = new Enemy_Attacks_Data[Enemy_Attacks_Data.count];
                ms.Seek(subPositions[Enemy_Attacks_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Enemy_Attacks_Data.count; i++)
                {
                    EnemyAttacksData[i].Read(br);
                }

                //Weapons Data
                WeaponsData = new Weapons_Data[Weapons_Data.count];
                ms.Seek(subPositions[Weapons_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Weapons_Data.count; i++)
                {
                    WeaponsData[i].Read(br,i);
                }

                //Characters                
                CharacterStats = new Character_Stats[Character_Stats.count];
                ms.Seek(subPositions[Character_Stats.id], SeekOrigin.Begin);
                for (int i = 0; i < Character_Stats.count; i++)
                {
                    CharacterStats[i].Read(br);
                }
            }
        }

        public class Weapons_Data
        {
            public const int count =33;
            public const int id=4;
            public FF8String Name { get; private set; }
            public override string ToString() => Name;

            //0x0000	2 bytes Offset to weapon name
            //0x0002	1 byte Renzokuken finishers

            //0x01 = Rough Divide
            //0x02 = Fated Circle
            //0x04 = Blasting Zone
            //0x08 = Lion Heart
            //0x0003	1 byte Unknown
            public Saves.Characters Character;//0x0004	1 byte Character ID

            //0x0005	1 bytes Attack Type
            //0x0006	1 byte Attack Power
            //0x0007	1 byte Attack Parameter
            //0x0008	1 byte STR Bonus
            //0x0009	1 byte Weapon Tier
            //0x000A	1 byte Crit Bonus
            //0x000B	1 byte Melee Weapon?

            internal void Read(BinaryReader br,int string_id = 0)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, string_id);
                br.BaseStream.Seek(2, SeekOrigin.Current);
            }
        }
    }
}