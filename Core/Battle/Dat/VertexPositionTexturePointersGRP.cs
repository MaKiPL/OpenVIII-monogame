using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII.Battle.Dat
{
    public struct VertexPositionTexturePointersGRP
    {
        #region Constructors

        public VertexPositionTexturePointersGRP(VertexPositionTexture[] vpt, byte[] texturePointers) : this()
        {
            this.VPT = vpt;
            this.TexturePointers = texturePointers;
        }

        #endregion Constructors

        #region Properties

        public byte[] TexturePointers { get; }
        public VertexPositionTexture[] VPT { get; }

        #endregion Properties
    }
}