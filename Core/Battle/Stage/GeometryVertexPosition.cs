using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        public class GeometryVertexPosition
        {
            #region Fields

            public GeometryInfoSupplier[] GeometryInfoSupplier;
            public VertexPositionTexture[] VertexPositionTexture;

            #endregion Fields

            #region Constructors

            public GeometryVertexPosition(GeometryInfoSupplier[] gis, VertexPositionTexture[] vpt)
            {
                this.GeometryInfoSupplier = gis;
                this.VertexPositionTexture = vpt;
            }

            #endregion Constructors
        }

        #endregion Classes
    }
}