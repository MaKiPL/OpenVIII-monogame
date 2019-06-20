using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        public class IGMDataItem_Face : IGMDataItem
        {
            private byte _palette;

            public Faces.ID Data { get; set; }

            public byte Palette
            {
                get => _palette; set
                {
                    if (value >= 16) value = 2;
                    _palette = value;
                }
            }

            public bool Blink { get; private set; }
            public float Blink_Adjustment { get; set; }

            public IGMDataItem_Face(Faces.ID data, Rectangle? pos = null, bool blink=false, float blink_adjustment = 1f) : base(pos)
            {
                Data = data;
                Blink = blink;
                Blink_Adjustment = blink_adjustment;
            }

            public override void Draw()
            {
                if (Enabled)
                {
                    Memory.Faces.Draw(Data, Pos, Vector2.UnitY, fade);
                    if (Blink)
                        Memory.Faces.Draw(Data, Pos, Vector2.UnitY, fade * blink_Amount * Blink_Adjustment);
                }
            }
        }
        #endregion Classes
    }
}