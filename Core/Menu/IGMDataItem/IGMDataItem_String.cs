using Microsoft.Xna.Framework;

namespace OpenVIII
{
        #region Classes

        public class IGMDataItem_String : IGMDataItem
        {
            private int _palette;

            public FF8String Data { get; set; }
            public Font.ColorID Colorid { get; set; }
            public Icons.ID? Icon { get; set; }
            public int Palette
            {
                get => _palette; set => _palette = value < Memory.Icons.PaletteCount ? value : 2;
            }

            public IGMDataItem_String(FF8String data, Rectangle? pos = null, Font.ColorID? color = null) : base(pos)
            {
                Data = data;
                Colorid = color ?? Font.ColorID.White;
            }

            public IGMDataItem_String(Icons.ID? icon, int palette, FF8String data, Rectangle? pos = null, Font.ColorID? color = null) : base(pos)
            {
                Icon = icon;
                Palette = palette;
                Data = data;
                Colorid = color ?? Font.ColorID.White;
            }

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
                        r.Offset(Memory.Icons.GetEntryGroup(Icon).Width * Scale.X, 0);
                    }
                    Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: Fade, color: Colorid);
                }
            }
        }
        #endregion Classes
    
}