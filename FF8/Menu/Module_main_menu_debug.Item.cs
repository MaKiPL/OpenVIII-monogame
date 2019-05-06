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
            public Point Point { get; set; }
            public FF8String Text { get; set; }

            public override bool Equals(object obj) => base.Equals(obj);
            public override int GetHashCode() => base.GetHashCode();
            public override string ToString() => Text.ToString();

            #endregion Properties


            public static implicit operator FF8String (Item i) => i.Text;
            public static implicit operator string(Item i) => i.Text.ToString();
            public static implicit operator Rectangle(Item i) => i.Loc;
            public static implicit operator Point (Item i) => i.Point;
        }

        #endregion Structs
    }
}