using Microsoft.Xna.Framework;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Structs

        /// <summary>
        /// Container for MenuItems containing relevant info.
        /// </summary>
        private struct Item
        {
            #region Properties

            public Rectangle Loc { get; set; }
            public byte[] Text { get; set; }

            #endregion Properties
        }

        #endregion Structs
    }
}