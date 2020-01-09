using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Accesses = new List<Access>(count);
                foreach (int i in Enumerable.Range(0, vs.Capacity))
                        vs.Add(new Vert { x = br.ReadInt16(), y = br.ReadInt16(), z = br.ReadInt16(), res = br.ReadInt16() });
                foreach (int i in Enumerable.Range(0, Accesses.Capacity))
                    Accesses.Add(new Access { br.ReadInt16(), br.ReadInt16(), br.ReadInt16() });
                Debug.Assert(br.BaseStream.Position == br.BaseStream.Length);
                max = new Vector3(vs.Max(x => x.x), vs.Max(x => x.y), vs.Max(x => x.z));
                min = new Vector3(vs.Min(x => x.x), vs.Min(x => x.y), vs.Min(x => x.z));
                distance = max - min;
                float maxvalue = Math.Max(Math.Max(distance.X, distance.Y), distance.Z);
                //Matrix scale = Matrix.CreateTranslation(0, 0, -Module.Cameras[0].Zoom);
                Matrix scale = Matrix.CreateScale(Module.Cameras[0].Zoom / 4096f);
                Vertices = vs.Select((x, i) => new VertexPositionColor(Vector3.Transform((new Vector3(x.x, x.y , x.z)),scale), i % sides == 0 ? Color.Red : i % sides == 1 ? Color.Green : Color.Blue)).ToList();
            }
        }
        public Vector3 max { get; private set; }
        public Vector3 min { get; private set; }
        public Vector3 distance { get; private set; }
    }
}