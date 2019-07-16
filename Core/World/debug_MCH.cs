using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Core;
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
            private Vector3 Bone0pos;
            public Vector3[] vecRot;
            public Matrix[] matrixRot;

            public Vector3 bone0pos { get => Bone0pos*.01f; set => Bone0pos = value; }
        }

        private struct Skeleton
        {
            public Bone[] bones;
            public SkinData[] skins;
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

            public float GetSize() => size / MODEL_SCALE;
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

        private struct SkinData
        {
            public short vertIndex;
            public short cVerts;
            public short boneId;
            public short unk;
        }

        /// <summary>
        /// This is final combination of skin+vertices data. You query the class as the face is querying ABCD, but it has boneId paired to it
        /// </summary>
        private struct GroupedVertices
        {
            public byte boneId;
            public Vector3 vertex;
        }

        private Header header;
        private Animation animation;
        private Skeleton skeleton;
        private Face[] faces;
        private Vector4[] vertices;
        private GroupedVertices[] gVertices;

        public Debug_MCH(MemoryStream ms, BinaryReader br)
        {
            this.ms = ms;
            this.br = br;
            pBase = (uint)ms.Position;
            header = Extended.ByteArrayToStructure<Header>(br.ReadBytes(64));
            if (header.Unk != 0)
            {
                ms.Seek(0, SeekOrigin.End); //rewind to end
                return;
            }

            ReadGeometry();
            ReadSkeleton();
            ReadAnimation();
            PairSkinWithVertex();
        }

        public bool bValid() => header.Unk == 0;

        public uint GetAnimationCount() => animation.cAnimations;

        public uint GetAnimationFramesCount(int animId) => animation.animations[animId].cAnimFrames;

        /// <summary>
        /// as index data for skinning boneId is not in the same place as vertices, therefore we create sorted buffer with Vertex and their boneId
        /// </summary>
        private void PairSkinWithVertex()
        {
            gVertices = new GroupedVertices[header.cVertices];
            int innerIndex = 0;
            for(int i = 0; i<skeleton.skins.Length; i++)
            {
                for(int n = skeleton.skins[i].vertIndex; n<skeleton.skins[i].vertIndex + skeleton.skins[i].cVerts; n++)
                {
                    gVertices[innerIndex] = new GroupedVertices()
                    {
                        boneId = (byte)(skeleton.skins[i].boneId - 1),
                        vertex = new Vector3(-vertices[innerIndex].X, -vertices[innerIndex].Z, -vertices[innerIndex].Y) //debug, replace with vertices[innerindex]
                    };
                    gVertices[innerIndex].vertex = Vector3.Transform(gVertices[innerIndex].vertex, Matrix.CreateFromYawPitchRoll(2920, 0, 0));
                    innerIndex++;
                }
            }
        }

        private void ReadSkeleton()
        {
            ms.Seek(pBase + header.pBones, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler
            skeleton = new Skeleton();
            skeleton.bones = new Bone[header.cSkeletonBones];
            for (int i = 0; i < header.cSkeletonBones; i++)
                skeleton.bones[i] = Extended.ByteArrayToStructure<Bone>(br.ReadBytes(64));
            ReadSkinning();
            return;
        }

        private void ReadSkinning()
        {
            ms.Seek(pBase + header.pSkinObjects, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler

            skeleton.skins = new SkinData[header.cSkinObjects];
            for (int i = 0; i < header.cSkinObjects; i++)
                skeleton.skins[i] = Extended.ByteArrayToStructure<SkinData>(br.ReadBytes(8));
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
        /// Method to parse available binary data to "animation" structure and calculates the final Matrix
        /// </summary>
        private void ReadAnimation()
        {
            ms.Seek(pBase + header.pAnimation, SeekOrigin.Begin);

            if (ms.Position > ms.Length)
                return; //error handler
            ushort animationCount = br.ReadUInt16();
            int innerIndex = 0;
            animation = new Animation() { cAnimations = animationCount, animations= new AnimationEntry[animationCount] };
            while (animationCount > 0)
            {
                ushort animationFramesCount = br.ReadUInt16();
                ushort cBones = br.ReadUInt16();
                animation.animations[innerIndex] = new AnimationEntry() { cAnimFrames = animationFramesCount, animationFrames = new AnimFrame[animationFramesCount] };
                List<AnimFrame> animKeypoints = new List<AnimFrame>();
                while (animationFramesCount > 0)
                {
                    AnimFrame keyPoint = new AnimFrame() { bone0pos= new Vector3(x:br.ReadInt16(),z:-br.ReadInt16(), y:br.ReadInt16())};
                    Vector3[] vetRot = new Vector3[cBones];
                    Matrix[] matrixRot = new Matrix[cBones];
                    for (int i = 0; i < cBones; i++)
                    {
                        short x = br.ReadInt16();
                        short y = br.ReadInt16();
                        short z = br.ReadInt16();

                        //#region REVERSE ENGINEERING WIP
                        ////TODO= MCH_BoneRoot : 00653EB0

                        //var esi3 = (byte)((x >> 10) & 3);
                        //var esi = (byte)(x >> 2);

                        //esi3 |= (byte)((byte)(y >> 8) & 0xc);

                        //var esi1 = (byte)(y >> 2);

                        //esi3 |= (byte)((z >> 6) & 0x30);
                        //var esi2 = (byte)(z >> 2);


                        ////esi3 = al
                        ////esi  = dl

                        //var edi = esi3 & 3;
                        //var ecx = esi3 & 0x0c;
                        //edi <<= 8;

                        //edi |= esi;
                        //var edx = esi1;
                        //var eax = esi3 & 0x30;
                        //ecx <<= 6;
                        //ecx |= edx;
                        //edi <<= 2;
                        //ecx <<= 0x12;
                        //ecx |= edi;

                        //var ebp0 = ecx; //x y 

                        //ecx = esi2;
                        //eax <<= 4;
                        //eax |= ecx;
                        //eax <<= 2;

                        //var ebp4 = eax;

                        //x = (short)(ebp0 & 0xFFF);
                        //y = (short)((ebp0 >> 16) & 0xFFF);
                        //z = (short)(ebp4 & 0xFFF);


                        //short xHelp1 = EngineConst.BoneRotationB924BC[x];
                        //short xHelp2 = EngineConst.BoneRotationHelperB944C0[x];

                        //short yHelp1 = EngineConst.BoneRotationB924BC[y];
                        //short yHelp2 = EngineConst.BoneRotationHelperB944C0[y];

                        //short zHelp1 = EngineConst.BoneRotationB924BC[z];
                        //short zHelp2 = EngineConst.BoneRotationHelperB944C0[z];

                        ////short xBoneHelpa = EngineConst.BoneRotationB924BC[x]; //cx, MCH_boneRot+85
                        ////ulong xBoneHelpb = (ulong)EngineConst.BoneRotationHelperB944C0[x]; //ax MCH_boneRot+8C

                        ////xBoneHelpb = Extended.WORD3(xBoneHelpb, (ulong)xBoneHelpa);

                        ////short yBoneHelpa = EngineConst.BoneRotationB924BC[y];
                        ////ulong yBoneHelpb = (ulong)EngineConst.BoneRotationHelperB944C0[y];

                        ////ulong v23 =0;
                        ////v23 = Extended.WORD2(v23, yBoneHelpb);

                        ////yBoneHelpb = Extended.WORD2(yBoneHelpb, (ulong)yBoneHelpa);

                        ////v23 = (ulong)-yBoneHelpa;

                        ////var zBoneHelpa = EngineConst.BoneRotationB924BC[z];
                        ////ulong zBoneHelpb = (ulong)EngineConst.BoneRotationHelperB944C0[z];

                        ////short v15 = (short)zBoneHelpb;

                        ////zBoneHelpb = Extended.WORD3(zBoneHelpb, (ulong)zBoneHelpa);
                        //////xBoneHelpb |= (ulong)(-xBoneHelpa << 48);

                        ////zBoneHelpb |= (ulong)(-zBoneHelpa << 48);



                        ////sub_56BEE0+11 -> zHelp1
                        ////sub_56BEE0+20 -> zHelp2

                        //var v1 = xHelp1;
                        //var v2 = xHelp2;
                        //var v3 = yHelp1;
                        //var v4 = yHelp2;
                        //var v5 = zHelp1;
                        //var v6 = zHelp2;

                        //var v7 = (-zHelp1);
                        //var v8 = (-xHelp1);


                        //var baseX = Memory.id.GetControl(InteractiveDebugger.ctrls.baseX)[0];
                        //var baseY = Memory.id.GetControl(InteractiveDebugger.ctrls.baseY)[0];
                        //var baseZ = Memory.id.GetControl(InteractiveDebugger.ctrls.baseZ)[0];

                        //var minusers = Memory.id.GetControl(InteractiveDebugger.ctrls.minuser);
                        //foreach(var ni in minusers)
                        //{
                        //    switch(ni)
                        //    {
                        //        case 0:
                        //            x = (short)-x;
                        //            break;
                        //        case 1:
                        //            y = (short)-y;
                        //            break;
                        //        case 2:
                        //            z = (short)-z;
                        //            break;

                        //        case 3:
                        //            v1 = (short)-v1;
                        //            break;
                        //        case 4:
                        //            v2 = (short)-v2;
                        //            break;

                        //        case 5:
                        //            v3 = (short)-v3;
                        //            break;
                        //        case 6:
                        //            v4 = (short)-v4;
                        //            break;

                        //        case 7:
                        //            v5 = (short)-v5;
                        //            break;
                        //        case 8:
                        //            v6 = (short)-v6;
                        //            break;
                        //    }

                        //}


                        //baseX = baseX == 0 ? x : baseX == 1 ? y : baseX == 2 ? z : baseX == 3 ? v1 : baseX == 4 ? v2: baseX==5?v3:baseX==6?v4:baseX==7?v5:baseX==8?v6:x;
                        //baseY = baseY == 0 ? x : baseY == 1 ? y : baseY == 2 ? z : baseY == 3 ? v1 : baseY == 4 ? v2: baseY==5?v3:baseY==6?v4:baseY==7?v5:baseY==8?v6:x;
                        //baseZ = baseZ == 0 ? x : baseZ == 1 ? y : baseZ == 2 ? z : baseZ == 3 ? v1 : baseZ == 4 ? v2: baseZ==5?v3:baseZ==6?v4:baseZ==7?v5:baseZ==8?v6:x;

                        //#endregion

                        Vector3 shortVector = new Vector3()
                        {
                            X = -y,
                            Y = -x,
                            Z = -z
                        };
                        vetRot[i] = Extended.S16VectorToFloat(shortVector) * 360f;
                    }
                    animationFramesCount--;
                    keyPoint.vecRot = vetRot;
                    keyPoint.matrixRot = matrixRot;
                    animKeypoints.Add(keyPoint);
                }
                animationCount--;
                animation.animations[innerIndex].animationFrames = animKeypoints.ToArray();
                innerIndex++;
            }
            CalculateBoneMatrix();
            return;
        }

        private void CalculateBoneMatrix()
        {
            for (int animId = 0; animId < animation.animations.Length; animId++)
                for (int frameId = 0; frameId < animation.animations[animId].cAnimFrames; frameId++)
                    for (int boneId = 0; boneId < skeleton.bones.Length; boneId++)
                    {
                    var boneRotation = animation.animations[animId].animationFrames[frameId].vecRot[boneId];
                    Matrix xRot = Extended.GetRotationMatrixX(-boneRotation.X);
                    Matrix yRot = Extended.GetRotationMatrixY(-boneRotation.Y);
                    Matrix zRot = Extended.GetRotationMatrixZ(-boneRotation.Z);
                    var MatrixZ = Extended.MatrixMultiply_transpose(yRot, xRot);
                    MatrixZ = Extended.MatrixMultiply_transpose(zRot, MatrixZ);
                    if (skeleton.bones[boneId].parentBone == 0) //if parentId is 0 then the current bone is core aka bone0
                    {
                            MatrixZ.M41 = animation.animations[animId].animationFrames[frameId].bone0pos.X;
                            MatrixZ.M42 = animation.animations[animId].animationFrames[frameId].bone0pos.Y; //up/down
                            MatrixZ.M43 = animation.animations[animId].animationFrames[frameId].bone0pos.Z;
                            MatrixZ.M44 = 1;

                        }
                    else
                    {
                        Matrix parentBone = animation.animations[animId].animationFrames[frameId].matrixRot[skeleton.bones[boneId].parentBone-1]; //gets the parent bone
                        MatrixZ.M43 = skeleton.bones[skeleton.bones[boneId].parentBone-1].GetSize();
                        Matrix rMatrix = Matrix.Multiply(parentBone, MatrixZ);
                        rMatrix.M41 = parentBone.M11 * MatrixZ.M41 + parentBone.M12 * MatrixZ.M42 + parentBone.M13 * MatrixZ.M43 + parentBone.M41;
                        rMatrix.M42 = parentBone.M21 * MatrixZ.M41 + parentBone.M22 * MatrixZ.M42 + parentBone.M23 * MatrixZ.M43 + parentBone.M42;
                        rMatrix.M43 = parentBone.M31 * MatrixZ.M41 + parentBone.M32 * MatrixZ.M42 + parentBone.M33 * MatrixZ.M43 + parentBone.M43;
                        rMatrix.M44 = 1;
                        MatrixZ = rMatrix;
                    }
                    animation.animations[animId].animationFrames[frameId].matrixRot[boneId] = MatrixZ;
            }
        }
        public int xa = 0;
        private static float xb = 0f;
        private static float cx = 0f;
        /// <summary>
        /// [WIP] - this method should return vertices based on animation/skeleton, not 'as-is'
        /// </summary>
        /// <param name="position">abs X Y Z position to draw model</param>
        /// <param name="animationId">absolute index of animation 0-based</param>
        /// <param name="animationFrame">index of animation frame- 0-based, length vary</param>
        /// <returns>Tuple{item1= VertexPositionColorTexture; item2= clutIndex</returns>
        public Tuple<VertexPositionColorTexture[], byte[]> GetVertexPositions(Vector3 position, int animationId, int animationFrame)
        {
            List<VertexPositionColorTexture> facesVertices = new List<VertexPositionColorTexture>();
            List<byte> texIndexes = new List<byte>();
            for(int i = 0; i<faces.Length; i++)
            {
                //We should have pre-calculated Matrices for all bones, frames, animations. Therefore we need to calculate final Vertex position
                var vertsCollection = faces[i].verticesA;
                if (!faces[i].BIsQuad) //triangle
                {
                    //let's first get the vertices we need from face. Those are indexes. We need to get their associated boneId to perform
                    //operations on them. Let's loop by face

                    for (int k = 0; k < 3; k++)
                    {
                        var face = CalculateFinalVertex(gVertices[vertsCollection[k]], animationId, animationFrame);
                        face = Vector3.Transform(face, Matrix.CreateTranslation(position));

                        Color clr = new Color(faces[i].vertColor[0], faces[i].vertColor[1], faces[i].vertColor[2], faces[i].vertColor[3]);
                        Vector2 texData = new Vector2(faces[i].TextureMap[k].u/ TEX_SIZEW, faces[i].TextureMap[k].v/ TEX_SIZEH);
                        facesVertices.Add( new VertexPositionColorTexture(face, clr, texData));
                        texIndexes.Add((byte)faces[i].texIndex);
                    }

                }
                else //retriangulation
                {
                    var faceA = CalculateFinalVertex(gVertices[vertsCollection[0]], animationId, animationFrame);
                    faceA = Vector3.Transform(faceA, Matrix.CreateTranslation(position));

                    var faceB = CalculateFinalVertex(gVertices[vertsCollection[1]], animationId, animationFrame);
                    faceB = Vector3.Transform(faceB, Matrix.CreateTranslation(position));

                    var faceC = CalculateFinalVertex(gVertices[vertsCollection[2]], animationId, animationFrame);
                    faceC = Vector3.Transform(faceC, Matrix.CreateTranslation(position));

                    var faceD = CalculateFinalVertex(gVertices[vertsCollection[3]], animationId, animationFrame);
                    faceD = Vector3.Transform(faceD, Matrix.CreateTranslation(position));

                    Vector2 t1 = new Vector2(faces[i].TextureMap[0].u / TEX_SIZEW, faces[i].TextureMap[0].v / TEX_SIZEH);
                    Vector2 t2 = new Vector2(faces[i].TextureMap[1].u / TEX_SIZEW, faces[i].TextureMap[1].v / TEX_SIZEH);
                    Vector2 t3 = new Vector2(faces[i].TextureMap[2].u / TEX_SIZEW, faces[i].TextureMap[2].v / TEX_SIZEH);
                    Vector2 t4 = new Vector2(faces[i].TextureMap[3].u / TEX_SIZEW, faces[i].TextureMap[3].v / TEX_SIZEH);

                    Color clr = new Color(faces[i].vertColor[0], faces[i].vertColor[1], faces[i].vertColor[2], faces[i].vertColor[3]);

                    facesVertices.Add(new VertexPositionColorTexture(faceA, clr, t1));
                    facesVertices.Add(new VertexPositionColorTexture(faceB, clr, t2));
                    facesVertices.Add(new VertexPositionColorTexture(faceD, clr, t4));

                    facesVertices.Add(new VertexPositionColorTexture(faceA, clr, t1));
                    facesVertices.Add(new VertexPositionColorTexture(faceC, clr, t3));
                    facesVertices.Add(new VertexPositionColorTexture(faceD, clr, t4));

                    texIndexes.Add((byte)faces[i].texIndex); texIndexes.Add((byte)faces[i].texIndex);
                }
            }

            return new Tuple<VertexPositionColorTexture[], byte[]>(facesVertices.ToArray(), texIndexes.ToArray());
        }

        
    private Vector3 CalculateFinalVertex(GroupedVertices groupedVertex, int animationId, int animationFrame)
        {

            var vertex = groupedVertex.vertex / MODEL_SCALE;
            Matrix faceMatrix = animation.animations[animationId].animationFrames[animationFrame].matrixRot[groupedVertex.boneId];


            Vector3 face = new Vector3(
                faceMatrix.M11 * vertex.X + faceMatrix.M41 + faceMatrix.M12 * vertex.Z + faceMatrix.M13 * -vertex.Y,
                faceMatrix.M21 * vertex.X + faceMatrix.M42 + faceMatrix.M22 * vertex.Z + faceMatrix.M23 * -vertex.Y,
                faceMatrix.M31 * vertex.X + faceMatrix.M43 + faceMatrix.M32 * vertex.Z + faceMatrix.M33 * -vertex.Y
                );

            return face;
        }
    }
}
