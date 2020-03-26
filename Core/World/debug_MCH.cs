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
        static float MODEL_SCALE = 20f;
        private const float TEX_SIZEW = 256f;
        private const float TEX_SIZEH = 256f;
        private Vector2[] textureSizes;
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

        public enum mchMode
        {
            World,
            FieldMain,
            FieldNPC
        }

        mchMode currentMchMode;
        public Debug_MCH(MemoryStream ms, BinaryReader br,mchMode mchMode = mchMode.World,float modelScale = 20f)
        {
            this.ms = ms;
            this.br = br;
            this.currentMchMode = mchMode;
            MODEL_SCALE = modelScale;
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

        /// <summary>
        /// to be used for VRAM atlases- it provides the texture sizes relative to texIndexes in GetVertexPositions. If not called then default 256x256 range is used
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="textureIndexes"></param>
        public void AssignTextureSizes(Texture2D[] textures, int[] textureIndexes)
        {
            textureSizes = new Vector2[textureIndexes.Length];
            for (var i = 0; i < textureIndexes.Length; i++)
                textureSizes[i] = new Vector2(textures[textureIndexes[i]].Width, textures[textureIndexes[i]].Height);
        }
        /// <summary>
        /// to be used for VRAM atlases- it provides the texture sizes relative to texIndexes in GetVertexPositions. If not called then default 256x256 range is used
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="textureIndexes"></param>
        public void AssignTextureSizes(TextureHandler[] textures, int[] textureIndexes)
        {
            textureSizes = new Vector2[textureIndexes.Length];
            for (var i = 0; i < textureIndexes.Length; i++)
                textureSizes[i] = new Vector2(textures[textureIndexes[i]].ClassicWidth, textures[textureIndexes[i]].ClassicHeight);
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
            var innerIndex = 0;
            for(var i = 0; i<skeleton.skins.Length; i++)
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
            for (var i = 0; i < header.cSkeletonBones; i++)
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
            for (var i = 0; i < header.cSkinObjects; i++)
                skeleton.skins[i] = Extended.ByteArrayToStructure<SkinData>(br.ReadBytes(8));
            return;
        }

        private void ReadGeometry()
        {
            ms.Seek(pBase + header.pVertices, SeekOrigin.Begin);
            if (ms.Position > ms.Length || header.pVertices+ms.Position > ms.Length) //pvert error handler
                return; //error handler
            vertices = new Vector4[header.cVertices];
            for (var i = 0; i < vertices.Length; i++)
              vertices[i] = new Vector4(br.ReadInt16(), -br.ReadInt16(), br.ReadInt16(), br.ReadInt16()); //change second to -Y because of warped geom

            ms.Seek(pBase + header.pFaces, SeekOrigin.Begin);
            var face = new List<Face>();
            for(var i = 0; i<header.cFaces; i++)
                face.Add(Extended.ByteArrayToStructure<Face>(br.ReadBytes(64)));
            faces = face.ToArray();
            return;
        }

        /// <summary>
        /// Method to parse available binary data to "animation" structure and calculates the final Matrix
        /// <paramref name="bIndependentStream">if true- then ms and br are custom/modified externally</paramref>
        /// </summary>
        private void ReadAnimation(bool bIndependentStream = false)
        {
            if(!bIndependentStream) //if normal parsing, then jump to animation pointer
                ms.Seek(pBase + header.pAnimation, SeekOrigin.Begin);
            //if not, then do nothing- ms will point to animation buffer

            if (ms.Position > ms.Length)
                return; //error handler
            var animationCount = br.ReadUInt16();
            var innerIndex = 0;
            animation = new Animation() { cAnimations = animationCount, animations= new AnimationEntry[animationCount] };
            while (animationCount > 0)
            {
                var animationFramesCount = br.ReadUInt16();
                var cBones = br.ReadUInt16();
                if (animationFramesCount * cBones >= ms.Length || cBones == 0 || cBones>100)
                {
                    Console.WriteLine($"Debug_MCH: Error at ReadAnimation()- animFrameCount was {animationFramesCount} and cBones were {cBones}, but that's more than file size!");
                    break;
                }
                animation.animations[innerIndex] = new AnimationEntry() { cAnimFrames = animationFramesCount, animationFrames = new AnimFrame[animationFramesCount] };
                var animKeypoints = new List<AnimFrame>();
                while (animationFramesCount > 0)
                {
                    var keyPoint = new AnimFrame() { bone0pos= new Vector3(x:br.ReadInt16(),z:-br.ReadInt16(), y:br.ReadInt16())};
                    var vetRot = new Vector3[cBones];
                    var matrixRot = new Matrix[cBones];
                    for (var i = 0; i < cBones; i++)
                        {
                        short x, y, z;
                        if (currentMchMode == mchMode.World)
                        {
                            x = br.ReadInt16();
                            y = br.ReadInt16();
                            z = br.ReadInt16();
                        }
                        else //Field NPC dataset - s16 4bytes simplified
                        {
                            var rot1 = br.ReadByte();
                            var rot2 = br.ReadByte();
                            var rot3 = br.ReadByte();
                            var rot4 = br.ReadByte();
                            x = (short)(rot1 << 2 | (rot4 >> 2 * 0 & 3) << 10);
                            y = (short)(rot2 << 2 | (rot4 >> 2 * 1 & 3) << 10);
                            z = (short)(rot3 << 2 | (rot4 >> 2 * 2 & 3) << 10);
                        }
                        var shortVector = new Vector3()
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
            for (var animId = 0; animId < animation.animations.Length; animId++)
                for (var frameId = 0; frameId < animation.animations[animId].cAnimFrames; frameId++)
                    for (var boneId = 0; boneId < skeleton.bones.Length; boneId++)
                    {
                    var boneRotation = animation.animations[animId].animationFrames[frameId].vecRot[boneId];
                    var xRot = Extended.GetRotationMatrixX(-boneRotation.X);
                    var yRot = Extended.GetRotationMatrixY(-boneRotation.Y);
                    var zRot = Extended.GetRotationMatrixZ(-boneRotation.Z);
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
                        var parentBone = animation.animations[animId].animationFrames[frameId].matrixRot[skeleton.bones[boneId].parentBone-1]; //gets the parent bone
                        MatrixZ.M43 = skeleton.bones[skeleton.bones[boneId].parentBone-1].GetSize();
                        var rMatrix = Matrix.Multiply(parentBone, MatrixZ);
                        rMatrix.M41 = parentBone.M11 * MatrixZ.M41 + parentBone.M12 * MatrixZ.M42 + parentBone.M13 * MatrixZ.M43 + parentBone.M41;
                        rMatrix.M42 = parentBone.M21 * MatrixZ.M41 + parentBone.M22 * MatrixZ.M42 + parentBone.M23 * MatrixZ.M43 + parentBone.M42;
                        rMatrix.M43 = parentBone.M31 * MatrixZ.M41 + parentBone.M32 * MatrixZ.M42 + parentBone.M33 * MatrixZ.M43 + parentBone.M43;
                        rMatrix.M44 = 1;
                        MatrixZ = rMatrix;
                    }
                    animation.animations[animId].animationFrames[frameId].matrixRot[boneId] = MatrixZ;
            }
        }
        //public int xa = 0;
        //private static float xb = 0f;
        //private static float cx = 0f;
        /// <summary>
        /// [WIP] - this method should return vertices based on animation/skeleton, not 'as-is'
        /// </summary>
        /// <param name="position">abs X Y Z position to draw model</param>
        /// <param name="animationId">absolute index of animation 0-based</param>
        /// <param name="animationFrame">index of animation frame- 0-based, length vary</param>
        /// <returns>Tuple{item1= VertexPositionColorTexture; item2= clutIndex</returns>
        public Tuple<VertexPositionColorTexture[], byte[]> GetVertexPositions(Vector3 position, Quaternion rotation, int animationId, int animationFrame)
        {
            var facesVertices = new List<VertexPositionColorTexture>();
            var texIndexes = new List<byte>();
            for(var i = 0; i<faces.Length; i++)
            {

                //We should have pre-calculated Matrices for all bones, frames, animations. Therefore we need to calculate final Vertex position
                var vertsCollection = faces[i].verticesA;
                if (!faces[i].BIsQuad) //triangle
                {
                    //let's first get the vertices we need from face. Those are indexes. We need to get their associated boneId to perform
                    //operations on them. Let's loop by face

                    for (var k = 0; k < 3; k++)
                    {
                        var face = CalculateFinalVertex(gVertices[vertsCollection[k]], animationId, animationFrame);
                        face = Vector3.Transform(face, Matrix.CreateFromQuaternion(rotation));
                        face = Vector3.Transform(face, Matrix.CreateTranslation(position));

                        var clr = new Color(faces[i].vertColor[0], faces[i].vertColor[1], faces[i].vertColor[2], faces[i].vertColor[3]);
                        Vector2 texData;
                        if(textureSizes != null)
                            texData = new Vector2(faces[i].TextureMap[k].u/ textureSizes[faces[i].texIndex].X, faces[i].TextureMap[k].v/ textureSizes[faces[i].texIndex].Y);
                        else
                            texData = new Vector2(faces[i].TextureMap[k].u / TEX_SIZEW, faces[i].TextureMap[k].v / TEX_SIZEH);
                        facesVertices.Add( new VertexPositionColorTexture(face, clr, texData));
                        texIndexes.Add((byte)faces[i].texIndex);
                    }

                }
                else //retriangulation
                {
                    var faceA = CalculateFinalVertex(gVertices[vertsCollection[0]], animationId, animationFrame);
                    faceA = Vector3.Transform(faceA, Matrix.CreateFromQuaternion(rotation));
                    faceA = Vector3.Transform(faceA, Matrix.CreateTranslation(position));

                    var faceB = CalculateFinalVertex(gVertices[vertsCollection[1]], animationId, animationFrame);
                    faceB = Vector3.Transform(faceB, Matrix.CreateFromQuaternion(rotation));
                    faceB = Vector3.Transform(faceB, Matrix.CreateTranslation(position));

                    var faceC = CalculateFinalVertex(gVertices[vertsCollection[2]], animationId, animationFrame);
                    faceC = Vector3.Transform(faceC, Matrix.CreateFromQuaternion(rotation));
                    faceC = Vector3.Transform(faceC, Matrix.CreateTranslation(position));

                    var faceD = CalculateFinalVertex(gVertices[vertsCollection[3]], animationId, animationFrame);
                    faceD = Vector3.Transform(faceD, Matrix.CreateFromQuaternion(rotation));
                    faceD = Vector3.Transform(faceD, Matrix.CreateTranslation(position));

                    var widthDividor = textureSizes == null ? TEX_SIZEW : textureSizes[faces[i].texIndex].X; //if VRAM indexes of tex sizes are not null, then use them for UV calculation
                    var heightDividor = textureSizes == null ? TEX_SIZEH : textureSizes[faces[i].texIndex].Y;
                    var t1 = new Vector2(faces[i].TextureMap[0].u / widthDividor, faces[i].TextureMap[0].v / heightDividor);
                    var t2 = new Vector2(faces[i].TextureMap[1].u / widthDividor, faces[i].TextureMap[1].v / heightDividor);
                    var t3 = new Vector2(faces[i].TextureMap[2].u / widthDividor, faces[i].TextureMap[2].v / heightDividor);
                    var t4 = new Vector2(faces[i].TextureMap[3].u / widthDividor, faces[i].TextureMap[3].v / heightDividor);

                    var clr = new Color(faces[i].vertColor[0], faces[i].vertColor[1], faces[i].vertColor[2], faces[i].vertColor[3]);

                    facesVertices.Add(new VertexPositionColorTexture(faceA, clr, t1));
                    facesVertices.Add(new VertexPositionColorTexture(faceB, clr, t2));
                    facesVertices.Add(new VertexPositionColorTexture(faceD, clr, t4));

                    facesVertices.Add(new VertexPositionColorTexture(faceA, clr, t1));
                    facesVertices.Add(new VertexPositionColorTexture(faceC, clr, t3));
                    facesVertices.Add(new VertexPositionColorTexture(faceD, clr, t4));

                    texIndexes.Add((byte)faces[i].texIndex);
                    texIndexes.Add((byte)faces[i].texIndex);
                    texIndexes.Add((byte)faces[i].texIndex);

                    texIndexes.Add((byte)faces[i].texIndex);
                    texIndexes.Add((byte)faces[i].texIndex);
                    texIndexes.Add((byte)faces[i].texIndex);
                }
            }

            //var a = (from b in faces from c in b.TextureMap select new { x = c.u, y = c.v, w = b.texIndex }).ToArray();
            //var min = a.Min(x => x.x);
            //var max = a.Max(x => x.x);

            //var mina = a.Min(x => x.y);
            //var maxb = a.Max(x => x.y);
            return new Tuple<VertexPositionColorTexture[], byte[]>(facesVertices.ToArray(), texIndexes.ToArray());
        }

        
    private Vector3 CalculateFinalVertex(GroupedVertices groupedVertex, int animationId, int animationFrame)
        {

            var vertex = groupedVertex.vertex / MODEL_SCALE;
            var faceMatrix = animation.animations[animationId].animationFrames[animationFrame].matrixRot[groupedVertex.boneId];


            var face = new Vector3(
                faceMatrix.M11 * vertex.X + faceMatrix.M41 + faceMatrix.M12 * vertex.Z + faceMatrix.M13 * -vertex.Y,
                faceMatrix.M21 * vertex.X + faceMatrix.M42 + faceMatrix.M22 * vertex.Z + faceMatrix.M23 * -vertex.Y,
                faceMatrix.M31 * vertex.X + faceMatrix.M43 + faceMatrix.M32 * vertex.Z + faceMatrix.M33 * -vertex.Y
                );

            return face;
        }


        /// <summary>
        /// This function takes animations data as input and merges it to current Mch instance.
        /// This is mandatory for main characters in fields, as their base does not contain animations
        /// </summary>
        /// <param name="animationsBuffer"></param>
        public void MergeAnimations(MemoryStream ms, BinaryReader br)
        {
            this.ms = ms;
            this.br = br;
            ReadAnimation(true);
        }
    }
}
