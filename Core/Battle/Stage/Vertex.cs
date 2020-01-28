using Microsoft.Xna.Framework;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        private struct Vertex
        {
            public short X;
            public short Y;
            public short Z;

            public static Vertex Read(BinaryReader br)
            => new Vertex()
            {
                X = br.ReadInt16(),
                Y = br.ReadInt16(),
                Z = br.ReadInt16(),
            };
            public static implicit operator Vector3(Vertex v) => new Vector3(v.X,v.Y,v.Z)/100f;
        }
    }
}