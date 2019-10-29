using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace OpenVIII
{
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

        //public override void DrawData() => Data.Where(m => m.Value != null && m.Value.Enabled && !m.Key.HasFlag(Mode.Header)).ForEach(m => m.Value.Draw());

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

        //public override void StartDraw()
        //{
        //    Matrix backupfocus = Focus;
        //    GenerateFocus(AltSize, Box_Options.Top);
        //    base.StartDraw();
        //    Data.Where(m => m.Value != null && m.Value.Enabled && m.Key.HasFlag(Mode.Header)).ForEach(m => m.Value.Draw());
        //    base.EndDraw();
        //    Focus = backupfocus;
        //    base.StartDraw();
        //}

        public override bool Update() => base.Update();

        protected override void Init()
        {
            Size = new Vector2 { X = 960, Y = 720 };
            //AltSize = new Vector2(1280, 720);
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
            Data[Mode.LoadHeader] = IGMData.ThreePieceHeader.Create(new Rectangle(base.X, base.Y, (int)Size.X, HeaderHeight));
            Data.Where(x => x.Value != null).ForEach(x => x.Value.AddModeChangeEvent(ref ModeChangeHandler));
            SetMode(Mode.LoadSlotChoose);
        }

        #endregion Methods
    }
}