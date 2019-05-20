using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    public class Debug_MCH
    {
        const float MODEL_SCALE = 1f;

        private uint pBase;
        private MemoryStream ms;
        private BinaryReader br;

        public uint cSkeletonBones;
        public uint cVertices;
        public uint cTexAnimations;
        public uint cFaces;
        public uint cUnk;
        public uint cSkinObjects;
        public uint Unk;
        public ushort cTris;
        public ushort cQuads;
        public uint pBones;
        public uint pVertices;
        public uint pTexAnimations;
        public uint pFaces;
        public uint pUnk;
        public uint pSkinObjects;
        public uint pAnimation;
        public uint Unk2;

        private struct AnimationKeypoint
        {
            public short X;
            public short Y;
            public short Z;
            public Vector3[] rot;
        }

        [StructLayout(LayoutKind.Sequential, Size = 64, Pack = 1)]
        private struct Face
        {
            public int polygonType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] unk;
            public short unknown;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] unk2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public short[] verticesA;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public short[] verticesB;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] vertColor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public TextureMap[] TextureMap;
            public ushort padding;
            public ushort texIndex;
            public ulong padding2;

            public bool BIsQuad => polygonType == 0x2d010709;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x40, Pack = 1)]
        private struct Bone
        {
            public ushort parentBone;
            public ushort unk;
            public uint unk2;
            public short size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst =54)]
            public byte[] unkBuffer;
        }

        [StructLayout(LayoutKind.Sequential, Size = 2, Pack = 1)]
        private struct TextureMap
        {
            public byte u;
            public byte v;
        }

        private AnimationKeypoint[] animationKeypoints;
        private Bone[] bones;
        private Face[] faces;
        private Vector4[] vertices;

        public Debug_MCH(MemoryStream ms, BinaryReader br)
        {
            this.ms = ms;
            this.br = br;
            pBase = (uint)ms.Position;
            cSkeletonBones = br.ReadUInt32();
            cVertices = br.ReadUInt32();
            cTexAnimations = br.ReadUInt32();
            cFaces = br.ReadUInt32();
            cUnk = br.ReadUInt32();
            cSkinObjects = br.ReadUInt32();
            Unk = br.ReadUInt32();
            cTris = br.ReadUInt16();
            cQuads = br.ReadUInt16();
            pBones = br.ReadUInt32();
            pVertices = br.ReadUInt32();
            pTexAnimations = br.ReadUInt32();
            pFaces = br.ReadUInt32();
            pUnk = br.ReadUInt32();
            pSkinObjects = br.ReadUInt32();
            pAnimation = br.ReadUInt32();
            Unk2 = br.ReadUInt32();

            ReadSkeleton();
            ReadGeometry();
            ReadAnimation();
        }

        private void ReadSkeleton()
        {
            ms.Seek(pBase + pBones, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler

            bones = new Bone[cSkeletonBones];
            for (int i = 0; i < cSkeletonBones; i++)
                bones[i] = Extended.ByteArrayToStructure<Bone>(br.ReadBytes(64));

            return;
        }
        private void ReadGeometry()
        {
            ms.Seek(pBase + pVertices, SeekOrigin.Begin);
            if (ms.Position > ms.Length || pVertices+ms.Position > ms.Length) //pvert error handler
                return; //error handler
            vertices = new Vector4[cVertices];
            for(int i = 0; i<vertices.Length; i++)
                vertices[i] = new Vector4(br.ReadInt16(), br.ReadInt16(), br.ReadInt16(), br.ReadInt16());

            ms.Seek(pBase + pFaces, SeekOrigin.Begin);
            List<Face> face = new List<Face>();
            for(int i = 0; i<cFaces; i++)
                face.Add(Extended.ByteArrayToStructure<Face>(br.ReadBytes(64)));
            faces = face.ToArray();
            return;
        }
        private void ReadAnimation()
        {
            ms.Seek(pBase + pAnimation, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler
            ushort animationCount = br.ReadUInt16();
            List<AnimationKeypoint> animKeypoints = new List<AnimationKeypoint>();

            while (animationCount != 0/*(cEntries = br.ReadUInt16()) != 0*/)
            {
                ushort animationFramesCount = br.ReadUInt16();
                ushort cBones = br.ReadUInt16();
                while (animationFramesCount != 0)
                {
                    AnimationKeypoint keyPoint = new AnimationKeypoint() { X = br.ReadInt16(), Y = br.ReadInt16(), Z = br.ReadInt16() };
                    Vector3[] vetRot = new Vector3[cBones];
                    for (int i = 0; i < cBones; i++)
                        vetRot[i] = new Vector3() { X = br.ReadInt16()/4096.0f, Y = br.ReadUInt16()/4096.0f, Z = br.ReadUInt16()/4096.0f };
                    animationFramesCount--;
                    keyPoint.rot = vetRot;
                    animKeypoints.Add(keyPoint);
                }
                animationCount--;
            }
            animationKeypoints = animKeypoints.ToArray();
            return;
        }

        public Tuple<VertexPositionColorTexture[], byte[]> GetVertexPositions(int baseX =0, int baseY=0, int baseZ=0)
        {
            List<VertexPositionColorTexture> facesVertices = new List<VertexPositionColorTexture>();
            List<byte> texIndexes = new List<byte>();
            for(int i = 0; i<faces.Length; i++)
            {
                if (!faces[i].BIsQuad) //triangles please
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Vector3 face = new Vector3(vertices[faces[i].verticesA[k]].X / MODEL_SCALE + baseX,
                        vertices[faces[i].verticesA[k]].Z / MODEL_SCALE * -1f + baseY,
                        vertices[faces[i].verticesA[k]].Y / MODEL_SCALE + baseZ);
                        Color clr = new Color(faces[i].vertColor[0], faces[i].vertColor[1], faces[i].vertColor[2], faces[i].vertColor[3]);
                        Vector2 texData = new Vector2(faces[i].TextureMap[k].u/256.0f, faces[i].TextureMap[k].v/256.0f);
                        facesVertices.Add( new VertexPositionColorTexture(face, clr, texData));
                        texIndexes.Add((byte)faces[i].texIndex);
                        if (faces[i].texIndex > byte.MaxValue)
                            throw new Exception("Reverse engineering: test texture index? above 255, but datatype is word");
                    }
                    
                }
                else //we may need to actually retriangulate
                {
                    Vector3 A = new Vector3(vertices[faces[i].verticesA[0]].X / MODEL_SCALE + baseX,
                    vertices[faces[i].verticesA[0]].Z / MODEL_SCALE * 1f + baseY,
                    vertices[faces[i].verticesA[0]].Y / MODEL_SCALE + baseZ);
                    Vector3 B = new Vector3(vertices[faces[i].verticesA[1]].X / MODEL_SCALE + baseX,
                    vertices[faces[i].verticesA[1]].Z / MODEL_SCALE * 1f + baseY,
                    vertices[faces[i].verticesA[1]].Y / MODEL_SCALE + baseZ);
                    Vector3 C = new Vector3(vertices[faces[i].verticesA[2]].X / MODEL_SCALE + baseX,
                    vertices[faces[i].verticesA[2]].Z / MODEL_SCALE * 1f + baseY,
                    vertices[faces[i].verticesA[2]].Y / MODEL_SCALE + baseZ);
                    Vector3 D = new Vector3(vertices[faces[i].verticesA[3]].X / MODEL_SCALE + baseX,
                    vertices[faces[i].verticesA[3]].Z / MODEL_SCALE * 1f + baseY,
                    vertices[faces[i].verticesA[3]].Y / MODEL_SCALE + baseZ);

                    Vector2 t1 = new Vector2(faces[i].TextureMap[0].u / 256.0f, faces[i].TextureMap[0].v / 256.0f);
                    Vector2 t2 = new Vector2(faces[i].TextureMap[1].u / 256.0f, faces[i].TextureMap[1].v / 256.0f);
                    Vector2 t3 = new Vector2(faces[i].TextureMap[2].u / 256.0f, faces[i].TextureMap[2].v / 256.0f);
                    Vector2 t4 = new Vector2(faces[i].TextureMap[3].u / 256.0f, faces[i].TextureMap[3].v / 256.0f);


                    Color clr = new Color(faces[i].vertColor[0], faces[i].vertColor[1], faces[i].vertColor[2], faces[i].vertColor[3]);

                    facesVertices.Add(new VertexPositionColorTexture(A, clr, t1));
                    facesVertices.Add(new VertexPositionColorTexture(B, clr, t2));
                    facesVertices.Add(new VertexPositionColorTexture(D, clr, t4));

                    facesVertices.Add(new VertexPositionColorTexture(A, clr, t1));
                    facesVertices.Add(new VertexPositionColorTexture(C, clr, t3));
                    facesVertices.Add(new VertexPositionColorTexture(D, clr, t4));

                    if (faces[i].texIndex > byte.MaxValue)
                        throw new Exception("Reverse engineering: test texture index? above 255, but datatype is word");
                    texIndexes.Add((byte)faces[i].texIndex); texIndexes.Add((byte)faces[i].texIndex);
                }
            }

            return new Tuple<VertexPositionColorTexture[], byte[]>(facesVertices.ToArray(), texIndexes.ToArray());
        }
    }
}
