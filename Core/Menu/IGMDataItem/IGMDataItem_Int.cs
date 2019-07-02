using Microsoft.Xna.Framework;

namespace OpenVIII
{
        #region Classes

        public class IGMDataItem_Int : IGMDataItem//<Int>
        {
            private byte _palette;
            private int _spaces;
            private int SpaceWidth;
            private Rectangle original_pos;

            public int Data
            {
                get => _data; set
                {
                    _data = value;

                    _digits = _data.ToString().Length;
                    if (_digits < _padding) _digits = (int)_padding;

                    _pos = original_pos;
                    _pos.Offset(SpaceWidth * (_spaces - _digits), 0);
                }
            }
            private byte _padding;
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

            private int _digits;
            private int _data;

            public IGMDataItem_Int(int data, Rectangle? pos = null, byte? palette = null, Icons.NumType? numtype = null, byte? padding = null, int? spaces = null, int? spacewidth = null, Font.ColorID? colorid = null) : base(pos)
            {
                original_pos = _pos;
                _padding = padding ?? 1;
                Palette = palette ?? 2;
                NumType = numtype ?? 0;
                _spaces = spaces ?? 1;
                SpaceWidth = spacewidth ?? 20;
                Colorid = colorid ?? Font.ColorID.White;
                Data = data;
            }

            public override void Draw()
            {
                if (Enabled)
                {
                    Memory.Icons.Draw(Data, NumType, Palette, $"D{_padding}", Pos.Location.ToVector2(), Scale, Fade, Colorid);
                }
            }
        }
        #endregion Classes
    }
