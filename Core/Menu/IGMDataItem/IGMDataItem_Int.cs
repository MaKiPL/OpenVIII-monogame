using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_Int : IGMDataItem, I_Data<int>, I_Palette, I_FontColor
    {
        #region Fields

        private int _data;
        private int _digits;
        private byte _padding;
        private int _spaces;
        private Rectangle original_pos;
        private int SpaceWidth;

        #endregion Fields

        #region Constructors

        public IGMDataItem_Int(int data, Rectangle? pos = null, byte? palette = null, Icons.NumType? numtype = null, byte? padding = null, int? spaces = null, int? spacewidth = null, Font.ColorID? fontcolor = null, byte? faded_palette = null, Font.ColorID? faded_fontcolor = null, float blink_adjustment = 1f) : base(pos)
        {
            original_pos = _pos; //= pos ?? Rectangle.Empty;
            _padding = padding ?? 1;
            Palette = palette ?? 2;
            NumType = numtype ?? 0;
            _spaces = spaces ?? 1;
            SpaceWidth = spacewidth ?? 20;
            FontColor = fontcolor ?? Font.ColorID.White;
            Faded_FontColor = faded_fontcolor ?? FontColor;
            Faded_Palette = faded_palette ?? Palette;
            Blink_Adjustment = blink_adjustment;
            Data = data;
        }

        #endregion Constructors

        #region Properties

        public override bool Blink { get => base.Blink && (Palette != Faded_Palette || FontColor != Faded_FontColor); set => base.Blink = value; }
        public int Data
        {
            get => _data;

            set
            {
                _data = value;

                _digits = Data.ToString().Length;
                if (_digits < _padding) _digits = _padding;

                _pos = original_pos;
                _pos.Offset(SpaceWidth * (_spaces - _digits), 0);
            }
        }

        public Font.ColorID Faded_FontColor { get; set; }
        public byte Faded_Palette { get; set; }
        public Font.ColorID FontColor { get; set; }
        public byte Palette { get; set; }

        public Icons.NumType NumType { get; set; }

        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                Memory.Icons.Draw(Data, NumType, Palette, $"D{_padding}", Pos.Location.ToVector2(), Scale, Fade, FontColor);
                if (Blink)
                    Memory.Icons.Draw(Data, NumType, Faded_Palette, $"D{_padding}", Pos.Location.ToVector2(), Scale, Fade * Blink_Amount * Blink_Adjustment, Faded_FontColor);
            }
        }

        #endregion Methods
    }
}