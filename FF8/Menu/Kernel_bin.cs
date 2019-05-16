using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        private ArchiveWorker aw;
        private readonly string ArchiveString = Memory.Archives.A_MAIN;
        internal static Magic_Data[] MagicData { get; private set; }//0
        internal static Dictionary<Saves.GFs, Junctionable_GFs_Data> JunctionableGFsData { get; private set; }//1
        internal static Enemy_Attacks_Data[] EnemyAttacksData { get; private set; }//2
        internal static Battle_Commands[] BattleCommands { get; private set; }//3
        internal static Weapons_Data[] WeaponsData { get; private set; }//4
        internal static Dictionary<Renzokeken_Level, Renzokuken_Finishers_Data> RenzokukenFinishersData; //5
        internal static Dictionary<Saves.Characters, Character_Stats> CharacterStats { get; private set; }//6
        internal static Battle_Items_Data[] BattleItemsData { get; private set; }//7
        internal static Non_battle_Items_Data[] NonbattleItemsData { get; private set; } //8 //only strings
        internal static Non_Junctionable_GFs_Attacks_Data[] NonJunctionableGFsAttacksData { get; private set; } //9
        internal static Dictionary<Command_ability, Command_ability_data> Commandabilitydata { get; private set; }//10
        internal static Junction_abilities[] Junctionabilities { get; private set; }//11
        internal static Command_abilities[] Commandabilities { get; private set; }//12
        internal static Stat_percent_abilities[] Statpercentabilities { get; private set; }//13
        internal static Character_abilities[] Characterabilities { get; private set; }//14
        internal static Party_abilities[] Partyabilities { get; private set; }//15
        internal static GF_abilities[] GFabilities { get; private set; }//16
        internal static Menu_abilities[] Menuabilities { get; private set; }//17
        internal static Temporary_character_limit_breaks[] Temporarycharacterlimitbreaks { get; private set; }//18
        internal static Blue_magic_Quistis_limit_break[] BluemagicQuistislimitbreak { get; private set; }//19
        internal static Quistis_limit_break_parameters[] Quistislimitbreakparameters { get; private set; }//20
        internal static Shot_Irvine_limit_break[] ShotIrvinelimitbreak { get; private set; }//21
        internal static Duel_Zell_limit_break[] DuelZelllimitbreak { get; private set; }//22
        internal static Zell_limit_break_parameters[] Zelllimitbreakparameters { get; private set; }//23
        internal static Rinoa_limit_breaks_part_1[] Rinoalimitbreakspart1 { get; private set; }//24
        internal static Rinoa_limit_breaks_part_2[] Rinoalimitbreakspart2 { get; private set; }//25
        internal static Slot_array[] Slotarray { get; private set; }//26
        internal static Selphie_limit_break_sets[] Selphielimitbreaksets { get; private set; }//27
        internal static Devour[] Devour_ { get; private set; }//28
        internal static Misc_section[] Miscsection { get; private set; }//29
        internal static Misc_text_pointers[] Misctextpointers { get; private set; }//30


        /// <summary>
        /// Read binary data from into structures and arrays
        /// </summary>
        /// <see cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Kernel.bin"/>
        internal Kernel_bin()
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
                    BattleCommands[i].Read(br, i);
                }
                //Magic data
                MagicData = new Magic_Data[Magic_Data.count];
                ms.Seek(subPositions[Magic_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Magic_Data.count; i++)
                {
                    MagicData[i].Read(br, i);
                }
                //Junctionable GFs data
                JunctionableGFsData = new Dictionary<Saves.GFs, Junctionable_GFs_Data>(Junctionable_GFs_Data.count);
                ms.Seek(subPositions[Junctionable_GFs_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Junctionable_GFs_Data.count; i++)
                {
                    Junctionable_GFs_Data tmp = new Junctionable_GFs_Data();
                    tmp.Read(br, i);
                    JunctionableGFsData.Add((Saves.GFs)i, tmp);
                }

                //Enemy Attacks data
                EnemyAttacksData = new Enemy_Attacks_Data[Enemy_Attacks_Data.count];
                ms.Seek(subPositions[Enemy_Attacks_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Enemy_Attacks_Data.count; i++)
                {
                    EnemyAttacksData[i].Read(br, i);
                }

                //Weapons Data
                WeaponsData = new Weapons_Data[Weapons_Data.count];
                ms.Seek(subPositions[Weapons_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Weapons_Data.count; i++)
                {
                    WeaponsData[i].Read(br, i);
                }

                //Renzokuken Finishers Data
                RenzokukenFinishersData = new Dictionary<Renzokeken_Level, Renzokuken_Finishers_Data>(Renzokuken_Finishers_Data.count);
                ms.Seek(subPositions[Renzokuken_Finishers_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Renzokuken_Finishers_Data.count; i++)
                {
                    Renzokuken_Finishers_Data tmp = new Renzokuken_Finishers_Data();
                    tmp.Read(br, i);
                    RenzokukenFinishersData.Add((Renzokeken_Level)i, tmp);
                }

                //Characters
                CharacterStats = new Dictionary<Saves.Characters, Character_Stats>(Character_Stats.count);
                ms.Seek(subPositions[Character_Stats.id], SeekOrigin.Begin);
                for (int i = 0; i < Character_Stats.count; i++)
                {
                    Character_Stats tmp = new Character_Stats();
                    tmp.Read(br, (Saves.Characters)i);
                    CharacterStats.Add((Saves.Characters)i, tmp);
                }

                //Battle_Items_Data
                BattleItemsData = new Battle_Items_Data[Battle_Items_Data.count];
                ms.Seek(subPositions[Battle_Items_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Battle_Items_Data.count; i++)
                {
                    BattleItemsData[i].Read(br, i);
                }


                NonbattleItemsData = new Non_battle_Items_Data[Non_battle_Items_Data.count];//8 //only strings
                //ms.Seek(subPositions[Non_battle_Items_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Non_battle_Items_Data.count; i++)
                {
                    NonbattleItemsData[i].Read(br, i);
                }

                //Non-Junctionable GFs Attacks Data
                NonJunctionableGFsAttacksData = new Non_Junctionable_GFs_Attacks_Data[Non_Junctionable_GFs_Attacks_Data.count];
                ms.Seek(subPositions[Non_Junctionable_GFs_Attacks_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Non_Junctionable_GFs_Attacks_Data.count; i++)
                {
                    NonJunctionableGFsAttacksData[i].Read(br, i);
                }
            }
        }
        /// <summary>
        /// Non battle Items Mame and Description Offsets Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Non-battle-item-name-and-description-offsets"/>
        internal struct Non_battle_Items_Data
        {
            internal static readonly int count = 166;
            internal static readonly int id = 8;

            public override string ToString() => Name;

            internal FF8String Name { get; private set; }

            //0x0000	2 bytes Offset to item name
            internal FF8String Description { get; private set; }

            //0x0002	2 bytes Offset to item description

            internal void Read(BinaryReader br, int i)
            {

                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to item name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to item description
                //br.BaseStream.Seek(4,SeekOrigin.Current);
            }
        }
        /// <summary>
        /// Command Ability
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-ability-data"/>
        internal enum Command_ability
        {
            Recover,
            Revive,
            Treatment,
            Mad_Rush,
            Doom,
            Absorb,
            LV_Down,
            LV_Up,
            Kamikaze,
            Devour,
            Card,
            Defend,
        }
        /// <summary>
        /// Command Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-ability-data"/>
        internal class Command_ability_data
        {
            internal const int count = 12;
            internal const int id = 10;

            public Magic_ID MagicID { get; private set; }
            public byte[] Unknown0 { get; private set; }
            public Attack_Type Attack_Type { get; private set; }
            public byte Attack_Power { get; private set; }
            public Attack_Flags Attack_Flags { get; private set; }
            public byte Hit_Count { get; private set; }
            public Element Element { get; private set; }
            public byte Status_Attack { get; private set; }
            public Statuses0 Statuses0 { get; private set; }
            public Statuses1 Statuses1 { get; private set; }

            internal void Read(BinaryReader br, int i)
            {

                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0000  2 bytes Magic ID
                Unknown0 = br.ReadBytes(2);
                //0x0002  2 bytes Unknown
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0004  1 byte Attack type
                Attack_Power = br.ReadByte();
                //0x0005  1 byte Attack power
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x0006  1 byte Attack flags
                Hit_Count = br.ReadByte();
                //0x0007  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x0008  1 byte Element
                Status_Attack = br.ReadByte();
                //0x0009  1 byte Status attack enabler
                Statuses0 = (Statuses0)br.ReadUInt16();
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x000A  6 bytes Statuses
            }
        }

        /// <summary>
        /// Junction Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junction-abilities"/>
        internal class Junction_abilities
        {
            internal const int count = 20;
            internal const int id = 11;
            public override string ToString() => Name;
            internal FF8String Name { get; private set; }
            internal FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public BitArray J_Flags { get; private set; }

            internal void Read(BinaryReader br, int i)
            {

                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4,SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                J_Flags = new BitArray(br.ReadBytes(3));
                //0x0005  3 byte J-Flag
            }
        }
        /// <summary>
        /// Command Abilities
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-abilities"/>
        internal class Command_abilities
        {
            internal const int count = 19;
            internal const int id = 12;
            public override string ToString() => Name;
            internal FF8String Name { get; private set; }
            internal FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public byte Index { get; private set; }
            public byte[] Unknown0 { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
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
        }
        /// <summary>
        /// Stat Percentage Increasing Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        internal class Stat_percent_abilities
        {
            internal const int count = 19;
            internal const int id = 13;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public Stat Stat { get; private set; }
            public byte Value { get; private set; }
            public byte Unknown0 { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                AP = br.ReadByte();
                //0x0004  1 byte AP needed to learn the ability
                Stat =(Stat) br.ReadByte();
                //0x0005  1 byte Stat to increase
                Value= br.ReadByte();
                //0x0006  1 byte Increase value
                Unknown0 = br.ReadByte();
                //0x0007  1 byte Unknown/ Unused
            }
        }
        /// <summary>
        /// Characters Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        internal class Character_abilities
        {
            internal const int count = 20;
            internal const int id = 14;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public BitArray Flags { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                Flags = new BitArray(br.ReadBytes(3));
                //0x0005  3 byte Flags
            }
        }
        /// <summary>
        /// Party Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Party-abilities"/>
        internal class Party_abilities
        {
            internal const int count = 5;
            internal const int id = 15;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public BitArray Flags { get; private set; }
            public byte[] Unknown0 { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                Flags = new BitArray(br.ReadBytes(1));
                //0x0005  1 byte Flags
                Unknown0 = br.ReadBytes(2);
                //0x0006  2 byte Unknown/ Unused
            }
        }
        internal enum Stat
        {
            HP,
            STR,
            VIT,
            MAG,
            SPR,
            SPD,
            EVA,
            HIT,
            LUCK
        }
        /// <summary>
        /// GF Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
        internal class GF_abilities
        {
            internal const int count = 9;
            internal const int id = 16;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public byte Boost { get; private set; }
            public Stat Stat { get; private set; }
            public byte Value { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                AP = br.ReadByte();
                //0x0004 1 byte AP needed to learn the ability
                Boost = br.ReadByte();
                //0x0005 Enable Boost
                Stat = (Stat)br.ReadByte();
                //0x0006  1 byte Stat to increase
                Value = br.ReadByte();
                //0x0007  1 byte Increase value
            }
        }
        /// <summary>
        /// Menu Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Menu-abilities"/>
        internal class Menu_abilities
        {
            internal const int count = 24;
            internal const int id = 17;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public byte Index { get; private set; }
            public byte Start { get; private set; }
            public byte End { get; private set; }

            internal void Read(BinaryReader br, int i)
            {

                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                Index = br.ReadByte();
                //0x0005  1 byte Index to m00X files in menu.fs
                //(first 3 sections are treated as special cases)
                Start = br.ReadByte();
                //0x0006  1 byte Start offset
                End = br.ReadByte();
                //0x0007  1 byte End offset
            }
        }
        /// <summary>
        /// Temporary Characters Limit Breaks
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Temporary-character-limit-breaks"/>
        internal class Temporary_character_limit_breaks
        {
            internal const int count = 5;
            internal const int id = 18;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public Magic_ID MagicID { get; private set; }
            public Attack_Type Attack_Type { get; private set; }
            public byte Attack_Power { get; private set; }
            public byte[] Unknown0 { get; private set; }
            public Target Target { get; private set; }
            public Attack_Flags Attack_Flags { get; private set; }
            public byte Hit_Count { get; private set; }
            public Element Element { get; private set; }
            public byte Element_Percent { get; private set; }
            public byte Status_Attack { get; private set; }
            public Statuses0 Statuses0 { get; private set; }
            public byte[] Unknown1 { get; private set; }
            public Statuses1 Statuses1 { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0006  1 byte Attack Type
                Attack_Power = br.ReadByte();
                //0x0007  1 byte Attack Power
                Unknown0 = br.ReadBytes(2);
                //0x0008  2 bytes Unknown
                Target = (Target)br.ReadByte();
                //0x000A  1 byte Target Info
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x000B  1 byte Attack Flags
                Hit_Count = br.ReadByte();
                //0x000C  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x000D  1 byte Element Attack
                Element_Percent = br.ReadByte();
                //0x000E  1 byte Element Attack %
                Status_Attack = br.ReadByte();
                //0x000F  1 byte Status Attack Enabler
                Statuses0 = (Statuses0)br.ReadUInt16();
                //0x0010  2 bytes status_0; //statuses 0-7
                Unknown1 = br.ReadBytes(2);
                //0x0012  2 bytes Unknown
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x0014  4 bytes status_1; //statuses 8-39
            }
        }
        /// <summary>
        /// Blue magic (Quistis limit break)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Blue-magic-%28Quistis-limit-break%29"/>
        internal class Blue_magic_Quistis_limit_break
        {
            internal const int count = 16;
            internal const int id = 19;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public Magic_ID MagicID { get; private set; }
            public byte Unknown0 { get; private set; }
            public Attack_Type Attack_Type { get; private set; }
            public byte[] Unknown1 { get; private set; }
            public Attack_Flags Attack_Flags { get; private set; }
            public byte Unknown2 { get; private set; }
            public Element Element { get; private set; }
            public byte Status_Attack { get; private set; }
            public byte Crit { get; private set; }
            public byte Unknown3 { get; private set; }
            public Quistis_limit_break_parameters[] Crisis_Levels { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                Unknown0 = br.ReadByte();
                //0x0006  1 byte Unknown
                Attack_Type = (Attack_Type) br.ReadByte();
                //0x0007  1 byte Attack Type
                Unknown1 = br.ReadBytes(2);
                //0x0008  2 bytes Unknown
                Attack_Flags = (Attack_Flags) br.ReadByte();
                //0x000A  1 byte Attack Flags
                Unknown2 = br.ReadByte();
                //0x000B  1 byte Unknown
                Element = (Element) br.ReadByte();
                //0x000C  1 byte Element
                Status_Attack = br.ReadByte();
                //0x000D  1 byte Status Attack
                Crit = br.ReadByte();
                //0x000E  1 byte Crit Bonus
                Unknown3 = br.ReadByte();
                //0x000F  1 byte Unknown
                var current = br.BaseStream.Position;

                br.BaseStream.Seek(Memory.Strings.Files[Strings.FileID.KERNEL].subPositions[Quistis_limit_break_parameters.id + Quistis_limit_break_parameters.size * i], SeekOrigin.Begin);
                Crisis_Levels = new Quistis_limit_break_parameters[Quistis_limit_break_parameters.size];
                for(i=0; i< Quistis_limit_break_parameters.count; i++)
                    Crisis_Levels[i].Read(br,i);
                br.BaseStream.Seek(current, SeekOrigin.Begin);
            }
        }
        /// <summary>
        /// Blue Magic Parameters - 4 for each spell for crisis level.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Blue_Magic_(Final_Fantasy_VIII)"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Quistis-limit-break-parameters"/>
        internal struct Quistis_limit_break_parameters
        {
            internal const int count = 4;//64 total but I want to add these to the Blue_magic_Quistis_limit_break in an array
            internal const int id = 20;
            internal const int size = 8;

            public Statuses1 Statuses1 { get; private set; }
            public Statuses0 Statuses0 { get; private set; }
            public byte Attack_Power { get; private set; }
            public byte Attack_Param { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x0000  4 bytes Status 1
                Statuses0 = (Statuses0)br.ReadUInt16();
                //0x0004  2 bytes Status 0
                Attack_Power = br.ReadByte();
                //0x0006  1 bytes Attack Power
                Attack_Param = br.ReadByte();
                //0x0007  1 byte Attack Param
            }
        }

        internal class Shot_Irvine_limit_break
        {
            internal const int count = 12;
            internal const int id = 21;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Duel_Zell_limit_break
        {
            internal const int count = 12;
            internal const int id = 22;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Zell_limit_break_parameters
        {
            internal const int count = 12;
            internal const int id = 23;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Rinoa_limit_breaks_part_1
        {
            internal const int count = 12;
            internal const int id = 24;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Rinoa_limit_breaks_part_2
        {
            internal const int count = 12;
            internal const int id = 25;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Slot_array
        {
            internal const int count = 12;
            internal const int id = 26;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Selphie_limit_break_sets
        {
            internal const int count = 12;
            internal const int id = 27;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Devour
        {
            internal const int count = 12;
            internal const int id = 28;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Misc_section
        {
            internal const int count = 12;
            internal const int id = 29;
            internal void Read(BinaryReader br, int i)
            {
            }
        }

        internal class Misc_text_pointers
        {
            internal const int count = 12;
            internal const int id = 30;
            internal void Read(BinaryReader br, int i)
            {
            }
        }
    }
}