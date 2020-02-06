using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        public const ushort MAX_HP_VALUE = 9999;
        public const byte MAX_STAT_VALUE = 255;

        private static List<Magic_Data> s_magicData;
        private static Dictionary<GFs, Junctionable_GFs_Data> s_junctionableGFsData;
        private static List<Enemy_Attacks_Data> s_enemyAttacksData;
        private static List<Battle_Commands> s_battleCommands;
        private static List<Weapons_Data> s_weaponsData;
        private static Dictionary<Renzokeken_Finisher, Renzokuken_Finishers_Data> s_renzokukenFinishersData;
        private static Dictionary<Characters, Character_Stats> s_characterStats;
        private static List<Battle_Items_Data> s_battleItemsData;
        public static List<Non_battle_Items_Data> s_nonbattleItemsData;
        private static List<Non_Junctionable_GFs_Attacks_Data> s_nonJunctionableGFsAttacksData;
        private static Dictionary<Abilities, Command_ability_data> s_commandabilitydata;
        private static Dictionary<Abilities, Junction_abilities> s_junctionabilities;
        private static Dictionary<Abilities, Command_abilities> s_commandabilities;
        private static Dictionary<Abilities, Stat_percent_abilities> s_statpercentabilities;
        private static Dictionary<Abilities, Character_abilities> s_characterabilities;
        private static Dictionary<Abilities, Party_abilities> s_partyabilities;
        private static Dictionary<Abilities, GF_abilities> s_gFabilities;
        private static Dictionary<Abilities, Menu_abilities> s_menuabilities;
        private static List<Temporary_character_limit_breaks> s_temporarycharacterlimitbreaks;
        private static Dictionary<Blue_Magic, Blue_magic_Quistis_limit_break> s_bluemagicQuistislimitbreak;
        private static List<Shot_Irvine_limit_break> s_shotIrvinelimitbreak;
        private static List<Duel_Zell_limit_break> s_duelZelllimitbreak;
        private static List<Zell_limit_break_parameters> s_zelllimitbreakparameters;
        private static List<Rinoa_limit_breaks_part_1> s_rinoalimitbreakspart1;
        private static List<Rinoa_limit_breaks_part_2> s_rinoalimitbreakspart2;
        private static List<Slot_array> s_slotarray;
        private static List<Selphie_limit_break_sets> s_selphielimitbreaksets;
        private static List<Devour> s_devour_;
        private static List<Misc_section> s_miscsection;
        private static List<Misc_text_pointers> s_misctextpointers;
        private static Dictionary<Abilities, Ability> s_allAbilities;
        private static Dictionary<Abilities, Equipable_Ability> s_equipableAbilities;

        private Memory.Archive ArchiveString { get; } = Memory.Archives.A_MAIN;
        public static IReadOnlyList<Magic_Data> MagicData => s_magicData; //0
        public static IReadOnlyDictionary<GFs, Junctionable_GFs_Data> JunctionableGFsData => s_junctionableGFsData; //1
        public static IReadOnlyList<Enemy_Attacks_Data> EnemyAttacksData => s_enemyAttacksData; //2
        public static IReadOnlyList<Battle_Commands> BattleCommands => s_battleCommands; //3
        public static IReadOnlyList<Weapons_Data> WeaponsData => s_weaponsData; //4
        public static IReadOnlyDictionary<Renzokeken_Finisher, Renzokuken_Finishers_Data> RenzokukenFinishersData => s_renzokukenFinishersData;  //5
        public static IReadOnlyDictionary<Characters, Character_Stats> CharacterStats => s_characterStats; //6
        public static IReadOnlyList<Battle_Items_Data> BattleItemsData => s_battleItemsData; //7
        public static IReadOnlyList<Non_battle_Items_Data> NonbattleItemsData => s_nonbattleItemsData;  //8 //only strings
        public static IReadOnlyList<Non_Junctionable_GFs_Attacks_Data> NonJunctionableGFsAttacksData => s_nonJunctionableGFsAttacksData;  //9
        public static IReadOnlyDictionary<Abilities, Command_ability_data> Commandabilitydata => s_commandabilitydata; //10
        public static IReadOnlyDictionary<Abilities, Junction_abilities> Junctionabilities => s_junctionabilities; //11
        public static IReadOnlyDictionary<Abilities, Command_abilities> Commandabilities => s_commandabilities; //12
        public static IReadOnlyDictionary<Abilities, Stat_percent_abilities> Statpercentabilities => s_statpercentabilities;  //13
        public static IReadOnlyDictionary<Abilities, Character_abilities> Characterabilities => s_characterabilities;  //14
        public static IReadOnlyDictionary<Abilities, Party_abilities> Partyabilities => s_partyabilities;  //15
        public static IReadOnlyDictionary<Abilities, GF_abilities> GFabilities => s_gFabilities;  //16
        public static IReadOnlyDictionary<Abilities, Menu_abilities> Menuabilities => s_menuabilities;  //17
        public static IReadOnlyList<Temporary_character_limit_breaks> Temporarycharacterlimitbreaks => s_temporarycharacterlimitbreaks;  //18
        public static IReadOnlyDictionary<Blue_Magic, Blue_magic_Quistis_limit_break> BluemagicQuistislimitbreak => s_bluemagicQuistislimitbreak;  //19

        //public static List<Quistis_limit_break_parameters> Quistislimitbreakparameters { get; private set; }//20
        public static IReadOnlyList<Shot_Irvine_limit_break> ShotIrvinelimitbreak => s_shotIrvinelimitbreak;  //21

        public static IReadOnlyList<Duel_Zell_limit_break> DuelZelllimitbreak => s_duelZelllimitbreak;  //22
        public static IReadOnlyList<Zell_limit_break_parameters> Zelllimitbreakparameters => s_zelllimitbreakparameters;  //23
        public static IReadOnlyList<Rinoa_limit_breaks_part_1> Rinoalimitbreakspart1 => s_rinoalimitbreakspart1;  //24
        public static IReadOnlyList<Rinoa_limit_breaks_part_2> Rinoalimitbreakspart2 => s_rinoalimitbreakspart2;  //25
        public static IReadOnlyList<Slot_array> Slotarray => s_slotarray;  //26
        public static IReadOnlyList<Selphie_limit_break_sets> Selphielimitbreaksets => s_selphielimitbreaksets;  //27
        public static IReadOnlyList<Devour> Devour_ => s_devour_;  //28
        public static IReadOnlyList<Misc_section> Miscsection => s_miscsection;  //29 //only_strings
        public static IReadOnlyList<Misc_text_pointers> Misctextpointers => s_misctextpointers;  //30

        public static IReadOnlyDictionary<Abilities, Ability> AllAbilities => s_allAbilities;   // should contain all abilities
        public static IReadOnlyDictionary<Abilities, Equipable_Ability> EquipableAbilities => s_equipableAbilities;   // contains 4 types;

        /// <summary>
        /// Read binary data from into structures and arrays
        /// </summary>
        /// <see cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Kernel.bin"/>
        public Kernel_bin()
        {

            Memory.Log.WriteLine($"{nameof(Kernel_bin)} :: new ");
            ArchiveBase aw = ArchiveWorker.Load(ArchiveString);
            byte[] buffer = aw.GetBinaryFile(Memory.Strings[Strings.FileID.KERNEL].GetFilenames()[0]);
            List<Loc> subPositions = Memory.Strings[Strings.FileID.KERNEL].GetFiles().subPositions;

            MemoryStream ms = null;
            if (buffer != null)
                using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
                {
                    ms.Seek(subPositions[Battle_Commands.id], SeekOrigin.Begin);
                    s_battleCommands = Battle_Commands.Read(br);
                    ms.Seek(subPositions[Magic_Data.id], SeekOrigin.Begin);
                    s_magicData = Magic_Data.Read(br);
                    ms.Seek(subPositions[Junctionable_GFs_Data.id], SeekOrigin.Begin);
                    s_junctionableGFsData = Junctionable_GFs_Data.Read(br);
                    ms.Seek(subPositions[Enemy_Attacks_Data.id], SeekOrigin.Begin);
                    s_enemyAttacksData = Enemy_Attacks_Data.Read(br);
                    ms.Seek(subPositions[Weapons_Data.id], SeekOrigin.Begin);
                    s_weaponsData = Weapons_Data.Read(br);
                    ms.Seek(subPositions[Renzokuken_Finishers_Data.id], SeekOrigin.Begin);
                    s_renzokukenFinishersData = Renzokuken_Finishers_Data.Read(br);
                    ms.Seek(subPositions[Character_Stats.id], SeekOrigin.Begin);
                    s_characterStats = Character_Stats.Read(br);
                    ms.Seek(subPositions[Battle_Items_Data.id], SeekOrigin.Begin);
                    s_battleItemsData = Battle_Items_Data.Read(br);
                    s_nonbattleItemsData = Non_battle_Items_Data.Read();
                    ms.Seek(subPositions[Non_Junctionable_GFs_Attacks_Data.id], SeekOrigin.Begin);
                    s_nonJunctionableGFsAttacksData = Non_Junctionable_GFs_Attacks_Data.Read(br);
                    ms.Seek(subPositions[Command_ability_data.id], SeekOrigin.Begin);
                    s_commandabilitydata = Command_ability_data.Read(br);
                    ms.Seek(subPositions[Junction_abilities.id], SeekOrigin.Begin);
                    s_junctionabilities = Junction_abilities.Read(br);
                    ms.Seek(subPositions[Command_abilities.id], SeekOrigin.Begin);
                    s_commandabilities = Command_abilities.Read(br);
                    ms.Seek(subPositions[Stat_percent_abilities.id], SeekOrigin.Begin);
                    s_statpercentabilities = Stat_percent_abilities.Read(br);
                    ms.Seek(subPositions[Character_abilities.id], SeekOrigin.Begin);
                    s_characterabilities = Character_abilities.Read(br);
                    ms.Seek(subPositions[Party_abilities.id], SeekOrigin.Begin);
                    s_partyabilities = Party_abilities.Read(br);
                    ms.Seek(subPositions[GF_abilities.id], SeekOrigin.Begin);
                    s_gFabilities = GF_abilities.Read(br);
                    ms.Seek(subPositions[Menu_abilities.id], SeekOrigin.Begin);
                    s_menuabilities = Menu_abilities.Read(br);
                    ms.Seek(subPositions[Temporary_character_limit_breaks.id], SeekOrigin.Begin);
                    s_temporarycharacterlimitbreaks = Temporary_character_limit_breaks.Read(br);
                    ms.Seek(subPositions[Blue_magic_Quistis_limit_break.id], SeekOrigin.Begin);
                    s_bluemagicQuistislimitbreak = Blue_magic_Quistis_limit_break.Read(br);
                    //ms.Seek(subPositions[Quistis_limit_break_parameters.id], SeekOrigin.Begin);
                    //Quistislimitbreakparameters = Quistis_limit_break_parameters.Read(br);
                    ms.Seek(subPositions[Shot_Irvine_limit_break.id], SeekOrigin.Begin);
                    s_shotIrvinelimitbreak = Shot_Irvine_limit_break.Read(br);
                    ms.Seek(subPositions[Duel_Zell_limit_break.id], SeekOrigin.Begin);
                    s_duelZelllimitbreak = Duel_Zell_limit_break.Read(br);
                    ms.Seek(subPositions[Zell_limit_break_parameters.id], SeekOrigin.Begin);
                    s_zelllimitbreakparameters = Zell_limit_break_parameters.Read(br);
                    ms.Seek(subPositions[Rinoa_limit_breaks_part_1.id], SeekOrigin.Begin);
                    s_rinoalimitbreakspart1 = Rinoa_limit_breaks_part_1.Read(br);
                    ms.Seek(subPositions[Rinoa_limit_breaks_part_2.id], SeekOrigin.Begin);
                    s_rinoalimitbreakspart2 = Rinoa_limit_breaks_part_2.Read(br);
                    ms.Seek(subPositions[Slot_array.id], SeekOrigin.Begin);
                    s_slotarray = Slot_array.Read(br);
                    ms.Seek(subPositions[Selphie_limit_break_sets.id], SeekOrigin.Begin);
                    s_selphielimitbreaksets = Selphie_limit_break_sets.Read(br);
                    ms.Seek(subPositions[Devour.id], SeekOrigin.Begin);
                    s_devour_ = Devour.Read(br);
                    s_devour_.Add(new Devour { Description = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 112) });
                    ms.Seek(subPositions[Misc_section.id], SeekOrigin.Begin);
                    s_miscsection = Misc_section.Read(br);
                    s_misctextpointers = Misc_text_pointers.Read();

                    s_allAbilities = new Dictionary<Abilities, Ability>(
                        Menu_abilities.count +
                        Junction_abilities.count +
                        Command_abilities.count +
                        Stat_percent_abilities.count +
                        Character_abilities.count +
                        Party_abilities.count +
                        GF_abilities.count);
                    foreach (Abilities ability in (Abilities[])(Enum.GetValues(typeof(Abilities))))
                    {
                        combine(Menuabilities);
                        combine(Statpercentabilities);
                        combine(Junctionabilities);
                        combine(Commandabilities);
                        combine(Characterabilities);
                        combine(Partyabilities);
                        combine(GFabilities);
                        bool combine<T>(IReadOnlyDictionary<Abilities, T> dict)
                            where T : Ability
                        {
                            if (dict.TryGetValue(ability, out T a))
                            {
                                s_allAbilities.Add(ability, a);
                                return true;
                            }
                            return false;
                        }
                    }

                    s_equipableAbilities = new Dictionary<Abilities, Equipable_Ability>(
                        Stat_percent_abilities.count +
                        Character_abilities.count +
                        Party_abilities.count +
                        GF_abilities.count);
                    foreach (Abilities ability in (Abilities[])(Enum.GetValues(typeof(Abilities))))
                    {
                        if (Statpercentabilities.ContainsKey(ability))
                            s_equipableAbilities[ability] = Statpercentabilities[ability];
                        else if (Characterabilities.ContainsKey(ability))
                            s_equipableAbilities[ability] = Characterabilities[ability];
                        else if (Partyabilities.ContainsKey(ability))
                            s_equipableAbilities[ability] = Partyabilities[ability];
                        else if (Characterabilities.ContainsKey(ability))
                            s_equipableAbilities[ability] = Characterabilities[ability];
                    }
                    ms = null;
                }
        }
    }
}