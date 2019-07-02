using Microsoft.Xna.Framework;

namespace OpenVIII
{
        #region Classes

        public class IGMDataItem_Icon : IGMDataItem//<Icons.ID>
        {
            private byte _palette;
            private byte _faded_palette;

            public Icons.ID Data { get; set; }

            public byte Palette
            {
                get => _palette; set
                {
                    if (value >= Memory.Icons.PaletteCount) value = 2;
                    _palette = value;
                }
            }

            public byte Faded_Palette
            {
                get => _faded_palette; set
                {
                    if (value >= Memory.Icons.PaletteCount) value = 2;
                    _faded_palette = value;
                }
            }

            public bool Blink => Faded_Palette != Palette;
            public float Blink_Adjustment { get; set; }

            public IGMDataItem_Icon(Icons.ID data, Rectangle? pos = null, byte? palette = null, byte? faded_palette = null, float blink_adjustment = 1f,Vector2? scale = null) : base(pos,scale)
            {
                Data = data;
                Palette = palette ?? 2;
                Faded_Palette = faded_palette ?? Palette;
                Blink_Adjustment = blink_adjustment;
            }

            public override void Draw()
            {
                if (Enabled)
                {
                    Memory.Icons.Draw(Data, Palette, Pos, Scale, Fade);
                    if (Blink)
                        Memory.Icons.Draw(Data, Faded_Palette, Pos, Scale, Fade * Blink_Amount * Blink_Adjustment);
                }
            }
        }
        #endregion Classes
    
}