using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    public class VertexData
    {
        #region Fields

        public readonly ushort BoneId;
        public readonly ushort CVertices;
        public readonly IReadOnlyList<Vector3> Vertices;

        #endregion Fields

        #region Constructors

        private VertexData(BinaryReader br)
        {
            BoneId = br.ReadUInt16();
            CVertices = br.ReadUInt16();
            Vertices = Vertex.CreateInstances(br, CVertices);
        }

        #endregion Constructors

        #region Methods

        public static VertexData CreateInstance(BinaryReader br) => new VertexData(br);

        public static IReadOnlyList<VertexData> CreateInstances(BinaryReader br, ushort count)
        => Enumerable.Range(0, count).Select(_ => CreateInstance(br)).ToList().AsReadOnly();

        #endregion Methods
    }
}