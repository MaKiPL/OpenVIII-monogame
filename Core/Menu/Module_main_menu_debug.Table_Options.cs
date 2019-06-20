using System;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        [Flags]
        public enum Table_Options
        {
            /// <summary>
            /// No flags set.
            /// </summary>
            Default = 0x0,

            /// <summary>
            /// Default fills 1 col at a time. This will make it fill 1 row at a time.
            /// </summary>
            FillRows = 0x1,
        }
        #endregion Classes
    }
}