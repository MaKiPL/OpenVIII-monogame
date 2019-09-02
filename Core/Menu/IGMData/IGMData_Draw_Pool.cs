using Microsoft.Xna.Framework;

namespace OpenVIII
{
    /// <summary>
    /// (Draw) or (Cast and display target window)
    /// </summary>
    public class IGMData_Draw_Commands : IGMData
    {
        #region Constructors

        public IGMData_Draw_Commands(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(3, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.CHOICE), 1, 2, character, visablecharacter)
        {
        }

        #endregion Constructors

        #region Properties

        public int _Draw => 0;
        public int Cast => 1;
        public BattleMenus.IGMData_TargetGroup Target_Group => (BattleMenus.IGMData_TargetGroup)(((IGMDataItem_IGMData)ITEM[Targets_Window, 0]).Data);
        public int Targets_Window => Count - 1;

        #endregion Properties

        #region Methods

        protected override void Init()
        {
            base.Init();
            ITEM[_Draw, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.KERNEL, 0, 12), SIZE[_Draw]);
            ITEM[Cast, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.KERNEL, 0, 18), SIZE[Cast]);
            ITEM[Targets_Window, 0] = new IGMDataItem_IGMData(new BattleMenus.IGMData_TargetGroup(Character, VisableCharacter, false));
            Cursor_Status = Cursor_Status.Enabled;
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
        public override bool Inputs()
        {
            if (Target_Group.Enabled)
                return Target_Group.Inputs();
            else return base.Inputs();
        }
        public override void Refresh(Characters character, Characters? visablecharacter = null)
        {
            base.Refresh(character, visablecharacter);
        }
        #endregion Methods
    }

    public class IGMData_Draw_Pool : IGMData_Pool<Saves.Data, Debug_battleDat.Magic>
    {
        #region Fields

        private bool Battle = true;

        #endregion Fields

        #region Constructors

        public IGMData_Draw_Pool(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(5, 3, new IGMDataItem_Box(pos: pos, title: Icons.ID.CHOICE), 4, 1, character, visablecharacter)
        {
            Battle = battle;
            Refresh();
        }

        #endregion Constructors

        #region Methods

        public override bool Inputs_CANCEL()
        {
            Hide();
            return base.Inputs_CANCEL();
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
            if (Source != null && Source.Items != null && Character != Characters.Blank)
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

        protected override void Init()
        {
            base.Init();
            for (byte pos = 0; pos < Rows; pos++)
            {
                ITEM[pos, 0] = new IGMDataItem_String(null, SIZE[pos]);
                ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.JunctionSYM, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 60, SIZE[pos].Y, 0, 0));
            }

            PointerZIndex = Rows - 1;
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
    }
}