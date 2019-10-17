using Microsoft.Xna.Framework;

namespace OpenVIII.IGMDataItem
{
    public class Integer : Base, I_Data<int>, I_Palette, I_FontColor
    {

        #region Fields

        private int _data = 0;
        private byte _padding = 1;
        private int _spaces = 1;
        private Rectangle original_pos;
        private int space_width = 20;

        bool changed = false;

        #endregion Fields
        
        #region Properties

        public override bool Blink { get => base.Blink && (Palette != Faded_Palette || FontColor != Faded_FontColor); set => base.Blink = value; }
        public int Data
        {
            get => _data;

            set
            {
                _data = value;
                changed = true;
            }
        }

        public int Digits => Data.ToString().Length;
        public Font.ColorID Faded_FontColor { get; set; }
        public byte Faded_Palette { get; set; } = 2;
        public Font.ColorID FontColor { get; set; } = Font.ColorID.White;
        public byte Padding
        {
            get => _padding; set
            {
                _padding = value;
                changed = true;
            }
        }
        public byte Palette { get; set; } = 2;
        public override Rectangle Pos
        {
            get => base.Pos; set
            {
                original_pos = value;
                changed = true;
            }
        }
        public int Space_Width
        {
            get => space_width; set
            {
                space_width = value;
                changed = true;
            }
        }

        public int Spaces
        {
            get => _spaces; set
            {
                _spaces = value;
                changed = true;
            }
        }

        public Icons.NumType NumType { get; set; } = 0;

        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                Memory.Icons.Draw(Data, NumType, Palette, $"D{_padding}", Pos.Location.ToVector2(), Scale, Fade, FontColor, Blink);
                //if (Blink)
                //    Memory.Icons.Draw(Data, NumType, Faded_Palette, $"D{_padding}", Pos.Location.ToVector2(), Scale, Fade * Blink_Amount * Blink_Adjustment, Faded_FontColor);
            }
        }
        public override bool Update()
        {
            bool r = base.Update();
            if (changed)
            {
                UpdateOffset();
                return true;
            }
            return r;
        }

        public void UpdateOffset()
        {
            _pos = original_pos;
            int digits = Digits;
            _pos.Offset(space_width * (_spaces - digits < _padding ? _padding : digits), 0);
        }

        #endregion Methods
    }
}