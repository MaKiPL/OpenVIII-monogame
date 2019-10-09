using Microsoft.Xna.Framework;

namespace OpenVIII.IGMDataItem
{
    public class Face : Base, I_Data<Faces.ID>
    {
        #region Fields

        private byte _palette;

        #endregion Fields

        #region Constructors

        public Face(Faces.ID data, Rectangle? pos = null, bool blink = false, float blink_adjustment = 1f) : base(pos)
        {
            Data = data;
            Blink = blink;
            Blink_Adjustment = blink_adjustment;
        }

        #endregion Constructors

        #region Properties

        public Faces.ID Data { get; set; }
        public byte Palette
        {
            get => _palette; set
            {
                if (value >= 16) value = 2;
                _palette = value;
            }
        }

        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
            {
                Memory.Faces.Draw(Data, Pos, Vector2.UnitY, Fade);
                if (Blink)
                    Memory.Faces.Draw(Data, Pos, Vector2.UnitY, Fade * Blink_Amount * Blink_Adjustment);
            }
        }

        #endregion Methods
    }
}