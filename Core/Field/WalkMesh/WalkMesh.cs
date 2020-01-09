using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
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
                const int sides = 3;
                List<Vert> vs = new List<Vert>(count * sides);
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
                max = new Vector3(vs.Max(x => x.x), vs.Max(x => x.y), vs.Max(x => x.z));
                min = new Vector3(vs.Min(x => x.x), vs.Min(x => x.y), vs.Min(x => x.z));
                distance = max - min;
                float maxvalue = Math.Max(Math.Max(distance.X, distance.Y), distance.Z);
                float scale = 6f;
                Vertices = vs.Select((x, i) => new VertexPositionColor(new Vector3(x.x, x.y, x.z)/ scale, i % sides == 0 ? Color.Red : i % sides == 1 ? Color.Green : Color.Blue)).ToList();
            }
        }
        public Vector3 max { get; private set; }
        public Vector3 min { get; private set; }
        public Vector3 distance { get; private set; }
    }
}