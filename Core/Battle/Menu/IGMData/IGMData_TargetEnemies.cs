using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public class IGMData_TargetEnemies : IGMData.Base
        {
            #region Methods

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

            #region Constructors

            public static IGMData_TargetEnemies Create(Rectangle pos) => 
                Create<IGMData_TargetEnemies>(6, 1, new IGMDataItem.Box(pos: pos, title: Icons.ID.TARGET), 2, 3);

            #endregion Constructors

            #region Properties

            public IGMData_TargetParty Target_Party { get; set; }

            #endregion Properties

            public override void Inputs_Left()
            {
                if (CURSOR_SELECT - Rows < 0)
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Target_Party.Cursor_Status |= Cursor_Status.Enabled;
                    Target_Party.CURSOR_SELECT = CURSOR_SELECT % Rows;
                    while (Target_Party.BLANKS[Target_Party.CURSOR_SELECT] && Target_Party.CURSOR_SELECT > 0)
                    {
                        Target_Party.CURSOR_SELECT--;
                    }
                }
                else
                {
                    SetCursor_select(CURSOR_SELECT - Rows);
                    while (BLANKS[CURSOR_SELECT] && CURSOR_SELECT != 0)
                    {
                        CURSOR_SELECT--;
                    }
                }
                base.Inputs_Left();
            }

            public override bool Inputs_OKAY() => false;

            public override void Inputs_Right()
            {
                if (CURSOR_SELECT + Rows > Count || (ITEM[CURSOR_SELECT + Rows, 0] == null || !ITEM[CURSOR_SELECT + Rows, 0].Enabled) || BLANKS[CURSOR_SELECT + Rows])
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Target_Party.Cursor_Status |= Cursor_Status.Enabled;
                    Target_Party.CURSOR_SELECT = CURSOR_SELECT % Rows;
                    while (Target_Party.BLANKS[Target_Party.CURSOR_SELECT] && Target_Party.CURSOR_SELECT > 0)
                    {
                        Target_Party.CURSOR_SELECT--;
                    }
                }
                else
                {
                    SetCursor_select(CURSOR_SELECT + Rows);
                    while (BLANKS[CURSOR_SELECT] && CURSOR_SELECT != 0)
                    {
                        CURSOR_SELECT--;
                    }
                }
                base.Inputs_Right();
            }

            public override void Refresh()
            {
                if (Memory.State?.Characters != null)
                {
                    int pos = 0;
                    if (Enemy.Party != null)
                    {
                        foreach (Enemy e in Enemy.Party)
                        {
                            //if(e.EII)
                            ITEM[pos, 0] = new IGMDataItem.Text { Data = e.Name, Pos = SIZE[pos], FontColor = Font.ColorID.White };
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
        }

        #endregion Classes
    }
}