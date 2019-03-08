using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF8
{
    public class Debug_battleDat
    {
        int id;

        public struct DatFile
        {
            public uint cSections;
            public uint[] pSections;
            public uint eof;
        }

        public DatFile datFile;

        #region section 1
        //[StructLayout(LayoutKind.Sequential,Pack = 1,Size =]
        public struct Skeleton
        {
            public ushort cBones;
            public byte[] unk;
            public Bone[] bones;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 48)]
        public struct Bone
        {
            public ushort parentId;
            public ushort boneSize;
            public byte[] Unk;
        }

        public Skeleton skeleton;

        #endregion

        #region section 2
        public struct Geometry
        {
            public uint cObjects;
            public uint[] pObjects;
            public Object[] objects;
            public uint cTotalVert;
        }

        public struct Object
        {
            public ushort cVertices;
            public VerticeData[] verticeData;
            public ushort cTriangles;
            public ushort cQuads;
            public ulong padding;
            public Triangle[] triangles;
            public Quad[] quads;
        }

        public struct VerticeData
        {
            public ushort boneId;
            public ushort cVertices;
            public Vertex[] vertices;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
        public struct Vertex
        {
            private short x;
            private short y;
            private short z;

            public Vector3 GetVector => new Vector3(X, -Z, Y);

            public short X { get => (short)(x/500); set => x = value; }
            public short Y { get => (short)(y/500); set => y = value; }
            public short Z { get => (short)(z/500); set => z = value; }
        }

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =16)]
        public struct Triangle
        {
            private ushort A;
            private ushort B;
            private ushort C;
            public UV vta;
            public UV vtb;
            public ushort texUnk;
            public UV vtc;
            public ushort u;

            public ushort A1 { get => (ushort)(A & 0xFFF); set => A = value; }
            public ushort B1 { get => (ushort)(B & 0xFFF); set => B = value; }
            public ushort C1 { get => (ushort)(C & 0xFFF); set => C = value; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 20)]
        public struct Quad
        {
            private ushort A;
            private ushort B;
            private ushort C;
            private ushort D;
            public UV vta;
            public ushort texUnk;
            public UV vtb;
            public ushort u;
            public UV vtc;
            public UV vtd;

            public ushort A1 { get => (ushort)(A & 0xFFF); set => A = value; }
            public ushort B1 { get => (ushort)(B & 0xFFF); set => B = value; }
            public ushort C1 { get => (ushort)(C & 0xFFF); set => C = value; }
            public ushort D1 { get => (ushort)(D & 0xFFF); set => D = value; }
        }

        [StructLayout(LayoutKind.Sequential, Pack =1,Size =2)]
        public struct UV
        {
            private byte U;
            private byte V;

            public float U1 { get => U/256.0f; set => U = (byte)value; }
            public float V1 { get => V/256.0f; set => V = (byte)value; }
        }

        public Geometry geometry;

        private void ReadSection2(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            geometry = new Geometry { cObjects = br.ReadUInt32() };
            geometry.pObjects = new uint[geometry.cObjects];
            for (int i = 0; i < geometry.cObjects; i++)
                geometry.pObjects[i] = br.ReadUInt32();
            geometry.objects = new Object[geometry.cObjects];
            for (int i = 0; i < geometry.cObjects; i++)
                geometry.objects[i] = ReadGeometryObject(ms, br);
            geometry.cTotalVert = br.ReadUInt32();
        }

        private Object ReadGeometryObject(MemoryStream ms, BinaryReader br)
        {
            Object @object = new Object { cVertices = br.ReadUInt16() };
            @object.verticeData = new VerticeData[@object.cVertices];
            for (int n = 0; n < @object.cVertices; n++){
                @object.verticeData[n].boneId = br.ReadUInt16();
                @object.verticeData[n].cVertices = br.ReadUInt16();
                @object.verticeData[n].vertices = new Vertex[@object.verticeData[n].cVertices];
                for (int i = 0; i < @object.verticeData[n].cVertices; i++)
                    @object.verticeData[n].vertices[i] = MakiExtended.ByteArrayToStructure<Vertex>(br.ReadBytes(6));}
            ms.Seek(4-(ms.Position%4), SeekOrigin.Current);
            @object.cTriangles = br.ReadUInt16();
            @object.cQuads = br.ReadUInt16();
            @object.padding = br.ReadUInt64();
            @object.triangles = new Triangle[@object.cTriangles];
            @object.quads = new Quad[@object.cQuads];
            for (int i = 0; i < @object.cTriangles; i++)
                @object.triangles[i] = MakiExtended.ByteArrayToStructure<Triangle>(br.ReadBytes(16));
            for (int i = 0; i < @object.cQuads; i++)
                @object.quads[i] = MakiExtended.ByteArrayToStructure<Quad>(br.ReadBytes(20));
            return @object;
        }

        public VertexPositionTexture[] GetVertexPositions(int objectId, Vector3 position)
        {
            //THIS IS DEBUG IN-DEV CLASS
            Object obj = geometry.objects[objectId];
            uint totalVerts = (uint)obj.verticeData.Sum(x => x.cVertices);
            Vector3[] vecTable = new Vector3[totalVerts]; //DEBUG
            int totalindex = 0;
            int i;
            for (i = 0; i<obj.cVertices; i++)
                for (int n = 0; n < obj.verticeData[i].cVertices; n++)
                    vecTable[totalindex++] = obj.verticeData[i].vertices[n].GetVector;
            VertexPositionTexture[] vpt = new VertexPositionTexture[obj.cTriangles*3+obj.cQuads*6]; //6 because retriangulation is needed;
            i = 0;
            for (; i < obj.cTriangles; i++) {
                vpt[i * 3 + 0] = new VertexPositionTexture(position + vecTable[obj.triangles[i].A1], new Vector2(obj.triangles[i].vta.U1, obj.triangles[i].vta.V1));
                vpt[i * 3 + 1] = new VertexPositionTexture(position + vecTable[obj.triangles[i].B1], new Vector2(obj.triangles[i].vtb.U1, obj.triangles[i].vtb.V1));
                vpt[i * 3 + 2] = new VertexPositionTexture(position + vecTable[obj.triangles[i].C1], new Vector2(obj.triangles[i].vtc.U1, obj.triangles[i].vtc.V1)); }
            for (int n = 0; n < obj.cQuads; n++) /*quad retriangulation*/ {
                vpt[i + n * 6 + 0] = new VertexPositionTexture(position + vecTable[obj.quads[n].A1], new Vector2(obj.quads[n].vta.U1, obj.quads[n].vta.V1));
                vpt[i + n * 6 + 1] = new VertexPositionTexture(position + vecTable[obj.quads[n].B1], new Vector2(obj.quads[n].vtb.U1, obj.quads[n].vtb.V1));
                vpt[i + n * 6 + 2] = new VertexPositionTexture(position + vecTable[obj.quads[n].D1], new Vector2(obj.quads[n].vtd.U1, obj.quads[n].vtd.V1));
                vpt[i + n * 6 + 3] = new VertexPositionTexture(position + vecTable[obj.quads[n].A1], new Vector2(obj.quads[n].vta.U1, obj.quads[n].vta.V1));
                vpt[i + n * 6 + 4] = new VertexPositionTexture(position + vecTable[obj.quads[n].C1], new Vector2(obj.quads[n].vtc.U1, obj.quads[n].vtc.V1));
                vpt[i + n * 6 + 5] = new VertexPositionTexture(position + vecTable[obj.quads[n].D1], new Vector2(obj.quads[n].vtd.U1, obj.quads[n].vtd.V1));
                ++i;}
            return vpt;
        }
        #endregion

        #region section 3
        public struct AnimHeader
        {
            public uint cAnimations;
            public uint[] pAnimations;
        }

        public struct Animation
        {
            public byte cFrames;
        }
        #endregion

        #region section 7
        [StructLayout(LayoutKind.Sequential, Pack =1, Size =380)]
        public struct Information
        {
            [MarshalAs(UnmanagedType.ByValArray,SizeConst =24)]
            private char[] monsterName;
            public uint hp;
            public uint str;
            public uint vit;
            public uint mag;
            public uint spr;
            public uint spd;
            public uint eva;
            public Abilities abilitiesLow;
            public Abilities abilitiesMed;
            public Abilities abilitiesHigh;
            public byte medLevelStart;
            public byte highLevelStart;
            public byte unk;
            public byte bitSwitch;
            public byte cardLow;
            public byte cardMed;
            public byte cardHigh;
            public byte devourLow;
            public byte devourMed;
            public byte devourHigh;
            public byte bitSwitch2;
            public byte unk2;
            public ushort extraExp;
            public ushort exp;
            public ulong drawLow;
            public ulong drawMed;
            public ulong drawHigh;
            public ulong mugLow;
            public ulong mugMed;
            public ulong mugHigh;
            public ulong dropLow;
            public ulong dropMed;
            public ulong dropHigh;
            public byte mugRate;
            public byte dropRate;
            public byte padding;
            public byte ap;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst =16)]
            public byte[] unk3;
            public byte fireResistance;
            public byte iceResistance;
            public byte thunderResistance;
            public byte earthResistance;
            public byte poisonResistance;
            public byte windResistance;
            public byte waterResistance;
            public byte holyResistance;

            public byte deathResistanceMental;
            public byte poisonResistanceMental;
            public byte petrifyResistanceMental;
            public byte darknessResistanceMental;
            public byte silenceResistanceMental;
            public byte berserkResistanceMental;
            public byte zombieResistanceMental;
            public byte sleepResistanceMental;
            public byte hasteResistanceMental;
            public byte slowResistanceMental;
            public byte stopResistanceMental;
            public byte regenResistanceMental;
            public byte reflectResistanceMental;
            public byte doomResistanceMental;
            public byte slowPetrifyResistanceMental;
            public byte floatResistanceMental;
            public byte confuseResistanceMental;
            public byte drainResistanceMental;
            public byte explusionResistanceMental;
            public byte unkResistanceMental;

            public string GetNameNormal => new string(monsterName);
        }

        public struct Abilities
        {
            public byte kernelId; //0x2 magic, 0x4 item, 0x8 mosterAbility;
            public byte unk;
            public ushort abilityId;

        }

        public Information information;
        #endregion

        #region section 11
        public struct Textures
        {
            public uint cTims;
            public uint[] pTims;
            public uint Eof;
            public TIM2[] Tims;
        }

        public Textures textures;
        #endregion

        private void ReadSection11(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            textures = new Textures() { cTims = br.ReadUInt32() };
            textures.pTims = new uint[textures.cTims];
            for (int i = 0; i < textures.cTims; i++)
                textures.pTims[i] = br.ReadUInt32();
            textures.Eof = br.ReadUInt32();
            textures.Tims = new TIM2[textures.cTims];
            for(int i = 0; i<textures.cTims; i++)
            {
                ms.Seek(v + textures.pTims[i], SeekOrigin.Begin);
                TIM2 tim = new TIM2(ms.ToArray(), (uint)ms.Position);
                textures.Tims[i] = tim;
            }
        }
        public Debug_battleDat(int monsterId)
        {
            id = monsterId;
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string path = aw.GetListOfFiles().First(x => x.ToLower().Contains($"c0m{id.ToString("D03")}")); //c0m000.dat
            byte[] buffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, path);
#if DEBUG
            MakiExtended.DumpBuffer(buffer, "/media/griever/Data/test.dat");
#endif
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                datFile = new DatFile { cSections = br.ReadUInt32()};
                datFile.pSections = new uint[datFile.cSections];
                for (int i = 0; i < datFile.cSections; i++)
                    datFile.pSections[i] = br.ReadUInt32();
                datFile.eof = br.ReadUInt32();

                //ReadSection1(datFile.pSections[0]);
                ReadSection2(datFile.pSections[1],ms,br);
                //ReadSection3(datFile.pSections[2]);
                //ReadSection4(datFile.pSections[3]);
                //ReadSection5(datFile.pSections[4]);
                //ReadSection6(datFile.pSections[5]);
                ReadSection7(datFile.pSections[6],ms,br);
                //ReadSection8(datFile.pSections[7]);
                //ReadSection9(datFile.pSections[8]);
                //ReadSection10(datFile.pSections[9]);
                ReadSection11(datFile.pSections[10],ms,br);
            }
        }


        private void ReadSection7(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
        }



        public int GetId => id;
    }
}
