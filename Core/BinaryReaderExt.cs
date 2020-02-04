using Microsoft.Xna.Framework;
using System.IO;

namespace OpenVIII
{
    public static class BinaryReaderExt
    {
        #region Methods

        public static Vector3 ReadVertex(this BinaryReader br) => new Vector3(br.ReadVertexDim(), 0 - br.ReadVertexDim(), br.ReadVertexDim());

        public static float ReadVertexDim(this BinaryReader br) => br.ReadInt16() / 2000.0f;

        #endregion Methods
    }
}