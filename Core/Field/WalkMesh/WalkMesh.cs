using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// WalkMesh
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF7/Field/Walkmesh"/>
    public partial class WalkMesh
    {
        public static WalkMesh Load(byte[] idb)
        {
            WalkMesh r = new WalkMesh();
            r.ReadData(idb);
            return r;
        }

        public List<Access> Accesses { get; private set; }
        public List<VertexPositionColor> Vertices { get; private set; }

        public int Count => Vertices?.Count / 3 ?? 0;

        private void ReadData(byte[] idb)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(idb)))
            {
                int count = checked((int)br.ReadUInt32());
                List<Vert> vs = new List<Vert>(count * 3);
                List<Access> access = new List<Access>(count);
                while (count-- > 0)
                {
                    foreach (int i in Enumerable.Range(0, 2))
                        vs.Add(new Vert { x = br.ReadInt16(), y = br.ReadInt16(), z = br.ReadInt16(), res = br.ReadInt16() });
                }
                count = access.Capacity;

                while (count-- > 0)
                {
                    foreach (int i in Enumerable.Range(0, 2))
                        access.Add(new Access { br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16() });
                }
                Accesses = access;
                Vertices = vs.Select((x, i) => new VertexPositionColor(new Vector3(-x.x, x.y, x.z/4096f)/4, i % 3 == 0 ? Color.Red : i % 3 == 1 ? Color.Green : Color.Blue)).ToList();
            }
        }
    }
}