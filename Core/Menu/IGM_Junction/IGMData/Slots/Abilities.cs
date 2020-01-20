using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData.Slots
{
    public class Abilities : IGMData.Base
    {
        #region Methods

        public static Abilities Create() => Create<Abilities>(4, 2, new IGMDataItem.Box { Pos = new Rectangle(0, 414, 435, 216), Title = Icons.ID.ABILITY }, 1, 4);

        public override void Refresh()
        {
            base.Refresh();

            if (Memory.State?.Characters != null && Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
            {
                for (int i = 0; i < Count; i++)
                {
                    int slots = 2;
                    if (c.UnlockedGFAbilities.Contains(Kernel_bin.Abilities.Abilityx3))
                        slots = 3;
                    if (c.UnlockedGFAbilities.Contains(Kernel_bin.Abilities.Abilityx4))
                        slots = 4;
                    if (i < slots)
                    {
                        ITEM[i, 0].Show();
                        if (c.Abilities[i] != Kernel_bin.Abilities.None)
                        {
                            ((IGMDataItem.Text)ITEM[i, 1]).Icon = Kernel_bin.EquipableAbilities[c.Abilities[i]].Icon;
                            ((IGMDataItem.Text)ITEM[i, 1]).Data = Kernel_bin.EquipableAbilities[c.Abilities[i]].Name;
                            ((IGMDataItem.Text)ITEM[i, 1]).Show();
                            Descriptions[i] = Kernel_bin.EquipableAbilities[c.Abilities[i]].Description;
                        }
                        else
                        {
                            ITEM[i, 1].Hide();
                        }
                        BLANKS[i] = false;
                    }
                    else
                    {
                        ITEM[i, 0].Hide();
                        ITEM[i, 1].Hide();
                        BLANKS[i] = true;
                    }
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            for (int i = 0; i < Count; i++)
            {
                ITEM[i, 0] = new IGMDataItem.Icon { Data = Icons.ID.Arrow_Right2, Pos = SIZE[i], Palette = 9 };
                ITEM[i, 0].Hide();
                ITEM[i, 1] = new IGMDataItem.Text
                {
                    Palette = 9,
                    Pos = new Rectangle(SIZE[i].X + 40, SIZE[i].Y, 0, 0)
                };
                ITEM[i, 1].Hide();
            }
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(80, 12 + (-8 * row));
            CURSOR[i].X += 40;
        }

        #endregion Methods
    }
}