using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        public class IGMDataItem_Int : IGMDataItem//<Int>
        {
            private byte _palette;
            private int Spaces;
            private int SpaceWidth;

            public int Data { get; set; }
            public byte Padding { get; set; }
            public Font.ColorID Colorid;

            public byte Palette
            {
                get => _palette; set
                {
                    if (value >= 16) value = 2;
                    _palette = value;
                }
            }

            public Icons.NumType NumType { get; set; }

            private int Digits;

            public IGMDataItem_Int(int data, Rectangle? pos = null, byte? palette = null, Icons.NumType? numtype = null, byte? padding = null, int? spaces = null, int? spacewidth = null, Font.ColorID? colorid = null) : base(pos)
            {
                Data = data;
                Padding = padding ?? 1;
                Palette = palette ?? 2;
                NumType = numtype ?? 0;
                Digits = data.ToString().Length;
                if (Digits < padding) Digits = (int)padding;
                Spaces = spaces??1;
                SpaceWidth = spacewidth??20;
                _pos.Offset(SpaceWidth * (Spaces - Digits), 0);
                Colorid = colorid?? Font.ColorID.White;
            }

            public override void Draw()
            {
                if (Enabled)
                {
                    Memory.Icons.Draw(Data, NumType, Palette, $"D{Padding}", Pos.Location.ToVector2(), Scale, fade, Colorid);
                }
            }
        }
        #endregion Classes
    }
}