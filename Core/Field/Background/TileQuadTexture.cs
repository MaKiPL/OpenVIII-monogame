using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII.Fields
{
    public partial class Background
    {
        #region Classes

        private class TileQuadTexture
        {
            #region Fields

            private readonly VertexPositionTexture[] _cache;
            private bool _enabled;

            #endregion Fields

            #region Constructors

            public TileQuadTexture(Tile tile, TextureHandler texture, float scale)
            {
                _enabled = true;
                GetTile = tile;
                _cache = tile.GetQuad(scale);
                Texture = texture;
            }

            #endregion Constructors

            #region Properties

            public byte AnimationID => GetTile.AnimationID;

            public byte AnimationState => GetTile.AnimationState;

            /*
                        public BlendMode BlendMode => _tile.BlendMode;
            */

            public bool Enabled => _enabled && Texture != null;

            public Tile GetTile { get; }

            public TextureHandler Texture { get; }

            #endregion Properties

            #region Methods

            public static explicit operator Tile(TileQuadTexture @in) => @in.GetTile;

            public static implicit operator Texture2D(TileQuadTexture @in) => (Texture2D)@in.Texture;

            public static implicit operator VertexPositionTexture[] (TileQuadTexture @in) => @in._cache;

            public void Hide() => _enabled = false;

            public void Show() => _enabled = true;

            #endregion Methods
        }

        #endregion Classes

        // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }
}