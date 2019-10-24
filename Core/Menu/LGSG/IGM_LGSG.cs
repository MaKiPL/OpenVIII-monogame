using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace OpenVIII
{

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

        public const int space = 4;
        protected override void Init()
        {
            Size = new Vector2 { X = 960, Y = 720 };
            AltSize = new Vector2(1280, 720);
            const int HeaderHeight = 140;
            Rectangle SlotsRectangle = new Rectangle(0, HeaderHeight+ space, (int)Size.X, (int)Size.Y-HeaderHeight- space);
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
    namespace IGMData.Pool
    {
        public class GameChoose : IGMData.Pool.Base<Saves.Data, Saves.Data>
        {
            public bool Save { get; private set; }
            public byte Slot { get; private set; }

            public static GameChoose Create(Rectangle pos)
            {
                var r = Create<GameChoose>(30, 1, new IGMDataItem.Empty(pos), 3, 10);
                return r;
            }
            protected override void ModeChangeEvent(object sender, Enum e)
            {
                base.ModeChangeEvent(sender, e);
                if (e.GetType() == typeof(IGM_LGSG.Mode))
                {
                    Save = e.HasFlag(IGM_LGSG.Mode.Save);
                    if (e.HasFlag(IGM_LGSG.Mode.Game) && e.HasFlag(IGM_LGSG.Mode.Choose))
                    {
                        if(Slot != 1 && e.HasFlag(IGM_LGSG.Mode.Slot1))                        
                            Slot = 0;                        
                        else         
                            Slot = 1;
                        for (int i = 0; i < Count - ExtraCount; i++)
                            ((GameBlock)ITEM[i, 0]).Refresh(Saves.FileList?[Slot, i]);
                        Show();
                        Refresh();
                    }
                    else
                        Hide();
                }
            }
            protected override void Init()
            {
                base.Init();
                RightArrow.Y = Y + Height / 2 - RightArrow.Height / 2;
                LeftArrow.Y = Y + Height / 2 - LeftArrow.Height / 2;

                for (int i = 0; i < Count-ExtraCount; i++)                
                    ITEM[i, 0] = GameBlock.Create(SIZE[i % Rows]);                

            }
        }
    }
    namespace IGMData
    {
        public class GameBlock : IGMData.Base
        {
            public Saves.Data Data { get; private set; }

            public static GameBlock Create(Rectangle pos)
            {
                var r = Create<GameBlock>(1, 30, new IGMDataItem.Box { Pos = pos });
                return r;
            }
            public override void Refresh()
            {
                base.Refresh();
                if (Data != null)
                {
                    foreach (var i in ITEM)
                        i?.Show();
                }
                else
                {
                    foreach (var i in ITEM)
                        i?.Hide();
                }
            }

            public void Refresh(Saves.Data data)
            {
                Data = data;
                Refresh();
            }
        }
    }
}