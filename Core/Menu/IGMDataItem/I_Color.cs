using Microsoft.Xna.Framework;

namespace OpenVIII.IGMDataItem
{
    public interface I_Color
    {
        #region Properties

        Color Color { get; set; }
        Color Faded_Color { get; set; }

        #endregion Properties
    }
}