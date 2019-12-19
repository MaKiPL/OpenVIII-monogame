using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

namespace OpenVIII.Battle
{
    /// <summary>
    /// Magic Obj Reader
    /// </summary>
    /// <see cref="http://forums.qhimm.com/index.php?topic=15056.msg211220"/>
    /// <seealso cref="http://forums.qhimm.com/index.php?topic=16283.0"/>
    /// <seealso cref="http://forums.qhimm.com/index.php?topic=15906.0"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_magfiles"/>
    /// <seealso cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/GF_AlternativeTexture.cs"/>
    /// <seealso cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/GfEnviro.cs"/>
    public class Mag
    {
        #region Fields

        public uint
            pBones,
            pTextureLimit,
            pGeometry,
            pSCOT,
            pTexture;

        private static readonly ushort[] _knownPolygons = new ushort[]
        {
            0x2,
            0x6,
            0x7,
            0x8,
            0x9,
            0xC,
            0x10,
            0x12,
            0x11,
            0x13,
        };

        public List<Geometry> Geometries;

        #endregion Fields

        #region Constructors

        public Mag(string filename, BinaryReader br)
        {
            FileName = filename;
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            if (TryReadTIM(br) == null)
            {
                isTIM = false;
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                //Offset Description
                //0x00    Probably always null
                uint pPadding = br.ReadUInt32();
                if (pPadding != 0)
                    return;
                //0x04    Probably bones/ animation data, might be 0x00
                pBones = br.ReadUInt32();
                //0x08    Unknown(used to determinate texture size *), might be 0x64
                pTextureLimit = br.ReadUInt32();
                //0x0C    Geometry pointer, might be 0xAC
                pGeometry = br.ReadUInt32();
                //0x10    SCOT pointer, might be 0x00
                pSCOT = br.ReadUInt32();
                //0x14    Texture pointer, might be 0x30
                pTexture = br.ReadUInt32();
                //0x18 == 0x98
                //0x1C == 0xAC
                if (pBones > br.BaseStream.Length ||
                    pTextureLimit > br.BaseStream.Length ||
                    pGeometry > br.BaseStream.Length ||
                    pSCOT > br.BaseStream.Length ||
                    pTexture > br.BaseStream.Length)
                {
                    return;
                }
                if (FileName == "c:\\ff8\\data\\eng\\battle\\mag201_b.19")
                {
                }
                else if (FileName == "c:\\ff8\\data\\eng\\battle\\mag204_b.04")
                {
                }
                isPackedMag = true;
                ReadGeometry(br);
                ReadTextures(br);
            }
        }

        #endregion Constructors

        #region Properties

        public byte DataType => getValue(2);

        public string FileName { get; private set; }

        public byte IDnumber => getValue(1);

        public bool isPackedMag { get; private set; } = false;

        public bool isTIM { get; private set; } = false;

        public byte SequenceNumber => getValue(3);

        public TIM2[] TIM { get; private set; }

        private bool bFileNameTest => FileName != null && Path.GetExtension(FileName).Trim('.').Length == 3;

        /// <summary>
        /// if quit loop because unknown type.
        /// </summary>
        public int UnknownType { get; private set; } = int.MinValue;

        #endregion Properties

        #region Methods

        public TIM2[] TryReadTIM(BinaryReader br)
        {
            try
            {
                TIM2 tim = new TIM2(br, noExec: true);
                if (tim.NOT_TIM)
                    return TIM = null;
                isTIM = true;
                return TIM = new TIM2[] { tim };
            }
            catch (InvalidDataException)
            {
                return TIM = null;
            }
        }

        private byte getValue(int index) => bFileNameTest ? byte.Parse(FileName.Substring(FileName.Length - index, 1),
                                                System.Globalization.NumberStyles.HexNumber) : (byte)0xff;

        private void ReadGeometry(BinaryReader br)
        {
            if (pGeometry == 0) return;
            br.BaseStream.Seek(pGeometry, SeekOrigin.Begin);
            int count = br.ReadInt32();
            //count = count > 0x24 ? 0x24 : count; // unsure why.
            List<uint> positions = new List<uint>();
            while (count-- > 0)
            {
                uint pos = br.ReadUInt32();
                if (pos > 0 && pos + pGeometry < br.BaseStream.Length)
                    positions.Add(pos + pGeometry);
            }

            Geometries = new List<Geometry>(positions.Count);

            foreach (uint pos in positions)
            {
                Geometry g = new Geometry();
                const int PassFromStart = 24;
                bool OnlyVertex = true;
                br.BaseStream.Seek(pos, SeekOrigin.Begin);
                count = br.ReadInt32();

                if (count > 12u)
                    continue;
                if (count != 2u)
                    OnlyVertex = false;
                br.BaseStream.Seek(pos + 8, SeekOrigin.Begin);
                uint _relativeJump = br.ReadUInt32() + pos;
                br.BaseStream.Seek(pos + PassFromStart, SeekOrigin.Begin);
                int _vertexCount = br.ReadUInt16() * 8;
                br.BaseStream.Seek(pos + PassFromStart - 4, SeekOrigin.Begin);
                uint _verticesOffset = br.ReadUInt16() + pos;
                ReadVertices();
                if (OnlyVertex) { continue; }
                br.BaseStream.Seek(_relativeJump, SeekOrigin.Begin);
                ushort _polygonType = br.ReadUInt16();
                ushort polygons = br.ReadUInt16();
                bool _generatefaces;
                bool isknownpolygon() => _knownPolygons.Any(x => x == _polygonType);
                long localoffset = _relativeJump + 4;
                int safeHandle = 0;
                while (!(_generatefaces = !isknownpolygon()))
                {
                    switch (_polygonType)
                    {
                        case 0x2:
                        case 0x7:
                            GetTriangle(20, 0xC);
                            break;

                        case 0x6:
                            GetTriangle(12, 0x4);
                            break;

                        case 0x8:
                            GetTriangle(20, 0xA);
                            break;

                        case 0x9:
                            GetTriangle(28, 0x12);
                            break;

                        case 0xC:
                            GetQuad(28, 0x14);
                            break;

                        case 0x12:
                            GetQuad(24, 0xC);
                            break;

                        case 0x13:
                            GetQuad(36, 0x18);
                            break;

                        case 0x10:
                            GetQuad(12, 0x4);
                            break;

                        case 0x11:
                            GetQuad(24, 0x10);
                            break;
                    }
                    br.BaseStream.Seek(localoffset + safeHandle, SeekOrigin.Begin);
                    if (br.BaseStream.Position + 4 > br.BaseStream.Length) return;
                    if (br.ReadUInt32() == uint.MaxValue)
                        break;
                    localoffset += safeHandle;
                    _polygonType = br.ReadUInt16();
                    polygons = br.ReadUInt16();
                    localoffset += 4;
                    void GetTriangle(int cnt, int off0)
                    {
                        int off1 = off0 + 2, off2 = off0 + 4;
                        int size = polygons * cnt;
                        for (int i = 0; i < size; i += cnt)
                        {
                            g.Triangles.Add(new Vector3(GetPolygonIndex(localoffset + i + off0),
                                GetPolygonIndex(localoffset + i + off1),
                                GetPolygonIndex(localoffset + i + off2)));
                        }
                        safeHandle = size;
                    }
                    void GetQuad(int cnt, int off0)
                    {
                        int off1 = off0 + 2, off2 = off0 + 6, off3 = off0 + 4;
                        int size = polygons * cnt;
                        for (int i = 0; i < size; i += cnt)
                        {
                            g.Quads.Add(new Vector4(GetPolygonIndex(localoffset + i + off0),
                                GetPolygonIndex(localoffset + i + off1),
                                GetPolygonIndex(localoffset + i + off2),
                                GetPolygonIndex(localoffset + i + off3)));
                        }
                        safeHandle = size;
                    }
                    ushort GetPolygonIndex(long offset)
                    {
                        br.BaseStream.Seek(offset, SeekOrigin.Begin);
                        ushort temp = checked((ushort)(br.ReadUInt16() / 8));
                        Debug.Assert(temp < g.Vertices.Count);
                        //return temp == 0 ? 1 : (temp / 8) + 1;
                        return temp;
                    }
                }
                if (_generatefaces)
                {
                    UnknownType = _polygonType;
                }
                void ReadVertices()
                {
                    br.BaseStream.Seek(_verticesOffset, SeekOrigin.Begin);
                    if (_vertexCount % 8 != 0) return;
                    for (int i = 0; i < _vertexCount / 8; i++)
                    {
                        br.BaseStream.Seek(_verticesOffset + i * 8, SeekOrigin.Begin);
                        g.Vertices.Add(br.ReadVertex());
                    }
                }
                Geometries.Add(g);
            }
        }

        private TIM2[] ReadTextures(BinaryReader br)
        { //this doesn't sound like a tim file per documentation.
            if (pTexture == 0 ||
                pTexture >= pTextureLimit ||
                pTexture + pTextureLimit >= br.BaseStream.Length ||
                pTextureLimit - pTexture % 4 != 0
                ) return null;

            br.BaseStream.Seek(pTexture, SeekOrigin.Begin);
            List<uint> positions = new List<uint>();
            //Debug.Assert(pTextureSize > 0 && pTextureSize < br.BaseStream.Length);
            while (pTextureLimit > 0 && br.BaseStream.Position < pTextureLimit && br.BaseStream.Position + 4 < br.BaseStream.Length)
            {
                uint pos = br.ReadUInt32();
                if (pos != 0)
                    positions.Add(pos + pTexture);
            }
            if (positions.Count > 0)
            {
                //textures here?
                return null;
            }
            else
                return null;
        }

        #endregion Methods

        #region Classes

        public class Geometry
        {
            #region Constructors

            public Geometry()
            {
                Vertices = new List<Vector3>();
                Triangles = new List<Vector3>();
                Quads = new List<Vector4>();
            }

            #endregion Constructors

            #region Properties

            public List<Vector4> Quads { get; private set; }
            public List<Vector3> Triangles { get; private set; }
            public List<Vector3> Vertices { get; private set; }

            #endregion Properties
        }

        #endregion Classes
    }
}