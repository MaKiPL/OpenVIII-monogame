using Microsoft.Xna.Framework;

namespace OpenVIII.IGMDataItem
{
    public class Integer : Base, I_Data<int>, I_Palette, I_FontColor
    {
        #region Fields

        private int _data = 0;
        private byte _padding = 1;
        private int _spaces = 1;
        private bool changed = false;
        private Rectangle original_pos;
        private int space_width = 20;

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

        public Rectangle DataSize { get; private set; }
        public int Digits => Data.ToString().Length;
        public Font.ColorID Faded_FontColor { get; set; }
        public byte Faded_Palette { get; set; } = 2;
        public Font.ColorID FontColor { get; set; } = Font.ColorID.White;
        public Icons.NumType NumType { get; set; } = 0;

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
                base.Pos = original_pos;
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

        #endregion Properties

        #region Methods

        public override void Draw() => Draw(false);

        public void Draw(bool skipdraw)
        {
            if (changed)
                //notice the integers would move from right to left over the course of 1 frame because draw was before update.
                Update(); 
            if (Enabled)
            {
                DataSize = Memory.Icons.Draw(Data, NumType, Palette, $"D{_padding}", Pos.Location.ToVector2() +
                    (OffsetAnchor ?? Vector2.Zero), Scale, Fade, FontColor, Blink, skipdraw);
            }
        }

        public override bool Update()
        {
            var r = base.Update();
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
            var digits = Digits;
            _pos.Offset(space_width * (_spaces - (digits < _padding ? _padding : digits)), 0);
            changed = false;
        }

        #endregion Methods
    }
}