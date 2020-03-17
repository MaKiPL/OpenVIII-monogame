using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 2b: Object Data
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_DAT#Object_Data"/>
    public class Object
    {
        #region Fields

        public readonly ushort CVertices;
        public readonly IReadOnlyList<VertexData> VertexData;
        public readonly ushort CTriangles;
        public readonly ushort CQuads;
        public readonly ulong Padding;
        public readonly IReadOnlyList<Triangle> Triangles;
        public readonly IReadOnlyList<Quad> Quads;

        #endregion Fields

        public static Object CreateInstance(BinaryReader br, long byteOffset)
        {
            br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
            return new Object(br);
        }

        private Object(BinaryReader br)
        { 
            CVertices = br.ReadUInt16();
            VertexData = Dat.VertexData.CreateInstances(br, CVertices);
            //padding
            br.BaseStream.Seek(4 - (br.BaseStream.Position % 4 == 0 ? 4 : br.BaseStream.Position % 4), SeekOrigin.Current);
            CTriangles = br.ReadUInt16();
            CQuads = br.ReadUInt16();
            Padding = br.ReadUInt64();
            Debug.Assert(Padding == 0);
            if (CTriangles == 0 && CQuads == 0)
                return;
            Triangles = Triangle.CreateInstances(br,CTriangles);
            Quads = Quad.CreateInstances(br, CQuads);
        }
    }
}