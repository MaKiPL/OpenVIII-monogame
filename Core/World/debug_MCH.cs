using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    public class Debug_MCH
    {
        const float MODEL_SCALE = 10f;
        private const float TEX_SIZEW = 64.0f;
        private const float TEX_SIZEH = 128.0f;
        private uint pBase;
        private MemoryStream ms;
        private BinaryReader br;


        [StructLayout(LayoutKind.Sequential, Size =64, Pack =1)]
        private struct Header
        {
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
        }

        /// <summary>
        /// Main anim struct. Model can contain AnimationEntry[] animations, which hold AnimFrame[] frames
        /// </summary>
        private struct Animation
        {
            public uint cAnimations;
            public AnimationEntry[] animations;
        }

        /// <summary>
        /// Animation struct- it holds all available animation frame keypoints for selected animation
        /// </summary>
        private struct AnimationEntry
        {
            public uint cAnimFrames;
            public AnimFrame[] animationFrames;
        }
        private struct AnimFrame
        {
            public Vector3 bone0pos;
            public Vector3[] vecRot;
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


        [StructLayout(LayoutKind.Sequential, Size = 2, Pack = 1)]
        private struct TextureMap
        {
            public byte u;
            public byte v;
        }

        private Header header;
        private AnimFrame[] animationKeypoints;
        private Animation animation;
        private Bone[] bones;
        private Face[] faces;
        private Vector4[] vertices;

        public Debug_MCH(MemoryStream ms, BinaryReader br)
        {
            this.ms = ms;
            this.br = br;
            pBase = (uint)ms.Position;
            header = Extended.ByteArrayToStructure<Header>(br.ReadBytes(64));

            ReadSkeleton();
            ReadGeometry();
            ReadAnimation();
        }

        private void ReadSkeleton()
        {
            ms.Seek(pBase + header.pBones, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler

            bones = new Bone[header.cSkeletonBones];
            for (int i = 0; i < header.cSkeletonBones; i++)
                bones[i] = Extended.ByteArrayToStructure<Bone>(br.ReadBytes(64));

            return;
        }
        private void ReadGeometry()
        {
            ms.Seek(pBase + header.pVertices, SeekOrigin.Begin);
            if (ms.Position > ms.Length || header.pVertices+ms.Position > ms.Length) //pvert error handler
                return; //error handler
            vertices = new Vector4[header.cVertices];
            for(int i = 0; i<vertices.Length; i++)
                vertices[i] = new Vector4(br.ReadInt16(), br.ReadInt16(), br.ReadInt16(), br.ReadInt16());

            ms.Seek(pBase + header.pFaces, SeekOrigin.Begin);
            List<Face> face = new List<Face>();
            for(int i = 0; i<header.cFaces; i++)
                face.Add(Extended.ByteArrayToStructure<Face>(br.ReadBytes(64)));
            faces = face.ToArray();
            return;
        }

        /// <summary>
        /// Method to parse available binary data to "animation" structure
        /// </summary>
        private void ReadAnimation()
        {
            ms.Seek(pBase + header.pAnimation, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler
            ushort animationCount = br.ReadUInt16();
            int innerIndex = 0;
            animation = new Animation() { cAnimations = animationCount, animations= new AnimationEntry[animationCount] };
            while (animationCount != 0)
            {
                ushort animationFramesCount = br.ReadUInt16();
                ushort cBones = br.ReadUInt16();
                animation.animations[innerIndex] = new AnimationEntry() { cAnimFrames = animationFramesCount, animationFrames = new AnimFrame[animationFramesCount] };
                List<AnimFrame> animKeypoints = new List<AnimFrame>();
                while (animationFramesCount != 0)
                {
                    AnimFrame keyPoint = new AnimFrame() { bone0pos= new Vector3(br.ReadInt16(),br.ReadInt16(), br.ReadInt16())};
                    Vector3[] vetRot = new Vector3[cBones];
                    for (int i = 0; i < cBones; i++)
                        vetRot[i] = new Vector3() { X = br.ReadInt16()/4096.0f * 360f, Y = br.ReadInt16()/4096.0f * 360f, Z = br.ReadInt16()/4096.0f * 360f };
                    animationFramesCount--;
                    keyPoint.vecRot = vetRot;
                    animKeypoints.Add(keyPoint);
                }
                animationCount--;
                animation.animations[innerIndex].animationFrames = animationKeypoints.ToArray();
                innerIndex++;
            }
            return;
        }

        /// <summary>
        /// [WIP] - this method should return vertices based on animation/skeleton, not 'as-is'
        /// </summary>
        /// <param name="position">abs X Y Z position to draw model</param>
        /// <returns>Tuple{item1= VertexPositionColorTexture; item2= clutIndex</returns>
        public Tuple<VertexPositionColorTexture[], byte[]> GetVertexPositions(Vector3 position)
        {
            List<VertexPositionColorTexture> facesVertices = new List<VertexPositionColorTexture>();
            List<byte> texIndexes = new List<byte>();
            for(int i = 0; i<faces.Length; i++)
            {
                if (!faces[i].BIsQuad) //triangle
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Vector3 face = new Vector3(vertices[faces[i].verticesA[k]].X / MODEL_SCALE + position.X,
                        vertices[faces[i].verticesA[k]].Z / MODEL_SCALE * -1f + position.Y,
                        vertices[faces[i].verticesA[k]].Y / MODEL_SCALE + position.Z);
                        Color clr = new Color(faces[i].vertColor[0], faces[i].vertColor[1], faces[i].vertColor[2], faces[i].vertColor[3]);
                        Vector2 texData = new Vector2(faces[i].TextureMap[k].u/ TEX_SIZEW, faces[i].TextureMap[k].v/ TEX_SIZEH);
                        facesVertices.Add( new VertexPositionColorTexture(face, clr, texData));
                        texIndexes.Add((byte)faces[i].texIndex);
                        if (faces[i].texIndex > byte.MaxValue)
                            throw new Exception("Reverse engineering: test texture index? above 255, but datatype is word");
                    }
                    
                }
                else //retriangulation
                {
                    Vector3 A = new Vector3(vertices[faces[i].verticesA[0]].X / MODEL_SCALE + position.X,
                    vertices[faces[i].verticesA[0]].Z / MODEL_SCALE + position.Y,
                    vertices[faces[i].verticesA[0]].Y / MODEL_SCALE + position.Z);
                    Vector3 B = new Vector3(vertices[faces[i].verticesA[1]].X / MODEL_SCALE + position.X,
                    vertices[faces[i].verticesA[1]].Z / MODEL_SCALE + position.Y,
                    vertices[faces[i].verticesA[1]].Y / MODEL_SCALE + position.Z);
                    Vector3 C = new Vector3(vertices[faces[i].verticesA[2]].X / MODEL_SCALE + position.X,
                    vertices[faces[i].verticesA[2]].Z / MODEL_SCALE + position.Y,
                    vertices[faces[i].verticesA[2]].Y / MODEL_SCALE + position.Z);
                    Vector3 D = new Vector3(vertices[faces[i].verticesA[3]].X / MODEL_SCALE + position.X,
                    vertices[faces[i].verticesA[3]].Z / MODEL_SCALE + position.Y,
                    vertices[faces[i].verticesA[3]].Y / MODEL_SCALE + position.Z);

                    Vector2 t1 = new Vector2(faces[i].TextureMap[0].u / TEX_SIZEW, faces[i].TextureMap[0].v / TEX_SIZEH);
                    Vector2 t2 = new Vector2(faces[i].TextureMap[1].u / TEX_SIZEW, faces[i].TextureMap[1].v / TEX_SIZEH);
                    Vector2 t3 = new Vector2(faces[i].TextureMap[2].u / TEX_SIZEW, faces[i].TextureMap[2].v / TEX_SIZEH);
                    Vector2 t4 = new Vector2(faces[i].TextureMap[3].u / TEX_SIZEW, faces[i].TextureMap[3].v / TEX_SIZEH);


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
