using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    public abstract class DatFile
    {
        #region Fields

        public const float ScaleHelper = 2048.0f;
        public int Frame;
        protected const float BaseLineMaxYFilter = 10f;
        private readonly string _prefix;
        private Vector3 _indicatorPoint;

        #endregion Fields

        #region Constructors

        protected DatFile(int fileId, EntityType entityType, int additionalFileId = -1, DatFile skeletonReference = null, Sections flags = Sections.All)
        {
            Flags = flags;

            ID = fileId;
            AltID = additionalFileId;
            var middle = string.Empty;
            switch (entityType)
            {
                case EntityType.Monster:
                    _prefix = "c0m";
                    FileName = $"{_prefix}{ID:D03}";
                    break;

                case EntityType.Character:
                case EntityType.Weapon:
                    _prefix = "d";
                    middle = entityType == EntityType.Character ? "c" : "w";
                    FileName = $"{_prefix}{ID:x}{middle}{AltID:D03}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }
            Memory.Log.WriteLine($"{nameof(DatFile)} Creating new BattleDat with {fileId},{entityType},{additionalFileId}");
            var aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);

            EntityType = entityType;
            string path;
            var search = "";
            if (!string.IsNullOrWhiteSpace(middle))
            {
                search = $"d{fileId:x}{middle}";
                IEnumerable<string> test = aw.GetListOfFiles()
                    .Where(x => x.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                path = test.FirstOrDefault(x => x.ToLower().Contains(FileName));

                if (string.IsNullOrWhiteSpace(path) && test.Any() && entityType == EntityType.Character)
                    path = test.First();
            }
            else path = aw.GetListOfFiles().FirstOrDefault(x => x.ToLower().Contains(FileName));

            if (!string.IsNullOrWhiteSpace(path))
                Buffer = aw.GetBinaryFile(path);
            if (Buffer == null || Buffer.Length < 0)
            {
                Memory.Log.WriteLine($"Search String: {search} Not Found skipping {entityType}; So resulting file buffer is null.");
                return;
            }
            ExportFile();
            using (Br = new BinaryReader(new MemoryStream(Buffer)))
            {
                DatHeader = DatHeader.CreateInstance(Br);
            }

            LoadFile(skeletonReference);
            FindAllLowHighPoints();
        }

        #endregion Constructors

        #region Properties

        public int AltID { get; }
        public AnimationData Animations { get; private set; }
        public DatHeader DatHeader { get; }

        public EntityType EntityType { get; }

        public string FileName { get; }

        public Sections Flags { get; }

        /// <summary>
        /// Section 2: Model Geometry
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_DAT#Section_2:_Model_geometry"/>
        public Geometry Geometry { get; private set; }

        public int GetId => ID;
        public int ID { get; }
        public Information Information { get; private set; }
        public IReadOnlyList<AnimationSequence> Sequences { get; private set; }
        public Skeleton Skeleton { get; private set; }
        public Textures Textures { get; private set; }
        protected List<AnimationYOffset> AnimationYOffsets { get; private set; }
        protected BinaryReader Br { get; private set; }
        protected byte[] Buffer { get; }
        protected float OffsetYLow { get; private set; }
        private float OffsetY { get; set; }

        private Vector3 OffsetYVector => new Vector3(0f, OffsetY, 0f);

        #endregion Properties

        ///// <summary>
        ///// Creates new instance of DAT class that provides every sections parsed into structs and
        ///// helper functions for renderer
        ///// </summary>
        ///// <param name="fileId">This number is used in c0m(fileId) or d(fileId)cXYZ</param>
        ///// <param name="entityType">Supply Monster, character or weapon (0,1,2)</param>
        ///// <param name="additionalFileId">Used only in character or weapon to supply for d(fileId)[c/w](additionalFileId)</param>

        #region Methods

        public static Vector3 TransformVertex(Vector3 vertex, Vector3 localTranslate, Quaternion rotation) =>
                    Vector3.Transform(Vector3.Transform(vertex, rotation), Matrix.CreateTranslation(localTranslate));

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
        /// an animation currentFrame from animation ID. You should pass incrementing currentFrame and reset to 0
        /// when frameCount max is hit
        /// </param>
        /// <param name="step">
        /// FEATURE: This float (0.0 - 1.0) is used in Linear interpolation in animation frames
        /// blending. 0.0 means frameN, 1.0 means FrameN+1. Usually this should be a result of
        /// deltaTime to see if computer is capable of rendering smooth animations rather than
        /// constant 15 FPS
        /// </param>
        /// <returns></returns>
        public VertexPositionTexturePointersGRP GetVertexPositions(int objectId, ref Vector3 translationPosition, Quaternion rotation, ref AnimationSystem refAnimationSystem, double step)
        {
            if (refAnimationSystem.AnimationFrame >= Animations[refAnimationSystem.AnimationId].Count || refAnimationSystem.AnimationFrame < 0)
                refAnimationSystem.AnimationFrame = 0;
            var nextFrame = Animations[refAnimationSystem.AnimationId][refAnimationSystem.AnimationFrame];
            var lastAnimationFrame = refAnimationSystem.LastAnimationFrame;
            IReadOnlyList<AnimationFrame> lastAnimationFrames = Animations[refAnimationSystem.LastAnimationId];
            lastAnimationFrame = lastAnimationFrames.Count > lastAnimationFrame ? lastAnimationFrame : lastAnimationFrames.Count - 1;
            var animationFrame = lastAnimationFrames[lastAnimationFrame];

            var obj = Geometry.Objects[objectId];
            var i = 0;

            var vectorBoneGroups = GetVertices(obj, animationFrame, nextFrame, step);
            //float minY = vertices.Min(x => x.Y);
            //Vector2 HLPTS = FindLowHighPoints(translationPosition, rotation, currentFrame, nextFrame, step);

            var texturePointers = new byte[obj.CTriangles + obj.CQuads * 2];
            var vpt = new List<VertexPositionTexture>(texturePointers.Length * 3);

            if (objectId == 0)
            {
                var animationSystem = refAnimationSystem;
                var lastOffsets = AnimationYOffsets?.First(x =>
                    x.ID == animationSystem.LastAnimationId &&
                    x.Frame == lastAnimationFrame) ?? default;
                var nextOffsets = AnimationYOffsets?.First(x =>
                    x.ID == animationSystem.AnimationId &&
                    x.Frame == animationSystem.AnimationFrame) ?? default;
                OffsetYLow = MathHelper.Lerp(lastOffsets.LowY, nextOffsets.LowY, (float)step);
                _indicatorPoint.X = MathHelper.Lerp(lastOffsets.MidX, nextOffsets.MidX, (float)step);
                _indicatorPoint.Y = MathHelper.Lerp(lastOffsets.HighY, nextOffsets.HighY, (float)step);
                _indicatorPoint.Z = MathHelper.Lerp(lastOffsets.MidZ, nextOffsets.MidZ, (float)step);
                // Move All Y axis down to 0 based on Lowest Y axis in Animation ID 0.
                if (OffsetY < 0)
                {
                    translationPosition.Y += OffsetY;
                }
                // If any Y axis readings are lower than 0 in Animation ID >0. Bring it up to zero.
            }

            //Triangle parsing
            for (; i < obj.CTriangles; i++)
            {
                var preVarTex = Textures[obj.Triangles[i].TextureIndex];
                vpt.AddRange(obj.Triangles[i].GenerateVPT(vectorBoneGroups, rotation, translationPosition, preVarTex));
                texturePointers[i] = obj.Triangles[i].TextureIndex;
            }

            //Quad parsing
            for (i = 0; i < obj.CQuads; i++)
            {
                var preVarTex = Textures[obj.Quads[i].TextureIndex];
                vpt.AddRange(obj.Quads[i].GenerateVPT(vectorBoneGroups, rotation, translationPosition, preVarTex));
                texturePointers[obj.CTriangles + i * 2] = obj.Quads[i].TextureIndex;
                texturePointers[obj.CTriangles + i * 2 + 1] = obj.Quads[i].TextureIndex;
            }

            return new VertexPositionTexturePointersGRP(vpt.ToArray(), texturePointers);
        }

        public Vector3 IndicatorPoint(Vector3 translationPosition)
        {
            if (OffsetYLow < 0)
                translationPosition.Y -= OffsetYLow;
            return _indicatorPoint + translationPosition;
        }

        /// <summary>
        /// Complex function that provides linear interpolation between two matrices of actual
        /// to-render animation currentFrame and next currentFrame data for blending
        /// </summary>
        /// <param name="vectorBoneGRP">the tuple that contains vertex and bone ident</param>
        /// <param name="currentFrame">current animation currentFrame to render</param>
        /// <param name="nextFrame">
        /// animation currentFrame to render that is AFTER the actual one. If last currentFrame, then usually 0 is
        /// the 'next' currentFrame
        /// </param>
        /// <param name="step">
        /// step is variable used to determinate the progress for linear interpolation. I.e. 0 for
        /// current currentFrame and 1 for next currentFrame; 0.5 for blend of two frames
        /// </param>
        /// <returns></returns>
        private VectorBoneGRP CalculateFrame(VectorBoneGRP vectorBoneGRP, AnimationFrame currentFrame,
            AnimationFrame nextFrame, double step)
        {
            Vector3 getVector(Matrix matrix)
            {
                var r = new Vector3(
                    matrix.M11 * vectorBoneGRP.X + matrix.M41 + matrix.M12 * vectorBoneGRP.Z + matrix.M13 * -vectorBoneGRP.Y,
                    matrix.M21 * vectorBoneGRP.X + matrix.M42 + matrix.M22 * vectorBoneGRP.Z + matrix.M23 * -vectorBoneGRP.Y,
                    matrix.M31 * vectorBoneGRP.X + matrix.M43 + matrix.M32 * vectorBoneGRP.Z + matrix.M33 * -vectorBoneGRP.Y);
                r = Vector3.Transform(r, Matrix.CreateScale(Skeleton.GetScale));
                return r;
            }
            var rootFramePos = getVector(currentFrame.BoneMatrix[vectorBoneGRP.BoneID]); //get's bone matrix
            if (!(step > 0f)) return new VectorBoneGRP(rootFramePos, vectorBoneGRP.BoneID);
            var nextFramePos = getVector(nextFrame.BoneMatrix[vectorBoneGRP.BoneID]);
            rootFramePos = Vector3.Lerp(rootFramePos, nextFramePos, (float)step);
            return new VectorBoneGRP(rootFramePos, vectorBoneGRP.BoneID);
        }

        private void ExportFile()
        {
            try
            {
                const string target = @"d:\";
                if (!Directory.Exists(target)) return;
                var drive = DriveInfo.GetDrives().Where(x =>
                    x.Name.IndexOf(Path.GetPathRoot(target), StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                var di = new DirectoryInfo(target);
                if (!di.Attributes.HasFlag(FileAttributes.ReadOnly) && drive.Count() == 1 && drive[0].DriveType == DriveType.Fixed)
                    Extended.DumpBuffer(Buffer, Path.Combine(target, "out.dat"));
            }
            catch (IOException)
            {
            }
        }

        private void FindAllLowHighPoints()
        {
            if (EntityType != EntityType.Character && EntityType != EntityType.Monster) return;
            // Get baseline from running function on only Animation 0;
            if (Animations.Count == 0)
                return;
            var baseline = Animations[0].Select(x => FindLowHighPoints(Vector3.Zero, Quaternion.Identity, x, x, 0f)).ToList();
            //X is lowY, Y is high Y, Z is mid x, W is mid z
            (var baselineLowY, var baselineHighY) = (baseline.Min(x => x.X), baseline.Max(x => x.Y));
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
            AnimationYOffsets = Animations.SelectMany((animation, animationID) =>
                animation.Select((animationFrame, animationFrameNumber) =>
                    new AnimationYOffset(animationID, animationFrameNumber, FindLowHighPoints(OffsetYVector, Quaternion.Identity, animationFrame, animationFrame, 0f)))).ToList();
        }

        private Vector4 FindLowHighPoints(Vector3 translationPosition, Quaternion rotation, AnimationFrame currentFrame,
                    AnimationFrame nextFrame, double step)
        {
            var vertices =
                Geometry.Objects.SelectMany(@object => GetVertices(@object, currentFrame, nextFrame, step)).ToList();
            if (translationPosition == Vector3.Zero && rotation == Quaternion.Identity)
                return new Vector4(vertices.Min(x => x.Y), vertices.Max(x => x.Y),
                    (vertices.Min(x => x.X) + vertices.Max(x => x.X)) / 2f,
                    (vertices.Min(x => x.Z) + vertices.Max(x => x.Z)) / 2f);
            {
                IEnumerable<Vector3> enumVertices = vertices
                    .Select(vertex => TransformVertex(vertex, translationPosition, rotation)).ToArray();
                // midpoints
                return new Vector4(enumVertices.Min(x => x.Y), enumVertices.Max(x => x.Y),
                    (enumVertices.Min(x => x.X) + enumVertices.Max(x => x.X)) / 2f,
                    (enumVertices.Min(x => x.Z) + enumVertices.Max(x => x.Z)) / 2f);
            }
        }

        private List<VectorBoneGRP>
                    GetVertices(Object @object, AnimationFrame currentFrame, AnimationFrame nextFrame, double step) => @object
                    .VertexData.SelectMany(vertexData => vertexData.Vertices.Select(vertex =>
                        CalculateFrame(new VectorBoneGRP(vertex, vertexData.BoneId), currentFrame, nextFrame, step))).ToList();

        private void LoadCharacter()
        {
            if (Flags.HasFlag(Sections.Skeleton))
                ReadSection1(DatHeader[0]);
            if (Flags.HasFlag(Sections.ModelAnimation))
                ReadSection3(DatHeader[2]);
            if (Flags.HasFlag(Sections.ModelGeometry))
                ReadSection2(DatHeader[1]);
            if (ID == 7 && EntityType == EntityType.Character) // edna has no weapons.
            {
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(DatHeader[8]);
                if (Flags.HasFlag(Sections.AnimationSequences))
                    ReadSection5(DatHeader[5], DatHeader[6]);
            }
            else if (Flags.HasFlag(Sections.Textures))
                ReadSection11(DatHeader[5]);
        }

        private void LoadFile(DatFile skeletonReference)
        {
            using (Br = new BinaryReader(new MemoryStream(Buffer)))
            {
                if (Br.BaseStream.Length - Br.BaseStream.Position < 4) return;
                switch (EntityType)
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
                    ReadSection7(DatHeader[0]);
                //if (Flags.HasFlag(Sections.Scripts))
                //    ReadSection8(datFile.pSections[7]);
                return;
            }
            if (Flags.HasFlag(Sections.Skeleton))
                ReadSection1(DatHeader[0]);
            if (Flags.HasFlag(Sections.ModelAnimation))
                ReadSection3(DatHeader[2]); // animation data
            if (Flags.HasFlag(Sections.ModelGeometry))
                ReadSection2(DatHeader[1]);

            //if (Flags.HasFlag(Sections.Section4_Unknown))
            //  ReadSection4(datFile.pSections[3]);
            if (Flags.HasFlag(Sections.AnimationSequences))
                ReadSection5(DatHeader[4], DatHeader[5]);
            //if (Flags.HasFlag(Sections.Section6_Unknown))
            //  ReadSection6(datFile.pSections[5]);
            if (Flags.HasFlag(Sections.Information))
                ReadSection7(DatHeader[6]);
            //if (Flags.HasFlag(Sections.Scripts))
            //ReadSection8(datFile.pSections[7]); // battle scripts/ai

            //if (Flags.HasFlag(Sections.Sounds))
            //    ReadSection9(datFile.pSections[8], datFile.pSections[9]); //AKAO sounds
            //if (Flags.HasFlag(Sections.Sounds_Unknown))
            //  ReadSection10(datFile.pSections[9], datFile.pSections[10], br, fileName);

            if (Flags.HasFlag(Sections.Textures))
                ReadSection11(DatHeader[10]);
        }

        private void LoadWeapon(DatFile skeletonReference)
        {
            if (ID != 1 && ID != 9)
            {
                if (Flags.HasFlag(Sections.Skeleton))
                    ReadSection1(DatHeader[0]);
                if (Flags.HasFlag(Sections.ModelAnimation))
                    ReadSection3(DatHeader[2]);
                if (Flags.HasFlag(Sections.ModelGeometry))
                    ReadSection2(DatHeader[1]);
                if (Flags.HasFlag(Sections.AnimationSequences))
                    ReadSection5(DatHeader[3], DatHeader[4]);
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(DatHeader[6]);
            }
            else if (skeletonReference != null)
            {
                Skeleton = skeletonReference.Skeleton;
                Animations = skeletonReference.Animations;
                if (Flags.HasFlag(Sections.ModelGeometry))
                    ReadSection2(DatHeader[0]);
                if (Flags.HasFlag(Sections.AnimationSequences))
                    ReadSection5(DatHeader[1], DatHeader[2]);
                if (Flags.HasFlag(Sections.Textures))
                    ReadSection11(DatHeader[4]);
            }
        }

        /// <summary>
        /// Skeleton data section
        /// </summary>
        /// <param name="start"></param>
        private void ReadSection1(uint start) => Skeleton = Skeleton.CreateInstance(Br, start);

        /// <summary>
        /// TIM - Textures
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection11(uint start)
        {
            var local = FileName;
            switch (EntityType)
            {
                case EntityType.Monster:
                    var id = ID;
                    switch (id)
                    {
                        //blue
                        case 71:
                        case 73:
                        case 76:
                        case 134:
                        case 143:
                            id = 73;
                            break;
                        //red
                        case 64:
                        case 72:
                        case 74:
                        case 135:
                            id = 74;
                            break;
                    }
                    local = $"{_prefix}{id:D03}";
                    break;

                case EntityType.Character:
                case EntityType.Weapon:
                //  local = $"{_prefix}{ID:x}{_middle}{AltID:D03}";
                  break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(EntityType), EntityType, null);
            }
            Textures = Textures.CreateInstance(Buffer, start, local);
        }

        /// <summary>
        /// Model Geometry section
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection2(uint start) => Geometry = Geometry.CreateInstance(Br, start);

        /// <summary>
        /// Model Animation section
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection3(uint start) => Animations = AnimationData.CreateInstance(Br, start, Skeleton);

        /// <summary>
        /// Animation Sequences
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        /// <see cref="http://forums.qhimm.com/index.php?topic=19362.msg270092"/>
        private void ReadSection5(uint start, uint end) =>
            Sequences = AnimationSequence.CreateInstances(Br, start, end);

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="start"></param>
        /// <param name="br"></param>
        /// <param name="fileName"></param>
        private void ReadSection7(uint start) => Information = Information.CreateInstance(Br, start);

        #endregion Methods
    }
}