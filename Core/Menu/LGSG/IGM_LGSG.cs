using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace OpenVIII
{
    namespace IGMData
    {
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
        }

        #endregion Enums

        #region Methods

        public static IGM_LGSG Create() => Create<IGM_LGSG>();

        public override void Draw() => base.Draw();

        public override void DrawData() => Data.Where(m => m.Value != null && m.Value.Enabled && !m.Key.HasFlag(Mode.Header)).ForEach(m => m.Value.Draw());

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
            Size = new Vector2 { X = 843, Y = 630 };
            AltSize = new Vector2(1280, 720);
            base.Init();
            Data[Mode.Slot | Mode.Choose] = null;
            Data[Mode.Slot | Mode.Checking] = null;
            Data[Mode.Game | Mode.Choose] = null;
            Data[Mode.Game | Mode.Checking] = null;
            Data[Mode.Header] = IGMData.ThreePieceHeader.Create(Strings.Name.GameFolder, Strings.Name.Load, Strings.Name.LoadFF8, new Rectangle(X, Y, (int)AltSize.X, 140));
            Data[Mode.Save | Mode.Slot | Mode.Choose] = null;
            Data[Mode.Save | Mode.Slot | Mode.Checking] = null;
            Data[Mode.Save | Mode.Game | Mode.Choose] = null;
            Data[Mode.Save | Mode.Game | Mode.Checking] = null;
            Data[Mode.Save | Mode.Header] = IGMData.ThreePieceHeader.Create(Strings.Name.GameFolder, Strings.Name.Save, Strings.Name.SaveFF8, Data[Mode.Header].Pos);
            Data[Mode.Header].Show();
            Data[Mode.Save | Mode.Header].Hide();
        }

        #endregion Methods
    }
}