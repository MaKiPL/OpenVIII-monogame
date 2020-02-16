using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Battle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    [Flags]
    public enum Sections : ushort
    {
        None = 0,

        /// <summary>
        /// Section 1
        /// </summary>
        Skeleton = 0x1,

        /// <summary>
        /// Section 2
        /// </summary>
        /// <remarks>Requires Skeleton</remarks>
        Model_Geometry = 0x2 | Skeleton,

        /// <summary>
        /// Section 3
        /// </summary>
        /// <remarks>Requires Model_Geometry</remarks>
        Model_Animation = 0x4 | Model_Geometry,

        /// <summary>
        /// Section 4
        /// </summary>
        Section4_Unknown = 0x8,

        /// <summary>
        /// Section 5
        /// </summary>
        Animation_Sequences = 0x10,

        /// <summary>
        /// Section 6
        /// </summary>
        Section6_Unknown = 0x20,

        /// <summary>
        /// Section 7
        /// </summary>
        Information = 0x40,

        /// <summary>
        /// Section 8
        /// </summary>
        Scripts = 0x80,

        /// <summary>
        /// Section 9
        /// </summary>
        Sounds = 0x100,

        /// <summary>
        /// Section 10
        /// </summary>
        Sounds_Unknown = 0x200,

        /// <summary>
        /// Section 11
        /// </summary>
        Textures = 0x400,

        /// <summary>
        /// All Sections
        /// </summary>
        All = 0x7FF,
    }

    public static class VertexPositionTexturePointersGRP_Ext
    {
        public static bool IsNotSet(this Debug_battleDat.VertexPositionTexturePointersGRP vertexPositionTexturePointersGRP)
        {
            if ((vertexPositionTexturePointersGRP.VPT?.Length ?? 0) > 0 && (vertexPositionTexturePointersGRP.TexturePointers?.Length ?? 0) > 0)
            {
                //3 vertices per every texture pointer.
                Debug.Assert(vertexPositionTexturePointersGRP.VPT.Length / 3 == vertexPositionTexturePointersGRP.TexturePointers.Length);
                return false;
            }
            return true;
        }

        public static bool IsSet(this Debug_battleDat.VertexPositionTexturePointersGRP vertexPositionTexturePointersGRP) => !vertexPositionTexturePointersGRP.IsNotSet();
    }

    public partial class Debug_battleDat
    {
        private byte[] buffer;
        private const float BaseLineMaxYFilter = 10f;
        public const float SCALEHELPER = 2048.0f;
        private const float DEGREES = 360f;

        public struct DatFile
        {
            public uint cSections;
            public uint[] pSections;
            public uint eof;
        }

        public DatFile datFile;

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
        public struct Skeleton
        {
            public ushort cBones;
            public ushort scale;
            public ushort unk2;
            public ushort unk3;
            public ushort unk4;
            public ushort unk5;
            public ushort unk6;
            public ushort unk7;
            public Bone[] bones;

            public Vector3 GetScale => new Vector3(scale / SCALEHELPER * 12, scale / SCALEHELPER * 12, scale / SCALEHELPER * 12);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 44)]
        public struct Bone
        {
            public ushort parentId;
            public short boneSize;
            private short rotx; //360
            private short roty; //360
            private short rotz; //360
            private short unk4;
            private short unk5;
            private short unk6;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            public byte[] Unk;

            public float Size => boneSize / SCALEHELPER;
            public float RotX => rotx / 4096.0f * 360.0f;  //rotX
            public float RotY => roty / 4096.0f * 360.0f;  //rotY
            public float RotZ => rotz / 4096.0f * 360.0f;  //rotZ
            public float Unk4 => unk4 / 4096.0f;  //unk1v
            public float Unk5 => unk5 / 4096.0f;  //unk2v
            public float Unk6 => unk6 / 4096.0f;  //unk3v
        }

        /// <summary>
        /// Skeleton data section
        /// </summary>
        /// <param name="start"></param>
        private void ReadSection1(uint start)
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);
#if _WINDOWS //looks like Linux Mono doesn't like marshalling structure with LPArray to Bone[]
            skeleton = Extended.ByteArrayToStructure<Skeleton>(br.ReadBytes(16));
#else
            skeleton = new Skeleton()
            {
                cBones = br.ReadUInt16(),
                scale = br.ReadUInt16(),
                unk2 = br.ReadUInt16(),
                unk3 = br.ReadUInt16(),
                unk4 = br.ReadUInt16(),
                unk5 = br.ReadUInt16(),
                unk6 = br.ReadUInt16(),
                unk7 = br.ReadUInt16()
            };
#endif
            skeleton.bones = new Bone[skeleton.cBones];
            for (int i = 0; i < skeleton.cBones; i++)
            {
                skeleton.bones[i] = Extended.ByteArrayToStructure<Bone>(br.ReadBytes(44));
                br.BaseStream.Seek(4, SeekOrigin.Current);
            }
            //string debugBuffer = string.Empty;
            //for (int i = 0; i< skeleton.cBones; i++)
            //    debugBuffer += $"{i}|{skeleton.bones[i].parentId}|{skeleton.bones[i].boneSize}|{skeleton.bones[i].Size}\n";
            //Console.WriteLine(debugBuffer);
            return;
        }

        public Skeleton skeleton;

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
            public VerticeData[] vertexData;
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
            public Vector3[] vertices;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
        public struct Vertex
        {
            public short x { get; private set; }
            public short y { get; private set; }
            public short z { get; private set; }

            public Vertex(short x, short y, short z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public static implicit operator Vector3(Vertex v) => new Vector3(-v.x / SCALEHELPER, -v.z / SCALEHELPER, -v.y / SCALEHELPER);

            public override string ToString() => $"x={x}, y={y}, z={z}, Vector3={(Vector3)this}";
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
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
            private VertexPositionTexture[] TempVPT;

            public ushort A1 { get => (ushort)(A & 0xFFF); set => A = value; }
            public ushort B1 { get => (ushort)(B & 0xFFF); set => B = value; }
            public ushort C1 { get => (ushort)(C & 0xFFF); set => C = value; }
            public byte textureIndex => (byte)((texUnk >> 6) & 0b111);

            public static Triangle Create(BinaryReader br)
            {
                Triangle r = new Triangle()
                {
                    A = br.ReadUInt16(),
                    B = br.ReadUInt16(),
                    C = br.ReadUInt16(),
                    vta = UV.Create(br),
                    vtb = UV.Create(br),
                    texUnk = br.ReadUInt16(),
                    vtc = UV.Create(br),
                    u = br.ReadUInt16(),
                    TempVPT = new VertexPositionTexture[Count]
                };
                return r;
            }

            public ushort GetIndex(int i)
            {
                switch (i)
                {
                    case 0:
                        return C1; //for some reason C is first in triangle

                    case 1:
                        return A1;

                    case 2:
                        return B1;
                }
                throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
            }

            public UV GetUV(int i)
            {
                switch (i)
                {
                    case 0:
                        return vta;

                    case 1:
                        return vtb;

                    case 2:
                        return vtc;
                }
                throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
            }

            public byte this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return 0;

                        case 1:
                            return 1;

                        case 2:
                            return 2;
                    }
                    throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
                }
            }

            public byte[] Indices => new byte[] { this[0], this[1], this[2] };
            public static byte Count => 3;

            public VertexPositionTexture[] GenerateVPT(List<VectorBoneGRP> verts, Quaternion rotation, Vector3 translationPosition, Texture2D preVarTex)
            {
                if (TempVPT == null)
                    TempVPT = new VertexPositionTexture[Count];
                VertexPositionTexture GetVPT(ref Triangle triangle, byte i)
                {
                    Vector3 GetVertex(ref Triangle _triangle, byte _i)
                    {
                        return TransformVertex(verts[_triangle.GetIndex(_i)], translationPosition, rotation);
                    }
                    return new VertexPositionTexture(GetVertex(ref triangle, i), triangle.GetUV(i).ToVector2(preVarTex.Width, preVarTex.Height));
                }
                TempVPT[0] = GetVPT(ref this, this[0]);
                TempVPT[1] = GetVPT(ref this, this[1]);
                TempVPT[2] = GetVPT(ref this, this[2]);
                return TempVPT;
            }
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

            private VertexPositionTexture[] TempVPT;

            public ushort A1 { get => (ushort)(A & 0xFFF); set => A = value; }
            public ushort B1 { get => (ushort)(B & 0xFFF); set => B = value; }
            public ushort C1 { get => (ushort)(C & 0xFFF); set => C = value; }
            public ushort D1 { get => (ushort)(D & 0xFFF); set => D = value; }
            public byte textureIndex => (byte)((texUnk >> 6) & 0b111);

            public static Quad Create(BinaryReader br) => new Quad()
            {
                A = br.ReadUInt16(),
                B = br.ReadUInt16(),
                C = br.ReadUInt16(),
                D = br.ReadUInt16(),
                vta = UV.Create(br),
                texUnk = br.ReadUInt16(),
                vtb = UV.Create(br),
                u = br.ReadUInt16(),
                vtc = UV.Create(br),
                vtd = UV.Create(br),
                TempVPT = new VertexPositionTexture[Count]
            };

            public ushort GetIndex(int i)
            {
                switch (i)
                {
                    case 0:
                        return A1;

                    case 1:
                        return B1;

                    case 2:
                        return C1;

                    case 3:
                        return D1;
                }
                throw new IndexOutOfRangeException($"{this} :: 0-3 are only valid values");
            }

            public UV GetUV(int i)
            {
                switch (i)
                {
                    case 0:
                        return vta;

                    case 1:
                        return vtb;

                    case 2:
                        return vtc;

                    case 3:
                        return vtd;
                }
                throw new IndexOutOfRangeException($"{this} :: 0-3 are only valid values");
            }

            public byte this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                        case 3:
                            return 0;

                        case 1:
                            return 1;

                        case 2:
                        case 5:
                            return 3;

                        case 4:
                            return 2;
                    }
                    throw new IndexOutOfRangeException($"{this} :: 0-5 are only valid values");
                }
            }

            public byte[] Indices => new byte[] { this[0], this[1], this[2], this[3], this[4], this[5] };
            public static byte Count => 6;

            public VertexPositionTexture[] GenerateVPT(List<VectorBoneGRP> verts, Quaternion rotation, Vector3 translationPosition, Texture2D preVarTex)
            {
                if (TempVPT == null)
                    TempVPT = new VertexPositionTexture[Count];
                VertexPositionTexture GetVPT(ref Quad quad, byte i)
                {
                    Vector3 GetVertex(ref Quad _quad, byte _i)
                    {
                        return TransformVertex(verts[_quad.GetIndex(_i)], translationPosition, rotation);
                    }
                    return new VertexPositionTexture(GetVertex(ref quad, i), quad.GetUV(i).ToVector2(preVarTex.Width, preVarTex.Height));
                }
                TempVPT[0] = TempVPT[3] = GetVPT(ref this, this[0]);
                TempVPT[1] = GetVPT(ref this, this[1]);
                TempVPT[4] = GetVPT(ref this, this[4]);
                TempVPT[2] = TempVPT[5] = GetVPT(ref this, this[2]);
                return TempVPT;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
        public struct UV
        {
            public byte U;
            public byte V;

            public float U1(float h = 128f) => U / h;

            public float V1(float w = 128f) => V > 128 ? //if bigger than 128, then multitexture index to odd
                    (V - 128f) / w
                    : w == 32 ?  //if equals 32, then it's weapon texture and should be in range of 96-128
                        (V - 96) / w
                        : V / w;  //if none of these cases, then divide by resolution;

            public static UV Create(BinaryReader br) => new UV()
            {
                U = br.ReadByte(),
                V = br.ReadByte()
            };

            public Vector2 ToVector2(float w, float h) => new Vector2(U1(w), V1(h));

            public override string ToString() => $"{U};{U1()};{V};{V1()}";
        }

        public Geometry geometry;

        /// <summary>
        /// Model Geometry section
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection2(uint start)
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            geometry = new Geometry { cObjects = br.ReadUInt32() };
            geometry.pObjects = new uint[geometry.cObjects];
            for (int i = 0; i < geometry.cObjects; i++)
                geometry.pObjects[i] = br.ReadUInt32();
            geometry.objects = new Object[geometry.cObjects];
            for (int i = 0; i < geometry.cObjects; i++)
                geometry.objects[i] = ReadGeometryObject(start + geometry.pObjects[i]);
            geometry.cTotalVert = br.ReadUInt32();
        }

        private Object ReadGeometryObject(uint start)
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            Object @object = new Object { cVertices = br.ReadUInt16() };
            @object.vertexData = new VerticeData[@object.cVertices];
            if (br.BaseStream.Position + @object.cVertices * 6 >= br.BaseStream.Length)
                return @object;
            for (int n = 0; n < @object.cVertices; n++)
            {
                @object.vertexData[n].boneId = br.ReadUInt16();
                @object.vertexData[n].cVertices = br.ReadUInt16();
                @object.vertexData[n].vertices = new Vector3[@object.vertexData[n].cVertices];
                for (int i = 0; i < @object.vertexData[n].cVertices; i++)
                    @object.vertexData[n].vertices[i] = Extended.ByteArrayToStructure<Vertex>(br.ReadBytes(6));
            }
            br.BaseStream.Seek(4 - (br.BaseStream.Position % 4 == 0 ? 4 : br.BaseStream.Position % 4), SeekOrigin.Current);
            @object.cTriangles = br.ReadUInt16();
            @object.cQuads = br.ReadUInt16();
            @object.padding = br.ReadUInt64();
            @object.triangles = new Triangle[@object.cTriangles];
            if (@object.cTriangles == 0 && @object.cQuads == 0)
                return @object;
            @object.quads = new Quad[@object.cQuads];
            for (int i = 0; i < @object.cTriangles; i++)
                @object.triangles[i] = Triangle.Create(br);//Extended.ByteArrayToStructure<Triangle>(br.ReadBytes(16));
            for (int i = 0; i < @object.cQuads; i++)
                @object.quads[i] = Quad.Create(br);// Extended.ByteArrayToStructure<Quad>(br.ReadBytes(20));

            return @object;
        }

        public struct VectorBoneGRP
        {
            private Vector3 Vector { get; set; }
            public int BoneID { get; private set; }
            public float X => Vector.X;
            public float Y => Vector.Y;
            public float Z => Vector.Z;

            public static implicit operator Vector3(VectorBoneGRP vbg) => vbg.Vector;

            public VectorBoneGRP(Vector3 vector, int boneID)
            {
                Vector = vector;
                BoneID = boneID;
            }

            public override string ToString() => $"Vector: {Vector}, Bone ID: {BoneID}";
        }

        public struct VertexPositionTexturePointersGRP
        {
            public VertexPositionTexturePointersGRP(VertexPositionTexture[] vpt, byte[] texturepointers) : this()
            {
                this.VPT = vpt;
                this.TexturePointers = texturepointers;
            }

            public VertexPositionTexture[] VPT { get; private set; }
            public byte[] TexturePointers { get; private set; }
        }

        public Vector3 IndicatorPoint(Vector3 translationPosition)
        {
            if (offsetylow < 0)
                translationPosition.Y -= offsetylow;
            return _indicatorPoint + translationPosition;
        }

        //public ConcurrentDictionary<Tuple<int, int>, float> lowpoints = new ConcurrentDictionary<Tuple<int, int>, float>();

        /// <summary>
        /// This method returns geometry data AFTER animation matrix translations, local
        /// position/rotation translations. This is the final step of calculation. This data should
        /// be used only by Renderer. Any translations/vertices manipulation should happen inside
        /// this method or earlier
        /// </summary>
        /// <param name="objectId">
        /// Monsters can have more than one object. Treat it like multi-model geometry. They are all
        /// needed to build whole model
        /// </param>
        /// <param name="position">a Vector3 to set global position</param>
        /// <param name="rotation">a Quaternion to set the correct rotation. 1=90, 2=180 ...</param>
        /// <param name="animationId">an animation pointer. Animation 0 is always idle</param>
        /// <param name="animationFrame">
        /// an animation frame from animation id. You should pass incrementing frame and reset to 0
        /// when frameCount max is hit
        /// </param>
        /// <param name="step">
        /// FEATURE: This float (0.0 - 1.0) is used in Linear interpolation in animation frames
        /// blending. 0.0 means frameN, 1.0 means FrameN+1. Usually this should be a result of
        /// deltaTime to see if computer is capable of rendering smooth animations rather than
        /// constant 15 FPS
        /// </param>
        /// <returns></returns>
        public VertexPositionTexturePointersGRP GetVertexPositions(int objectId, ref Vector3 translationPosition, Quaternion rotation, ref AnimationSystem animationSystem, double step)
        {
            if (animationSystem.AnimationFrame >= animHeader.animations[animationSystem.AnimationId].animationFrames.Length || animationSystem.AnimationFrame < 0)
                animationSystem.AnimationFrame = 0;
            AnimationFrame nextFrame = animHeader.animations[animationSystem.AnimationId].animationFrames[animationSystem.AnimationFrame];
            int lastAnimationFrame = animationSystem.LastAnimationFrame;
            AnimationFrame[] lastAnimationFrames = animHeader.animations[animationSystem.LastAnimationId].animationFrames;
            lastAnimationFrame = lastAnimationFrames.Length > lastAnimationFrame ? lastAnimationFrame : lastAnimationFrames.Length - 1;
            AnimationFrame frame = lastAnimationFrames[lastAnimationFrame];

            Object obj = geometry.objects[objectId];
            int i = 0;

            List<VectorBoneGRP> verts = GetVertices(obj, frame, nextFrame, step);
            //float minY = verts.Min(x => x.Y);
            //Vector2 HLPTS = FindLowHighPoints(translationPosition, rotation, frame, nextFrame, step);

            byte[] texturePointers = new byte[obj.cTriangles + obj.cQuads * 2];
            List<VertexPositionTexture> vpt = new List<VertexPositionTexture>(texturePointers.Length * 3);

            if (objectId == 0)
            {
                Battle.AnimationSystem _animationSystem = animationSystem;
                AnimationYOffset lastoffsets = AnimationYOffsets?.First(x => x.ID == _animationSystem.LastAnimationId && x.Frame == lastAnimationFrame) ?? default;
                AnimationYOffset nextoffsets = AnimationYOffsets?.First(x => x.ID == _animationSystem.AnimationId && x.Frame == _animationSystem.AnimationFrame) ?? default;
                offsetylow = MathHelper.Lerp(lastoffsets.LowY, nextoffsets.LowY, (float)step);
                _indicatorPoint.X = MathHelper.Lerp(lastoffsets.MidX, nextoffsets.MidX, (float)step);
                _indicatorPoint.Y = MathHelper.Lerp(lastoffsets.HighY, nextoffsets.HighY, (float)step);
                _indicatorPoint.Z = MathHelper.Lerp(lastoffsets.MidZ, nextoffsets.MidZ, (float)step);
                // Move All Y axis down to 0 based on Lowest Y axis in Animation ID 0.
                if (OffsetY < 0)
                {
                    translationPosition.Y += OffsetY;
                }
                // If any Y axis readings are lower than 0 in Animation ID >0. Bring it up to zero.
            }

            //Triangle parsing
            for (; i < obj.cTriangles; i++)
            {
                Texture2D preVarTex = (Texture2D)textures.textures[obj.triangles[i].textureIndex];
                vpt.AddRange(obj.triangles[i].GenerateVPT(verts, rotation, translationPosition, preVarTex));
                texturePointers[i] = obj.triangles[i].textureIndex;
            }

            //Quad parsing
            for (i = 0; i < obj.cQuads; i++)
            {
                Texture2D preVarTex = (Texture2D)textures.textures[obj.quads[i].textureIndex];
                vpt.AddRange(obj.quads[i].GenerateVPT(verts, rotation, translationPosition, preVarTex));
                texturePointers[obj.cTriangles + i * 2] = obj.quads[i].textureIndex;
                texturePointers[obj.cTriangles + i * 2 + 1] = obj.quads[i].textureIndex;
            }

            return new VertexPositionTexturePointersGRP(vpt.ToArray(), texturePointers);
        }

        private List<VectorBoneGRP> GetVertices(Object @object, AnimationFrame frame, AnimationFrame nextFrame, double step) => @object.vertexData.SelectMany(vertexdata => vertexdata.vertices.Select(vertex => CalculateFrame(new VectorBoneGRP(vertex, vertexdata.boneId), frame, nextFrame, step))).ToList();

        private Vector4 FindLowHighPoints(Vector3 translationPosition, Quaternion rotation, AnimationFrame frame, AnimationFrame nextFrame, double step)
        {
            List<VectorBoneGRP> vertices =
                geometry.objects.SelectMany(@object => GetVertices(@object, frame, nextFrame, step)).ToList();
            if (translationPosition != Vector3.Zero || rotation != Quaternion.Identity)
            {
                IEnumerable<Vector3> _vertices = vertices.Select(vertex => TransformVertex(vertex, translationPosition, rotation));
                // midpoints
                return new Vector4(_vertices.Min(x => x.Y), _vertices.Max(x => x.Y), (_vertices.Min(x => x.X) + _vertices.Max(x => x.X)) / 2f, (_vertices.Min(x => x.Z) + _vertices.Max(x => x.Z)) / 2f);
            }
            else
                // alt midpoints
                return new Vector4(vertices.Min(x => x.Y), vertices.Max(x => x.Y), (vertices.Min(x => x.X) + vertices.Max(x => x.X)) / 2f, (vertices.Min(x => x.Z) + vertices.Max(x => x.Z)) / 2f);
        }

        public static Vector3 TransformVertex(Vector3 vertex, Vector3 localTranslate, Quaternion rotation) => Vector3.Transform(Vector3.Transform(vertex, rotation), Matrix.CreateTranslation(localTranslate));

        /// <summary>
        /// Complex function that provides linear interpolation between two matrices of actual
        /// to-render animation frame and next frame data for blending
        /// </summary>
        /// <param name="VBG">the tuple that contains vertex and bone ident</param>
        /// <param name="frame">current animation frame to render</param>
        /// <param name="nextFrame">
        /// animation frame to render that is AFTER the actual one. If last frame, then usually 0 is
        /// the 'next' frame
        /// </param>
        /// <param name="step">
        /// step is variable used to determinate the progress for linear interpolation. I.e. 0 for
        /// current frame and 1 for next frame; 0.5 for blend of two frames
        /// </param>
        /// <returns></returns>
        private VectorBoneGRP CalculateFrame(VectorBoneGRP VBG, AnimationFrame frame, AnimationFrame nextFrame, double step)
        {
            Vector3 getvector(Matrix matrix)
            {
                Vector3 r = new Vector3(
                    matrix.M11 * VBG.X + matrix.M41 + matrix.M12 * VBG.Z + matrix.M13 * -VBG.Y,
                    matrix.M21 * VBG.X + matrix.M42 + matrix.M22 * VBG.Z + matrix.M23 * -VBG.Y,
                    matrix.M31 * VBG.X + matrix.M43 + matrix.M32 * VBG.Z + matrix.M33 * -VBG.Y);
                r = Vector3.Transform(r, Matrix.CreateScale(skeleton.GetScale));
                return r;
            }
            Vector3 rootFramePos = getvector(frame.boneMatrix[VBG.BoneID]); //get's bone matrix
            if (step > 0f)
            {
                Vector3 nextFramePos = getvector(nextFrame.boneMatrix[VBG.BoneID]);
                rootFramePos = Vector3.Lerp(rootFramePos, nextFramePos, (float)step);
            }
            return new VectorBoneGRP(rootFramePos, VBG.BoneID);
        }

        public struct AnimationData
        {
            public uint cAnimations;
            public uint[] pAnimations;
            public Animation[] animations;
        }

        public struct Animation
        {
            public byte cFrames;
            public AnimationFrame[] animationFrames;
        }

        public struct AnimationFrame
        {
            private Vector3 position;
            public Vector3[] bonesVectorRotations;
            public Matrix[] boneMatrix;

            public Vector3 Position { get => position; set => position = value; }
        }

        /// <summary>
        /// Model Animation section
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection3(uint start)
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            animHeader = new AnimationData() { cAnimations = br.ReadUInt32() };
            animHeader.pAnimations = new uint[animHeader.cAnimations];
            for (int i = 0; i < animHeader.cAnimations; i++)
            {
                animHeader.pAnimations[i] = br.ReadUInt32();
                //Console.WriteLine($"{i}|{animHeader.pAnimations[i]}");
            }
            animHeader.animations = new Animation[animHeader.cAnimations];
            for (int i = 0; i < animHeader.cAnimations; i++) //animation
            {
                br.BaseStream.Seek(start + animHeader.pAnimations[i], SeekOrigin.Begin); //Get to pointer of animation Id
                animHeader.animations[i] = new Animation() { cFrames = br.ReadByte() }; //Create new animation with cFrames frames
                animHeader.animations[i].animationFrames = new AnimationFrame[animHeader.animations[i].cFrames];
                ExtapathyExtended.BitReader bitReader = new ExtapathyExtended.BitReader(br.BaseStream);
                for (int n = 0; n < animHeader.animations[i].cFrames; n++) //frames
                {
                    //Step 1. It starts with bone0.position. Let's read that into AnimationFrames[animId]- it's only one position per frame

                    float x = bitReader.ReadPositionType() * .01f;
                    float y = bitReader.ReadPositionType() * .01f;
                    float z = bitReader.ReadPositionType() * .01f;
                    animHeader.animations[i].animationFrames[n] = n == 0
                        ? new AnimationFrame()
                        { Position = new Vector3(x, y, z) }
                        : new AnimationFrame()
                        {
                            Position = new Vector3(
                    animHeader.animations[i].animationFrames[n - 1].Position.X + x,
                    animHeader.animations[i].animationFrames[n - 1].Position.Y + y,
                    animHeader.animations[i].animationFrames[n - 1].Position.Z + z)
                        };
                    byte ModeTest = (byte)bitReader.ReadBits(1); //used to determine if additional info is required
                    if (i == 0 && n == 0)
                        Console.WriteLine($"{i} {n}: {ModeTest}");
                    animHeader.animations[i].animationFrames[n].boneMatrix = new Matrix[skeleton.cBones];
                    animHeader.animations[i].animationFrames[n].bonesVectorRotations = new Vector3[skeleton.cBones];

                    //Step 2. We read the position and we need to store the bones rotations or save base rotation if frame==0

                    for (int k = 0; k < skeleton.cBones; k++) //bones iterator
                    {
                        if (n != 0) //just like position the data for next frames are added to previous
                        {
                            animHeader.animations[i].animationFrames[n].bonesVectorRotations[k] = new Vector3()
                            {
                                X = bitReader.ReadRotationType(),
                                Y = bitReader.ReadRotationType(),
                                Z = bitReader.ReadRotationType()
                            };
                            if (ModeTest > 0)
                                _ = GetAdditionalRotationInformation(bitReader);
                            Vector3 previousFrame = animHeader.animations[i].animationFrames[n - 1].bonesVectorRotations[k];
                            Vector3 currentFrame = animHeader.animations[i].animationFrames[n].bonesVectorRotations[k];
                            animHeader.animations[i].animationFrames[n].bonesVectorRotations[k] = previousFrame + currentFrame;
                        }
                        else //if this is zero frame, then we need to set the base rotations for bones
                        {
                            animHeader.animations[i].animationFrames[n].bonesVectorRotations[k] = new Vector3()
                            {
                                X = bitReader.ReadRotationType(),
                                Y = bitReader.ReadRotationType(),
                                Z = bitReader.ReadRotationType()
                            };
                            if (ModeTest > 0)
                                _ = GetAdditionalRotationInformation(bitReader);
                        }
                    }

                    //Step 3. We now have all bone rotations stored into short. We need to convert that into Matrix and 360/4096
                    for (int k = 0; k < skeleton.cBones; k++)
                    {
                        Vector3 boneRotation = animHeader.animations[i].animationFrames[n].bonesVectorRotations[k];
                        boneRotation = Extended.S16VectorToFloat(boneRotation); //we had vector3 containing direct copy of short to float, now we need them in real floating point values
                        boneRotation *= DEGREES; //bone rotations are in 360 scope
                        //maki way
                        Matrix xRot = Extended.GetRotationMatrixX(-boneRotation.X);
                        Matrix yRot = Extended.GetRotationMatrixY(-boneRotation.Y);
                        Matrix zRot = Extended.GetRotationMatrixZ(-boneRotation.Z);

                        //this is the monogame way and gives same results as above.
                        //Matrix xRot = Matrix.CreateRotationX(MathHelper.ToRadians(boneRotation.X));
                        //Matrix yRot = Matrix.CreateRotationY(MathHelper.ToRadians(boneRotation.Y));
                        //Matrix zRot = Matrix.CreateRotationZ(MathHelper.ToRadians(boneRotation.Z));

                        Matrix MatrixZ = Extended.MatrixMultiply_transpose(yRot, xRot);
                        MatrixZ = Extended.MatrixMultiply_transpose(zRot, MatrixZ);

                        if (skeleton.bones[k].parentId == 0xFFFF) //if parentId is 0xFFFF then the current bone is core aka bone0
                        {
                            MatrixZ.M41 = -animHeader.animations[i].animationFrames[n].Position.X;
                            MatrixZ.M42 = -animHeader.animations[i].animationFrames[n].Position.Y; //up/down
                            MatrixZ.M43 = animHeader.animations[i].animationFrames[n].Position.Z;
                            MatrixZ.M44 = 1;
                        }
                        else
                        {
                            Matrix parentBone = animHeader.animations[i].animationFrames[n].boneMatrix[skeleton.bones[k].parentId]; //gets the parent bone
                            MatrixZ.M43 = skeleton.bones[skeleton.bones[k].parentId].Size;
                            Matrix rMatrix = Matrix.Multiply(parentBone, MatrixZ);
                            rMatrix.M41 = parentBone.M11 * MatrixZ.M41 + parentBone.M12 * MatrixZ.M42 + parentBone.M13 * MatrixZ.M43 + parentBone.M41;
                            rMatrix.M42 = parentBone.M21 * MatrixZ.M41 + parentBone.M22 * MatrixZ.M42 + parentBone.M23 * MatrixZ.M43 + parentBone.M42;
                            rMatrix.M43 = parentBone.M31 * MatrixZ.M41 + parentBone.M32 * MatrixZ.M42 + parentBone.M33 * MatrixZ.M43 + parentBone.M43;
                            rMatrix.M44 = 1;
                            MatrixZ = rMatrix;
                        }

                        animHeader.animations[i].animationFrames[n].boneMatrix[k] = MatrixZ;
                    }
                }
            }
        }

        /// <summary>
        /// Some enemies use additional information that is saved for bone AFTER rotation types. We
        /// are still not sure what it does as enemy works without it
        /// </summary>
        /// <param name="bitReader"></param>
        /// <returns></returns>
        private Tuple<short, short, short> GetAdditionalRotationInformation(ExtapathyExtended.BitReader bitReader)
        {
            short unk1v = 0, unk2v = 0, unk3v = 0;

            byte unk1 = (byte)bitReader.ReadBits(1);
            if (unk1 > 0)
                unk1v = (short)(bitReader.ReadBits(16) + 1024);
            else unk1v = 1024;

            byte unk2 = (byte)bitReader.ReadBits(1);
            if (unk2 > 0)
                unk2v = (short)(bitReader.ReadBits(16) + 1024);
            else unk2v = 1024;

            byte unk3 = (byte)bitReader.ReadBits(1);
            if (unk3 > 0)
                unk3v = (short)(bitReader.ReadBits(16) + 1024);
            else unk3v = 1024;

            return new Tuple<short, short, short>(unk1v, unk2v, unk3v);
        }

        public AnimationData animHeader;
        public int frame;
        public float frameperFPS = 0.0f;

        public struct Textures
        {
            /// <summary>
            /// TIM count
            /// </summary>
            public uint cTims;

            /// <summary>
            /// File pointers
            /// </summary>
            public uint[] pTims;

            /// <summary>
            /// EOF
            /// </summary>
            public uint Eof;

            /// <summary>
            /// Texture 2D wrapped in TextureHandler for mod support
            /// </summary>
            public TextureHandler[] textures;
        }

        /// <summary>
        /// TIMS - Textures
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection11(uint start)
        {
#if DEBUG
            //Dump for debug
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            using (BinaryWriter fs = new BinaryWriter(File.Create(Path.Combine(Path.GetTempPath(), $"{start}.dump"), (int)(br.BaseStream.Length - br.BaseStream.Position), FileOptions.None)))
                fs.Write(br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position)));
#endif
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            //Begin create Textures struct
            //populate the tim count;
            textures = new Textures() { cTims = br.ReadUInt32() };
            //create arrays per count.
            textures.pTims = new uint[textures.cTims];
            textures.textures = new TextureHandler[textures.cTims];
            //Read pointers into array
            for (int i = 0; i < textures.cTims; i++)
                textures.pTims[i] = br.ReadUInt32();
            //Read EOF
            textures.Eof = br.ReadUInt32();
            //Read TIM -> TextureHandler into array
            for (int i = 0; i < textures.cTims; i++)
                if (buffer[start + textures.pTims[i]] == 0x10)
                {
                    TIM2 tm = new TIM2(buffer, start + textures.pTims[i]); //broken
                    textures.textures[i] = TextureHandler.Create($"{fileName}_{i/*.ToString("D2")*/}", tm, 0);// tm.GetTexture(0);
                }
                else
                    Debug.WriteLine($"DEBUG: {this}.{this.id}.{start + textures.pTims[i]} :: Not a tim file!");
        }

        public Textures textures;
        private BinaryReader br;
        //private float lowpoint;

        public enum EntityType
        {
            Monster,
            Character,
            Weapon
        };

        public Debug_battleDat()
        {
        }

        /// <summary>
        /// Creates new instance of DAT class that provides every sections parsed into structs and
        /// helper functions for renderer
        /// </summary>
        /// <param name="fileId">This number is used in c0m(fileId) or d(fileId)cXYZ</param>
        /// <param name="entityType">Supply Monster, character or weapon (0,1,2)</param>
        /// <param name="additionalFileId">Used only in character or weapon to supply for d(fileId)[c/w](additionalFileId)</param>
        public static Debug_battleDat Load(int fileId, EntityType entityType, int additionalFileId = -1, Debug_battleDat skeletonReference = null, Sections flags = Sections.All)
        {
            Flags = flags;
            Debug_battleDat r = new Debug_battleDat()
            {
                id = fileId,
                altid = additionalFileId
            };
            Console.WriteLine($"DEBUG: Creating new BattleDat with {fileId},{entityType},{additionalFileId}");
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);
            char et = entityType == EntityType.Weapon ? 'w' : entityType == EntityType.Character ? 'c' : default;
            string fileName = entityType == EntityType.Monster ? $"c0m{r.id.ToString("D03")}" :
                entityType == EntityType.Character || entityType == EntityType.Weapon ? $"d{fileId.ToString("x")}{et}{additionalFileId.ToString("D03")}"
                : string.Empty;
            r.entityType = entityType;
            if (string.IsNullOrEmpty(fileName))
                return null;
            string path = null;
            string searchstring = "";
            if (et != default)
            {
                searchstring = $"d{fileId.ToString("x")}{et}";
                IEnumerable<string> test = aw.GetListOfFiles().Where(x => x.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase) >= 0);
                path = test.FirstOrDefault(x => x.ToLower().Contains(fileName));

                if (string.IsNullOrWhiteSpace(path) && test.Count() > 0 && entityType == EntityType.Character)
                    path = test.First();
            }
            else path = aw.GetListOfFiles().FirstOrDefault(x => x.ToLower().Contains(fileName));
            //Debug.Assert("c:\\ff8\\data\\ger\\battle\\d0w007.dat" != path);
            r.fileName = fileName;
            if (!string.IsNullOrWhiteSpace(path))
                r.buffer = aw.GetBinaryFile(path);
            if (r.buffer == null || r.buffer.Length < 0)
            {
                Debug.WriteLine($"Search String: {searchstring} Not Found skipping {entityType}; So resulting file buffer is null.");
                return null;
            }
            r.ExportFile();
            r.LoadFile(skeletonReference);
            r.FindAllLowHighPoints();
            return r;
        }

        private void FindAllLowHighPoints()
        {
            if (entityType == EntityType.Character || entityType == EntityType.Monster)
            {
                // Get baseline from running function on only Animation 0;
                if (animHeader.animations == null)
                    return;
                List<Vector4> baseline = animHeader.animations[0].animationFrames.Select(x => FindLowHighPoints(Vector3.Zero, Quaternion.Identity, x, x, 0f)).ToList();
                //X is lowY, Y is high Y, Z is mid x, W is mid z
                float baselinelowY = baseline.Min(x => x.X);
                float baselinehighY = baseline.Max(x => x.Y);
                offsetY = 0f;
                if (Math.Abs(baselinelowY) < BaseLineMaxYFilter)
                {
                    offsetY -= baselinelowY;
                    baselinehighY += offsetY;
                }
                // Default indicator point
                _indicatorPoint = new Vector3(0, baselinehighY, 0);
                // Need to add this later to bring baselinelow to 0.
                //OffsetY = offsetY;
                // Brings all Y values less than baselinelow to baselinelow
                AnimationYOffsets = animHeader.animations.SelectMany((animation, animationid) =>
                animation.animationFrames.Select((animationframe, animationframenumber) =>
                new AnimationYOffset(animationid, animationframenumber, FindLowHighPoints(OffsetYVector, Quaternion.Identity, animationframe, animationframe, 0f)))).ToList();
            }
        }

        private List<AnimationYOffset> AnimationYOffsets;
        private Vector3 _indicatorPoint;
        private float offsetY;
        private float offsetylow;

        private float OffsetY => offsetY;
        private Vector3 OffsetYVector => new Vector3(0f, OffsetY, 0f);

        public struct AnimationYOffset
        {
            public int ID { get; private set; }
            public int Frame { get; private set; }
            public float LowY { get; private set; }
            public float HighY { get; private set; }
            public float MidX { get; private set; }
            public float MidZ { get; private set; }

            public AnimationYOffset(int iD, int frame, Vector4 lowhigh)
                : this(iD, frame, lowhigh.X, lowhigh.Y, lowhigh.Z, lowhigh.W)
            { }

            public AnimationYOffset(int iD, int frame, float low, float high, float midx, float midz)
            {
                ID = iD;
                Frame = frame;
                LowY = low;
                HighY = high;
                MidX = midx;
                MidZ = midz;
            }

            public override string ToString() => $"[{ID}, {Frame}, {LowY}, {HighY}, {MidX}, {MidZ}]";
        }

        private void ExportFile()
        {
#if _WINDOWS && DEBUG
            try
            {
                string targetdir = @"d:\";
                if (Directory.Exists(targetdir))
                {
                    DriveInfo[] drivei = DriveInfo.GetDrives().Where(x => x.Name.IndexOf(Path.GetPathRoot(targetdir), StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                    DirectoryInfo di = new DirectoryInfo(targetdir);
                    if (!di.Attributes.HasFlag(FileAttributes.ReadOnly) && drivei.Count() == 1 && drivei[0].DriveType == DriveType.Fixed)
                        Extended.DumpBuffer(buffer, Path.Combine(targetdir, "out.dat"));
                }
            }
            catch (IOException)
            {
            }
#endif
        }

        private Debug_battleDat LoadFile(Debug_battleDat skeletonReference)
        {
            using (br = new BinaryReader(new MemoryStream(buffer)))
            {
                if (br.BaseStream.Length - br.BaseStream.Position < 4)
                    return null;
                Init();
                switch (entityType)
                {
                    case EntityType.Monster:
                        return LoadMonster();

                    case EntityType.Character:
                        return LoadCharacter();

                    case EntityType.Weapon:
                        return LoadWeapon(skeletonReference);
                }
                return null;
            }
        }

        private void Init()
        {
            datFile = new DatFile { cSections = br.ReadUInt32() };
            datFile.pSections = new uint[datFile.cSections];
            for (int i = 0; i < datFile.cSections; i++)
                datFile.pSections[i] = br.ReadUInt32();
            datFile.eof = br.ReadUInt32();
        }

        private Debug_battleDat LoadMonster()
        {
            if (id == 127)
            {
                // per wiki 127 only have 7 & 8
                if (Flags.HasFlag(Sections.Information))
                    ReadSection7(datFile.pSections[0]);
                //if (Flags.HasFlag(Sections.Scripts))
                //    ReadSection8(datFile.pSections[7]);
                return this;
            }
            if (Flags.HasFlag(Sections.Skeleton))
                ReadSection1(datFile.pSections[0]);
            if (Flags.HasFlag(Sections.Model_Animation))
                ReadSection3(datFile.pSections[2]); // animation data
            if (Flags.HasFlag(Sections.Model_Geometry))
                ReadSection2(datFile.pSections[1]);

            //if (Flags.HasFlag(Sections.Section4_Unknown))
            //  ReadSection4(datFile.pSections[3]);
            if (Flags.HasFlag(Sections.Animation_Sequences))
                ReadSection5(datFile.pSections[4], datFile.pSections[5]);
            //if (Flags.HasFlag(Sections.Section6_Unknown))
            //  ReadSection6(datFile.pSections[5]);
            if (Flags.HasFlag(Sections.Information))
                ReadSection7(datFile.pSections[6]);
            //if (Flags.HasFlag(Sections.Scripts))
            //ReadSection8(datFile.pSections[7]); // battle scripts/ai

            //if (Flags.HasFlag(Sections.Sounds))
            //    ReadSection9(datFile.pSections[8], datFile.pSections[9]); //AKAO sounds
            //if (Flags.HasFlag(Sections.Sounds_Unknown))
            //  ReadSection10(datFile.pSections[9], datFile.pSections[10], br, fileName);

            if (Flags.HasFlag(Sections.Textures))
                ReadSection11(datFile.pSections[10]);
            return this;
        }

        private Debug_battleDat LoadCharacter()
        {
            if (Flags.HasFlag(Sections.Skeleton))
                ReadSection1(datFile.pSections[0]);
            if (Flags.HasFlag(Sections.Model_Animation))
                ReadSection3(datFile.pSections[2]);
            if (Flags.HasFlag(Sections.Model_Geometry))
                ReadSection2(datFile.pSections[1]);
            if (id == 7 && entityType == EntityType.Character) // edna has no weapons.
            {
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(datFile.pSections[8]);
                if (Flags.HasFlag(Sections.Animation_Sequences))
                    ReadSection5(datFile.pSections[5], datFile.pSections[6]);
            }
            else if (Flags.HasFlag(Sections.Textures))
                ReadSection11(datFile.pSections[5]);
            return this;
        }

        private Debug_battleDat LoadWeapon(Debug_battleDat skeletonReference)
        {
            if (id != 1 && id != 9)
            {
                if (Flags.HasFlag(Sections.Skeleton))
                    ReadSection1(datFile.pSections[0]);
                if (Flags.HasFlag(Sections.Model_Animation))
                    ReadSection3(datFile.pSections[2]);
                if (Flags.HasFlag(Sections.Model_Geometry))
                    ReadSection2(datFile.pSections[1]);
                if (Flags.HasFlag(Sections.Animation_Sequences))
                    ReadSection5(datFile.pSections[3], datFile.pSections[4]);
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(datFile.pSections[6]);
            }
            else if (skeletonReference != null)
            {
                skeleton = skeletonReference.skeleton;
                animHeader = skeletonReference.animHeader;
                if (Flags.HasFlag(Sections.Model_Geometry))
                    ReadSection2(datFile.pSections[0]);
                if (Flags.HasFlag(Sections.Animation_Sequences))
                    ReadSection5(datFile.pSections[1], datFile.pSections[2]);
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(datFile.pSections[4]);
            }
            return this;
        }

        /// <summary>
        /// Sounds
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection9(uint start, uint end)
        {
            //Contains AKAO sequences(can be empty).
            //Offset Length  Description
            //0   2 bytes Number of AKAOs
            //2   nbAKAOs * 2 bytes AKAOs Positions
            //2 + nbAKAOs * 2 2 bytes End of section 9
            //4 + nbAKAOs * 2 Varies* nbAKAOs    AKAOs

            // nothing final in here just was trying to dump data to see what was there.
            // http://wiki.ffrtt.ru/index.php?title=FF7/Field/Script/Opcodes/F2_AKAO related?
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            uint[] offsets = new uint[br.ReadUInt16()];
            for (ushort i = 0; i < offsets.Length; i++)
            {
                ushort offset = br.ReadUInt16();
                if (offset == 0)
                    continue;
                offsets[i] = offset + start;
            }
            uint newend = br.ReadUInt16() + start;
            if (newend < end) end = newend;
            List<uint> sortedoffsets = offsets.Where(x => x > 0).Distinct().OrderBy(x => x).ToList();
            Dictionary<uint, byte[]> dataatoffsets = new Dictionary<uint, byte[]>(sortedoffsets.Count);
            for (ushort i = 0; i < sortedoffsets.Count; i++)
            {
                uint offset = sortedoffsets[i];
                uint localend = end;
                if (i + 1 < sortedoffsets.Count)
                    localend = sortedoffsets[i + 1];
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                if (offset < localend)
                    dataatoffsets.Add(offset, br.ReadBytes(checked((int)(localend - offset))));
            }
        }

        public List<AnimationSequence> Sequences { get; private set; }

        public struct AnimationSequence
        {
            public int id;
            public uint offset;
            public byte[] data;

            /// <summary>
            /// Test-Reason for list is so i can go read the data with out removing it.
            /// </summary>
            public List<byte> AnimationQueue { get; set; }

            //public static Dictionary<byte, Action<byte[], int>> ParseData = new Dictionary<byte, Action<byte[], int>>{
            //    { 0xA3, (byte[] data, int i) => { } } };
            public void GenerateQueue(Debug_battleDat dat)
            {
                AnimationQueue = new List<byte>();
                for (int i = 0; data != null && i < data.Length; i++)
                {
                    byte b;
                    byte[] data = this.data;
                    byte get(int _i = -1)
                    {
                        return b = data[_i < 0 ? i : _i];
                    }
                    if (get() < (dat.animHeader.animations?.Length ?? 0))
                    {
                        AnimationQueue.Add(b);
                    }
                    //else switch(b)
                    //{
                    //        case 0xA3:
                    //            // following value is animation.
                    //            break;
                    //        case 0xE6:
                    //            switch (get(++i))
                    //            {
                    //                case 0x03:
                    //                    i += 1;
                    //                    break;
                    //            }
                    //            break;
                    //        case 0xEA:
                    //            switch (get(++i))
                    //            {
                    //                case 0x05:
                    //                    i += 1;
                    //                    break;
                    //                case 0x06:
                    //                    i += 2;
                    //                    break;
                    //            }
                    //            break;
                    //        default:
                    //            i++;//skip next byte //as might not be a animation.
                    //            break;
                    //}
                }
            }
        }

        /// <summary>
        /// Animation Sequences
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        /// <see cref="http://forums.qhimm.com/index.php?topic=19362.msg270092"/>
        private void ReadSection5(uint start, uint end)
        {
            // nothing final in here just was trying to dump data to see what was there.
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            uint[] offsets = new uint[br.ReadUInt16()];
            for (ushort i = 0; i < offsets.Length; i++)
            {
                ushort offset = br.ReadUInt16();
                if (offset == 0)
                    continue;
                offsets[i] = offset + start;
            }
            List<uint> sortedoffsets = offsets.Where(x => x > 0).Distinct().OrderBy(x => x).ToList();
            Sequences = new List<AnimationSequence>(sortedoffsets.Count);
            //Dictionary<uint, byte[]> dataatoffsets = new Dictionary<uint, byte[]>(sortedoffsets.Count);
            for (ushort i = 0; i < sortedoffsets.Count; i++)
            {
                uint offset = sortedoffsets[i];
                //uint tie = endpoint;
                //if (i + 1 < t.Count)
                //    tie = t[i + 1];
                uint localend = offset;
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                do
                    localend++;
                while (br.ReadByte() != 0xa2 && br.BaseStream.Position < end && (i + 1 < sortedoffsets.Count ? br.BaseStream.Position < sortedoffsets[i + 1] : true));
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                foreach (var offsetindexed in offsets.Select((value, index) => new { value, index }).Where(x => x.value == offset))
                {
                    AnimationSequence sequence = new AnimationSequence { id = offsetindexed.index, offset = offsetindexed.value, data = br.ReadBytes(checked((int)(localend - offset))) };
                    sequence.GenerateQueue(this);
                    Sequences.Add(sequence);
                }
            }
            //foreach (KeyValuePair<uint, byte[]> ob in dataatoffsets)
            //{
            //    Debug.Write($"{ob.Key}({string.Format("{0:x}", offsets[ob.Key])}) - ");
            //    for (int i = 0; i< ob.Value.Length; i++)
            //    {
            //        byte b;
            //        byte Get(int pos = -1)
            //        { return b = ob.Value[pos<0?i:pos]; }
            //        switch (Get())
            //        {
            //            case 0xa5:
            //                Debug.Write("{Aura-A5}");
            //                switch (Get(++i))
            //                {
            //                    case 0x00:
            //                        Debug.Write("{Magic-00}");
            //                        break;

            //                    case 0x01:
            //                        Debug.Write("{GF-01}");
            //                        break;

            //                    case 0x02:
            //                        Debug.Write("{Limit-02}");
            //                        break;

            //                    case 0x03:
            //                        Debug.Write("{Finisher-03}");
            //                        break;

            //                    case 0x04:
            //                        Debug.Write("{Enemy Magic-04}");
            //                        break;
            //                    default:
            //                        Debug.Write(string.Format("{0:x2}", b));
            //                        break;

            //                }
            //                break;
            //            case 0xb5:
            //                Debug.Write("Sound-B5");
            //                break;
            //            case 0xbb:
            //                Debug.Write("{Effect-BB}");
            //                break;

            //            case 0xa2:
            //                Debug.Write("{Return-A2}");
            //                break;

            //            case 0xa0:
            //                Debug.Write("{Loop-A0}");
            //                Debug.Write("{Anim}");
            //                Debug.Write(string.Format("{0:x2} ", Get(++i)));
            //                break;

            //            case 0xc1:
            //                if (Get(i + 2) == 0xe5 && Get(i + 3) == 0x7f)
            //                {
            //                    Debug.Write("{Repeat-C1}");
            //                    //loop 0x1E times Animation 28
            //                    //C1 1E E5 7F 28
            //                    Debug.Write("{Count}");
            //                    Debug.Write($"{Get(++i)} ");
            //                    Debug.Write(string.Format("{0:x2} ", Get(++i)));
            //                    Debug.Write(string.Format("{0:x2} ", Get(++i)));
            //                    Debug.Write("{Anim}");
            //                    Debug.Write(string.Format("{0:x2} ", Get(++i)));
            //                }
            //                else if(Get(i + 1) == 0x00 && Get(i + 2) == 0xe5 && Get(i + 3) == 0x0f)
            //                {
            //                    //Return to home location.
            //                    //C1 00 E5 0F
            //                    Debug.Write("{Place model at home location}");
            //                }
            //                else
            //                    Debug.Write(string.Format(" {0:x2}", Get()));

            //                break;
            //            case 0xc3:
            //                Debug.Write("{Special-C3}");
            //                //C3 7F C5 FF E5 7F E7 F9 //wait till previous sequence is complete.
            //                //C3 0c e1 23 e5 7f ba
            //                //C3 08 d8 00 01 e5 08 {04,05}
            //                break;

            //            case 0x91:
            //                Debug.Write("{Text-91}");
            //                break;

            //            case 0x1e:
            //                Debug.Write("{TextREF-1E}");
            //                break;

            //            case 0xa3:
            //                Debug.Write("{End-A3}");
            //                Debug.Write("{Anim}");
            //                Debug.Write(string.Format("{0:x2} ", Get(++i)));
            //                i += 2;
            //                break;

            //            case 0xa8:
            //                Debug.Write("{Visibility-A8}");
            //                switch (Get(++i))
            //                {
            //                    case 0x02:
            //                        Debug.Write("{Hide-02}");
            //                        break;

            //                    case 0x03:
            //                        Debug.Write("{Show-03}");
            //                        break;
            //                    default:
            //                        Debug.Write(string.Format("{0:x2}", b));
            //                        break;
            //                }
            //                Debug.Write("{Anim}");
            //                Debug.Write(string.Format("{0:x2} ", Get(++i)));
            //                break;
            //            default:
            //                Debug.Write(string.Format(" {0:x2}", b));
            //                break;
            //        }
            //    }
            //    Debug.Write($"   ({ob.Value.Length} length)\n");
            //}
        }

        public int GetId => id;

        public int altid { get; private set; }
        public int id { get; private set; }
        public EntityType entityType { get; private set; }
        public string fileName { get; private set; }
        public static Sections Flags { get; private set; }
    }
}