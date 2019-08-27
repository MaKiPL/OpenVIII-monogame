using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    public class Debug_battleDat
    {
        int id;
        readonly EntityType entityType;
        byte[] buffer;

        public const float SCALEHELPER = 2048.0f;
        private const float DEGREES = 360f;

        public struct DatFile
        {
            public uint cSections;
            public uint[] pSections;
            public uint eof;
        }

        public DatFile datFile;        

        #region section 1 Skeleton
        [StructLayout(LayoutKind.Sequential,Pack = 1,Size =16)]
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

            public Vector3 GetScale => new Vector3(scale/SCALEHELPER*12, scale/SCALEHELPER*12, scale/SCALEHELPER*12);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 48)]
        public struct Bone
        {
            public ushort parentId;
            public short boneSize;
            private short unk1; //360
            private short unk2; //360
            private short unk3; //360
            private short unk4;
            private short unk5;
            private short unk6;
            [MarshalAs(UnmanagedType.ByValArray,SizeConst = 28 )]
            public byte[] Unk;

            public float Size { get => boneSize / SCALEHELPER; }
            public float Unk1 { get => unk1 / 4096.0f * 360.0f; } //rotX
            public float Unk2 { get => unk2 / 4096.0f * 360.0f; } //rotY
            public float Unk3 { get => unk3 / 4096.0f * 360.0f; } //rotZ
            public float Unk4 { get => unk4 / 4096.0f; } //unk1v
            public float Unk5 { get => unk5 / 4096.0f; } //unk2v
            public float Unk6 { get => unk6 / 4096.0f; } //unk3v
        }

        /// <summary>
        /// Skeleton data section
        /// </summary>
        /// <param name="v"></param>
        /// <param name="ms"></param>
        /// <param name="br"></param>
        private void ReadSection1(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
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
                skeleton.bones[i] = Extended.ByteArrayToStructure<Bone>(br.ReadBytes(48));
            //string debugBuffer = string.Empty;
            //for (int i = 0; i< skeleton.cBones; i++)
            //    debugBuffer += $"{i}|{skeleton.bones[i].parentId}|{skeleton.bones[i].boneSize}|{skeleton.bones[i].Size}\n";
            //Console.WriteLine(debugBuffer);
            return;
        }

        public Skeleton skeleton;

#endregion

#region section 2 Geometry
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
            public short x;
            public short y;
            public short z;

            public Vector3 GetVector => new Vector3(-x/ SCALEHELPER, -z/SCALEHELPER, -y/SCALEHELPER);
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
            public byte textureIndex { get => (byte)((texUnk >> 6) & 0b111); }
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
            public byte textureIndex { get => (byte)((texUnk >> 6) & 0b111); }
        }

        [StructLayout(LayoutKind.Sequential, Pack =1,Size =2)]
        public struct UV
        {
            public byte U;
            public byte V;

            public float U1(float h=128f) { return U / h; }
            public float V1(float w=128f) { return V > 128 ? //if bigger than 128, then multitexture index to odd
                    (V - 128f)/w 
                    : w==32 ?  //if equals 32, then it's weapon texture and should be in range of 96-128
                        (V-96)/w 
                        : V/w; } //if none of these cases, then divide by resolution;

            public override string ToString()
            {
                return $"{U};{U1()};{V};{V1()}";
            }
        }

        public Geometry geometry;

        /// <summary>
        /// Geometry section
        /// </summary>
        /// <param name="v"></param>
        /// <param name="ms"></param>
        /// <param name="br"></param>
        private void ReadSection2(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            geometry = new Geometry { cObjects = br.ReadUInt32() };
            geometry.pObjects = new uint[geometry.cObjects];
            for (int i = 0; i < geometry.cObjects; i++)
                geometry.pObjects[i] = br.ReadUInt32();
            geometry.objects = new Object[geometry.cObjects];
            for (int i = 0; i < geometry.cObjects; i++)
                geometry.objects[i] = ReadGeometryObject(v+geometry.pObjects[i], ms, br);
            geometry.cTotalVert = br.ReadUInt32();
        }

        private Object ReadGeometryObject(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            Object @object = new Object { cVertices = br.ReadUInt16() };
            @object.verticeData = new VerticeData[@object.cVertices];
            if (ms.Position + @object.cVertices * 6 >= ms.Length)
                return @object;
            for (int n = 0; n < @object.cVertices; n++){
                @object.verticeData[n].boneId = br.ReadUInt16();
                @object.verticeData[n].cVertices = br.ReadUInt16();
                @object.verticeData[n].vertices = new Vertex[@object.verticeData[n].cVertices];
                for (int i = 0; i < @object.verticeData[n].cVertices; i++)
                    @object.verticeData[n].vertices[i] = Extended.ByteArrayToStructure<Vertex>(br.ReadBytes(6));}
            ms.Seek(4-(ms.Position%4 == 0 ? 4 : ms.Position%4), SeekOrigin.Current);
            @object.cTriangles = br.ReadUInt16();
            @object.cQuads = br.ReadUInt16();
            @object.padding = br.ReadUInt64();
            @object.triangles = new Triangle[@object.cTriangles];
            if (@object.cTriangles == 0 && @object.cQuads == 0)
                return @object;
            @object.quads = new Quad[@object.cQuads];
            for (int i = 0; i < @object.cTriangles; i++)
                @object.triangles[i] = Extended.ByteArrayToStructure<Triangle>(br.ReadBytes(16));
            for (int i = 0; i < @object.cQuads; i++)
                @object.quads[i] = Extended.ByteArrayToStructure<Quad>(br.ReadBytes(20));

            return @object;
        }

        /// <summary>
        /// This method returns geometry data AFTER animation matrix translations, local position/rotation translations. This is the final step of calculation. 
        /// This data should be used only by Renderer. Any translations/vertices manipulation should happen inside this method or earlier
        /// </summary>
        /// <param name="objectId">Monsters can have more than one object. Treat it like multi-model geometry. They are all needed to build whole model</param>
        /// <param name="position">a Vector3 to set global position</param>
        /// <param name="rotation">a Quaternion to set the correct rotation. 1=90, 2=180 ... </param>
        /// <param name="animationId">an animation pointer. Animation 0 is always idle</param>
        /// <param name="animationFrame">an animation frame from animation id. You should pass incrementing frame and reset to 0 when frameCount max is hit</param>
        /// <param name="step">FEATURE: This float (0.0 - 1.0) is used in Linear interpolation in animation frames blending. 0.0 means frameN, 1.0 means FrameN+1. Usually this should be a result of deltaTime to see if computer is capable of rendering smooth animations rather than constant 15 FPS</param>
        /// <returns></returns>
        public Tuple<VertexPositionTexture[],byte[]> GetVertexPositions(int objectId, Vector3 position,Quaternion rotation, int animationId, int animationFrame, float step)
        {
            Object obj = geometry.objects[objectId];
            if (animationFrame >= animHeader.animations[animationId].animationFrames.Length || animationFrame<0)
                animationFrame = 0;
            AnimationFrame frame = animHeader.animations[animationId].animationFrames[animationFrame];
            AnimationFrame nextFrame = animationFrame +1 >= animHeader.animations[animationId].animationFrames.Length
                ? animHeader.animations[animationId].animationFrames[0]
                : animHeader.animations[animationId].animationFrames[animationFrame+1];
            List<VertexPositionTexture> vpt = new List<VertexPositionTexture>();
            List<Tuple<Vector3, int>> verts = new List<Tuple<Vector3, int>>();  

            int i = 0;
            foreach (var a in obj.verticeData)
                foreach (var b in a.vertices)
                    verts.Add(CalculateFrame(new Tuple<Vector3, int>(b.GetVector, a.boneId),frame,nextFrame, step));
            byte[] texturePointers = new byte[obj.cTriangles + obj.cQuads*2];
            Vector3 translationPosition = position /*+ Vector3.SmoothStep(frame.Position, nextFrame.Position, step) + snapToGround*/;

            //Triangle parsing
            for (;i<obj.cTriangles; i++ )
            {
                Vector3 VerticeDataC = TranslateVertex(verts[obj.triangles[i].C1].Item1, rotation, translationPosition);
                Vector3 VerticeDataA = TranslateVertex(verts[obj.triangles[i].A1].Item1, rotation, translationPosition);
                Vector3 VerticeDataB = TranslateVertex(verts[obj.triangles[i].B1].Item1, rotation, translationPosition);

                var prevarTexT = textures.textures[obj.triangles[i].textureIndex];
                vpt.Add(new VertexPositionTexture(VerticeDataC, new Vector2(obj.triangles[i].vta.U1(prevarTexT.Width), obj.triangles[i].vta.V1(prevarTexT.Height))));
                vpt.Add(new VertexPositionTexture(VerticeDataA, new Vector2(obj.triangles[i].vtb.U1(prevarTexT.Width), obj.triangles[i].vtb.V1(prevarTexT.Height))));
                vpt.Add(new VertexPositionTexture(VerticeDataB, new Vector2(obj.triangles[i].vtc.U1(prevarTexT.Width), obj.triangles[i].vtc.V1(prevarTexT.Height))));
                texturePointers[i] = obj.triangles[i].textureIndex;
            }

            //Quad parsing
            for (i = 0; i < obj.cQuads; i++)
            {
                
                Vector3 VerticeDataA = TranslateVertex(verts[obj.quads[i].A1].Item1, rotation, translationPosition);
                Vector3 VerticeDataB = TranslateVertex(verts[obj.quads[i].B1].Item1, rotation, translationPosition);
                Vector3 VerticeDataC = TranslateVertex(verts[obj.quads[i].C1].Item1, rotation, translationPosition);
                Vector3 VerticeDataD = TranslateVertex(verts[obj.quads[i].D1].Item1, rotation, translationPosition);

                var preVarTex = textures.textures[obj.quads[i].textureIndex];
                vpt.Add(new VertexPositionTexture(VerticeDataA, new Vector2(obj.quads[i].vta.U1(preVarTex.Width), obj.quads[i].vta.V1(preVarTex.Height))));
                vpt.Add(new VertexPositionTexture(VerticeDataB, new Vector2(obj.quads[i].vtb.U1(preVarTex.Width), obj.quads[i].vtb.V1(preVarTex.Height))));
                vpt.Add(new VertexPositionTexture(VerticeDataD, new Vector2(obj.quads[i].vtd.U1(preVarTex.Width), obj.quads[i].vtd.V1(preVarTex.Height))));

                vpt.Add(new VertexPositionTexture(VerticeDataA, new Vector2(obj.quads[i].vta.U1(preVarTex.Width), obj.quads[i].vta.V1(preVarTex.Height))));
                vpt.Add(new VertexPositionTexture(VerticeDataC, new Vector2(obj.quads[i].vtc.U1(preVarTex.Width), obj.quads[i].vtc.V1(preVarTex.Height))));
                vpt.Add(new VertexPositionTexture(VerticeDataD, new Vector2(obj.quads[i].vtd.U1(preVarTex.Width), obj.quads[i].vtd.V1(preVarTex.Height))));

                texturePointers[obj.cTriangles+i*2] = obj.quads[i].textureIndex;
                texturePointers[obj.cTriangles + i * 2+1] = obj.quads[i].textureIndex;
            }

            return new Tuple<VertexPositionTexture[], byte[]>(vpt.ToArray(), texturePointers);
        }

        Vector3 TranslateVertex(Vector3 vertex, Quaternion rotation, Vector3 localTranslate)
        {
            Vector3 verticeData = vertex;
            verticeData = Vector3.Transform(verticeData, Matrix.CreateFromQuaternion(rotation));
            verticeData = Vector3.Transform(verticeData, Matrix.CreateTranslation(localTranslate));
            return verticeData;
        }

        /// <summary>
        /// Complex function that provides linear interpolation between two matrices of actual to-render animation frame and next frame data for blending
        /// </summary>
        /// <param name="tuple">the tuple that contains vertex and bone ident</param>
        /// <param name="frame">current animation frame to render</param>
        /// <param name="nextFrame">animation frame to render that is AFTER the actual one. If last frame, then usually 0 is the 'next' frame</param>
        /// <param name="step">step is variable used to determinate the progress for linear interpolation. I.e. 0 for current frame and 1 for next frame; 0.5 for blend of two frames</param>
        /// <returns></returns>
        private Tuple<Vector3, int> CalculateFrame(Tuple<Vector3, int> tuple, AnimationFrame frame,AnimationFrame nextFrame, float step)
        {
            Matrix matrix = frame.boneMatrix[tuple.Item2]; //get's bone matrix
            Vector3 rootFramePos = new Vector3(
                matrix.M11 * tuple.Item1.X + matrix.M41 + matrix.M12 * tuple.Item1.Z + matrix.M13 * -tuple.Item1.Y,
                matrix.M21 * tuple.Item1.X + matrix.M42 + matrix.M22 * tuple.Item1.Z + matrix.M23 * -tuple.Item1.Y,
                matrix.M31 * tuple.Item1.X + matrix.M43 + matrix.M32 * tuple.Item1.Z + matrix.M33 * -tuple.Item1.Y);
            matrix = nextFrame.boneMatrix[tuple.Item2];
            Vector3 nextFramePos = new Vector3(
                matrix.M11 * tuple.Item1.X + matrix.M41 + matrix.M12 * tuple.Item1.Z + matrix.M13 * -tuple.Item1.Y,
                matrix.M21 * tuple.Item1.X + matrix.M42 + matrix.M22 * tuple.Item1.Z + matrix.M23 * -tuple.Item1.Y,
                matrix.M31 * tuple.Item1.X + matrix.M43 + matrix.M32 * tuple.Item1.Z + matrix.M33 * -tuple.Item1.Y);
            rootFramePos = Vector3.Transform(rootFramePos, Matrix.CreateScale(skeleton.GetScale));
            nextFramePos = Vector3.Transform(nextFramePos, Matrix.CreateScale(skeleton.GetScale));
            rootFramePos = Vector3.SmoothStep(rootFramePos, nextFramePos, step);
            return new Tuple<Vector3, int>(rootFramePos, tuple.Item2);
        }
        #endregion

        #region section 3 Animation
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
        /// Animation section
        /// </summary>
        /// <param name="v"></param>
        /// <param name="ms"></param>
        /// <param name="br"></param>
        private void ReadSection3(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            animHeader = new AnimationData() {cAnimations = br.ReadUInt32() };
            animHeader.pAnimations = new uint[animHeader.cAnimations];
            for (int i = 0; i < animHeader.cAnimations; i++)
            {
                animHeader.pAnimations[i] = br.ReadUInt32();
                //Console.WriteLine($"{i}|{animHeader.pAnimations[i]}");
            }
            animHeader.animations = new Animation[animHeader.cAnimations];
            for (int i = 0; i < animHeader.cAnimations; i++) //animation
            { 
                ms.Seek(v + animHeader.pAnimations[i], SeekOrigin.Begin); //Get to pointer of animation Id
                animHeader.animations[i] = new Animation() {cFrames = br.ReadByte() }; //Create new animation with cFrames frames
                animHeader.animations[i].animationFrames = new AnimationFrame[animHeader.animations[i].cFrames];
                ExtapathyExtended.BitReader bitReader = new ExtapathyExtended.BitReader(ms);
                for(int n = 0; n<animHeader.animations[i].cFrames; n++) //frames
                {
                    //Step 1. It starts with bone0.position. Let's read that into AnimationFrames[animId]- it's only one position per frame

                    float x = bitReader.ReadPositionType() * .01f;
                    float y = bitReader.ReadPositionType() * .01f;
                    float z = bitReader.ReadPositionType() * .01f;
                    animHeader.animations[i].animationFrames[n] = n == 0
                        ? new AnimationFrame()
                        {Position = new Vector3(x,y,z)}
                        : new AnimationFrame()
                        {Position = new Vector3(
                    animHeader.animations[i].animationFrames[n - 1].Position.X + x,
                    animHeader.animations[i].animationFrames[n - 1].Position.Y + y,
                    animHeader.animations[i].animationFrames[n - 1].Position.Z + z)
                        };
                    byte ModeTest =  (byte)bitReader.ReadBits(1); //used to determine if additional info is required
                    if(i == 0 && n == 0)
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
                    for(int k = 0; k<skeleton.cBones; k++)
                    {
                        Vector3 boneRotation = animHeader.animations[i].animationFrames[n].bonesVectorRotations[k];
                        boneRotation = Extended.S16VectorToFloat(boneRotation); //we had vector3 containing direct copy of short to float, now we need them in real floating point values
                        boneRotation *= DEGREES; //bone rotations are in 360 scope
                        Matrix xRot = Extended.GetRotationMatrixX(-boneRotation.X);
                        Matrix yRot = Extended.GetRotationMatrixY(-boneRotation.Y);
                        Matrix zRot = Extended.GetRotationMatrixZ(-boneRotation.Z);
                        var MatrixZ = Extended.MatrixMultiply_transpose(yRot, xRot);
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
        /// Some enemies use additional information that is saved for bone AFTER rotation types. We are still not sure what it does as enemy works without it
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
        #endregion

        #region section 7 Information
        private const int Section7Size = 380;

        [StructLayout(LayoutKind.Sequential, Pack =1, Size =Section7Size)]
        public struct Information
        {
            [MarshalAs(UnmanagedType.ByValArray,SizeConst =24)]
            private byte[] monsterName;
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

            public FF8String GetNameNormal => monsterName;
        }

        public struct Abilities
        {
            public byte kernelId; //0x2 magic, 0x4 item, 0x8 mosterAbility;
            public byte unk;
            public ushort abilityId;

        }

        private void ReadSection7(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            information = Extended.ByteArrayToStructure<Information>(br.ReadBytes(Section7Size));
        }

        public Information information;
#endregion

#region section 11 Textures
        public struct Textures
        {
            public uint cTims;
            public uint[] pTims;
            public uint Eof;
            public Texture2D[] textures;
        }

        private void ReadSection11(uint v, MemoryStream ms, BinaryReader br)
        {
            ms.Seek(v, SeekOrigin.Begin);
            textures = new Textures() { cTims = br.ReadUInt32() };
            textures.pTims = new uint[textures.cTims];
            for (int i = 0; i < textures.cTims; i++)
                textures.pTims[i] = br.ReadUInt32();
            textures.Eof = br.ReadUInt32();
            textures.textures = new Texture2D[textures.cTims];
            for(int i = 0; i<textures.cTims; i++)
            {
                ms.Seek(v + textures.pTims[i], SeekOrigin.Begin);
                TIM2 tm = new TIM2(buffer, (uint)ms.Position); //broken
                textures.textures[i] = tm.GetTexture(0, true);
            }
        }
        public Textures textures;
#endregion


        public enum EntityType
        {
            Monster,
            Character,
            Weapon
        };
        
        /// <summary>
        /// Creates new instance of DAT class that provides every sections parsed into structs and helper functions for renderer
        /// </summary>
        /// <param name="fileId">This number is used in c0m(fileId) or d(fileId)cXYZ</param>
        /// <param name="entityType">Supply Monster, character or weapon (0,1,2)</param>
        /// <param name="additionalFileId">Used only in character or weapon to supply for d(fileId)[c/w](additionalFileId)</param>
        public Debug_battleDat(int fileId, EntityType entityType, int additionalFileId = -1, Debug_battleDat skeletonReference = null)
        {
            id = fileId;
            Console.WriteLine($"DEBUG: Creating new BattleDat with {fileId},{entityType},{additionalFileId}");
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string fileName = entityType == EntityType.Monster ? $"c0m{id.ToString("D03")}" :
                entityType == EntityType.Character ? $"d{fileId.ToString("x")}c{additionalFileId.ToString("D03")}" :
                entityType == EntityType.Weapon ? $"d{fileId.ToString("x")}w{additionalFileId.ToString("D03")}" : string.Empty;
            this.entityType = entityType;
            if (string.IsNullOrEmpty(fileName))
                return;
            string path = aw.GetListOfFiles().First(x => x.ToLower().Contains(fileName));
            buffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, path);

#if _WINDOWS && DEBUG
            try
            {
                string targetdir = @"d:\";
                if (Directory.Exists(targetdir))
                {
                    var drivei = DriveInfo.GetDrives().Where(x => x.Name.IndexOf(Path.GetPathRoot(targetdir),StringComparison.OrdinalIgnoreCase)>=0).ToArray();                    
                    var di = new DirectoryInfo(targetdir);
                    if(!di.Attributes.HasFlag(FileAttributes.ReadOnly) && drivei.Count()==1 && drivei[0].DriveType == DriveType.Fixed)
                        Extended.DumpBuffer(buffer, Path.Combine(targetdir,"out.dat"));
                }
            }
            catch(IOException)
            {
            }
#endif

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                datFile = new DatFile { cSections = br.ReadUInt32()};
                datFile.pSections = new uint[datFile.cSections];
                for (int i = 0; i < datFile.cSections; i++)
                    datFile.pSections[i] = br.ReadUInt32();
                datFile.eof = br.ReadUInt32();
                switch (this.entityType)
                {
                    case EntityType.Monster:
                        if (id == 127) return;
                        ReadSection1(datFile.pSections[0], ms, br);
                        ReadSection3(datFile.pSections[2], ms, br);
                        ReadSection2(datFile.pSections[1], ms, br);
                        //ReadSection4(datFile.pSections[3]);
                        //ReadSection5(datFile.pSections[4]);
                        //ReadSection6(datFile.pSections[5]);
                        ReadSection7(datFile.pSections[6], ms, br);
                        //ReadSection8(datFile.pSections[7]);
                        //ReadSection9(datFile.pSections[8]);
                        //ReadSection10(datFile.pSections[9]);
                        ReadSection11(datFile.pSections[10], ms, br);
                        break;
                    case EntityType.Character:
                        ReadSection1(datFile.pSections[0], ms, br);
                        ReadSection3(datFile.pSections[2], ms, br);
                        ReadSection2(datFile.pSections[1], ms, br);
                        if(fileId==7 && entityType==EntityType.Character)
                            ReadSection11(datFile.pSections[8], ms, br);
                        else
                            ReadSection11(datFile.pSections[5], ms, br);
                        break;
                    case EntityType.Weapon:
                        if (skeletonReference == null)
                        {
                            ReadSection1(datFile.pSections[0], ms, br);
                            ReadSection3(datFile.pSections[2], ms, br);
                            ReadSection2(datFile.pSections[1], ms, br);
                            ReadSection11(datFile.pSections[6], ms, br);
                        }
                        else
                        {
                            skeleton = skeletonReference.skeleton;
                            animHeader = skeletonReference.animHeader;
                            ReadSection2(datFile.pSections[0], ms, br);
                            ReadSection11(datFile.pSections[4], ms, br);
                        }
                        break;
                }
            }
        }

        public int GetId => id;
    }
}