using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Fields

        private const bool EnableDumpingData = false;

        /// <summary>
        /// skyRotating floats are hardcoded
        /// </summary>
        private static readonly float[] SkyRotators = { 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x10, 0x10, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x2, 0x0, 0x0, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x4, 0x0, 0x8, 0x0, 0x4, 0x4, 0x0, 0x4, 0x0, 0x4, 0xfffc, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x8, 0xfffc, 0x4, 0x4, 0x4, 0x4, 0x8, 0x8, 0x4, 0xfffc, 0xfffc, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x8, 0x8, 0x0, 0x8, 0xfffc, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x8, 0x0, 0x8, 0x8 };

        private readonly IReadOnlyList<Animation> _animations;
        private readonly ModelGroups _modelGroups;
        private readonly IReadOnlyList<TextureAnimation> _textureAnimations;

        /// <summary>
        /// a rotator is a float that holds current axis rotation for sky. May be malformed by skyRotators or TimeCompression magic
        /// </summary>
        private float _localRotator;

        #endregion Fields

        #region Constructors

        private Stage(BinaryReader br)
        {
            MainGeometrySection mainSection = MainGeometrySection.Read(br);
            ObjectsGroup[] objectsGroups = {
                    ObjectsGroup.Read(mainSection.Group1Pointer,br),
                    ObjectsGroup.Read(mainSection.Group2Pointer,br),
                    ObjectsGroup.Read(mainSection.Group3Pointer,br),
                    ObjectsGroup.Read(mainSection.Group4Pointer,br)
            };

            _modelGroups = new ModelGroups(
                    ModelGroup.Read(objectsGroups[0].objectListPointer, br),
                    ModelGroup.Read(objectsGroups[1].objectListPointer, br),
                    ModelGroup.Read(objectsGroups[2].objectListPointer, br),
                    ModelGroup.Read(objectsGroups[3].objectListPointer, br)
            );

            Textures = ReadTexture(mainSection.TexturePointer, br);
            Scenario = Memory.Encounters.Scenario;

            if (Textures == null || Textures.All(x => x.Value == null)) return;

            TextureHandler t = Textures.First(x => x.Value != null).Value;
            Width = t.Width;
            Height = t.Height;

            //Seems most animations skip the first frame. Can override with skip:0
            //Count defaults to rows * cols. Can override this to be less than that.
            //public Animation(int width, int height, byte clut, byte texturePage, byte cols, byte rows, ModelGroups _mg, int Count = 0, int x = 0, int y =0, int skip =1)
            if (Scenario == 8)//need to update the source texture with the animation frames or add new vertices and uvs because first frame is 3x larger
                _textureAnimations = new List<TextureAnimation>
                    {new TextureAnimation(Textures[1], 32, 96, 2, 6, 1, skip: 3, y: 128)}.AsReadOnly();
            else if (Scenario == 31 || Scenario == 30)
                _animations =
                    new List<Animation> {new Animation(64, 64, 4, 4, 2, _modelGroups, skip: 0)}.AsReadOnly();
            else if (Scenario == 20)
                _animations = new List<Animation> { new Animation(64, 128, 2, 4, 2, _modelGroups) }.AsReadOnly();
            else if (Scenario == 48)
                _animations =
                    new List<Animation> {new Animation(84, 32, 4, 3, 3, _modelGroups, 8, 0, 32)}.AsReadOnly();
            else if (Scenario == 51)
                _animations =
                    new List<Animation> {new Animation(64, 64, 0, 4, 2, _modelGroups, skip: 0)}.AsReadOnly();
            else if (Scenario == 52)
                _animations =
                    new List<Animation> {new Animation(64, 64, 0, 4, 1, _modelGroups, skip: 0)}.AsReadOnly();
            else if (Scenario == 79)
                _animations = new List<Animation> {new Animation(32, 64, 4, 8, 1, _modelGroups, y: 192, skip: 0)}
                    .AsReadOnly();
            else if (Scenario == 107 || Scenario == 108 || Scenario == 136)
                _animations = new List<Animation>
                {
                    new Animation(128, 32, 4, 2, 4, _modelGroups, topDown: true, reversible: true,
                        totalFrameTime: TimeSpan.FromMilliseconds(1000f / 5f),
                        pauseAtStart: TimeSpan.FromMilliseconds(500f))
                    
                }.AsReadOnly();
            else if (Scenario == 147)
                _animations = new List<Animation> {
                        new Animation(32, 32, 2, 8, 2, _modelGroups, y: 192),
                        new Animation(32,32,1,1,3, _modelGroups,x:96,y:160)
                    }.AsReadOnly();
        }

        #endregion Constructors

        #region Properties

        private static Matrix ProjectionMatrix => ModuleBattleDebug.ProjectionMatrix;
        private static Matrix ViewMatrix => ModuleBattleDebug.ViewMatrix;
        private static Matrix WorldMatrix => ModuleBattleDebug.WorldMatrix;
        public int Height { get; }
        public byte Scenario { get; }
        public IReadOnlyDictionary<ushort, TextureHandler> Textures { get; }

        public int Width { get; }

        #endregion Properties

        #region Methods

        public static BinaryReader Open()
        {
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_BATTLE);
            string filename = Memory.Encounters.Filename;
            Memory.Log.WriteLine($"{nameof(Battle)} :: Loading {nameof(Camera)} :: {filename}");
            byte[] stageBuffer = aw.GetBinaryFile(filename);

            return stageBuffer == null ? null : new BinaryReader(new MemoryStream(stageBuffer));
        }

        public static Stage Read(uint offset, BinaryReader br)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            uint sectionCounter = br.ReadUInt32();
            if (sectionCounter == 6) return new Stage(br);
            Memory.Log.WriteLine($"BS_PARSER_PRE_OBJECT SECTION: Main geometry section has no 6 pointers at: {br.BaseStream.Position}");
            ModuleBattleDebug.BattleModule++;
            return null;
        }

        public Vector2 CalculateUV(Vector2 uv, byte texPage)
        {
            //old code from my wiki page
            //Float U = (float)U_Byte / (float)(TIM_Texture_Width * 2) + ((float)Texture_Page / (TIM_Texture_Width * 2));
            (float x, float y) = uv;
            const float texPageWidth = 128f;
            return new Vector2((x + texPage * texPageWidth) / Width, y / Height);
        }

        public void Draw()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            //Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            using (AlphaTestEffect ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            {
                Projection = ProjectionMatrix,
                View = ViewMatrix,
                World = WorldMatrix,
            })
            using (new BasicEffect(Memory.graphics.GraphicsDevice)
            {
                TextureEnabled = true,
            })
            {
                //var models = (from model group in modelGroups
                // where (model group?.Count ?? 0) > 0
                // from model in model group
                // where model.quads != null && model.triangles != null && model.vertices != null
                // select model);
                int[] order = { 3, 0, 1, 2 };
                foreach (int n in order.Where(x => x < (_modelGroups?.Count ?? 0)))
                    foreach (Model b in _modelGroups[n])
                    {
                        GeometryVertexPosition vpt = GetVertexBuffer(b);
                        if (n == 3 && Math.Abs(SkyRotators[Memory.Encounters.Scenario]) > float.Epsilon)
                            CreateRotation(vpt);
                        if (vpt == null) continue;
                        int localVertexIndex = 0;
                        foreach (GeometryInfoSupplier gis in vpt.GeometryInfoSupplier.Where(x => !x.GPU.HasFlag(GPU.v2_add)))
                        {
                            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                            process(gis);
                        }

                        //BlendState bs = new BlendState
                        //{
                        //    //ColorWriteChannels = ColorWriteChannels.Blue | ColorWriteChannels.Green | ColorWriteChannels.Red,
                        //    ColorSourceBlend = Blend.One,
                        //    AlphaSourceBlend = Blend.One,
                        //    ColorDestinationBlend = Blend.InverseSourceColor,
                        //    AlphaDestinationBlend = Blend.One,
                        //    ColorBlendFunction = BlendFunction.Max
                        //};
                        foreach (GeometryInfoSupplier gis in vpt.GeometryInfoSupplier.Where(x => x.GPU.HasFlag(GPU.v2_add)))
                        {
                            Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Add;//bs;
                            process(gis);
                        }
                        // bs?.Dispose();
                        void process(GeometryInfoSupplier gis)
                        {
                            ate.Texture = (Texture2D)Textures[gis.clut]; //provide texture per-face
                            foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                if (gis.bQuad)
                                {
                                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                                        vertexData: vpt.VertexPositionTexture, vertexOffset: localVertexIndex, primitiveCount: 2);
                                    localVertexIndex += 6;
                                }
                                else
                                {
                                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                                        vertexData: vpt.VertexPositionTexture, vertexOffset: localVertexIndex, primitiveCount: 1);
                                    localVertexIndex += 3;
                                }
                            }
                        }
                    }
            }
        }

        public void Update()
        {
            _animations?.ForEach(x => x.Update());
            _textureAnimations?.ForEach(x => x.Update());
        }

        private static byte GetClutId(ushort clut)
        {
            ushort bb = Extended.UshortLittleEndian(clut);
            return (byte)(((bb >> 14) & 0x03) | (bb << 2) & 0x0C);
        }

        private static byte GetTexturePage(byte texturePage) => (byte)(texturePage & 0x0F);

        /// <summary>
        /// Moves sky
        /// </summary>
        /// <param name="vpt"></param>
        private void CreateRotation(GeometryVertexPosition vpt)
        {
            _localRotator += (short)SkyRotators[Memory.Encounters.Scenario] / 4096f * Memory.ElapsedGameTime.Milliseconds;
            if (_localRotator <= 0)
                return;
            for (int i = 0; i < vpt.VertexPositionTexture.Length; i++)
                vpt.VertexPositionTexture[i].Position = Vector3.Transform(vpt.VertexPositionTexture[i].Position, Matrix.CreateRotationY(MathHelper.ToRadians(_localRotator)));
        }

        /// <summary>
        /// Converts requested Model data (Stage group geometry) into MonoGame VertexPositionTexture
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private GeometryVertexPosition GetVertexBuffer(Model model)
        {
            List<VertexPositionTexture> vptDynamic = new List<VertexPositionTexture>();
            List<GeometryInfoSupplier> bsRendererSupplier = new List<GeometryInfoSupplier>();
            if (model.Vertices == null) return null;
            if (model.Triangles != null)
                foreach (Triangle triangle in model.Triangles)
                {
                    (Vertex a, Vertex b, Vertex c) =
                        (model.Vertices[triangle.A], model.Vertices[triangle.B], model.Vertices[triangle.C]);
                    vptDynamic.Add(new VertexPositionTexture(a,
                        CalculateUV(triangle.UVs[1], triangle.TexturePage)));
                    vptDynamic.Add(new VertexPositionTexture(b,
                        CalculateUV(triangle.UVs[2], triangle.TexturePage)));
                    vptDynamic.Add(new VertexPositionTexture(c,
                        CalculateUV(triangle.UVs[0], triangle.TexturePage)));
                    bsRendererSupplier.Add(new GeometryInfoSupplier()
                    {
                        bQuad = false,
                        clut = triangle.Clut,
                        texPage = triangle.TexturePage,
                        GPU = triangle.GPU
                    });
                }

            if (model.Quads == null)
                return new GeometryVertexPosition(bsRendererSupplier.ToArray(), vptDynamic.ToArray());
            {
                    foreach (Quad quad in model.Quads)
                    {
                        //I have to re-triangulate it. Fortunately I had been working on this lately
                        (Vertex a, Vertex b, Vertex c, Vertex d) =
                            (model.Vertices[quad.A], //1
                                model.Vertices[quad.B], //2
                                model.Vertices[quad.C], //4
                                model.Vertices[quad.D]); //3

                        //triangulation wing-reorder
                        //1 2 4
                        vptDynamic.Add(new VertexPositionTexture(a,
                            CalculateUV(quad.UVs[0], quad.TexturePage)));
                        vptDynamic.Add(new VertexPositionTexture(b,
                            CalculateUV(quad.UVs[1], quad.TexturePage)));
                        vptDynamic.Add(new VertexPositionTexture(d,
                            CalculateUV(quad.UVs[3], quad.TexturePage)));

                        //1 3 4
                        vptDynamic.Add(new VertexPositionTexture(d,
                            CalculateUV(quad.UVs[3], quad.TexturePage)));
                        vptDynamic.Add(new VertexPositionTexture(c,
                            CalculateUV(quad.UVs[2], quad.TexturePage)));
                        vptDynamic.Add(new VertexPositionTexture(a,
                            CalculateUV(quad.UVs[0], quad.TexturePage)));

                        bsRendererSupplier.Add(new GeometryInfoSupplier()
                        {
                            bQuad = true,
                            clut = quad.Clut,
                            texPage = quad.TexturePage,
                            GPU = quad.GPU
                        });
                    }
            }

            return new GeometryVertexPosition(bsRendererSupplier.ToArray(), vptDynamic.ToArray());
        }

        /// <summary>
        /// Method designed for Stage texture loading.
        /// </summary>
        /// <param name="texturePointer">Absolute pointer to TIM texture header in stageBuffer</param>
        private IReadOnlyDictionary<ushort, TextureHandler> ReadTexture(uint texturePointer, BinaryReader br)
        {
            TIM2 textureInterface = new TIM2(br, texturePointer);

            IEnumerable<Model> temp = (from mg in _modelGroups
                                       from m in mg
                                       select m).Where(x => x.Vertices != null && x.Triangles != null && x.Quads != null);
            IEnumerable<Model> enumerable = temp as Model[] ?? temp.ToArray();
            var tuv = (from m in enumerable
                       where m.Triangles != null && m.Triangles.Count > 0
                       from t in m.Triangles
                       where t != null
                       select new { clut = t.Clut, t.TexturePage, t.MinUV, t.MaxUV, t.Rectangle }).Distinct().ToList();
            var quv = (from m in enumerable
                       where m.Quads != null && m.Quads.Count > 0
                       from q in m.Quads
                       where q != null
                       select new { clut = q.Clut, q.TexturePage, q.MinUV, q.MaxUV, q.Rectangle }).Distinct().ToList();

            HashSet<byte> allCluts = tuv.Union(quv).Select(x => x.clut).ToHashSet();
            IReadOnlyDictionary<ushort, TextureHandler> textures = allCluts.ToDictionary(i => (ushort)i, i => TextureHandler.Create(Memory.Encounters.Filename, textureInterface, palette: i));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (!Memory.EnableDumpingData && !EnableDumpingData) return textures;
            var all = tuv.Union(quv);
            foreach (var tpGroup in all.GroupBy(x => x.TexturePage))
            {
                byte texturePage = tpGroup.Key;
                foreach (var clutGroup in tpGroup.GroupBy(x => x.clut))
                {
                    byte clut = clutGroup.Key;
                    string filename = Path.GetFileNameWithoutExtension(Memory.Encounters.Filename);
                    string p = Path.Combine(Path.GetTempPath(), "Battle Stages",
                        filename ?? throw new InvalidOperationException(), "Reference");
                    Directory.CreateDirectory(p);
                    filename = $"{filename}_{clut}_{texturePage}.png";
                    using (Texture2D tex = textureInterface.GetTexture(clut))
                    using (RenderTarget2D tmp = new RenderTarget2D(Memory.graphics.GraphicsDevice, 256, 256))
                    {
                        Memory.graphics.GraphicsDevice.SetRenderTarget(tmp);
                        Memory.SpriteBatchStartAlpha();
                        Memory.graphics.GraphicsDevice.Clear(Color.TransparentBlack);
                        foreach (Rectangle r in clutGroup.Select(x => x.Rectangle))
                        {
                            Rectangle src = r;
                            Rectangle dst = r;
                            src.Offset(texturePage * 128, 0);
                            Memory.spriteBatch.Draw(tex, dst, src, Color.White);
                        }

                        Memory.SpriteBatchEnd();
                        Memory.graphics.GraphicsDevice.SetRenderTarget(null);
                        using (FileStream fs = new FileStream(Path.Combine(p, filename), FileMode.Create,
                            FileAccess.Write, FileShare.ReadWrite))
                            tmp.SaveAsPng(fs, 256, 256);
                    }
                }
            }

            string path = Path.Combine(Path.GetTempPath(), "Battle Stages",
                Path.GetFileNameWithoutExtension(Memory.Encounters.Filename) ??
                throw new InvalidOperationException());
            Directory.CreateDirectory(path);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            string fullPath = Path.Combine(path,
                $"{Path.GetFileNameWithoutExtension(Memory.Encounters.Filename)}_Clut.png");
            if (!File.Exists(fullPath))
                textureInterface.SaveCLUT(fullPath);

            Textures.ForEach(x => x.Value.Save(path, false));

            return textures;
        }

        #endregion Methods
    }
}