using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OpenVIII
{
    public class IGMLoadSaveGame : Menu
    {
        #region Fields

        public const int Space = 4;

        #endregion Fields

        #region Constructors

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        public IGMLoadSaveGame()
        {
        }

        #endregion Constructors

        #region Enums

        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            /// MemoryCard slots or Game folders
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

        public static IGMLoadSaveGame Create() => Create<IGMLoadSaveGame>();

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

        protected override void Init()
        {
            Size = new Vector2 { X = 960, Y = 720 };
            const int headerHeight = 140;
            var slotsRectangle = new Rectangle(0, headerHeight + Space, (int)Size.X, (int)Size.Y - headerHeight - Space);
            var loadBarRectangle = slotsRectangle;
            var blocksRectangle = slotsRectangle;
            slotsRectangle.Inflate(-Size.X * .32f, -Size.Y * .28f); // (307,341,346,178)
            loadBarRectangle.Inflate(-Size.X * .15f, -Size.Y * .35f);

            base.Init();
            Data[Mode.LoadSlotChoose] = IGMData.SlotChoose.Create(slotsRectangle);
            Data[Mode.Checking] = IGMData.LoadBarBox.Create(loadBarRectangle);
            Data[Mode.LoadGameChoose] = IGMData.Pool.GameChoose.Create(blocksRectangle);
            Data[Mode.LoadHeader] = IGMData.ThreePieceHeader.Create(new Rectangle(base.X, base.Y, (int)Size.X, headerHeight));
            Data.Where(x => x.Value != null).ForEach(x => ModeChangeHandler += x.Value.ModeChangeEvent);
            SetMode(Mode.LoadSlotChoose);
        }

        #endregion Methods
    }
}