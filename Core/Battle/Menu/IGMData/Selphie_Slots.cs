using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII.IGMData
{
    internal class Selphie_Slots : Base
    {
        #region Fields

        private Kernel_bin.Magic_Data _magicData;

        #endregion Fields

        #region Properties

        public int Casts { get => Number.Data; private set => Number.Data = value; }

        public Kernel_bin.Magic_Data MagicData
        {
            get => _magicData; private set
            {
                _magicData = value;
                Name = value.Name;
            }
        }

        public FF8String Name { get => Spell.Data; private set => Spell.Data = value; }

        private IGMDataItem.Text Cast { get => (IGMDataItem.Text)ITEM[6, 0]; set => ITEM[6, 0] = value; }

        private IGMDataItem.Text Do_Over { get => (IGMDataItem.Text)ITEM[3, 0]; set => ITEM[3, 0] = value; }

        private IGMDataItem.Integer Number { get => (IGMDataItem.Integer)ITEM[1, 0]; set => ITEM[1, 0] = value; }

        private IGMDataItem.Text Spell { get => (IGMDataItem.Text)ITEM[0, 0]; set => ITEM[0, 0] = value; }

        private Kernel_bin.Slot Spell_Data
        {
            get
            {
                IReadOnlyList<Kernel_bin.Slot> spells = Spells;
                return spells[Memory.Random.Next(spells.Count)];
            }
        }

        /// <summary>
        /// List of Spells
        /// </summary>
        private IReadOnlyList<Kernel_bin.Slot> Spells => Kernel_bin.Selphielimitbreaksets[SpellSet.SlotID].Slots;

        /// <summary>
        /// Spell set
        /// </summary>
        private Kernel_bin.Slot_array SpellSet => Kernel_bin.Slotarray[MathHelper.Clamp(TombolaLevel * 12 + TombolaLevel, 0, Kernel_bin.Slotarray.Count)];

        private Target.Group TargetGroup { get => (Target.Group)ITEM[8, 0]; set => ITEM[8, 0] = value; }

        private IGMDataItem.Text Times { get => (IGMDataItem.Text)ITEM[2, 0]; set => ITEM[2, 0] = value; }

        /// <summary>
        /// Selphie's TombolaLevel
        /// </summary>
        private short TombolaLevel => Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c) ? checked((short)(Damageable.Level / 10 + c.CurrentCrisisLevel + Memory.Random.Next(5) - 1)) : (short)0;

        /// <summary>
        /// Selphie's TombolaLevel
        /// </summary>
        private byte TombolaMod
        {
            get
            {
                byte rnd = checked((byte)Memory.Random.Next(256));
                if (rnd >= 249) return 4;
                else if (rnd >= 209) return 3;
                else if (rnd >= 159) return 2;
                else if (rnd >= 39) return 1;
                else return 0;
            }
        }

        #endregion Properties

        #region Methods

        public override bool Inputs_CANCEL()
        {
            base.Inputs_CANCEL();
            Refresh();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            switch (CURSOR_SELECT)
            {
                case 0:
                    Inputs_CANCEL();
                    break;

                case 1:
                    base.Inputs_OKAY();
                    if (MagicData != null)
                    {
                        // will probably need an extra method to make sure positive spells cast only on party members.
                        // and negative spells only hit the enemies.

                        //Single targets are random but if cursor is on enemy it will only hit an enemy.
                        // and if it's on a party member it'll only hit a party member.
                        TargetGroup?.SelectTargetWindows(MagicData, Casts, new Target.Random { Single = true });
                        // show target window so target selection code works.
                        TargetGroup?.ShowTargetWindows();
                        // Execute the spells.
                        // This should hide the targetwindow as turn ends.
                        TargetGroup?.Execute();
                    }
                    break;
            }
            return true;
        }

        public override void Refresh()
        {
            Kernel_bin.Slot spell_data = Spell_Data;
            MagicData = spell_data.Magic_Data;
            Casts = spell_data.Casts;
            SetCursor_select(3);
            base.Refresh();
        }

        protected override void Init()
        {
            base.Init();

            // 0,1,2
            // 3,4,5
            // 6,7,8

            Spell = new IGMDataItem.Text { Pos = SIZE[0] };
            BLANKS[0] = true;
            Number = new IGMDataItem.Integer { Pos = SIZE[1], Spaces = 3 };
            BLANKS[1] = true;
            Times = new IGMDataItem.Text { Data = Memory.Strings[Strings.FileID.KERNEL][30, 65], Pos = SIZE[2] };
            BLANKS[2] = true;
            Do_Over = new IGMDataItem.Text { Data = Memory.Strings[Strings.FileID.KERNEL][30, 67], Pos = SIZE[3] };
            BLANKS[3] = false;
            Cast = new IGMDataItem.Text { Data = Memory.Strings[Strings.FileID.KERNEL][30, 66], Pos = SIZE[6] };
            BLANKS[6] = false;
            TargetGroup = Target.Group.Create(Damageable);
            TargetGroup.Hide();
        }

        private Selphie_Slots Create(Rectangle pos, Damageable damageable = null) => Create<Selphie_Slots>(9, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.SPECIAL }, 3, 3);

        #endregion Methods
    }
}