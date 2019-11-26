using Microsoft.Xna.Framework;

namespace OpenVIII.IGMDataItem
{
    public class Text : Base, I_Data<FF8String>, I_Palette, I_FontColor
    {
        #region Fields

        private FF8String _data;
        private Icons.ID? _icon = Icons.ID.None;
        private byte _palette;

        #endregion Fields

        #region Constructors

        public Text()
        {
            FontColor = Font.ColorID.White;
            Blink_Adjustment = 1f;
            Palette = 2;
        }

        #endregion Constructors

        #region Properties

        public override bool Blink { get => base.Blink; set => base.Blink = value; }

        public FF8String Data
        {
            get => _data; set
            {
                _data = value;
                DataSize = Memory.font.RenderBasicText(_data, Pos.Location, Scale, skipdraw: true);
                OffsetIcon();
            }
        }

        public Rectangle DataSize { get; private set; }

        public byte Faded_Palette { get; set; }

        public Font.ColorID FontColor { get; set; }

        public override int Height => DataSize.Height;

        public Icons.ID? Icon
        {
            get => _icon; set
            {
                _icon = value;
                OffsetIcon();
            }
        }

        public byte Palette
        {
            get => _palette; set => _palette = (byte)(value < Memory.Icons.PaletteCount ? value : 2);
        }

        public override int Width => DataSize.Width;

        #endregion Properties

        #region Methods

        public override void Draw() => Draw(false);

        public void Draw(bool skipdraw)
        {
            if (Enabled)
            {
                Rectangle r = Pos;
                if (OffsetAnchor != null)
                    r.Offset(OffsetAnchor);

                Rectangle r2 = r;
                if (Icon != null && Icon != Icons.ID.None)
                {
                    r2.Size = Point.Zero;
                    if (!skipdraw)
                        Memory.Icons.Draw(Icon, Palette, r2, new Vector2(Scale.X), Fade);

                    if (Blink)
                        Memory.Icons.Draw(Icon, Faded_Palette, r2, new Vector2(Scale.X), Fade * Blink_Amount * Blink_Adjustment);
                    r.Offset(Memory.Icons.GetEntryGroup(Icon).Width * Scale.X, 0);
                }
                DataSize = Rectangle.Union(r2, Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade, color: FontColor, blink: Blink, skipdraw: skipdraw));
            }
        }

        private void OffsetIcon()
        {
            if (_icon != Icons.ID.None)
                DataSize.Offset(Memory.Icons.GetEntryGroup(_icon).Width * Scale.X, 0);
        }

        #endregion Methods
    }
}