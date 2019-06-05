using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        private class IGMDataItem_String : IGMDataItem
        {
            private int _pallet;

            public FF8String Data { get; set; }
            public Font.ColorID Colorid { get; set; }
            public Icons.ID? Icon { get; set; }
            public int Pallet
            {
                get => _pallet; set => _pallet = value < Memory.Icons.PalletCount ? value : 2;
            }

            public IGMDataItem_String(FF8String data, Rectangle? pos = null, Font.ColorID? color = null) : base(pos)
            {
                Data = data;
                Colorid = color ?? Font.ColorID.White;
            }

            public IGMDataItem_String(Icons.ID? icon, int pallet, FF8String data, Rectangle? pos = null, Font.ColorID? color = null) : base(pos)
            {
                Icon = icon;
                Pallet = pallet;
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
                        Memory.Icons.Draw(Icon, Pallet, r2, new Vector2(Scale.X), fade);
                        r.Offset(Memory.Icons.GetEntryGroup(Icon).Width * Scale.X, 0);
                    }
                    Memory.font.RenderBasicText(Data, r.Location, Scale, Fade: fade, color: Colorid);
                }
            }
        }
        #endregion Classes
    }
}