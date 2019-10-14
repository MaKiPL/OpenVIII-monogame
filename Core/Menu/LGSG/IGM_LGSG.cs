using System;

namespace OpenVIII
{
    public class IGM_LGSG : Menu
    {
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
        }

        protected override void Init()
        {
            Data[Mode.Nothing] = null;
            Data[Mode.Slot | Mode.Choose] = null;
            Data[Mode.Slot | Mode.Checking] = null;
            Data[Mode.Game | Mode.Choose] = null;
            Data[Mode.Game | Mode.Checking] = null;
            Data[Mode.Save | Mode.Slot | Mode.Choose] = null;
            Data[Mode.Save | Mode.Slot | Mode.Checking] = null;
            Data[Mode.Save | Mode.Game | Mode.Choose] = null;
            Data[Mode.Save | Mode.Game | Mode.Checking] = null;
            base.Init();
        }
    }
}