using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII.IGMData
{
    internal class Selphie_Slots : Base
    {
        private Selphie_Slots Create(Rectangle pos, Damageable damageable = null) => Create<Selphie_Slots>(9, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.SPECIAL }, 3, 3);

        // 0,1,2
        // 3,4,5
        // 6,7,8
        public IGMDataItem.Text Spell { get => (IGMDataItem.Text)ITEM[0, 0]; set => ITEM[0, 0] = value; }
        public IGMDataItem.Integer Number { get => (IGMDataItem.Integer)ITEM[1, 0]; set => ITEM[1, 0] = value; }
        public IGMDataItem.Text Times { get => (IGMDataItem.Text)ITEM[2, 0]; set => ITEM[2, 0] = value; }
        public IGMDataItem.Text Do_Over { get => (IGMDataItem.Text)ITEM[3, 0]; set => ITEM[3, 0] = value; }
        public IGMDataItem.Text Cast { get => (IGMDataItem.Text)ITEM[6, 0]; set => ITEM[6, 0] = value; }

        protected override void Init()
        {
            base.Init();

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
        }

        /// <summary>
        /// Selphie's TombolaLevel
        /// </summary>
        private short TombolaLevel => Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c) ? checked((short)(Damageable.Level / 10 + c.CurrentCrisisLevel + Memory.Random.Next(5) - 1)) : (short)0;

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

        private Kernel_bin.Slot_array SpellSet => Kernel_bin.Slotarray[MathHelper.Clamp(TombolaLevel * 12 + TombolaLevel, 0, Kernel_bin.Slotarray.Count)];

        private IReadOnlyList<Kernel_bin.Slot> Spells => Kernel_bin.Selphielimitbreaksets[SpellSet.SlotID].Slots;

        public Kernel_bin.Magic_Data MagicData { get; private set; }

        public override void Refresh()
        {
            var spells = Spells;
            var i = Memory.Random.Next(Spells.Count);
            MagicData = Kernel_bin.MagicData[Spells[i].MagicID];
            Spell.Data = MagicData.Name;
            Number.Data = Memory.Random.Next(1,Spells[i].Count+1);
            base.Refresh();
        }
        public override bool Inputs_OKAY()
        {
            return true;
        }

        public override bool Inputs_CANCEL()
        {
            base.Inputs_CANCEL();
            Refresh();
            return true;
        }
    }
}