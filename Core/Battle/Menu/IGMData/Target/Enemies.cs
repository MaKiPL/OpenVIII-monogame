using System.Linq;
using Microsoft.Xna.Framework;
using OpenVIII.IGMDataItem;

namespace OpenVIII.IGMData.Target
{
    public class Enemies : Base
    {
        #region Properties

        public Party Target_Party { get; set; }

        #endregion Properties

        #region Methods

        public static Enemies Create(Rectangle pos) =>
            Create<Enemies>(6, 1, new Box { Pos = pos, Title = Icons.ID.TARGET }, 2, 3);

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

        public void Random() => SetCursor_select(BLANKS.Cast<bool>().Select((enabled, index) => new { enabled, index }).Where(x => !x.enabled).Random().index);

        public override void Refresh()
        {
            if (Memory.State?.Characters != null)
            {
                var pos = 0;
                if (Enemy.Party != null)
                {
                    foreach (var e in Enemy.Party)
                    {
                        //if(e.EII)
                        ITEM[pos, 0] = new Text { Data = e.Name, Pos = SIZE[pos], FontColor = Font.ColorID.White };
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

        protected override void InitCursor(int i, int col, int row, bool zero = false)
        {
            base.InitCursor(i, col, row, zero);
            CURSOR[i].X += 8;
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-8, -20);
            SIZE[i].Offset(2 + (-4 * col), 0);
            SIZE[i].Y -= 7 * row + 2;
            //SIZE[i].Inflate(-22, -8);
            //SIZE[i].Offset(0, 12 + (-8 * row));
            SIZE[i].Height = (int)(12 * TextScale.Y);
        }

        #endregion Methods
    }
}