using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_Icon : IGMDataItem, I_Data<Icons.ID>
    {
        #region Fields

        private byte _faded_palette;
        private byte _palette;

        #endregion Fields

        #region Constructors

        public IGMDataItem_Icon(Icons.ID data, Rectangle? pos = null, byte? palette = null, byte? faded_palette = null, float blink_adjustment = 1f, Vector2? scale = null) : base(pos, scale)
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
            get => base.Blink && (Faded_Palette != Palette); set => base.Blink = value;
        }

        public Icons.ID Data { get;set; }
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
                Memory.Icons.Draw(Data, Palette, Pos, Scale, Fade);
                if (Blink)
                    Memory.Icons.Draw(Data, Faded_Palette, Pos, Scale, Fade * Blink_Amount * Blink_Adjustment);
            }
        }

        #endregion Methods
    }
}