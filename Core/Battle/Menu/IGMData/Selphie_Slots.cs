using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OpenVIII.IGMData.Target;
using OpenVIII.IGMDataItem;
using OpenVIII.Kernel;

namespace OpenVIII.IGMData
{
    public class SelphieSlots : Base
    {
        #region Fields

        private MagicData _magicData;


        #endregion Fields

        #region Properties

        public int Casts { get => Number.Data; private set => Number.Data = value; }

        public MagicData MagicData
        {
            get => _magicData; private set
            {
                _magicData = value;
                Name = value?.Name;
            }
        }

        public FF8String Name { get => Spell.Data; private set => Spell.Data = value; }

        private Text Cast { get => (Text)ITEM[6, 0]; set => ITEM[6, 0] = value; }

        private Text DoOver { get => (Text)ITEM[3, 0]; set => ITEM[3, 0] = value; }

        private Integer Number { get => (Integer)ITEM[1, 0]; set => ITEM[1, 0] = value; }

        private Text Spell { get => (Text)ITEM[0, 0]; set => ITEM[0, 0] = value; }

        private Slot SpellData
        {
            get
            {
                if (Spells == null) return default;
                var spells = Spells;
                return spells[Memory.Random.Next(spells.Count)];
            }
        }

        /// <summary>
        /// List of Spells
        /// </summary>
        private IReadOnlyList<Slot> Spells => (SpellSet?.SlotID ?? -1) == -1 ? null : Memory.Kernel_Bin.SelphieLimitBreakSets[SpellSet.SlotID]?.Slots;

        /// <summary>
        /// Spell set
        /// </summary>
        private SlotArray SpellSet => Memory.Kernel_Bin.SlotArray != null ? Memory.Kernel_Bin.SlotArray[MathHelper.Clamp((TombolaLevel * 12) + TombolaLevel, 0, Memory.Kernel_Bin.SlotArray.Count - 1)] : null;

        private Target.Group TargetGroup { get => (Target.Group)ITEM[8, 0]; set => ITEM[8, 0] = value; }

        private Text Times { get => (Text)ITEM[2, 0]; set => ITEM[2, 0] = value; }

        /// <summary>
        /// Selphie's TombolaLevel
        /// </summary>
        private short TombolaLevel => Damageable != null && Damageable.GetCharacterData(out var c) ? checked((short)(Damageable.Level / 10 + c.CurrentCrisisLevel + Memory.Random.Next(5) - 1)) : (short)0;

        /// <summary>
        /// Selphie's TombolaLevel
        /// </summary>
        private byte TombolaMod
        {
            get
            {
                var rnd = checked((byte)Memory.Random.Next(256));
                if (rnd >= 249) return 4;
                if (rnd >= 209) return 3;
                if (rnd >= 159) return 2;
                if (rnd >= 39) return 1;
                return 0;
            }
        }

        #endregion Properties

        #region Methods

        public override bool Inputs_CANCEL()
        {
            base.Inputs_OKAY();
            Refresh();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            switch (CURSOR_SELECT)
            {
                case 3:
                    Inputs_CANCEL();
                    break;

                case 6:
                    base.Inputs_OKAY();
                    if (MagicData != null)
                    {
                        // will probably need an extra method to make sure positive spells cast only on party members.
                        // and negative spells only hit the enemies.

                        //Single targets are random but if cursor is on enemy it will only hit an enemy.
                        // and if it's on a party member it'll only hit a party member.
                        TargetGroup?.SelectTargetWindows(MagicData, Casts, new Random { Single = true, PositiveMatters = true });
                        // show target window so target selection code works.
                        TargetGroup?.ShowTargetWindows();
                        // Execute the spells.
                        // This should hide the targetwindow as turn ends.
                        TargetGroup?.Execute();
                        Hide();
                    }
                    else
                        Inputs_CANCEL();
                    break;
            }
            return true;
        }

        public override void Refresh()
        {
            var spell_data = SpellData;
            MagicData = spell_data.MagicData;
            Casts = spell_data.Casts;
            SetCursor_select(3);
            base.Refresh();
        }

        protected override void Init()
        {
            Table_Options |= Table_Options.FillRows;
            base.Init();

            // 0,1,2
            // 3,4,5
            // 6,7,8
            BLANKS.SetAll(true);
            Spell = new Text { Pos = SIZE[0] };
            Number = new Integer { Pos = SIZE[1], Spaces = 3, NumType = Icons.NumType.SysFntBig };
            Times = new Text { Data = Memory.Strings[Strings.FileID.Kernel][30, 65], Pos = SIZE[2] };
            DoOver = new Text { Data = Memory.Strings[Strings.FileID.Kernel][30, 67], Pos = SIZE[3] };
            BLANKS[3] = false;
            Cast = new Text { Data = Memory.Strings[Strings.FileID.Kernel][30, 66], Pos = SIZE[6] };
            BLANKS[6] = false;
            TargetGroup = Target.Group.Create(Damageable);
            TargetGroup.Hide();
            Cursor_Status |= Cursor_Status.Enabled;
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -12);
            if (row == 0) SIZE[i].Offset(0, 8);
            if (col == 1) SIZE[i].Offset(8, 0);
            if (col == 2) SIZE[i].Offset(-16, 0);
            if (row > 0) SIZE[i].Offset(20, 0);
        }

        public static SelphieSlots Create(Rectangle pos, Damageable damageable = null, bool battle = true) => Create<SelphieSlots>(9, 1, new Box { Pos = pos, Title = Icons.ID.SPECIAL }, 3, 3, damageable, battle: battle);

        #endregion Methods
    }
}
