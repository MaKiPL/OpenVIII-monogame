using Microsoft.Xna.Framework;
using System.IO;

namespace OpenVIII
{
    public static class BinaryReaderExt
    {
        #region Methods

        public static Vector3 ReadVertex(this BinaryReader br)
        {
            if (br.BaseStream.Position + 6 < br.BaseStream.Length)
                return new Vector3(br.ReadVertexDim(), 0 - br.ReadVertexDim(), br.ReadVertexDim());
            return default;
        }

        public static float ReadVertexDim(this BinaryReader br) => br.ReadInt16() / 2000.0f;

        #endregion Methods
    }
}