using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData.Pool
{

    public partial class Draw : IGMData.Pool.Base<Saves.Data, Debug_battleDat.Magic>
    {
        #region Fields

        private bool Battle = true;

        #endregion Fields

        #region Methods

        protected override void Init()
        {
            base.Init();
            Rectangle r = CONTAINER.Pos;
            r.Inflate(-Width * .25f,-Height *.25f);
            for (byte pos = 0; pos < Rows; pos++)
            {
                ITEM[pos, 0] = new IGMDataItem.Text { Pos = SIZE[pos] };
                ITEM[pos, 1] = new IGMDataItem.Icon { Data = Icons.ID.JunctionSYM, Pos = new Rectangle(SIZE[pos].X + SIZE[pos].Width - 60, SIZE[pos].Y, 0, 0) };
                ITEM[pos, 2] = Commands.Create(r, Damageable, Battle);
                ITEM[pos, 2].Hide();
            }

            DepthFirst = true;
            PointerZIndex = 0;
        }

        public override bool Inputs()
        {
            if (ITEM[CURSOR_SELECT, 2].Enabled)
            {
                Cursor_Status |= Cursor_Status.Blinking;
                return ITEM[CURSOR_SELECT, 2].Inputs();
            }
            else
                Cursor_Status &= ~Cursor_Status.Blinking;
            return base.Inputs();
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            //SIZE[i].Inflate(-18, -20);
            //SIZE[i].Y -= 5 * row;
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
            SIZE[i].Height = (int)(12 * TextScale.Y);
        }

        #endregion Methods

        #region Constructors
        static public Draw Create(Rectangle pos, Damageable damageable, bool battle = false)
        {
            Draw r = Create<Draw>(5, 3, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.CHOICE }, 4, 1, damageable);
            r.Battle = battle;
            r.Refresh();
            return r;
        }


        #endregion Constructors

        public override bool Inputs_CANCEL()
        {
            Hide();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            ITEM[CURSOR_SELECT, 2]?.Show();
            return base.Inputs_OKAY();
        }

        public void Refresh(Debug_battleDat.Magic[] magics)
        {
            Contents = magics;
            Refresh();
        }

        public override void Refresh()
        {
            base.Refresh();
            Source = Memory.State;
            if (Source != null && Damageable != null)
            {
                byte pos = 0;
                int skip = Page * Rows;
                for (byte i = 0; pos < Rows && i < Contents.Length; i++)
                {
                    bool unlocked = Source.UnlockedGFs().Contains(Contents[i].GF);
                    bool junctioned = (Damageable.GetCharacterData(out Saves.CharacterData c) && c.Stat_J.ContainsValue(Contents[i].ID));
                    ((IGMDataItem.Text)(ITEM[pos, 0])).Data = Contents[i].Name;
                    ((IGMDataItem.Text)(ITEM[pos, 0])).Show();
                    if (junctioned)
                        ((IGMDataItem.Icon)(ITEM[pos, 1])).Show();
                    else
                        ((IGMDataItem.Icon)(ITEM[pos, 1])).Hide();
                    ((Commands)ITEM[pos, 2]).Refresh(Contents[i]);
                    BLANKS[pos] = false;
                    pos++;
                }
                for (; pos < Rows; pos++)
                {
                    ((IGMDataItem.Text)(ITEM[pos, 0])).Hide();
                    ((IGMDataItem.Icon)(ITEM[pos, 1])).Hide();
                    BLANKS[pos] = true;
                }
            }
        }
    }
}