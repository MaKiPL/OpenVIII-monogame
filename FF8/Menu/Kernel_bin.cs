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
        internal static Misc_section[] Miscsection { get; private set; }//29 //only_strings
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
                    NonbattleItemsData[i].Read(i);
                }

                //Non-Junctionable GFs Attacks Data
                NonJunctionableGFsAttacksData = new Non_Junctionable_GFs_Attacks_Data[Non_Junctionable_GFs_Attacks_Data.count];
                ms.Seek(subPositions[Non_Junctionable_GFs_Attacks_Data.id], SeekOrigin.Begin);
                for (int i = 0; i < Non_Junctionable_GFs_Attacks_Data.count; i++)
                {
                    NonJunctionableGFsAttacksData[i].Read(br, i);
                }

                ms.Seek(subPositions[Command_ability_data.id], SeekOrigin.Begin);
                Commandabilitydata = Command_ability_data.Read(br);
                ms.Seek(subPositions[Junction_abilities.id], SeekOrigin.Begin);
                Junctionabilities = Junction_abilities.Read(br);
                ms.Seek(subPositions[Command_abilities.id], SeekOrigin.Begin);
                Commandabilities = Command_abilities.Read(br);
                ms.Seek(subPositions[Stat_percent_abilities.id], SeekOrigin.Begin);
                Statpercentabilities = Stat_percent_abilities.Read(br);
                ms.Seek(subPositions[Character_abilities.id], SeekOrigin.Begin);
                Characterabilities = Character_abilities.Read(br);
                ms.Seek(subPositions[Party_abilities.id], SeekOrigin.Begin);
                Partyabilities = Party_abilities.Read(br);
                ms.Seek(subPositions[GF_abilities.id], SeekOrigin.Begin);
                GFabilities = GF_abilities.Read(br);
                ms.Seek(subPositions[Menu_abilities.id], SeekOrigin.Begin);
                Menuabilities = Menu_abilities.Read(br);
                ms.Seek(subPositions[Temporary_character_limit_breaks.id], SeekOrigin.Begin);
                Temporarycharacterlimitbreaks = Temporary_character_limit_breaks.Read(br);
                ms.Seek(subPositions[Blue_magic_Quistis_limit_break.id], SeekOrigin.Begin);
                BluemagicQuistislimitbreak = Blue_magic_Quistis_limit_break.Read(br);
                ms.Seek(subPositions[Quistis_limit_break_parameters.id], SeekOrigin.Begin);
                //Quistislimitbreakparameters = Quistis_limit_break_parameters.Read(br);
                //ms.Seek(subPositions[Shot_Irvine_limit_break.id], SeekOrigin.Begin);
                ShotIrvinelimitbreak = Shot_Irvine_limit_break.Read(br);
                ms.Seek(subPositions[Duel_Zell_limit_break.id], SeekOrigin.Begin);
                DuelZelllimitbreak = Duel_Zell_limit_break.Read(br);
                ms.Seek(subPositions[Zell_limit_break_parameters.id], SeekOrigin.Begin);
                Zelllimitbreakparameters = Zell_limit_break_parameters.Read(br);
                ms.Seek(subPositions[Rinoa_limit_breaks_part_1.id], SeekOrigin.Begin);
                Rinoalimitbreakspart1 = Rinoa_limit_breaks_part_1.Read(br);
                ms.Seek(subPositions[Rinoa_limit_breaks_part_2.id], SeekOrigin.Begin);
                Rinoalimitbreakspart2 = Rinoa_limit_breaks_part_2.Read(br);
                ms.Seek(subPositions[Slot_array.id], SeekOrigin.Begin);
                Slotarray = Slot_array.Read(br);
                ms.Seek(subPositions[Selphie_limit_break_sets.id], SeekOrigin.Begin);
                Selphielimitbreaksets = Selphie_limit_break_sets.Read(br);
                ms.Seek(subPositions[Devour.id], SeekOrigin.Begin);
                Devour_ = Devour.Read(br);
                ms.Seek(subPositions[Misc_section.id], SeekOrigin.Begin);
                Miscsection = Misc_section.Read(br);
                ms.Seek(subPositions[Misc_text_pointers.id], SeekOrigin.Begin);
                Misctextpointers = Misc_text_pointers.Read();
            }
        }
    }
}