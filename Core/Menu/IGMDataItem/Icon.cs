using Microsoft.Xna.Framework;
namespace OpenVIII.IGMDataItem
{
    public class Icon : Base, I_Data<Icons.ID>, I_Palette
    {
        #region Fields

        private byte _faded_palette;
        private byte _palette;

        #endregion Fields

        #region Constructors

        public Icon(Icons.ID data, Rectangle? pos = null, byte? palette = null, byte? faded_palette = null, float blink_adjustment = 1f, Vector2? scale = null) : base(pos, scale)
        {
            Data = data;
            Palette = palette ?? 2;
            Faded_Palette = faded_palette ?? Palette;
            Blink_Adjustment = blink_adjustment;
        }

        #endregion Constructors

        #region Properties

        public override bool Blink
        {
            get => base.Blink; set => base.Blink = value;
        }

        public Icons.ID Data { get; set; }
        public byte Faded_Palette
        {
            get => _faded_palette; set
            {
                if (value >= Memory.Icons.PaletteCount) value = 2;
                _faded_palette = value;
            }
        }

        public byte Palette
        {
            get => _palette; set
            {
                if (value >= Memory.Icons.PaletteCount) value = 2;
                _palette = value;
            }
        }


        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                if (!Blink)
                    Memory.Icons.Draw(Data, Palette, Pos, Scale, Fade, Color);
                else
                {
                    if (Faded_Palette != Palette)
                    {
                        Memory.Icons.Draw(Data, Palette, Pos, Scale, Fade, Color);
                        Memory.Icons.Draw(Data, Faded_Palette, Pos, Scale, Fade * Blink_Amount * Blink_Adjustment, Color);
                    }
                    else
                    {
                        Memory.Icons.Draw(Data, Faded_Palette, Pos, Scale, Fade * Blink_Adjustment, Color.Lerp(Color,Faded_Color,Blink_Amount));
                    }
                }
            }
        }

        #endregion Methods
    }
}