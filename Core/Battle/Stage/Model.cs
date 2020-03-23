using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Structs

        private struct Model
        {
            #region Fields

            public readonly IReadOnlyList<Quad> Quads;
            public readonly IReadOnlyList<Triangle> Triangles;
            public readonly IReadOnlyList<Vertex> Vertices;

            #endregion Fields

            #region Constructors

            private Model(BinaryReader br)
            {
                bool bSpecial = false;
                uint header = Extended.UintLittleEndian(br.ReadUInt32());
                if (header != 0x01000100) //those may be some switches, but I don't know what they mean
                {
                    Memory.Log.WriteLine("WARNING- THIS STAGE IS DIFFERENT! It has weird object section. INTERESTING, TO REVERSE!");
                    bSpecial = true;
                }

                Vertices = Enumerable.Range(0, br.ReadUInt16()).Select(_ => Vertex.Read(br)).ToList().AsReadOnly();
                if (bSpecial && Memory.Encounters.Scenario == 20)
                {
                    Triangles = null;
                    Quads = null;
                    return;
                }
                br.BaseStream.Seek((br.BaseStream.Position % 4) + 4, SeekOrigin.Current);
                ushort trianglesCount = br.ReadUInt16();
                ushort quadsCount = br.ReadUInt16();
                br.BaseStream.Seek(4, SeekOrigin.Current);
                Triangles = Enumerable.Range(0, trianglesCount).Select(_ => Triangle.Read(br)).ToList().AsReadOnly();
                Quads = Enumerable.Range(0, quadsCount).Select(_ => Quad.Read(br)).ToList().AsReadOnly();
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// This is the main class that reads given Stage geometry group. It stores the data into
            /// Model structure
            /// </summary>
            /// <param name="pointer">absolute pointer in buffer for given Stage geometry group</param>
            /// <returns></returns>
            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static Model Read(uint pointer, BinaryReader br)
            {
                br.BaseStream.Seek(pointer, SeekOrigin.Begin);
                return new Model(br);
            }

            #endregion Methods
        }

        #endregion Structs
    }
}