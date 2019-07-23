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
                if (_digits < _padding) _digits = _padding;

                _pos = original_pos;
                _pos.Offset(SpaceWidth * (_spaces - _digits), 0);
            }
        }

        private byte _padding;
        public Font.ColorID Colorid { get; set; }
        public Font.ColorID Faded_Colorid { get; set; }
        public byte Faded_Palette { get; set; }
        public float Blink_Adjustment { get; set; }
        public byte Palette { get; set; }

        public Icons.NumType NumType { get; set; }
        public bool Blink => Palette != Faded_Palette || Colorid != Faded_Colorid;
        private int _digits;
        private int _data;

        public IGMDataItem_Int(int data, Rectangle? pos = null, byte? palette = null, Icons.NumType? numtype = null, byte? padding = null, int? spaces = null, int? spacewidth = null, Font.ColorID? colorid = null, byte? faded_palette = null, Font.ColorID? faded_colorid = null, float blink_adjustment = 1f) : base(pos)
        {
            original_pos = _pos;
            _padding = padding ?? 1;
            Palette = palette ?? 2;
            NumType = numtype ?? 0;
            _spaces = spaces ?? 1;
            SpaceWidth = spacewidth ?? 20;
            Colorid = colorid ?? Font.ColorID.White;
            Faded_Colorid = faded_colorid ?? Colorid;
            Faded_Palette = faded_palette ?? Palette;
            Blink_Adjustment = blink_adjustment;
            Data = data;
        }

        public override void Draw()
        {
            if (Enabled)
            {
                Memory.Icons.Draw(Data, NumType, Palette, $"D{_padding}", Pos.Location.ToVector2(), Scale, Fade, Colorid);
                if (Blink)
                    Memory.Icons.Draw(Data, NumType, Faded_Palette, $"D{_padding}", Pos.Location.ToVector2(), Scale, Fade * Blink_Amount * Blink_Adjustment, Faded_Colorid);
            }
        }
    }

    #endregion Classes
}