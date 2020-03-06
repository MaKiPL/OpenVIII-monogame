using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Kernel
{
    public class KernelBin
    {
        #region Fields

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
        public KernelBin()
        {
            Memory.Log.WriteLine($"{nameof(KernelBin)} :: new ");
            ArchiveBase aw = ArchiveWorker.Load(ArchiveString);
            byte[] buffer = aw.GetBinaryFile(Memory.Strings[Strings.FileID.KERNEL].GetFilenames()[0]);
            List<Loc> subPositions = Memory.Strings[Strings.FileID.KERNEL].GetFiles().subPositions;

            MemoryStream ms;
            if (buffer == null) return;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(subPositions[BattleCommand.ID], SeekOrigin.Begin);
                BattleCommands = BattleCommand.Read(br);
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
                ms.Seek(subPositions[BattleItemData.ID], SeekOrigin.Begin);
                BattleItemsData = BattleItemData.Read(br);
                NonBattleItemsData = Non_battle_Items_Data.Read();
                ms.Seek(subPositions[Non_Junctionable_GFs_Attacks_Data.id], SeekOrigin.Begin);
                NonJunctionableGFsAttacksData = Non_Junctionable_GFs_Attacks_Data.Read(br);
                ms.Seek(subPositions[Command_ability_data.id], SeekOrigin.Begin);
                CommandAbilityData = Command_ability_data.Read(br);
                ms.Seek(subPositions[Kernel.JunctionAbilities.ID], SeekOrigin.Begin);
                JunctionAbilities = Kernel.JunctionAbilities.Read(br);
                ms.Seek(subPositions[Kernel.CommandAbilities.ID], SeekOrigin.Begin);
                CommandAbilities = Kernel.CommandAbilities.Read(br);
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
                ms.Seek(subPositions[Blue_Magic_Quistis_limit_break.id], SeekOrigin.Begin);
                BlueMagicQuistisLimitBreak = Blue_Magic_Quistis_limit_break.Read(br);
                //ms.Seek(subPositions[Quistis_limit_break_parameters.BattleID], SeekOrigin.Begin);
                //QuistisLimitBreakParameters = Quistis_limit_break_parameters.Read(br);
                ms.Seek(subPositions[Shot_Irvine_limit_break.id], SeekOrigin.Begin);
                ShotIrvineLimitBreak = Shot_Irvine_limit_break.Read(br);
                ms.Seek(subPositions[Duel_Zell_limit_break.id], SeekOrigin.Begin);
                DuelZellLimitBreak = Duel_Zell_limit_break.Read(br);
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
                ms.Seek(subPositions[Kernel.Devour.id], SeekOrigin.Begin);
                List<Devour> tmp = Kernel.Devour.Read(br);
                tmp.Add(new Devour { Description = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 112) });
                Devour = tmp;
                ms.Seek(subPositions[Misc_section.id], SeekOrigin.Begin);
                MiscSection = Misc_section.Read(br);
                MiscTextPointers = Misc_text_pointers.Read();

                Dictionary<Abilities, Ability> allAbilities = new Dictionary<Abilities, Ability>(Kernel.MenuAbilities.Count + Kernel.JunctionAbilities.Count + Kernel.CommandAbilities.Count + StatPercentageAbilities.Count + Kernel.CharacterAbilities.Count + Kernel.PartyAbilities.Count + Kernel.GFAbilities.Count);
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
                        where T : Ability
                    {
                        if (!dict.TryGetValue(ability, out T a)) return;
                        allAbilities.Add(ability, a);
                    }
                }

                AllAbilities = allAbilities;
                Dictionary<Abilities, EquippableAbility> equippableAbilities = new Dictionary<Abilities, EquippableAbility>(
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
                EquippableAbilities = equippableAbilities;
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

        public IReadOnlyDictionary<Abilities, Ability> AllAbilities { get; }
        public IReadOnlyList<BattleCommand> BattleCommands { get; }
        public IReadOnlyList<BattleItemData> BattleItemsData { get; }
        public IReadOnlyDictionary<Blue_Magic, Blue_Magic_Quistis_limit_break> BlueMagicQuistisLimitBreak { get; }
        public IReadOnlyDictionary<Abilities, CharacterAbilities> CharacterAbilities { get; }
        public IReadOnlyDictionary<Characters, Character_Stats> CharacterStats { get; }
        public IReadOnlyDictionary<Abilities, CommandAbilities> CommandAbilities { get; }
        public IReadOnlyDictionary<Abilities, Command_ability_data> CommandAbilityData { get; }
        public IReadOnlyList<Devour> Devour { get; }
        public IReadOnlyList<Duel_Zell_limit_break> DuelZellLimitBreak { get; }
        public IReadOnlyList<Enemy_Attacks_Data> EnemyAttacksData { get; }

        // should contain all abilities
        public IReadOnlyDictionary<Abilities, EquippableAbility> EquippableAbilities { get; }

        public IReadOnlyDictionary<Abilities, GFAbilities> GFAbilities { get; }

        //10
        public IReadOnlyDictionary<Abilities, JunctionAbilities> JunctionAbilities { get; }

        public IReadOnlyDictionary<GFs, Junctionable_GFs_Data> JunctionableGFsData { get; }
        public IReadOnlyList<Magic_Data> MagicData { get; }

        //16
        public IReadOnlyDictionary<Abilities, MenuAbilities> MenuAbilities { get; }

        //28
        public IReadOnlyList<Misc_section> MiscSection { get; }

        //29 //only_strings
        public IReadOnlyList<Misc_text_pointers> MiscTextPointers { get; }

        //6
        //7
        public IReadOnlyList<Non_battle_Items_Data> NonBattleItemsData { get; }

        //8 //only strings
        public IReadOnlyList<Non_Junctionable_GFs_Attacks_Data> NonJunctionableGFsAttacksData { get; }

        //14
        public IReadOnlyDictionary<Abilities, PartyAbilities> PartyAbilities { get; }

        public IReadOnlyDictionary<Renzokeken_Finisher, Renzokuken_Finishers_Data> RenzokukenFinishersData { get; }
        public IReadOnlyList<Rinoa_limit_breaks_part_1> RinoaLimitBreaksPart1 { get; }

        //24
        public IReadOnlyList<Rinoa_limit_breaks_part_2> RinoaLimitBreaksPart2 { get; }

        public IReadOnlyList<Selphie_limit_break_sets> SelphieLimitBreakSets { get; }

        //public static List<Quistis_limit_break_parameters> QuistisLimitBreakParameters { get; private set; }//20
        public IReadOnlyList<Shot_Irvine_limit_break> ShotIrvineLimitBreak { get; }

        //25
        public IReadOnlyList<Slot_array> SlotArray { get; }

        //5
        //9
        //11
        //12
        public IReadOnlyDictionary<Abilities, StatPercentageAbilities> StatPercentAbilities { get; }

        //13
        //15
        //17
        public IReadOnlyList<Temporary_character_limit_breaks> TemporaryCharacterLimitBreaks { get; }

        //0
        //1
        //2
        //3
        public IReadOnlyList<Weapons_Data> WeaponsData { get; }

        //22
        public IReadOnlyList<Zell_limit_break_parameters> ZellLimitBreakParameters { get; }

        private Memory.Archive ArchiveString { get; } = Memory.Archives.A_MAIN;

        #endregion Properties

        //4
        //18
        //19

        //21

        //23
        //26
        //27
        //30

        // contains 4 types;
    }
}