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
        public static Dictionary<Saves.GFs, Junctionable_GFs_Data> JunctionableGFsData { get; private set; }//1
        public static Enemy_Attacks_Data[] EnemyAttacksData { get; private set; }//2
        public static Battle_Commands[] BattleCommands { get; private set; }//3
        public static Weapons_Data[] WeaponsData { get; private set; }//4
        public static Dictionary<Renzokeken_Level, Renzokuken_Finishers_Data> RenzokukenFinishersData; //5
        public static Dictionary<Saves.Characters, Character_Stats> CharacterStats { get; private set; }//6
        public static Battle_Items_Data[] BattleItemsData { get; private set; }//7
        public static Non_battle_Items_Data[] NonbattleItemsData { get; private set; } //8 //only strings
        public static Non_Junctionable_GFs_Attacks_Data[] NonJunctionableGFsAttacksData { get; private set; } //9
        public static Dictionary<Command_ability, Command_ability_data> Commandabilitydata { get; private set; }//10
        public static Dictionary<Abilities,Junction_abilities> Junctionabilities { get; private set; }//11
        public static Dictionary<Abilities, Command_abilities> Commandabilities { get; private set; }//12
        public static Dictionary<Abilities, Stat_percent_abilities> Statpercentabilities { get; private set; }//13
        public static Dictionary<Abilities, Character_abilities> Characterabilities { get; private set; }//14
        public static Dictionary<Abilities, Party_abilities> Partyabilities { get; private set; }//15
        public static Dictionary<Abilities, GF_abilities> GFabilities { get; private set; }//16
        public static Dictionary<Abilities, Menu_abilities> Menuabilities { get; private set; }//17
        public static Temporary_character_limit_breaks[] Temporarycharacterlimitbreaks { get; private set; }//18
        public static Blue_magic_Quistis_limit_break[] BluemagicQuistislimitbreak { get; private set; }//19
        //public static Quistis_limit_break_parameters[] Quistislimitbreakparameters { get; private set; }//20
        public static Shot_Irvine_limit_break[] ShotIrvinelimitbreak { get; private set; }//21
        public static Duel_Zell_limit_break[] DuelZelllimitbreak { get; private set; }//22
        public static Zell_limit_break_parameters[] Zelllimitbreakparameters { get; private set; }//23
        public static Rinoa_limit_breaks_part_1[] Rinoalimitbreakspart1 { get; private set; }//24
        public static Rinoa_limit_breaks_part_2[] Rinoalimitbreakspart2 { get; private set; }//25
        public static Slot_array[] Slotarray { get; private set; }//26
        public static Selphie_limit_break_sets[] Selphielimitbreaksets { get; private set; }//27
        public static Devour[] Devour_ { get; private set; }//28
        public static Misc_section[] Miscsection { get; private set; }//29 //only_strings
        public static Misc_text_pointers[] Misctextpointers { get; private set; }//30

        public static Dictionary<Abilities, Equipable_Ability> EquipableAbilities; // contains 4 types;

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
                BattleCommands = Battle_Commands.Read(br);
                ms.Seek(subPositions[Magic_Data.id], SeekOrigin.Begin);
                MagicData = Magic_Data.Read(br);
                ms.Seek(subPositions[Junctionable_GFs_Data.id], SeekOrigin.Begin);
                JunctionableGFsData = Junctionable_GFs_Data.Read(br);
                ms.Seek(subPositions[Enemy_Attacks_Data.id], SeekOrigin.Begin);
                EnemyAttacksData = Enemy_Attacks_Data.Read(br);
                ms.Seek(subPositions[Weapons_Data.id], SeekOrigin.Begin);
                WeaponsData = Weapons_Data.Read(br);
                ms.Seek(subPositions[Renzokuken_Finishers_Data.id], SeekOrigin.Begin);
                RenzokukenFinishersData = Renzokuken_Finishers_Data.Read(br);
                ms.Seek(subPositions[Character_Stats.id], SeekOrigin.Begin);
                CharacterStats = Character_Stats.Read(br);
                ms.Seek(subPositions[Battle_Items_Data.id], SeekOrigin.Begin);
                BattleItemsData = Battle_Items_Data.Read(br);
                NonbattleItemsData = Non_battle_Items_Data.Read();
                ms.Seek(subPositions[Non_Junctionable_GFs_Attacks_Data.id], SeekOrigin.Begin);
                NonJunctionableGFsAttacksData = Non_Junctionable_GFs_Attacks_Data.Read(br);
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
                //ms.Seek(subPositions[Quistis_limit_break_parameters.id], SeekOrigin.Begin);
                //Quistislimitbreakparameters = Quistis_limit_break_parameters.Read(br);
                ms.Seek(subPositions[Shot_Irvine_limit_break.id], SeekOrigin.Begin);
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
                Misctextpointers = Misc_text_pointers.Read();

                EquipableAbilities = new Dictionary<Abilities,Equipable_Ability>(
                    Stat_percent_abilities.count +
                    Character_abilities.count +
                    Party_abilities.count +
                    GF_abilities.count);
                foreach (Abilities ability in (Abilities[])Enum.GetValues(typeof(Abilities)))
                {
                    if (Statpercentabilities.ContainsKey(ability))
                        EquipableAbilities[ability] = Statpercentabilities[ability];
                    else if (Characterabilities.ContainsKey(ability))
                        EquipableAbilities[ability] = Characterabilities[ability];
                    else if (Partyabilities.ContainsKey(ability))
                        EquipableAbilities[ability] = Partyabilities[ability];
                    else if (Characterabilities.ContainsKey(ability))
                        EquipableAbilities[ability] = Characterabilities[ability];
                }
            }
        }
    }
}