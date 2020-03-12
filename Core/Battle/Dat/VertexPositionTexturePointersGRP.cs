using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII.Battle.Dat
{
    public struct VertexPositionTexturePointersGRP
    {
        public VertexPositionTexturePointersGRP(VertexPositionTexture[] vpt, byte[] texturePointers) : this()
        {
            this.VPT = vpt;
            this.TexturePointers = texturePointers;
        }

        public VertexPositionTexture[] VPT { get; }
        public byte[] TexturePointers { get; }
    }
}