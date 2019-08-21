using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #endregion Methods
        #region Classes

        public class IGMData_TargetEnemies : IGMData
        {
            #region Constructors

            public IGMData_TargetEnemies(Rectangle pos) : base(6, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.TARGET), 2, 3)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                if (Memory.State?.Characters != null)
                {
                    int pos = 0;
                    if (Enemy.EnemyParty != null)
                    {
                        foreach (Enemy e in Enemy.EnemyParty)
                        {
                            //if(e.EII)
                            ITEM[pos, 0] = new IGMDataItem_String(e.Name, SIZE[pos], Font.ColorID.White);
                            ITEM[pos, 0].Show();
                            BLANKS[pos] = false;

                            pos++;
                        }
                        for (; pos < Count; pos++)
                        {
                            ITEM[pos, 0]?.Hide();
                            BLANKS[pos] = true;
                        }
                    }
                }
            }

            public override void Inputs_Left()
            {
                if (CURSOR_SELECT < 3)
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.Cursor_Status |= Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.CURSOR_SELECT = CURSOR_SELECT % 3;
                }
                else
                {
                    SetCursor_select(CURSOR_SELECT - 3);
                }
                base.Inputs_Left();
            }
            public override void Inputs_Right()
            {
                if (CURSOR_SELECT >= 3 || (ITEM[3, 0] == null || !ITEM[3, 0].Enabled))
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.Cursor_Status |= Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.CURSOR_SELECT = CURSOR_SELECT % 3;
                }
                else
                {
                    SetCursor_select(CURSOR_SELECT + 3);
                }
                base.Inputs_Right();
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-18, -20);
                SIZE[i].Y -= 7 * row + 2;
                //SIZE[i].Inflate(-22, -8);
                //SIZE[i].Offset(0, 12 + (-8 * row));
                SIZE[i].Height = (int)(12 * TextScale.Y);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}