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

        #region Methods

        private void OffsetIcon()
        {
            if (_icon != Icons.ID.None)
                DataSize.Offset(Memory.Icons.GetEntryGroup(_icon).Width * Scale.X, 0);
        }

        #endregion Methods

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
        public override int Width { get => DataSize.Width; }
        public override int Height { get => DataSize.Height; }
        public Rectangle DataSize { get; private set; }
        public byte Faded_Palette { get; set; }

        public Font.ColorID FontColor { get; set; }
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

        #endregion Properties

        public override void Draw()
        {
            if (Enabled)
            {
                Rectangle r = Pos;
                if (Icon != null && Icon != Icons.ID.None)
                {
                    Rectangle r2 = r;
                    r2.Size = Point.Zero;
                    Memory.Icons.Draw(Icon, Palette, r2, new Vector2(Scale.X), Fade);

                    if (Blink)
                        Memory.Icons.Draw(Icon, Faded_Palette, r2, new Vector2(Scale.X), Fade * Blink_Amount * Blink_Adjustment);
                    r.Offset(Memory.Icons.GetEntryGroup(Icon).Width * Scale.X, 0);
                }
                DataSize = Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade, color: FontColor, blink: Blink);
                //if (Blink)
                //    Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade * Blink_Amount * Blink_Adjustment, color: Faded_FontColor);
            }
        }
    }
}