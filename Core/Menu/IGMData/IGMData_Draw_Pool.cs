using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenVIII
{
    /// <summary>
    /// (Draw) or (Cast and display target window)
    /// </summary>
    public class IGMData_Draw_Commands : IGMData
    {
        #region Fields

        private Debug_battleDat.Magic Magic;

        #endregion Fields

        #region Methods

        private bool Inputs_OKAY_Cast()
        {
            Debug.WriteLine($"{Memory.Strings.GetName(VisableCharacter)} Casting {Magic.Name}({Magic.ID}) from enemy.");
            Target_Group.ShowTargetWindows();
            return true;
        }

        private bool Inputs_OKAY_Draw()
        {
            Debug.WriteLine($"{Memory.Strings.GetName(VisableCharacter)} Drawing {Magic.Name}({Magic.ID}) from enemy.");
            return true;
        }

        protected override void Init()
        {
            base.Init();
            ITEM[_Draw, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.KERNEL, 0, 12), SIZE[_Draw]);
            ITEM[Cast, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.KERNEL, 0, 18), SIZE[Cast]);
            ITEM[Targets_Window, 0] = new BattleMenus.IGMData_TargetGroup(Character, VisableCharacter, false);
            Cursor_Status = Cursor_Status.Enabled;
            OKAY_Actions = new Dictionary<int, Func<bool>>
            {
                {_Draw, Inputs_OKAY_Draw },
                {Cast, Inputs_OKAY_Cast },
            };
            PointerZIndex = 0;
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

        public Dictionary<int, Func<bool>> OKAY_Actions;

        public IGMData_Draw_Commands(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(3, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.CHOICE), 1, 2, character, visablecharacter)
        {
        }

        public int _Draw => 0;
        public int Cast => 1;
        public BattleMenus.IGMData_TargetGroup Target_Group => (BattleMenus.IGMData_TargetGroup)(((IGMData)ITEM[Targets_Window, 0]));
        public int Targets_Window => Count - 1;

        public override bool Inputs()
        {
            if (Target_Group.Enabled)
            {
                Cursor_Status |= Cursor_Status.Blinking;
                return Target_Group.Inputs();
            }
            else
            {
                Cursor_Status &= ~Cursor_Status.Blinking;
                return base.Inputs();
            }
        }

        public override bool Inputs_CANCEL()
        {
            Hide();
            return true;
        }

        public override bool Inputs_OKAY()
        {
            bool ret = false;
            if (OKAY_Actions.TryGetValue(CURSOR_SELECT, out Func<bool> func))
                ret = func();
            if(ret)
                base.Inputs_OKAY();
            return ret;
        }

        public override void Refresh()
        {
            if (Magic.ID > 0)
            {
                bool gf = Magic.GF != GFs.Blank;
                bool full = (Memory.State[Character].Magics.TryGetByKey(Magic.ID, out byte qty) && qty < 100);
                bool candraw = gf || !full;
                if (!candraw)
                {
                    ((IGMDataItem_String)ITEM[_Draw, 0]).FontColor = Font.ColorID.Dark_Gray;
                    BLANKS[_Draw] = true;
                }
                else
                {
                    ((IGMDataItem_String)ITEM[_Draw, 0]).FontColor = Font.ColorID.White;
                    BLANKS[_Draw] = false;
                }
                if (gf)
                {
                    ((IGMDataItem_String)ITEM[Cast, 0]).FontColor = Font.ColorID.Dark_Gray;
                    BLANKS[_Draw] = true;
                }
                else
                {
                    Target_Group.SelectTargetWindows(Magic.DATA);
                    ((IGMDataItem_String)ITEM[Cast, 0]).FontColor = Font.ColorID.White;
                    BLANKS[_Draw] = false;
                }
            }
            base.Refresh();
        }

        public void Refresh(Debug_battleDat.Magic magic)
        {
            if (Magic.ID != magic.ID)
            {
                Magic = magic;
                Refresh();
            }
        }
    }

    public class IGMData_Draw_Pool : IGMData_Pool<Saves.Data, Debug_battleDat.Magic>
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
                ITEM[pos, 0] = new IGMDataItem_String(null, SIZE[pos]);
                ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.JunctionSYM, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 60, SIZE[pos].Y, 0, 0));
                ITEM[pos, 2] = new IGMData_Draw_Commands(r, Character, VisableCharacter, Battle);
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

        public IGMData_Draw_Pool(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(5, 3, new IGMDataItem_Box(pos: pos, title: Icons.ID.CHOICE), 4, 1, character, visablecharacter)
        {
            Battle = battle;
            Refresh();
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
            if (Source != null && Character != Characters.Blank)
            {
                byte pos = 0;
                int skip = Page * Rows;
                for (byte i = 0; pos < Rows && i < Contents.Length; i++)
                {
                    bool unlocked = Source.UnlockedGFs().Contains(Contents[i].GF);
                    bool junctioned = Source[Character].Stat_J.ContainsValue(Contents[i].ID);
                    ((IGMDataItem_String)(ITEM[pos, 0])).Data = Contents[i].Name;
                    ((IGMDataItem_String)(ITEM[pos, 0])).Show();
                    if (junctioned)
                        ((IGMDataItem_Icon)(ITEM[pos, 1])).Show();
                    else
                        ((IGMDataItem_Icon)(ITEM[pos, 1])).Hide();
                    ((IGMData_Draw_Commands)ITEM[pos, 2]).Refresh(Contents[i]);
                    BLANKS[pos] = false;
                    pos++;
                }
                for (; pos < Rows; pos++)
                {
                    ((IGMDataItem_String)(ITEM[pos, 0])).Hide();
                    ((IGMDataItem_Icon)(ITEM[pos, 1])).Hide();
                    BLANKS[pos] = true;
                }
            }
        }
    }
}