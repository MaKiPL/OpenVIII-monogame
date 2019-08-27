using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_String : IGMDataItem, I_Data<FF8String>, I_Palette, I_FontColor
    {
        #region Fields

        private byte _palette;

        #endregion Fields

        #region Constructors

        public IGMDataItem_String(FF8String data, Rectangle? pos = null, Font.ColorID? fontcolor = null, Font.ColorID? faded_fontcolor = null, float blink_adjustment = 1f) : base(pos)
        {
            Data = data;
            FontColor = fontcolor ?? Font.ColorID.White;
            Faded_FontColor = faded_fontcolor ?? FontColor;
            Blink_Adjustment = blink_adjustment;
        }

        public IGMDataItem_String(Icons.ID? icon, byte? palette, FF8String data, Rectangle? pos = null, Font.ColorID? color = null, byte? faded_palette = null, Font.ColorID? faded_color = null, float blink_adjustment = 1f) : this(data, pos, color, faded_color, blink_adjustment)
        {
            Icon = icon;
            Palette = palette ?? 2;
            Faded_Palette = faded_palette ?? Palette;
        }

        #endregion Constructors

        #region Properties

        public override bool Blink { get => base.Blink && (Palette != Faded_Palette || FontColor != Faded_FontColor); set => base.Blink = value; }
        public FF8String Data { get; set; }
        public Font.ColorID Faded_FontColor { get; set; }
        public byte Faded_Palette { get; set; }
        public Font.ColorID FontColor { get; set; }
        public Icons.ID? Icon { get; set; }
        public byte Palette
        {
            get => _palette; set => _palette = (byte)(value < Memory.Icons.PaletteCount ? value : 2);
        }

        #endregion Properties

        #region Methods

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
                Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade, color: FontColor);
                if (Blink)
                    Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade * Blink_Amount * Blink_Adjustment, color: Faded_FontColor);
            }
        }

        #endregion Methods
    }
}