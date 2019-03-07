using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

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
            public Vertex[] vertices;
            public ushort cTriangles;
            public ushort cQuads;
            public ulong padding;
            public Triangle[] triangles;
            public Quad[] quads;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
        public struct Vertex
        {
            private float x;
            private float y;
            private float z;
            private float w;

            public Vector3 GetVector => new Vector3(X, Y, Z);

            public float X { get => x; set => x = value / 2000.0f; }
            public float Y { get => y; set => y = value / 2000.0f; }
            public float Z { get => z; set => z = value / 2000.0f; }
            public float W { get => w; set => w = value / 2000.0f; }
        }

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =16)]
        public struct Triangle
        {
            private short A;
            private short B;
            private short C;
            public UV vta;
            public UV vtb;
            public ushort texUnk;
            public UV vtc;
            public ushort u;

            public short A1 { get => A; set => A = (short)(value & 0xFFF); }
            public short B1 { get => B; set => B = (short)(value & 0xFFF); }
            public short C1 { get => C; set => C = (short)(value & 0xFFF); }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 20)]
        public struct Quad
        {
            private short A;
            private short B;
            private short C;
            private short D;
            public UV vta;
            public ushort texUnk;
            public UV vtb;
            public ushort u;
            public UV vtc;
            public UV vtd;

            public short A1 { get => A; set => A = (short)(value & 0xFFF); }
            public short B1 { get => B; set => B = (short)(value & 0xFFF); }
            public short C1 { get => C; set => C = (short)(value & 0xFFF); }
            public short D1 { get => D; set => D = (short)(value & 0xFFF); }
        }

        [StructLayout(LayoutKind.Sequential, Pack =1,Size =2)]
        public struct UV
        {
            private float U;
            private float V;

            public float U1 { get => U; set => U = value / 256.0f; }
            public float V1 { get => V; set => V = value / 256.0f; }
        }

        public Geometry geometry;

        private void ReadSection2(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            geometry = new Geometry { cObjects = br.ReadUInt32() };
            for (int i = 0; i < geometry.cObjects; i++)
                geometry.pObjects[i] = br.ReadUInt32();

        }
        #endregion

        #region section 3
        struct AnimHeader
        {
            public uint cAnimations;
            public uint[] pAnimations;
        }

        struct Animation
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
        struct Textures
        {
            public uint cTims;
            public uint[] pTims;
            public uint Eof;
            public TIM2[] Tims;
        }
        #endregion

        public Debug_battleDat(int monsterId)
        {
            id = monsterId;
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string path = aw.GetListOfFiles().First(x => x.ToLower().Contains($"c0m{id.ToString("D03")}")); //c0m000.dat
            byte[] buffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, path);
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                datFile = new DatFile { cSections = br.ReadUInt32(), pSections=new uint[datFile.cSections] };
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

        private void ReadSection11(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
        }

        private void ReadSection7(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
        }



        public int GetId => id;
    }
}
