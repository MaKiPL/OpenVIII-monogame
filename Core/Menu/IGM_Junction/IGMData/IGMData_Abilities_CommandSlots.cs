using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Abilities_CommandSlots : IGMData
        {
            #region Constructors

            public IGMData_Abilities_CommandSlots() : base(4, 2, new IGMDataItem_Box(pos: new Rectangle(0, 198, 435, 216), title: Icons.ID.COMMAND), 1, 4)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                base.Refresh();

                if (Memory.State.Characters != null && Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    for (int i = 0; i < Count; i++)
                    {
                        if (i == 0)
                        {
                            ITEM[i, 1] = new IGMDataItem_String(
                                    Kernel_bin.BattleCommands[
                                        c.Abilities.Contains(Kernel_bin.Abilities.Mug) ?
                                        12 :
                                        1].Name,
                                    new Rectangle(SIZE[i].X + 80, SIZE[i].Y, 0, 0));
                        }
                        else
                        {
                            ITEM[i, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Right2, SIZE[i], 9);
                            ITEM[i, 1] = c.Commands[i - 1] != Kernel_bin.Abilities.None ? new IGMDataItem_String(
                                Icons.ID.Ability_Command, 9,
                            Kernel_bin.Commandabilities[c.Commands[i - 1]].Name,
                            new Rectangle(SIZE[i].X + 40, SIZE[i].Y, 0, 0)) : null;
                            Kernel_bin.Abilities k = c.Commands[i - 1];
                            Descriptions[i] = k == Kernel_bin.Abilities.None ? null : Kernel_bin.Commandabilities[k].BattleCommand.Description;
                        }
                    }
                }
            }

            protected override void Init()
            {
                base.Init();
                CURSOR[0] = Point.Zero; //disable this cursor location
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

        #endregion Classes
    }
}