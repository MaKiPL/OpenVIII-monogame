using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData.Slots
{
    public class Command : IGMData.Base
    {
        #region Methods

        public static Command Create() => Create<Command>(4, 2, new IGMDataItem.Box { Pos = new Rectangle(0, 198, 435, 216), Title = Icons.ID.COMMAND }, 1, 4);

        public override void Refresh()
        {
            base.Refresh();

            if (Memory.State.Characters != null && Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
            {
                for (int i = 0; i < Count; i++)
                {
                    if (i > 0)
                    {
                        if (c.Commands[i - 1] != Kernel_bin.Abilities.None)
                        {
                            ((IGMDataItem.Text)ITEM[i, 1]).Data = Kernel_bin.Commandabilities[c.Commands[i - 1]].Name;
                            ITEM[i, 1].Show();
                            Kernel_bin.Abilities k = c.Commands[i - 1];
                            Descriptions[i] = Kernel_bin.Commandabilities[k].BattleCommand.Description;
                        }
                        else
                        {
                            ITEM[i, 1].Hide();
                        }
                    }
                    else if (i == 0)
                    {
                        ((IGMDataItem.Text)ITEM[i, 1]).Data = Kernel_bin.BattleCommands[
                                    c.Abilities.Contains(Kernel_bin.Abilities.Mug) ?
                                    12 :
                                    1].Name;
                    }
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            CURSOR[0] = Point.Zero; //disable this cursor location

            for (int i = 0; i < Count; i++)
            {
                if (i > 0)
                {
                    ITEM[i, 0] = new IGMDataItem.Icon { Data = Icons.ID.Arrow_Right2, Pos = SIZE[i], Palette = 9 };
                    ITEM[i, 1] = new IGMDataItem.Text
                    {
                        Icon = Icons.ID.Ability_Command,
                        Palette = 9,
                        Pos = new Rectangle(SIZE[i].X + 40, SIZE[i].Y, 0, 0)
                    };
                }
                else if (i == 0)
                    ITEM[i, 1] = new IGMDataItem.Text
                    {
                        Pos = new Rectangle(SIZE[i].X + 80, SIZE[i].Y, 0, 0)
                    };
            }
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
            CURSOR[i].X += 40;
        }

        #endregion Methods
    }
}