using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    public partial class Background
    {
        #region Classes

        private class TileQuadTexture
        {
            #region Fields

            private VertexPositionTexture[] cache;
            private bool enabled;
            private TextureHandler texture;
            private Tile tile;

            #endregion Fields

            #region Constructors

            public TileQuadTexture(Tile tile, TextureHandler texture, float scale)
            {
                this.enabled = true;
                this.tile = tile;
                this.cache = tile.GetQuad(scale);
                this.texture = texture;
            }

            #endregion Constructors

            #region Properties

            public byte AnimationID => tile.AnimationID;

            public byte AnimationState => tile.AnimationState;

            public BlendMode BlendMode => tile.BlendMode;

            public bool Enabled => enabled && texture != null;

            public Tile GetTile => tile;

            #endregion Properties

            #region Methods

            public static explicit operator Tile(TileQuadTexture @in) => @in.tile;

            public static implicit operator Texture2D(TileQuadTexture @in) => (Texture2D)@in.texture;

            public static implicit operator VertexPositionTexture[] (TileQuadTexture @in) => @in.cache;

            public void Hide() => enabled = false;

            public void Show() => enabled = true;

            #endregion Methods
        }

        #endregion Classes

        // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }
}