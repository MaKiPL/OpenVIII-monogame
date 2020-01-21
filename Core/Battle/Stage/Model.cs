using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Structs

        private struct Model
        {
            #region Fields

            public Quad[] quads;
            public Triangle[] triangles;
            public Vertex[] vertices;

            #endregion Fields

            #region Methods

            /// <summary>
            /// This is the main class that reads given Stage geometry group. It stores the data into
            /// Model structure
            /// </summary>
            /// <param name="pointer">absolute pointer in buffer for given Stage geometry group</param>
            /// <returns></returns>
            public static Model Read(uint pointer, BinaryReader br)
            {
                bool bSpecial = false;
                br.BaseStream.Seek(pointer, System.IO.SeekOrigin.Begin);
                uint header = Extended.UintLittleEndian(br.ReadUInt32());
                if (header != 0x01000100) //those may be some switches, but I don't know what they mean
                {
                    Memory.Log.WriteLine("WARNING- THIS STAGE IS DIFFERENT! It has weird object section. INTERESTING, TO REVERSE!");
                    bSpecial = true;
                }
                ushort verticesCount = br.ReadUInt16();
                Vertex[] vertices = new Vertex[verticesCount];
                for (int i = 0; i < verticesCount; i++)
                    vertices[i] = Vertex.Read(br);
                if (bSpecial && Memory.Encounters.Current().Scenario == 20)
                    return new Model();
                br.BaseStream.Seek((br.BaseStream.Position % 4) + 4, SeekOrigin.Current);
                ushort trianglesCount = br.ReadUInt16();
                ushort quadsCount = br.ReadUInt16();
                br.BaseStream.Seek(4, SeekOrigin.Current);
                Triangle[] triangles = new Triangle[trianglesCount];
                Quad[] quads = new Quad[quadsCount];
                if (trianglesCount > 0)
                    for (int i = 0; i < trianglesCount; i++)
                        triangles[i] = Triangle.Read(br);
                if (quadsCount > 0)
                    for (int i = 0; i < quadsCount; i++)
                        quads[i] = Quad.Read(br);
                return new Model()
                {
                    vertices = vertices,
                    triangles = triangles,
                    quads = quads
                };
            }

            #endregion Methods
        }

        #endregion Structs
    }
}