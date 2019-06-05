using System;

namespace FF8
{
    public static partial class Module_main_menu_debug
    {
        #region Methods

        [Flags]
        public enum Box_Options
        {
            Default = 0x0,
            Indent = 0x1,
            Buttom = 0x2,
            SkipDraw = 0x4,
            Center = 0x8,
            Middle = 0x10,
        }

        #endregion Methods
    }
}