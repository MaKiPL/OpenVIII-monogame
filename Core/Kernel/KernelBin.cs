using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;

namespace OpenVIII.Kernel
{
    public class KernelBin
    {
        public const ushort MAX_HP_VALUE = 9999;
        public const byte MAX_STAT_VALUE = 255;

        private static List<MagicData> s_magicData;
        private static Dictionary<GFs, JunctionableGFsData> s_junctionableGFsData;
        private static List<EnemyAttacksData> s_enemyAttacksData;
        private static List<BattleCommand> s_battleCommands;
        private static List<Weapons_Data> s_weaponsData;
        private static Dictionary<Renzokeken_Finisher, Renzokuken_Finishers_Data> s_renzokukenFinishersData;
        private static Dictionary<Characters, CharacterStats> s_characterStats;
        private static List<BattleItemData> s_battleItemsData;
        public static List<NonBattleItemsData> s_nonbattleItemsData;
        private static List<Non_Junctionable_GFs_Attacks_Data> s_nonJunctionableGFsAttacksData;
        private static Dictionary<Abilities, CommandAbilityData> s_commandabilitydata;
        private static Dictionary<Abilities, JunctionAbilities> s_junctionabilities;
        private static Dictionary<Abilities, Commandabilities> s_commandabilities;
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

        public const ushort MaxHPValue = 9999;

        public const byte MaxStatValue = 255;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Read binary data from into structures and arrays
        /// </summary>
        /// <see cref="http://forums.qhimm.com/index.php?topic=16923.msg240609#msg240609"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Kernel.bin"/>
        private KernelBin()
        {

            Memory.Log.WriteLine($"{nameof(KernelBin)} :: new ");
            ArchiveBase aw = ArchiveWorker.Load(ArchiveString);
            byte[] buffer = aw.GetBinaryFile(Memory.Strings[Strings.FileID.Kernel].GetFileNames()[0]);
            List<Loc> subPositions = Memory.Strings[Strings.FileID.Kernel].GetFiles().SubPositions;

            MemoryStream ms;
            if (buffer == null) return;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(subPositions[BattleCommand.ID], SeekOrigin.Begin);
                BattleCommands = BattleCommand.Read(br);
                ms.Seek(subPositions[Kernel.MagicData.ID], SeekOrigin.Begin);
                MagicData = Kernel.MagicData.Read(br);
                ms.Seek(subPositions[Kernel.JunctionableGFsData.ID], SeekOrigin.Begin);
                JunctionableGFsData = Kernel.JunctionableGFsData.Read(br);
                ms.Seek(subPositions[Kernel.EnemyAttacksData.ID], SeekOrigin.Begin);
                EnemyAttacksData = Kernel.EnemyAttacksData.Read(br,BattleCommands);
                ms.Seek(subPositions[Weapons_Data.id], SeekOrigin.Begin);
                WeaponsData = Weapons_Data.Read(br);
                ms.Seek(subPositions[Renzokuken_Finishers_Data.id], SeekOrigin.Begin);
                RenzokukenFinishersData = Renzokuken_Finishers_Data.Read(br);
                ms.Seek(subPositions[Kernel.CharacterStats.ID], SeekOrigin.Begin);
                CharacterStats = Kernel.CharacterStats.Read(br);
                ms.Seek(subPositions[BattleItemData.ID], SeekOrigin.Begin);
                BattleItemsData = BattleItemData.Read(br);
                NonBattleItemsData = Kernel.NonBattleItemsData.Read();
                ms.Seek(subPositions[Non_Junctionable_GFs_Attacks_Data.id], SeekOrigin.Begin);
                NonJunctionableGFsAttacksData = Non_Junctionable_GFs_Attacks_Data.Read(br);
                ms.Seek(subPositions[Kernel.CommandAbilityData.ID], SeekOrigin.Begin);
                CommandAbilityData = Kernel.CommandAbilityData.Read(br);
                ms.Seek(subPositions[Kernel.JunctionAbilities.ID], SeekOrigin.Begin);
                JunctionAbilities = Kernel.JunctionAbilities.Read(br);
                ms.Seek(subPositions[Kernel.CommandAbilities.ID], SeekOrigin.Begin);
                CommandAbilities = Kernel.CommandAbilities.Read(br, BattleCommands);
                ms.Seek(subPositions[StatPercentageAbilities.ID], SeekOrigin.Begin);
                StatPercentAbilities = StatPercentageAbilities.Read(br);
                ms.Seek(subPositions[Kernel.CharacterAbilities.ID], SeekOrigin.Begin);
                CharacterAbilities = Kernel.CharacterAbilities.Read(br);
                ms.Seek(subPositions[Kernel.PartyAbilities.ID], SeekOrigin.Begin);
                PartyAbilities = Kernel.PartyAbilities.Read(br);
                ms.Seek(subPositions[Kernel.GFAbilities.ID], SeekOrigin.Begin);
                GFAbilities = Kernel.GFAbilities.Read(br);
                ms.Seek(subPositions[Kernel.MenuAbilities.ID], SeekOrigin.Begin);
                MenuAbilities = Kernel.MenuAbilities.Read(br);
                ms.Seek(subPositions[Temporary_character_limit_breaks.id], SeekOrigin.Begin);
                TemporaryCharacterLimitBreaks = Temporary_character_limit_breaks.Read(br);
                ms.Seek(subPositions[Kernel.BlueMagicQuistisLimitBreak.ID], SeekOrigin.Begin);
                BlueMagicQuistisLimitBreak = Kernel.BlueMagicQuistisLimitBreak.Read(br);
                //ms.Seek(subPositions[Quistis_limit_break_parameters.BattleID], SeekOrigin.Begin);
                //QuistisLimitBreakParameters = Quistis_limit_break_parameters.Read(br);
                ms.Seek(subPositions[Shot_Irvine_limit_break.id], SeekOrigin.Begin);
                ShotIrvineLimitBreak = Shot_Irvine_limit_break.Read(br);
                ms.Seek(subPositions[Kernel.DuelZellLimitBreak.ID], SeekOrigin.Begin);
                DuelZellLimitBreak = Kernel.DuelZellLimitBreak.Read(br);
                ms.Seek(subPositions[Zell_limit_break_parameters.id], SeekOrigin.Begin);
                ZellLimitBreakParameters = Zell_limit_break_parameters.Read(br);
                ms.Seek(subPositions[Rinoa_limit_breaks_part_1.id], SeekOrigin.Begin);
                RinoaLimitBreaksPart1 = Rinoa_limit_breaks_part_1.Read(br);
                ms.Seek(subPositions[Rinoa_limit_breaks_part_2.id], SeekOrigin.Begin);
                RinoaLimitBreaksPart2 = Rinoa_limit_breaks_part_2.Read(br);
                ms.Seek(subPositions[Slot_array.id], SeekOrigin.Begin);
                SlotArray = Slot_array.Read(br);
                ms.Seek(subPositions[Selphie_limit_break_sets.id], SeekOrigin.Begin);
                SelphieLimitBreakSets = Selphie_limit_break_sets.Read(br);
                ms.Seek(subPositions[Kernel.Devour.ID], SeekOrigin.Begin);
                //List<Devour> tmp = Kernel.Devour.Read(br);
                //tmp.Add(new Devour { Description = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 112) });
                Devour = Kernel.Devour.Read(br);
                ms.Seek(subPositions[Kernel.MiscSection.ID], SeekOrigin.Begin);
                MiscSection = Kernel.MiscSection.Read(br);
                MiscTextPointers = Kernel.MiscTextPointers.Read();

                Dictionary<Abilities, IAbility> allAbilities = new Dictionary<Abilities, IAbility>(Kernel.MenuAbilities.Count + Kernel.JunctionAbilities.Count + Kernel.CommandAbilities.Count + StatPercentageAbilities.Count + Kernel.CharacterAbilities.Count + Kernel.PartyAbilities.Count + Kernel.GFAbilities.Count);
                foreach (Abilities ability in (Abilities[])(Enum.GetValues(typeof(Abilities))))
                {
                    combine(MenuAbilities);
                    combine(StatPercentAbilities);
                    combine(JunctionAbilities);
                    combine(CommandAbilities);
                    combine(CharacterAbilities);
                    combine(PartyAbilities);
                    combine(GFAbilities);
                    void combine<T>(IReadOnlyDictionary<Abilities, T> dict)
                        where T : IAbility
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

                AllAbilities = allAbilities;
                Dictionary<Abilities, IEquippableAbility> equippableAbilities = new Dictionary<Abilities, IEquippableAbility>(
                    StatPercentageAbilities.Count +
                    Kernel.CharacterAbilities.Count +
                    Kernel.PartyAbilities.Count +
                    Kernel.GFAbilities.Count);
                foreach (Abilities ability in (Abilities[])(Enum.GetValues(typeof(Abilities))))
                {
                    if (StatPercentAbilities.ContainsKey(ability))
                        equippableAbilities[ability] = StatPercentAbilities[ability];
                    else if (CharacterAbilities.ContainsKey(ability))
                        equippableAbilities[ability] = CharacterAbilities[ability];
                    else if (PartyAbilities.ContainsKey(ability))
                        equippableAbilities[ability] = PartyAbilities[ability];
                    else if (CharacterAbilities.ContainsKey(ability))
                        equippableAbilities[ability] = CharacterAbilities[ability];
                }
        }

        #endregion Constructors

        #region Properties

        public static IReadOnlyList<Stat> AutoAtk { get; } = new List<Stat>
        {
            Stat.STR,
            Stat.HIT,
            Stat.EL_Atk,
            Stat.ST_Atk,
            Stat.MAG,
            Stat.SPD,
            Stat.LUCK,
            Stat.HP,
            Stat.VIT,
            Stat.SPR,
            Stat.EVA,
            Stat.EL_Def_1,
            Stat.ST_Def_1,
            Stat.EL_Def_2,
            Stat.ST_Def_2,
            Stat.EL_Def_3,
            Stat.ST_Def_3,
            Stat.EL_Def_4,
            Stat.ST_Def_4
        };

        public static IReadOnlyList<Stat> AutoDef { get; } = new List<Stat>
        {
            Stat.HP,
            Stat.VIT,
            Stat.SPR,
            Stat.EVA,
            Stat.EL_Def_1,
            Stat.ST_Def_1,
            Stat.EL_Def_2,
            Stat.ST_Def_2,
            Stat.EL_Def_3,
            Stat.ST_Def_3,
            Stat.EL_Def_4,
            Stat.ST_Def_4,
            Stat.SPD,
            Stat.LUCK,
            Stat.MAG,
            Stat.STR,
            Stat.HIT,
            Stat.EL_Atk,
            Stat.ST_Atk
        };

        public static IReadOnlyList<Stat> AutoMAG { get; } = new List<Stat>
        {
            Stat.MAG,
            Stat.SPR,
            Stat.SPD,
            Stat.LUCK,
            Stat.HP,
            Stat.VIT,
            Stat.EVA,
            Stat.EL_Def_1,
            Stat.ST_Def_1,
            Stat.EL_Def_2,
            Stat.ST_Def_2,
            Stat.EL_Def_3,
            Stat.ST_Def_3,
            Stat.EL_Def_4,
            Stat.ST_Def_4,
            Stat.STR,
            Stat.HIT,
            Stat.EL_Atk,
            Stat.ST_Atk
        };

        /// <summary>
        /// Convert stat to stat junction
        /// </summary>
        public static IReadOnlyDictionary<Stat, Abilities> Stat2Ability { get; } = new Dictionary<Stat, Abilities>
        {
            { Stat.HP, Abilities.HPJ },
            { Stat.STR, Abilities.StrJ },
            { Stat.VIT, Abilities.VitJ},
            { Stat.MAG, Abilities.MagJ},
            { Stat.SPR, Abilities.SprJ },
            { Stat.SPD, Abilities.SpdJ },
            { Stat.EVA, Abilities.EvaJ },
            { Stat.LUCK, Abilities.LuckJ },
            { Stat.HIT, Abilities.HitJ },
            { Stat.EL_Atk, Abilities.ElAtkJ },
            { Stat.ST_Atk, Abilities.StAtkJ },
            { Stat.EL_Def_1, Abilities.ElDefJ },//or Elem_Def_Jx2 or Elem_Def_Jx4
            { Stat.EL_Def_2, Abilities.ElDefJ2 },//or Elem_Def_Jx4
            { Stat.EL_Def_3, Abilities.ElDefJ4 },
            { Stat.EL_Def_4, Abilities.ElDefJ4 },
            { Stat.ST_Def_1, Abilities.StDefJ },//or ST_Def_Jx2 or ST_Def_Jx4
            { Stat.ST_Def_2, Abilities.StDefJ2 },//or ST_Def_Jx4
            { Stat.ST_Def_3, Abilities.StDefJ4 },
            { Stat.ST_Def_4, Abilities.StDefJ4 }
        };

        public IReadOnlyDictionary<Abilities, IAbility> AllAbilities { get; }

        public IReadOnlyList<BattleCommand> BattleCommands { get; }

        public IReadOnlyList<BattleItemData> BattleItemsData { get; }

        public IReadOnlyDictionary<BlueMagic, BlueMagicQuistisLimitBreak> BlueMagicQuistisLimitBreak { get; }

        public IReadOnlyDictionary<Abilities, CharacterAbilities> CharacterAbilities { get; }

        public IReadOnlyDictionary<Characters, CharacterStats> CharacterStats { get; }

        public IReadOnlyDictionary<Abilities, CommandAbilities> CommandAbilities { get; }

        public IReadOnlyDictionary<Abilities, CommandAbilityData> CommandAbilityData { get; }

        public IReadOnlyList<Devour> Devour { get; }

        public IReadOnlyList<DuelZellLimitBreak> DuelZellLimitBreak { get; }

        public IReadOnlyList<EnemyAttacksData> EnemyAttacksData { get; }
        public IReadOnlyDictionary<Abilities, IEquippableAbility> EquippableAbilities { get; }

        public IReadOnlyDictionary<Abilities, GFAbilities> GFAbilities { get; }
        public IReadOnlyDictionary<Abilities, JunctionAbilities> JunctionAbilities { get; }

        public IReadOnlyDictionary<GFs, JunctionableGFsData> JunctionableGFsData { get; }

        public IReadOnlyList<MagicData> MagicData { get; }
        public IReadOnlyDictionary<Abilities, MenuAbilities> MenuAbilities { get; }
        public IReadOnlyList<MiscSection> MiscSection { get; }
        public IReadOnlyList<MiscTextPointers> MiscTextPointers { get; }
        public IReadOnlyList<NonBattleItemsData> NonBattleItemsData { get; }
        public IReadOnlyList<Non_Junctionable_GFs_Attacks_Data> NonJunctionableGFsAttacksData { get; }
        public IReadOnlyDictionary<Abilities, PartyAbilities> PartyAbilities { get; }

        public IReadOnlyDictionary<Renzokeken_Finisher, Renzokuken_Finishers_Data> RenzokukenFinishersData { get; }

        public IReadOnlyList<Rinoa_limit_breaks_part_1> RinoaLimitBreaksPart1 { get; }
        public IReadOnlyList<Rinoa_limit_breaks_part_2> RinoaLimitBreaksPart2 { get; }

        public IReadOnlyList<Selphie_limit_break_sets> SelphieLimitBreakSets { get; }
        public IReadOnlyList<Shot_Irvine_limit_break> ShotIrvineLimitBreak { get; }

        public IReadOnlyList<Slot_array> SlotArray { get; }

        public IReadOnlyDictionary<Abilities, StatPercentageAbilities> StatPercentAbilities { get; }

        public IReadOnlyList<Temporary_character_limit_breaks> TemporaryCharacterLimitBreaks { get; }

        public IReadOnlyList<Weapons_Data> WeaponsData { get; }

        public IReadOnlyList<Zell_limit_break_parameters> ZellLimitBreakParameters { get; }

        private Memory.Archive ArchiveString { get; } = Memory.Archives.A_MAIN;

        #endregion Properties

        #region Methods

        public static KernelBin CreateInstance() => new KernelBin();


        #endregion Methods
    }
}
