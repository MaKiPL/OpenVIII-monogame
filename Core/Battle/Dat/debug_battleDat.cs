using OpenVIII.Battle;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII.Battle.Dat
{
    public class DebugBattleDat
        {
            private readonly byte[] _buffer;
            private const float BaseLineMaxYFilter = 10f;
            public const float ScaleHelper = 2048.0f;
            private const float Degrees = 360f;



            public DatFile DatFile { get; }





            /// <summary>
            /// Skeleton data section
            /// </summary>
            /// <param name="start"></param>
            private void ReadSection1(uint start)
            {
                br.BaseStream.Seek(start, SeekOrigin.Begin);
            #if _WINDOWS //looks like Linux Mono doesn't like marshalling structure with LPArray to Bone[]
                Skeleton = Extended.ByteArrayToStructure<Skeleton>(br.ReadBytes(16));
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
                Skeleton.bones = new Bone[Skeleton.cBones];
                for (int i = 0; i < Skeleton.cBones; i++)
                {
                    Skeleton.bones[i] = Extended.ByteArrayToStructure<Bone>(br.ReadBytes(44));
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                }
                //string debugBuffer = string.Empty;
                //for (int i = 0; i< skeleton.cBones; i++)
                //    debugBuffer += $"{i}|{skeleton.bones[i].parentId}|{skeleton.bones[i].boneSize}|{skeleton.bones[i].Size}\n";
                //Console.WriteLine(debugBuffer);
            }

            public Skeleton Skeleton;




        #region Fields

        public Information information;

        public const int Section7Size = 380;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection7(uint start)
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            information = Extended.ByteArrayToStructure<Information>(br.ReadBytes(Section7Size));
        }

        #endregion Methods



        public Geometry Geometry;

        /// <summary>
        /// Model Geometry section
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection2(uint start)
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            Geometry = new Geometry { CObjects = br.ReadUInt32() };
            Geometry.PObjects = new uint[Geometry.CObjects];
            for (int i = 0; i < Geometry.CObjects; i++)
                Geometry.PObjects[i] = br.ReadUInt32();
            Geometry.Objects = new Object[Geometry.CObjects];
            for (int i = 0; i < Geometry.CObjects; i++)
                Geometry.Objects[i] = ReadGeometryObject(start + Geometry.PObjects[i]);
            Geometry.CTotalVertices = br.ReadUInt32();
        }

        private Object ReadGeometryObject(uint start)
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            Object @object = new Object { CVertices = br.ReadUInt16() };
            @object.VertexData = new VertexData[@object.CVertices];
            if (br.BaseStream.Position + @object.CVertices * 6 >= br.BaseStream.Length)
                return @object;
            for (int n = 0; n < @object.CVertices; n++)
            {
                @object.VertexData[n].BoneId = br.ReadUInt16();
                @object.VertexData[n].CVertices = br.ReadUInt16();
                @object.VertexData[n].Vertices = new Vector3[@object.VertexData[n].CVertices];
                for (int i = 0; i < @object.VertexData[n].CVertices; i++)
                    @object.VertexData[n].Vertices[i] = Extended.ByteArrayToStructure<Vertex>(br.ReadBytes(6));
            }
            br.BaseStream.Seek(4 - (br.BaseStream.Position % 4 == 0 ? 4 : br.BaseStream.Position % 4), SeekOrigin.Current);
            @object.CTriangles = br.ReadUInt16();
            @object.CQuads = br.ReadUInt16();
            @object.Padding = br.ReadUInt64();
            @object.Triangles = new Triangle[@object.CTriangles];
            if (@object.CTriangles == 0 && @object.CQuads == 0)
                return @object;
            @object.Quads = new Quad[@object.CQuads];
            for (int i = 0; i < @object.CTriangles; i++)
                @object.Triangles[i] = Triangle.Create(br);//Extended.ByteArrayToStructure<Triangle>(br.ReadBytes(16));
            for (int i = 0; i < @object.CQuads; i++)
                @object.Quads[i] = Quad.Create(br);// Extended.ByteArrayToStructure<Quad>(br.ReadBytes(20));

            return @object;
        }

        

        public Vector3 IndicatorPoint(Vector3 translationPosition)
        {
            if (_offsetYLow < 0)
                translationPosition.Y -= _offsetYLow;
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
        /// an animation frame from animation BattleID. You should pass incrementing frame and reset to 0
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

            Object obj = Geometry.Objects[objectId];
            int i = 0;

            List<VectorBoneGRP> verts = GetVertices(obj, frame, nextFrame, step);
            //float minY = vertices.Min(x => x.Y);
            //Vector2 HLPTS = FindLowHighPoints(translationPosition, rotation, frame, nextFrame, step);

            byte[] texturePointers = new byte[obj.CTriangles + obj.CQuads * 2];
            List<VertexPositionTexture> vpt = new List<VertexPositionTexture>(texturePointers.Length * 3);

            if (objectId == 0)
            {
                Battle.AnimationSystem _animationSystem = animationSystem;
                AnimationYOffset lastoffsets = _animationYOffsets?.First(x => x.ID == _animationSystem.LastAnimationId && x.Frame == lastAnimationFrame) ?? default;
                AnimationYOffset nextoffsets = _animationYOffsets?.First(x => x.ID == _animationSystem.AnimationId && x.Frame == _animationSystem.AnimationFrame) ?? default;
                _offsetYLow = MathHelper.Lerp(lastoffsets.LowY, nextoffsets.LowY, (float)step);
                _indicatorPoint.X = MathHelper.Lerp(lastoffsets.MidX, nextoffsets.MidX, (float)step);
                _indicatorPoint.Y = MathHelper.Lerp(lastoffsets.HighY, nextoffsets.HighY, (float)step);
                _indicatorPoint.Z = MathHelper.Lerp(lastoffsets.MidZ, nextoffsets.MidZ, (float)step);
                // Move All Y axis down to 0 based on Lowest Y axis in Animation BattleID 0.
                if (OffsetY < 0)
                {
                    translationPosition.Y += OffsetY;
                }
                // If any Y axis readings are lower than 0 in Animation BattleID >0. Bring it up to zero.
            }

            //Triangle parsing
            for (; i < obj.CTriangles; i++)
            {
                Texture2D preVarTex = (Texture2D)textures.textures[obj.Triangles[i].TextureIndex];
                vpt.AddRange(obj.Triangles[i].GenerateVPT(verts, rotation, translationPosition, preVarTex));
                texturePointers[i] = obj.Triangles[i].TextureIndex;
            }

            //Quad parsing
            for (i = 0; i < obj.CQuads; i++)
            {
                Texture2D preVarTex = (Texture2D)textures.textures[obj.Quads[i].TextureIndex];
                vpt.AddRange(obj.Quads[i].GenerateVPT(verts, rotation, translationPosition, preVarTex));
                texturePointers[obj.CTriangles + i * 2] = obj.Quads[i].TextureIndex;
                texturePointers[obj.CTriangles + i * 2 + 1] = obj.Quads[i].TextureIndex;
            }

            return new VertexPositionTexturePointersGRP(vpt.ToArray(), texturePointers);
        }

        private List<VectorBoneGRP> GetVertices(Object @object, AnimationFrame frame, AnimationFrame nextFrame, double step) => @object.VertexData.SelectMany(vertexdata => vertexdata.Vertices.Select(vertex => CalculateFrame(new VectorBoneGRP(vertex, vertexdata.BoneId), frame, nextFrame, step))).ToList();

        private Vector4 FindLowHighPoints(Vector3 translationPosition, Quaternion rotation, AnimationFrame frame, AnimationFrame nextFrame, double step)
        {
            List<VectorBoneGRP> vertices =
                Geometry.Objects.SelectMany(@object => GetVertices(@object, frame, nextFrame, step)).ToList();
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
                r = Vector3.Transform(r, Matrix.CreateScale(Skeleton.GetScale));
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
                    animHeader.animations[i].animationFrames[n].boneMatrix = new Matrix[Skeleton.cBones];
                    animHeader.animations[i].animationFrames[n].bonesVectorRotations = new Vector3[Skeleton.cBones];

                    //Step 2. We read the position and we need to store the bones rotations or save base rotation if frame==0

                    for (int k = 0; k < Skeleton.cBones; k++) //bones iterator
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
                    for (int k = 0; k < Skeleton.cBones; k++)
                    {
                        Vector3 boneRotation = animHeader.animations[i].animationFrames[n].bonesVectorRotations[k];
                        boneRotation = Extended.S16VectorToFloat(boneRotation); //we had vector3 containing direct copy of short to float, now we need them in real floating point values
                        boneRotation *= Degrees; //bone rotations are in 360 scope
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

                        if (Skeleton.bones[k].parentId == 0xFFFF) //if parentId is 0xFFFF then the current bone is core aka bone0
                        {
                            MatrixZ.M41 = -animHeader.animations[i].animationFrames[n].Position.X;
                            MatrixZ.M42 = -animHeader.animations[i].animationFrames[n].Position.Y; //up/down
                            MatrixZ.M43 = animHeader.animations[i].animationFrames[n].Position.Z;
                            MatrixZ.M44 = 1;
                        }
                        else
                        {
                            Matrix parentBone = animHeader.animations[i].animationFrames[n].boneMatrix[Skeleton.bones[k].parentId]; //gets the parent bone
                            MatrixZ.M43 = Skeleton.bones[Skeleton.bones[k].parentId].Size;
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
            //populate the tim Count;
            textures = new Textures() { cTims = br.ReadUInt32() };
            //create arrays per Count.
            textures.pTims = new uint[textures.cTims];
            textures.textures = new TextureHandler[textures.cTims];
            //Read pointers into array
            for (int i = 0; i < textures.cTims; i++)
                textures.pTims[i] = br.ReadUInt32();
            //Read EOF
            textures.Eof = br.ReadUInt32();
            //Read TIM -> TextureHandler into array
            for (int i = 0; i < textures.cTims; i++)
                if (_buffer[start + textures.pTims[i]] == 0x10)
                {
                    TIM2 tm = new TIM2(_buffer, start + textures.pTims[i]); //broken
                    textures.textures[i] = TextureHandler.Create($"{FileName}_{i/*.ToString("D2")*/}", tm, 0);// tm.GetTexture(0);
                }
                else
                    Debug.WriteLine($"DEBUG: {this}.{this.ID}.{start + textures.pTims[i]} :: Not a tim file!");
        }

        public Textures textures;
        private BinaryReader br;
        //private float lowpoint;



        public DebugBattleDat(int fileId, EntityType entityType, int additionalFileId = -1, DebugBattleDat skeletonReference = null, Sections flags = Sections.All)
        {
            Flags = flags;


            ID = fileId;
            AltID = additionalFileId;

            Memory.Log.WriteLine($"{nameof(DebugBattleDat)}::{nameof(Load)}: Creating new BattleDat with {fileId},{entityType},{additionalFileId}");
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);
            char et = entityType == EntityType.Weapon ? 'w' : entityType == EntityType.Character ? 'c' : default;
            string fileName = entityType == EntityType.Monster ? $"c0m{ID:D03}" :
                entityType == EntityType.Character || entityType == EntityType.Weapon ? $"d{fileId:x}{et}{additionalFileId:D03}"
                : string.Empty;
            this.entityType = entityType;
            if (string.IsNullOrEmpty(fileName))
                return;
            string path;
            string search = "";
            if (et != default)
            {
                search = $"d{fileId:x}{et}";
                IEnumerable<string> test = aw.GetListOfFiles().Where(x => x.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                path = test.FirstOrDefault(x => x.ToLower().Contains(fileName));

                if (string.IsNullOrWhiteSpace(path) && test.Any() && entityType == EntityType.Character)
                    path = test.First();
            }
            else path = aw.GetListOfFiles().FirstOrDefault(x => x.ToLower().Contains(fileName));
            //Debug.Assert("c:\\ff8\\data\\ger\\battle\\d0w007.dat" != path);
            FileName = fileName;
            if (!string.IsNullOrWhiteSpace(path))
                _buffer = aw.GetBinaryFile(path);
            if (_buffer == null || _buffer.Length < 0)
            {
                Memory.Log.WriteLine($"Search String: {search} Not Found skipping {entityType}; So resulting file buffer is null.");
                return;
            }
            ExportFile();
            using (br = new BinaryReader(new MemoryStream(_buffer)))
            {
                DatFile = DatFile.CreateInstance(br);
            }

            LoadFile(skeletonReference);
            FindAllLowHighPoints();
        }

        /// <summary>
        /// Creates new instance of DAT class that provides every sections parsed into structs and
        /// helper functions for renderer
        /// </summary>
        /// <param name="fileId">This number is used in c0m(fileId) or d(fileId)cXYZ</param>
        /// <param name="entityType">Supply Monster, character or weapon (0,1,2)</param>
        /// <param name="additionalFileId">Used only in character or weapon to supply for d(fileId)[c/w](additionalFileId)</param>
        public static DebugBattleDat Load(int fileId, EntityType entityType, int additionalFileId = -1, DebugBattleDat skeletonReference = null, Sections flags = Sections.All)
            => new DebugBattleDat(fileId, entityType, additionalFileId, skeletonReference, flags);

        private void FindAllLowHighPoints()
        {
            if (entityType != EntityType.Character && entityType != EntityType.Monster) return;
            // Get baseline from running function on only Animation 0;
            if (animHeader.animations == null)
                return;
            List<Vector4> baseline = animHeader.animations[0].animationFrames.Select(x => FindLowHighPoints(Vector3.Zero, Quaternion.Identity, x, x, 0f)).ToList();
            //X is lowY, Y is high Y, Z is mid x, W is mid z
            (float baselineLowY, float baselineHighY) = (baseline.Min(x => x.X), baseline.Max(x => x.Y));
            OffsetY = 0f;
            if (Math.Abs(baselineLowY) < BaseLineMaxYFilter)
            {
                OffsetY -= baselineLowY;
                baselineHighY += OffsetY;
            }
            // Default indicator point
            _indicatorPoint = new Vector3(0, baselineHighY, 0);
            // Need to add this later to bring baseline low to 0.
            //OffsetY = offsetY;
            // Brings all Y values less than baseline low to baseline low
            _animationYOffsets = animHeader.animations.SelectMany((animation, animationID) =>
                animation.animationFrames.Select((animationFrame, animationFrameNumber) =>
                    new AnimationYOffset(animationID, animationFrameNumber, FindLowHighPoints(OffsetYVector, Quaternion.Identity, animationFrame, animationFrame, 0f)))).ToList();
        }

        private List<AnimationYOffset> _animationYOffsets;
        private Vector3 _indicatorPoint;
        private float _offsetYLow;

        private float OffsetY { get; set; }

        private Vector3 OffsetYVector => new Vector3(0f, OffsetY, 0f);

        

        private void ExportFile()
        {
#if _WINDOWS && DEBUG
            try
            {
                const string target = @"d:\";
                if (!Directory.Exists(target)) return;
                DriveInfo[] drive = DriveInfo.GetDrives().Where(x => x.Name.IndexOf(Path.GetPathRoot(target), StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                DirectoryInfo di = new DirectoryInfo(target);
                if (!di.Attributes.HasFlag(FileAttributes.ReadOnly) && drive.Count() == 1 && drive[0].DriveType == DriveType.Fixed)
                    Extended.DumpBuffer(_buffer, Path.Combine(target, "out.dat"));
            }
            catch (IOException)
            {
            }
#endif
        }

        private void LoadFile(DebugBattleDat skeletonReference)
        {
            using (br = new BinaryReader(new MemoryStream(_buffer)))
            {
                if (br.BaseStream.Length - br.BaseStream.Position < 4) return;
                switch (entityType)
                {
                    case EntityType.Monster:
                        LoadMonster();
                        return;

                    case EntityType.Character:
                        LoadCharacter();
                        return;

                    case EntityType.Weapon:
                        LoadWeapon(skeletonReference);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        private void LoadMonster()
        {
            if (ID == 127)
            {
                // per wiki 127 only have 7 & 8
                if (Flags.HasFlag(Sections.Information))
                    ReadSection7(DatFile.PSections[0]);
                //if (Flags.HasFlag(Sections.Scripts))
                //    ReadSection8(datFile.pSections[7]);
                return;
            }
            if (Flags.HasFlag(Sections.Skeleton))
                ReadSection1(DatFile.PSections[0]);
            if (Flags.HasFlag(Sections.ModelAnimation))
                ReadSection3(DatFile.PSections[2]); // animation data
            if (Flags.HasFlag(Sections.ModelGeometry))
                ReadSection2(DatFile.PSections[1]);

            //if (Flags.HasFlag(Sections.Section4_Unknown))
            //  ReadSection4(datFile.pSections[3]);
            if (Flags.HasFlag(Sections.AnimationSequences))
                ReadSection5(DatFile.PSections[4], DatFile.PSections[5]);
            //if (Flags.HasFlag(Sections.Section6_Unknown))
            //  ReadSection6(datFile.pSections[5]);
            if (Flags.HasFlag(Sections.Information))
                ReadSection7(DatFile.PSections[6]);
            //if (Flags.HasFlag(Sections.Scripts))
            //ReadSection8(datFile.pSections[7]); // battle scripts/ai

            //if (Flags.HasFlag(Sections.Sounds))
            //    ReadSection9(datFile.pSections[8], datFile.pSections[9]); //AKAO sounds
            //if (Flags.HasFlag(Sections.Sounds_Unknown))
            //  ReadSection10(datFile.pSections[9], datFile.pSections[10], br, fileName);

            if (Flags.HasFlag(Sections.Textures))
                ReadSection11(DatFile.PSections[10]);
        }

        private void LoadCharacter()
        {
            if (Flags.HasFlag(Sections.Skeleton))
                ReadSection1(DatFile.PSections[0]);
            if (Flags.HasFlag(Sections.ModelAnimation))
                ReadSection3(DatFile.PSections[2]);
            if (Flags.HasFlag(Sections.ModelGeometry))
                ReadSection2(DatFile.PSections[1]);
            if (ID == 7 && entityType == EntityType.Character) // edna has no weapons.
            {
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(DatFile.PSections[8]);
                if (Flags.HasFlag(Sections.AnimationSequences))
                    ReadSection5(DatFile.PSections[5], DatFile.PSections[6]);
            }
            else if (Flags.HasFlag(Sections.Textures))
                ReadSection11(DatFile.PSections[5]);
        }

        private void LoadWeapon(DebugBattleDat skeletonReference)
        {
            if (ID != 1 && ID != 9)
            {
                if (Flags.HasFlag(Sections.Skeleton))
                    ReadSection1(DatFile.PSections[0]);
                if (Flags.HasFlag(Sections.ModelAnimation))
                    ReadSection3(DatFile.PSections[2]);
                if (Flags.HasFlag(Sections.ModelGeometry))
                    ReadSection2(DatFile.PSections[1]);
                if (Flags.HasFlag(Sections.AnimationSequences))
                    ReadSection5(DatFile.PSections[3], DatFile.PSections[4]);
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(DatFile.PSections[6]);
            }
            else if (skeletonReference != null)
            {
                Skeleton = skeletonReference.Skeleton;
                animHeader = skeletonReference.animHeader;
                if (Flags.HasFlag(Sections.ModelGeometry))
                    ReadSection2(DatFile.PSections[0]);
                if (Flags.HasFlag(Sections.AnimationSequences))
                    ReadSection5(DatFile.PSections[1], DatFile.PSections[2]);
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(DatFile.PSections[4]);
            }
        }

        public List<AnimationSequence> Sequences { get; set; }

       

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
                    AnimationSequence sequence = new AnimationSequence { ID = offsetindexed.index, Offset = offsetindexed.value, Data = br.ReadBytes(checked((int)(localend - offset))) };
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

        public int GetId => ID;

        public int AltID { get; }
        public int ID { get; }
        public EntityType entityType { get; }
        public string FileName { get; }
        public Sections Flags { get; }
    }
}