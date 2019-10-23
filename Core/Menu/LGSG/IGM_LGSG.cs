using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace OpenVIII
{
    namespace IGMData
    {
        public class SlotChoose : IGMData.Base
        {
            #region Properties

            public bool Save { get; protected set; } = false;
            public IGMDataItem.Box Slot1Main { get => (IGMDataItem.Box)ITEM[0, 0]; set => ITEM[0, 0] = value; }

            public IGMDataItem.Box Slot1Title { get => (IGMDataItem.Box)ITEM[0, 1]; set => ITEM[0, 1] = value; }

            public IGMDataItem.Box Slot2Main { get => (IGMDataItem.Box)ITEM[1, 0]; set => ITEM[1, 0] = value; }

            public IGMDataItem.Box Slot2Title { get => (IGMDataItem.Box)ITEM[1, 1]; set => ITEM[1, 1] = value; }

            #endregion Properties

            #region Methods

            public static SlotChoose Create(Rectangle pos)
            {
                SlotChoose r = new SlotChoose();
                r.Init(2, 2, new IGMDataItem.Empty(pos), 1, 2);
                return r;
            }
            protected override void ModeChangeEvent(object sender, Enum e)
            {
                base.ModeChangeEvent(sender, e);
                if (e.GetType() == typeof(IGM_LGSG.Mode))
                {
                    Save = e.HasFlag(IGM_LGSG.Mode.Save);
                    if (e.HasFlag(IGM_LGSG.Mode.Slot) && e.HasFlag(IGM_LGSG.Mode.Choose))
                        Show();
                    else
                        Hide();
                }
            }

            public override bool Inputs() => base.Inputs();

            protected override void Init()
            {
                base.Init();
                Point offset = new Point(-8, -28);
                Point size = new Point(132, 60);
                Slot1Main = new IGMDataItem.Box { Data = Strings.Name.FF8, Pos = SIZE[0], Options = Box_Options.Buttom };
                Slot2Main = new IGMDataItem.Box { Data = Strings.Name.FF8, Pos = SIZE[1], Options = Box_Options.Buttom };
                Point p = SIZE[0].Location;
                p = p.Offset(offset);
                Slot1Title = new IGMDataItem.Box { Data = Strings.Name.Slot1, Pos = new Rectangle(p, size), Options = Box_Options.Middle | Box_Options.Center };
                p = SIZE[1].Location;
                p = p.Offset(offset);
                Slot2Title = new IGMDataItem.Box { Data = Strings.Name.Slot2, Pos = new Rectangle(p, size), Options = Box_Options.Middle | Box_Options.Center };
                Slot1Main.Draw(true);
                Slot2Main.Draw(true);
                CURSOR[0] = Slot1Main.Dims.Cursor;
                CURSOR[1] = Slot2Main.Dims.Cursor;
                Cursor_Status = Cursor_Status.Enabled;
            }

            protected override void InitShift(int i, int col, int row)
            {
                int SpaceBetween = 60;
                base.InitShift(i, col, row);
                switch (i)
                {
                    case 0:
                        SIZE[i].Y -= SpaceBetween / 2;
                        break;

                    case 1:
                    default:
                        SIZE[i].Y += row * SpaceBetween / 2;
                        break;
                }
            }
            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                if(!Save)
                init_debugger_Audio.StopMusic();
                Menu.FadeIn();
                Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.MainLobby;

                return true;
            }

            #endregion Methods
        }
    }

    public class IGM_LGSG : Menu
    {
        #region Fields

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
            Rectangle SlotsRectangle = new Rectangle(0, HeaderHeight, (int)Size.X, (int)Size.Y-HeaderHeight);
            SlotsRectangle.Inflate(-Size.X * .32f, -Size.Y * .28f); // (307,341,346,178)

            base.Init();
            Data[Mode.LoadSlotChoose] = IGMData.SlotChoose.Create(SlotsRectangle);
            Data[Mode.LoadSlotChecking] = null;
            Data[Mode.LoadGameChoose] = null;
            Data[Mode.LoadGameChecking] = null;
            Data[Mode.LoadHeader] = IGMData.ThreePieceHeader.Create(null, null, null, new Rectangle(base.X, base.Y, (int)AltSize.X, HeaderHeight));
            
            //Data[Mode.SaveSlotChoose] = IGMData.SlotChoose.Create(SlotsRectangle, true);
            //Data[Mode.SaveSlotChecking] = null;
            //Data[Mode.SaveGameChoose] = null;
            //Data[Mode.SaveGameChecking] = null;
            //Data[Mode.SaveHeader] = IGMData.ThreePieceHeader.Create(Strings.Name.GameFolder, Strings.Name.Save, Strings.Name.SaveFF8, Data[Mode.Header].Pos);
            Data.Where(x => x.Value != null).ForEach(x => x.Value.AddModeChangeEvent(ref ModeChangeHandler));
            SetMode(Mode.LoadSlotChoose);
        }
        #endregion Methods
    }
}