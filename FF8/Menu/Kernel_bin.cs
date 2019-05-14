using System;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {

        private ArchiveWorker aw;
        private string ArchiveString = Memory.Archives.A_MAIN;
        private Character_Stats[] CharacterStats;
        private Magic_Data[] MagicData;
        private Battle_Commands[] BattleCommands;

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
            int id; //6 is characters
            int count;

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                id = 0; //Battle commands
                count = 39;
                CharacterStats = new Character_Stats[count];
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    BattleCommands[i].Read(br);
                }

                id = 1; //Magic data
                count = 57;
                CharacterStats = new Character_Stats[count];
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    MagicData[i].Read(br);
                }

                id = 6; //Characters
                count = 11;
                CharacterStats = new Character_Stats[count];
                ms.Seek(subPositions[id], SeekOrigin.Begin);
                for (int i = 0; i < count; i++)
                {
                    CharacterStats[i].Read(br);
                }
            }
        }
        /// <summary>
        /// Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Magic-data"/>
        public struct Magic_Data
        {
            //0x0000	2 bytes Offset to spell name
            //0x0002	2 bytes Offset to spell description
            ushort MagicID;     //0x0004	2 bytes Magic ID
            byte Unknown;       //0x0006  1 byte  Unknown
            byte Attack_type;   //0x0007  1 byte  Attack type
            byte Spellpower;    //0x0008  1 byte  Spell power(used in damage formula)
            //0x0009  1 byte  Unknown
            //0x000A  1 byte  Default target
            //0x000B  1 byte  Attack Flags
            //0x000C  1 byte  Draw resist(how hard is the magic to draw)
            //0x000D  1 byte  Hit count(works with meteor animation, not sure about others)
            //0x000E  1 byte Element
            //0x000F  1 byte  Unknown
            //0x0010  4 bytes Statuses 1
            //0x0014  2 bytes Statuses 0
            //0x0016  1 byte  Status attack enabler
            //0x0017  1 byte  Characters HP junction value
            //0x0018  1 byte  Characters STR junction value
            //0x0019  1 byte  Characters VIT junction value
            //0x001A  1 byte  Characters MAG junction value
            //0x001B  1 byte  Characters SPR junction value
            //0x001C  1 byte  Characters SPD junction value
            //0x001D  1 byte  Characters EVA junction value
            //0x001E  1 byte  Characters HIT junction value
            //0x001F  1 byte  Characters LUCK junction value
            //0x0020  1 byte Characters J - Elem attack
            //0x0021  1 byte  Characters J - Elem attack value
            //0x0022  1 byte Characters J - Elem defense
            //0x0023  1 byte  Characters J - Elem defense value
            //0x0024  1 byte  Characters J - Status attack value
            //0x0025  1 byte  Characters J - Status defense value
            //0x0026  2 bytes Characters J - Statuses Attack
            //0x0028  2 bytes Characters J - Statuses Defend
            //0x002A  1 byte  Quezacolt compatibility
            //0x002B  1 byte  Shiva compatibility
            //0x002C  1 byte  Ifrit compatibility
            //0x002D  1 byte  Siren compatibility
            //0x002E  1 byte  Brothers compatibility
            //0x002F  1 byte  Diablos compatibility
            //0x0030  1 byte  Carbuncle compatibility
            //0x0031  1 byte  Leviathan compatibility
            //0x0032  1 byte  Pandemona compatibility
            //0x0033  1 byte  Cerberus compatibility
            //0x0034  1 byte  Alexander compatibility
            //0x0035  1 byte  Doomtrain compatibility
            //0x0036  1 byte  Bahamut compatibility
            //0x0037  1 byte  Cactuar compatibility
            //0x0038  1 byte  Tonberry compatibility
            //0x0039  1 byte  Eden compatibility
            //0x003A  2 bytes Unknown
            public void Read(BinaryReader br)
            {
                MagicID = br.ReadUInt16();
                Unknown = br.ReadByte();
                Attack_type = br.ReadByte();
                Spellpower = br.ReadByte();
            }
        }
    }
}

