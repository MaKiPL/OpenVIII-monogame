using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace OpenVIII
{
    namespace IGMData
    {
        public class GameBlock : IGMData.Base, I_Data<Saves.Data>
        {
            #region Fields

            private object _lastpage = -1;
            private object _page = -1;

            #endregion Fields

            #region Properties

            public Saves.Data Data { get; set; }

            public byte ID
            {
                get => checked((byte)(BlockNumber?.Data ?? 0)); private set
                {
                    if (BlockNumber != null)
                        BlockNumber.Data = value;
                }
            }

            private int _rows => 3;

            private IGMDataItem.Integer BlockNumber { get => (IGMDataItem.Integer)ITEM[0, 0]; set => ITEM[0, 0] = value; }

            private IGMDataItem.Icon Colon { get => (IGMDataItem.Icon)ITEM[0, 11]; set => ITEM[0, 11] = value; }

            private IGMDataItem.Icon Disc { get => (IGMDataItem.Icon)ITEM[0, 7]; set => ITEM[0, 7] = value; }

            private IGMDataItem.Integer Disc_Num { get => (IGMDataItem.Integer)ITEM[0, 8]; set => ITEM[0, 8] = value; }

            private int expectedpage => (ID - 1) / _rows;

            private IGMDataItem.Face Face1 { get => (IGMDataItem.Face)ITEM[0, 1]; set => ITEM[0, 1] = value; }

            private IGMDataItem.Face Face2 { get => (IGMDataItem.Face)ITEM[0, 2]; set => ITEM[0, 2] = value; }

            private IGMDataItem.Face Face3 { get => (IGMDataItem.Face)ITEM[0, 3]; set => ITEM[0, 3] = value; }

            private IGMDataItem.Icon G { get => (IGMDataItem.Icon)ITEM[0, 14]; set => ITEM[0, 14] = value; }

            private IGMDataItem.Integer Gil { get => (IGMDataItem.Integer)ITEM[0, 13]; set => ITEM[0, 13] = value; }

            private IGMDataItem.Integer Hours { get => (IGMDataItem.Integer)ITEM[0, 10]; set => ITEM[0, 10] = value; }

            private IGMDataItem.Box Location { get => (IGMDataItem.Box)ITEM[0, 15]; set => ITEM[0, 15] = value; }

            private IGMDataItem.Text LV { get => (IGMDataItem.Text)ITEM[0, 5]; set => ITEM[0, 5] = value; }

            private IGMDataItem.Integer LV_Num { get => (IGMDataItem.Integer)ITEM[0, 6]; set => ITEM[0, 6] = value; }

            private IGMDataItem.Integer Mins { get => (IGMDataItem.Integer)ITEM[0, 12]; set => ITEM[0, 12] = value; }

            private IGMDataItem.Text Name { get => (IGMDataItem.Text)ITEM[0, 4]; set => ITEM[0, 4] = value; }

            private IGMDataItem.Icon Play { get => (IGMDataItem.Icon)ITEM[0, 9]; set => ITEM[0, 9] = value; }

            #endregion Properties

            #region Methods

            public static GameBlock Create(Rectangle pos)
            {
                GameBlock r = Create<GameBlock>(1, 16, new IGMDataItem.Box { Pos = pos });
                return r;
            }

            public void AddPageChangeEvent(ref EventHandler<int> pageChangeEventHandler) => pageChangeEventHandler += PageChangeEvent;

            public override void Draw() => base.Draw();

            public override void Refresh()
            {
                base.Refresh();
                if (Data != null)
                {
                    Face1.Data = Data.Party[0].ToFacesID();
                    Face2.Data = Data.Party[1].ToFacesID();
                    Face3.Data = Data.Party[2].ToFacesID();
                    Saves.CharacterData characterData = Data[Data.Party.First(x => !x.Equals(Characters.Blank))];
                    Name.Data = characterData.Name;
                    LV_Num.Data = characterData.Level;
                    Disc_Num.Data = checked((int)Data.CurrentDisk);
                    Hours.Data = checked((int)MathHelper.Clamp(checked((float)Data.Timeplayed.TotalHours), 0, 99));
                    if (Hours.Data < 99)
                        Mins.Data = checked((int)MathHelper.Clamp(checked((float)Data.Timeplayed.Minutes), 0, 99));
                    else
                        Mins.Data = 99;
                    Gil.Data = checked((int)Data.AmountofGil);
                    Location.Data = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Data.LocationID);
                    foreach (Menu_Base i in ITEM)
                        i?.Show();
                }
                else
                {
                    foreach (Menu_Base i in ITEM)
                        i?.Hide();
                }
            }

            public void Refresh(byte id, Saves.Data data)
            {
                ID = ++id;
                Data = data;
                Refresh();
            }

            protected override void Init()
            {
                base.Init();
                BlockNumber = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0), NumType = Icons.NumType.Num_8x16_0, Spaces = 2, Padding = 2 };
                Face1 = new IGMDataItem.Face { Pos = new Rectangle(BlockNumber.X + 44, SIZE[0].Y, 124, SIZE[0].Height), Border = true };
                Face2 = new IGMDataItem.Face { Pos = new Rectangle(Face1.X + Face1.Width, SIZE[0].Y, Face1.Width, SIZE[0].Height), Border = true };
                Face3 = new IGMDataItem.Face { Pos = new Rectangle(Face2.X + Face1.Width, SIZE[0].Y, Face1.Width, SIZE[0].Height), Border = true };
                int Face3offsetx = Face3.X + Face1.Width + 4;
                Name = new IGMDataItem.Text { Pos = new Rectangle(Face3offsetx, SIZE[0].Y, 0, 0) };
                LV = new IGMDataItem.Text { Data = Strings.Name.LV, Pos = new Rectangle(Name.X, SIZE[0].Y + 64, 0, 0) };
                LV_Num = new IGMDataItem.Integer { Pos = new Rectangle(Name.X + 80, LV.Y, 0, 0), NumType = Icons.NumType.sysFntBig };
                Disc = new IGMDataItem.Icon { Data = Icons.ID.DISC, Pos = new Rectangle(LV_Num.X + 100, LV.Y, 0, 0) };
                Disc_Num = new IGMDataItem.Integer { Pos = new Rectangle(Disc.X + 80, LV.Y, 0, 0) };
                int col3x = SIZE[0].X + SIZE[0].Width - 180;
                Play = new IGMDataItem.Icon { Data = Icons.ID.PLAY, Pos = new Rectangle(col3x, SIZE[0].Y, 0, 0), Palette = 13 };
                Hours = new IGMDataItem.Integer { Pos = new Rectangle(Play.X + 80, SIZE[0].Y, 0, 0), Spaces = 2 };
                Colon = new IGMDataItem.Icon { Data = Icons.ID.Colon, Pos = new Rectangle(Hours.X + 40, SIZE[0].Y, 0, 0), Blink = true, Palette = 13, Faded_Palette = 2, Blink_Adjustment = .5f };
                Mins = new IGMDataItem.Integer { Pos = new Rectangle(Colon.X + 20, SIZE[0].Y, 0, 0), Spaces = 2, Padding = 2 };
                Gil = new IGMDataItem.Integer { Pos = new Rectangle(col3x, LV.Y, 0, 0), Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 8 };
                G = new IGMDataItem.Icon { Data = Icons.ID.G, Pos = new Rectangle(Gil.X + 20 * Gil.Spaces, LV.Y, 0, 0), Palette = 2 };
                const int locationheight = 72;
                Location = new IGMDataItem.Box { Pos = new Rectangle(Face3offsetx, base.Y + base.Height - locationheight, base.Width + base.X - Face3offsetx, locationheight) };
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-8, -8);
            }

            private void PageChangeEvent(object sender, int page)
            {
                _lastpage = _page;
                _page = page;
                if (expectedpage != page)
                    Hide();
                else
                    Show();
            }

            #endregion Methods
        }
    }

    namespace IGMData.Pool
    {
        public class GameChoose : IGMData.Pool.Base<Saves.Data, Saves.Data>
        {
            #region Fields

            private EventHandler<int> PageChangeEventHandler;

            #endregion Fields

            #region Properties

            public bool Save { get; private set; }
            public byte Slot { get; private set; }

            #endregion Properties

            #region Methods

            public static GameChoose Create(Rectangle pos)
            {
                GameChoose r = Create<GameChoose>(30, 1, new IGMDataItem.Empty(pos), 3, 10);
                return r;
            }

            public override void Refresh()
            {
                base.Refresh();
                PageChangeEventHandler?.Invoke(this, Page);
                int r = 0;
                foreach (Menu_Base i in ITEM)
                {
                    if (i.Enabled)
                    {
                        Contents[r++] = ((GameBlock)i).Data;
                        if (r >= Contents.Length) break;
                    }
                }
            }

            protected override void Init()
            {
                base.Init();
                RightArrow.Y = Y + Height / 2 - RightArrow.Height / 2;
                LeftArrow.Y = Y + Height / 2 - LeftArrow.Height / 2;

                for (int i = 0; i < Count - ExtraCount; i++)
                {
                    ITEM[i, 0] = GameBlock.Create(SIZE[i % Rows]);
                    ((GameBlock)ITEM[i, 0]).AddPageChangeEvent(ref PageChangeEventHandler);
                }
                Cursor_Status &= ~Cursor_Status.Horizontal;
            }

            protected override void InitCursor(int i, bool zero = false) =>
                //base.InitCursor(i, zero);
                CURSOR[i] = new Point(SIZE[i].X + 20, SIZE[i].Y + SIZE[i].Height / 2 - 4);

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-20, 0);
            }

            protected override void ModeChangeEvent(object sender, Enum e)
            {
                base.ModeChangeEvent(sender, e);
                if (e.GetType() == typeof(IGM_LGSG.Mode))
                {
                    Save = e.HasFlag(IGM_LGSG.Mode.Save);
                    if (e.HasFlag(IGM_LGSG.Mode.Game) && e.HasFlag(IGM_LGSG.Mode.Choose))
                    {
                        if (Slot != 1 && e.HasFlag(IGM_LGSG.Mode.Slot1))
                            Slot = 0;
                        else
                            Slot = 1;
                        int total = Count - ExtraCount;

                        for (byte i = 0; i < total; i++)
                        {
                            ((GameBlock)ITEM[i, 0]).Refresh(i, Saves.FileList?[Slot, i]);
                        }
                        Show();
                        Refresh();
                    }
                    else
                        Hide();
                }
            }

            protected override void PAGE_NEXT()
            {
                do
                {
                    base.PAGE_NEXT();
                    Refresh();
                }
                while (Contents[0] == null && Page != 0);
            }

            protected override void PAGE_PREV()
            {
                do
                {
                    base.PAGE_PREV();
                    Refresh();
                }
                while (Contents[0] == null && Page != 0);
            }

            #endregion Methods
        }
    }

    public class IGM_LGSG : Menu
    {
        #region Fields

        public const int space = 4;
        private Vector2 AltSize;

        #endregion Fields

        #region Enums

        [Flags]
        public enum Mode : byte
        {
            /// <summary>
            /// Loading or nothing
            /// </summary>
            Nothing = 0x0,

            /// <summary>
            /// Save flag to switch to saving instead of loading.
            /// </summary>
            Save = 0x1,

            /// <summary>
            /// MemoryCard slots or Gamefolders
            /// </summary>
            Slot = 0x2,

            /// <summary>
            /// Game list.
            /// </summary>
            Game = 0x4,

            /// <summary>
            /// choose from list.
            /// </summary>
            Choose = 0x8,

            /// <summary>
            /// Load bar
            /// </summary>
            Checking = 0x10,

            /// <summary>
            /// Header
            /// </summary>
            Header = 0x20,

            /// <summary>
            /// Slot1
            /// </summary>
            Slot1 = 0x40,

            /// <summary>
            /// Slot2
            /// </summary>
            Slot2 = 0x80,

            LoadSlotChoose = Slot | Choose,
            SaveSlotChoose = Save | Slot | Choose,
            LoadSlotChecking = Slot | Checking,
            SaveSlotChecking = Save | Slot | Checking,
            LoadGameChoose = Game | Choose,
            SaveGameChoose = Save | Game | Choose,
            LoadGameChecking = Game | Checking,
            SaveGameChecking = Save | Game | Checking,
            LoadHeader = Header,
            SaveHeader = Save | Header,
        }

        #endregion Enums

        #region Methods

        public static IGM_LGSG Create() => Create<IGM_LGSG>();

        public override void Draw() => base.Draw();

        public override void DrawData() => Data.Where(m => m.Value != null && m.Value.Enabled && !m.Key.HasFlag(Mode.Header)).ForEach(m => m.Value.Draw());

        public override bool Inputs()
        {
            try
            {
                return Data.First(m =>
                !m.Key.HasFlag(Mode.Header) &&
                m.Value != null &&
                m.Value.Enabled &&
                m.Value.Cursor_Status.HasFlag(Cursor_Status.Enabled)).Value.Inputs();
            }
            catch (InvalidOperationException)
            { return false; }
        }

        public override void Refresh() => base.Refresh();

        public override void StartDraw()
        {
            Matrix backupfocus = Focus;
            GenerateFocus(AltSize, Box_Options.Top);
            base.StartDraw();
            Data.Where(m => m.Value != null && m.Value.Enabled && m.Key.HasFlag(Mode.Header)).ForEach(m => m.Value.Draw());
            base.EndDraw();
            Focus = backupfocus;
            base.StartDraw();
        }

        public override bool Update() => base.Update();

        protected override void Init()
        {
            Size = new Vector2 { X = 960, Y = 720 };
            AltSize = new Vector2(1280, 720);
            const int HeaderHeight = 140;
            Rectangle SlotsRectangle = new Rectangle(0, HeaderHeight + space, (int)Size.X, (int)Size.Y - HeaderHeight - space);
            Rectangle LoadBarRectangle = SlotsRectangle;
            Rectangle BlocksRectangle = SlotsRectangle;
            SlotsRectangle.Inflate(-Size.X * .32f, -Size.Y * .28f); // (307,341,346,178)
            LoadBarRectangle.Inflate(-Size.X * .15f, -Size.Y * .35f);

            base.Init();
            Data[Mode.LoadSlotChoose] = IGMData.SlotChoose.Create(SlotsRectangle);
            Data[Mode.Checking] = IGMData.LoadBarBox.Create(LoadBarRectangle);
            Data[Mode.LoadGameChoose] = IGMData.Pool.GameChoose.Create(BlocksRectangle);
            Data[Mode.LoadHeader] = IGMData.ThreePieceHeader.Create(new Rectangle(base.X, base.Y, (int)AltSize.X, HeaderHeight));
            Data.Where(x => x.Value != null).ForEach(x => x.Value.AddModeChangeEvent(ref ModeChangeHandler));
            SetMode(Mode.LoadSlotChoose);
        }

        #endregion Methods
    }
}