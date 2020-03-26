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
        private KernelBin()
        {
            Memory.Log.WriteLine($"{nameof(KernelBin)} :: new ");
            var aw = ArchiveWorker.Load(ArchiveString);
            var buffer = aw.GetBinaryFile(Memory.Strings[Strings.FileID.Kernel].GetFileNames()[0]);
            var subPositions = Memory.Strings[Strings.FileID.Kernel].GetFiles().SubPositions;

            MemoryStream ms;
            if (buffer == null) return;
            using (var br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(subPositions[BattleCommand.ID], SeekOrigin.Begin);
                BattleCommands = BattleCommand.Read(br);
                ms.Seek(subPositions[Kernel.MagicData.ID], SeekOrigin.Begin);
                MagicData = Kernel.MagicData.Read(br);
                ms.Seek(subPositions[Kernel.JunctionableGFsData.ID], SeekOrigin.Begin);
                JunctionableGFsData = Kernel.JunctionableGFsData.Read(br);
                ms.Seek(subPositions[Kernel.EnemyAttacksData.ID], SeekOrigin.Begin);
                EnemyAttacksData = Kernel.EnemyAttacksData.Read(br, BattleCommands);
                ms.Seek(subPositions[Kernel.WeaponsData.ID], SeekOrigin.Begin);
                WeaponsData = Kernel.WeaponsData.Read(br);
                ms.Seek(subPositions[Kernel.RenzokukenFinishersData.ID], SeekOrigin.Begin);
                RenzokukenFinishersData = Kernel.RenzokukenFinishersData.Read(br);
                ms.Seek(subPositions[Kernel.CharacterStats.ID], SeekOrigin.Begin);
                CharacterStats = Kernel.CharacterStats.Read(br);
                ms.Seek(subPositions[BattleItemData.ID], SeekOrigin.Begin);
                BattleItemsData = BattleItemData.Read(br);
                NonBattleItemsData = Kernel.NonBattleItemsData.Read();
                ms.Seek(subPositions[Kernel.NonJunctionableGFsAttacksData.ID], SeekOrigin.Begin);
                NonJunctionableGFsAttacksData = Kernel.NonJunctionableGFsAttacksData.Read(br);
                ms.Seek(subPositions[Kernel.CommandAbilityData.ID], SeekOrigin.Begin);
                CommandAbilityData = Kernel.CommandAbilityData.Read(br);
                ms.Seek(subPositions[Kernel.JunctionAbilities.ID], SeekOrigin.Begin);
                JunctionAbilities = Kernel.JunctionAbilities.Read(br);
                ms.Seek(subPositions[Kernel.CommandAbility.ID], SeekOrigin.Begin);
                CommandAbilities = Kernel.CommandAbility.Read(br, BattleCommands);
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
                ms.Seek(subPositions[Kernel.TemporaryCharacterLimitBreaks.ID], SeekOrigin.Begin);
                TemporaryCharacterLimitBreaks = Kernel.TemporaryCharacterLimitBreaks.Read(br);
                ms.Seek(subPositions[Kernel.BlueMagicQuistisLimitBreak.ID], SeekOrigin.Begin);
                BlueMagicQuistisLimitBreak = Kernel.BlueMagicQuistisLimitBreak.Read(br);
                //ms.Seek(subPositions[Quistis_limit_break_parameters.ID], SeekOrigin.Begin);
                //QuistisLimitBreakParameters = Quistis_limit_break_parameters.Read(br);
                ms.Seek(subPositions[Kernel.ShotIrvineLimitBreak.ID], SeekOrigin.Begin);
                ShotIrvineLimitBreak = Kernel.ShotIrvineLimitBreak.Read(br);
                ms.Seek(subPositions[Kernel.DuelZellLimitBreak.ID], SeekOrigin.Begin);
                DuelZellLimitBreak = Kernel.DuelZellLimitBreak.Read(br);
                ms.Seek(subPositions[Kernel.ZellLimitBreakParameters.ID], SeekOrigin.Begin);
                ZellLimitBreakParameters = Kernel.ZellLimitBreakParameters.Read(br);
                ms.Seek(subPositions[Kernel.RinoaLimitBreaksPart1.ID], SeekOrigin.Begin);
                RinoaLimitBreaksPart1 = Kernel.RinoaLimitBreaksPart1.Read(br);
                ms.Seek(subPositions[Kernel.RinoaLimitBreaksPart2.ID], SeekOrigin.Begin);
                RinoaLimitBreaksPart2 = Kernel.RinoaLimitBreaksPart2.Read(br);
                ms.Seek(subPositions[Kernel.SlotArray.ID], SeekOrigin.Begin);
                SlotArray = Kernel.SlotArray.Read(br);
                ms.Seek(subPositions[Kernel.SelphieLimitBreakSets.ID], SeekOrigin.Begin);
                SelphieLimitBreakSets = Kernel.SelphieLimitBreakSets.Read(br);
                ms.Seek(subPositions[Kernel.Devour.ID], SeekOrigin.Begin);
                //List<Devour> tmp = Kernel.Devour.Read(br);
                //tmp.Add(new Devour { Description = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 112) });
                Devour = Kernel.Devour.Read(br);
                ms.Seek(subPositions[Kernel.MiscSection.ID], SeekOrigin.Begin);
                MiscSection = Kernel.MiscSection.Read(br);
                MiscTextPointers = Kernel.MiscTextPointers.Read();

                var allAbilities = new Dictionary<Abilities, IAbility>(
                    Kernel.MenuAbilities.Count + Kernel.JunctionAbilities.Count + Kernel.CommandAbility.Count +
                    StatPercentageAbilities.Count + Kernel.CharacterAbilities.Count + Kernel.PartyAbilities.Count +
                    Kernel.GFAbilities.Count);

                var equippableAbilities =
                    new Dictionary<Abilities, IEquippableAbility>(
                        StatPercentageAbilities.Count +
                        Kernel.CharacterAbilities.Count +
                        Kernel.PartyAbilities.Count +
                        Kernel.GFAbilities.Count);

                foreach (var ability in (Abilities[])(Enum.GetValues(typeof(Abilities))))
                {
                    combine(MenuAbilities);
                    combine(StatPercentAbilities);
                    combine(JunctionAbilities);
                    combine(CommandAbilities);
                    combine(CharacterAbilities);
                    combine(PartyAbilities);
                    combine(GFAbilities);

                    combine2(StatPercentAbilities);
                    combine2(CharacterAbilities);
                    combine2(PartyAbilities);
                    combine2(CharacterAbilities);

                    void combine<T>(IReadOnlyDictionary<Abilities, T> dict)
                            where T : IAbility
                    {
                        if (!dict.TryGetValue(ability, out var a) || allAbilities.ContainsKey(ability)) return;
                        allAbilities.Add(ability, a);
                    }

                    void combine2<T>(IReadOnlyDictionary<Abilities, T> dict)
                        where T : IEquippableAbility
                    {
                        if (!dict.TryGetValue(ability, out var a) || equippableAbilities.ContainsKey(ability)) return;
                        equippableAbilities.Add(ability, a);
                    }
                }
                AllAbilities = allAbilities;
                EquippableAbilities = equippableAbilities;
            }
        }

        #endregion Constructors

        #region Properties

        public static IReadOnlyList<Stat> AutoAtk { get; } = new List<Stat>
        {
            Stat.STR,
            Stat.HIT,
            Stat.ElAtk,
            Stat.StAtk,
            Stat.MAG,
            Stat.SPD,
            Stat.Luck,
            Stat.HP,
            Stat.VIT,
            Stat.SPR,
            Stat.EVA,
            Stat.ElDef1,
            Stat.StDef1,
            Stat.ElDef2,
            Stat.StDef2,
            Stat.ElDef3,
            Stat.StDef3,
            Stat.ElDef4,
            Stat.StDef4
        };

        public static IReadOnlyList<Stat> AutoDef { get; } = new List<Stat>
        {
            Stat.HP,
            Stat.VIT,
            Stat.SPR,
            Stat.EVA,
            Stat.ElDef1,
            Stat.StDef1,
            Stat.ElDef2,
            Stat.StDef2,
            Stat.ElDef3,
            Stat.StDef3,
            Stat.ElDef4,
            Stat.StDef4,
            Stat.SPD,
            Stat.Luck,
            Stat.MAG,
            Stat.STR,
            Stat.HIT,
            Stat.ElAtk,
            Stat.StAtk
        };

        public static IReadOnlyList<Stat> AutoMAG { get; } = new List<Stat>
        {
            Stat.MAG,
            Stat.SPR,
            Stat.SPD,
            Stat.Luck,
            Stat.HP,
            Stat.VIT,
            Stat.EVA,
            Stat.ElDef1,
            Stat.StDef1,
            Stat.ElDef2,
            Stat.StDef2,
            Stat.ElDef3,
            Stat.StDef3,
            Stat.ElDef4,
            Stat.StDef4,
            Stat.STR,
            Stat.HIT,
            Stat.ElAtk,
            Stat.StAtk
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
            { Stat.Luck, Abilities.LuckJ },
            { Stat.HIT, Abilities.HitJ },
            { Stat.ElAtk, Abilities.ElAtkJ },
            { Stat.StAtk, Abilities.StAtkJ },
            { Stat.ElDef1, Abilities.ElDefJ },//or Elem_Def_Jx2 or Elem_Def_Jx4
            { Stat.ElDef2, Abilities.ElDefJ2 },//or Elem_Def_Jx4
            { Stat.ElDef3, Abilities.ElDefJ4 },
            { Stat.ElDef4, Abilities.ElDefJ4 },
            { Stat.StDef1, Abilities.StDefJ },//or ST_Def_Jx2 or ST_Def_Jx4
            { Stat.StDef2, Abilities.StDefJ2 },//or ST_Def_Jx4
            { Stat.StDef3, Abilities.StDefJ4 },
            { Stat.StDef4, Abilities.StDefJ4 }
        };

        public IReadOnlyDictionary<Abilities, IAbility> AllAbilities { get; }

        public IReadOnlyList<BattleCommand> BattleCommands { get; }

        public IReadOnlyList<BattleItemData> BattleItemsData { get; }

        public IReadOnlyDictionary<BlueMagic, BlueMagicQuistisLimitBreak> BlueMagicQuistisLimitBreak { get; }

        public IReadOnlyDictionary<Abilities, CharacterAbilities> CharacterAbilities { get; }

        public IReadOnlyDictionary<Characters, CharacterStats> CharacterStats { get; }

        public IReadOnlyDictionary<Abilities, CommandAbility> CommandAbilities { get; }

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
        public IReadOnlyList<NonJunctionableGFsAttacksData> NonJunctionableGFsAttacksData { get; }
        public IReadOnlyDictionary<Abilities, PartyAbilities> PartyAbilities { get; }

        public IReadOnlyDictionary<RenzokukenFinisher, RenzokukenFinishersData> RenzokukenFinishersData { get; }

        public IReadOnlyList<RinoaLimitBreaksPart1> RinoaLimitBreaksPart1 { get; }
        public IReadOnlyList<RinoaLimitBreaksPart2> RinoaLimitBreaksPart2 { get; }

        public IReadOnlyList<SelphieLimitBreakSets> SelphieLimitBreakSets { get; }
        public IReadOnlyList<ShotIrvineLimitBreak> ShotIrvineLimitBreak { get; }

        public IReadOnlyList<SlotArray> SlotArray { get; }

        public IReadOnlyDictionary<Abilities, StatPercentageAbilities> StatPercentAbilities { get; }

        public IReadOnlyList<TemporaryCharacterLimitBreaks> TemporaryCharacterLimitBreaks { get; }

        public IReadOnlyList<WeaponsData> WeaponsData { get; }

        public IReadOnlyList<ZellLimitBreakParameters> ZellLimitBreakParameters { get; }

        private Memory.Archive ArchiveString { get; } = Memory.Archives.A_MAIN;

        #endregion Properties

        #region Methods

        public static KernelBin CreateInstance() => new KernelBin();

        #endregion Methods
    }
}