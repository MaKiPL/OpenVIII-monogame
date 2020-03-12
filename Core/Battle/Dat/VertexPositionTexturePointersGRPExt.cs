using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Battle.Dat
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class VertexPositionTexturePointersGRPExt
    {
        #region Methods

        public static bool IsNotSet(this VertexPositionTexturePointersGRP vertexPositionTexturePointersGRP)
        {
            if ((vertexPositionTexturePointersGRP.VPT?.Length ?? 0) <= 0 ||
                (vertexPositionTexturePointersGRP.TexturePointers?.Length ?? 0) <= 0) return true;
            //3 vertices per every texture pointer.
            Debug.Assert(vertexPositionTexturePointersGRP.VPT.Length / 3 == vertexPositionTexturePointersGRP.TexturePointers.Length);
            return false;
        }

        public static bool IsSet(this VertexPositionTexturePointersGRP vertexPositionTexturePointersGRP) => !vertexPositionTexturePointersGRP.IsNotSet();

        #endregion Methods
    }
}