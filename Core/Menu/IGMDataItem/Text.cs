using Microsoft.Xna.Framework;
namespace OpenVIII.IGMDataItem
{
    public class Text : Base, I_Data<FF8String>, I_Palette, I_FontColor
    {
        #region Fields

        private byte _palette;

        #endregion Fields

        #region Constructors

        public Text(FF8String data, Rectangle? pos = null, Font.ColorID? fontcolor = null, float blink_adjustment = 1f) : base(pos)
        {
            Data = data;
            DataSize = Memory.font.RenderBasicText(Data, Pos.Location, Scale, skipdraw: true);
            FontColor = fontcolor ?? Font.ColorID.White;
            Blink_Adjustment = blink_adjustment;
        }

        public Text(Icons.ID? icon, byte? palette, FF8String data, Rectangle? pos = null, Font.ColorID? color = null, Font.ColorID? faded_color = null, float blink_adjustment = 1f) : this(data, pos, color, blink_adjustment)
        {
            Icon = icon;
            DataSize.Offset(Memory.Icons.GetEntryGroup(Icon).Width * Scale.X, 0);
            Palette = palette ?? 2;
        }

        #endregion Constructors

        #region Properties

        public override bool Blink { get => base.Blink; set => base.Blink = value; }
        public FF8String Data { get; set; }
        public Rectangle DataSize { get; private set; }

        //public Font.ColorID Faded_FontColor { get; set; }
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
                DataSize = Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade, color: FontColor, blink: Blink);
                //if (Blink)
                //    Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade * Blink_Amount * Blink_Adjustment, color: Faded_FontColor);
            }
        }

        #endregion Methods
    }
}