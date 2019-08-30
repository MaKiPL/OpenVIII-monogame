using Microsoft.Xna.Framework;

namespace OpenVIII
{
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

        #region Properties

        public BattleMenus.IGMData_TargetGroup Target_Group => (BattleMenus.IGMData_TargetGroup)(((IGMDataItem_IGMData)ITEM[Targets_Window, 0]).Data);

        private int Targets_Window => Rows;

        #endregion Properties

        #region Methods

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

            ITEM[Targets_Window, 0] = new IGMDataItem_IGMData(new BattleMenus.IGMData_TargetGroup(Character,VisableCharacter,false));
            
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
        public override bool Inputs_CANCEL()
        {
            Hide();
            return base.Inputs_CANCEL();
        }
        #endregion Methods
    }
}