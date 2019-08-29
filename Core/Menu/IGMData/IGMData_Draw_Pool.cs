using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMData_Draw_Pool : IGMData_Pool<Saves.Data, Debug_battleDat.Magic>
    {
        #region Fields

        private bool Battle = true;
        private bool skipReinit = false;

        #endregion Fields

        #region Constructors

        public IGMData_Draw_Pool(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(5, 3, new IGMDataItem_Box(pos: pos, title: Icons.ID.MAGIC), 4, 13, character, visablecharacter)
        {
            Battle = battle;
            skipReinit = true;
            Refresh();
        }

        public IGMData_Draw_Pool() : base(6, 3, new IGMDataItem_Box(pos: new Rectangle(135, 150, 300, 192), title: Icons.ID.MAGIC), 4, 13)
        {
        }

        #endregion Constructors

        #region Properties

        public BattleMenus.IGMData_TargetGroup Target_Group => (BattleMenus.IGMData_TargetGroup)(((IGMDataItem_IGMData)ITEM[Targets_Window, 0]).Data);

        private int Targets_Window => Rows;

        #endregion Properties

        #region Methods

        public void Refresh(Debug_battleDat.Magic[] magics) => Contents = magics;

        public override void Refresh()
        {
            if (!Battle && !eventSet && Menu.IGM_Items != null)
            {
                Menu.IGM_Items.ModeChangeHandler += ModeChangeEvent;
                Menu.IGM_Items.ReInitCompletedHandler += ReInitCompletedEvent;
                eventSet = true;
            }
            base.Refresh();
            Source = Memory.State;
            if (Source != null && Source.Items != null)
            {
                ((IGMDataItem_Box)CONTAINER).Title = Pages <= 1 ? (Icons.ID?)Icons.ID.ITEM : (Icons.ID?)(Icons.ID.ITEM_PG1 + (byte)Page);
                byte pos = 0;
                int skip = Page * Rows;
                for (byte i = 0; pos < Rows && i < Source.Items.Count; i++)
                {
                    Saves.Item item = Source.Items[i];
                    if (item.ID == 0 || item.QTY == 0) continue; // skip empty values.
                    if (skip-- > 0) continue; //skip items that are on prev pages.
                    Item_In_Menu itemdata = item.DATA ?? new Item_In_Menu();
                    if (Battle && itemdata.Battle == null) continue;
                    if (itemdata.ID == 0) continue; // skip empty values.
                    Font.ColorID color = Font.ColorID.White;
                    byte palette = itemdata.Palette;
                    if (!itemdata.ValidTarget(Battle))
                    {
                        color = Font.ColorID.Grey;
                        BLANKS[pos] = true;
                        palette = itemdata.Faded_Palette;
                    }
                    else
                        BLANKS[pos] = false;
                    ((IGMDataItem_String)(ITEM[pos, 0])).Data = itemdata.Name;
                    ((IGMDataItem_String)(ITEM[pos, 0])).Icon = itemdata.Icon;
                    ((IGMDataItem_String)(ITEM[pos, 0])).Palette = palette;
                    ((IGMDataItem_String)(ITEM[pos, 0])).FontColor = color;
                    ((IGMDataItem_Int)(ITEM[pos, 1])).Data = item.QTY;
                    ((IGMDataItem_Int)(ITEM[pos, 1])).Show();
                    ((IGMDataItem_Int)(ITEM[pos, 1])).FontColor = color;
                    _helpStr[pos] = itemdata.Description;
                    Contents[pos] = itemdata;
                    pos++;
                }
                for (; pos < Rows; pos++)
                {
                    ((IGMDataItem_Int)(ITEM[pos, 1])).Hide();
                    if (pos == 0) return; // if page turning. this till be enough to trigger a try next page.
                    ((IGMDataItem_String)(ITEM[pos, 0])).Data = null;
                    ((IGMDataItem_Int)(ITEM[pos, 1])).Data = 0;
                    ((IGMDataItem_String)(ITEM[pos, 0])).Icon = Icons.ID.None;
                    BLANKS[pos] = true;
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            _helpStr = new FF8String[Count];
            for (byte pos = 0; pos < Rows; pos++)
            {
                ITEM[pos, 0] = new IGMDataItem_String(null, SIZE[pos]);
                ITEM[pos, 1] = new IGMDataItem_Int(0, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 60, SIZE[pos].Y, 0, 0), numtype: Icons.NumType.sysFntBig, spaces: 3);
            }
            ITEM[Targets_Window, 0] = new IGMDataItem_IGMData(new BattleMenus.IGMData_TargetGroup());
            ITEM[Rows - 1, 2] = new IGMDataItem_Icon(Icons.ID.NUM_, new Rectangle(SIZE[Rows - 1].X + SIZE[Rows - 1].Width - 60, Y, 0, 0), scale: new Vector2(2.5f));
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