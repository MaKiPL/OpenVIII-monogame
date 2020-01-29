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

        private const bool EnableDumpingData = true;

        /// <summary>
        /// skyRotating floats are hardcoded
        /// </summary>
        private static readonly float[] skyRotators = { 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x10, 0x10, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x2, 0x0, 0x0, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x4, 0x0, 0x8, 0x0, 0x4, 0x4, 0x0, 0x4, 0x0, 0x4, 0xfffc, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x8, 0xfffc, 0x4, 0x4, 0x4, 0x4, 0x8, 0x8, 0x4, 0xfffc, 0xfffc, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x8, 0x8, 0x0, 0x8, 0xfffc, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x8, 0x0, 0x8, 0x8 };

        private List<Animation> Animations;

        /// <summary>
        /// a rotator is a float that holds current axis rotation for sky. May be malformed by skyRotators or TimeCompression magic
        /// </summary>
        private float localRotator = 0.0f;

        private ModelGroups modelGroups;

        #endregion Fields

        #region Constructors

        public Stage()
        { }

        #endregion Constructors

        #region Properties

        public static byte Scenario { get; private set; }
        public int Height { get; private set; }

        public TextureHandler[] textures { get; private set; }

        public int Width { get; private set; }

        private Matrix projectionMatrix => Module_battle_debug.ProjectionMatrix;

        private Matrix viewMatrix => Module_battle_debug.ViewMatrix;

        private Matrix worldMatrix => Module_battle_debug.WorldMatrix;

        #endregion Properties

        #region Methods

        public static BinaryReader Open()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string filename = Memory.Encounters.Filename;
            Memory.Log.WriteLine($"{nameof(Battle)} :: Loading {nameof(Camera)} :: {filename}");
            byte[] stageBuffer = aw.GetBinaryFile(filename);

            BinaryReader br;
            MemoryStream ms;
            if (stageBuffer == null)
                return null;
            return br = new BinaryReader(ms = new MemoryStream(stageBuffer));
        }

        public static Stage Read(uint offset, BinaryReader br)
        {
            Scenario = Memory.Encounters.Scenario;
            Stage s = new Stage();
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            uint sectionCounter = br.ReadUInt32();
            if (sectionCounter != 6)
            {
                Memory.Log.WriteLine($"BS_PARSER_PRE_OBJECTSECTION: Main geometry section has no 6 pointers at: {br.BaseStream.Position}");
                Module_battle_debug.battleModule++;
                return null;
            }
            MainGeometrySection MainSection = MainGeometrySection.Read(br);
            ObjectsGroup[] objectsGroups = new ObjectsGroup[4]
            {
                    ObjectsGroup.Read(MainSection.Group1Pointer,br),
                    ObjectsGroup.Read(MainSection.Group2Pointer,br),
                    ObjectsGroup.Read(MainSection.Group3Pointer,br),
                    ObjectsGroup.Read(MainSection.Group4Pointer,br)
            };

            s.modelGroups = new ModelGroups(
                    ModelGroup.Read(objectsGroups[0].objectListPointer, br),
                    ModelGroup.Read(objectsGroups[1].objectListPointer, br),
                    ModelGroup.Read(objectsGroups[2].objectListPointer, br),
                    ModelGroup.Read(objectsGroups[3].objectListPointer, br)
            );

            s.ReadTexture(MainSection.TexturePointer, br);

            return s;
        }

        public Vector2 CalculateUV(Vector2 UV, byte texPage)
        {
            //old code from my wiki page
            //Float U = (float)U_Byte / (float)(TIM_Texture_Width * 2) + ((float)Texture_Page / (TIM_Texture_Width * 2));
            float fU = (UV.X + texPage * 128f) / Width;
            float fV = UV.Y / Height;
            return new Vector2(fU, fV);
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
                Projection = projectionMatrix,
                View = viewMatrix,
                World = worldMatrix,
            })
            using (BasicEffect effect = new BasicEffect(Memory.graphics.GraphicsDevice)
            {
                TextureEnabled = true,
            })
            {
                //var models = (from modelgroup in modelGroups
                // where (modelgroup?.Count ?? 0) > 0
                // from model in modelgroup
                // where model.quads != null && model.triangles != null && model.vertices != null
                // select model);
                int[] order = new int[] { 3, 0, 1, 2 };
                foreach(int n in order.Where(x=>x < (modelGroups?.Count ?? 0)))
                    foreach (Model b in modelGroups[n])
                    {
                        GeometryVertexPosition vpt = GetVertexBuffer(b);
                        if (n == 3 && skyRotators[Memory.Encounters.Scenario] != 0)
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
                            ate.Texture = (Texture2D)textures[gis.clut]; //provide texture per-face
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
            if (Animations == null || Animations.Count == 0)
            {
                //Seems most animations skip the first frame. Can override with skip:0
                //count defaults to rows * cols. Can override this to be less than that.
                //public Animation(int width, int height, byte clut, byte texturePage, byte cols, byte rows, ModelGroups _mg, int count = 0, int x = 0, int y =0, int skip =1)
                if (Scenario == 31 || Scenario == 30)
                    Animations = new List<Animation> { new Animation(64, 64, 4, 4, 4, 2, modelGroups, skip: 0) };
                else if (Scenario == 20)
                    Animations = new List<Animation> { new Animation(64, 128, 3, 2, 4, 2, modelGroups) };
                else if (Scenario == 48)
                    Animations = new List<Animation> { new Animation(84, 32, 3, 4, 3, 3, modelGroups, 8, 0, 32) };
                else if (Scenario == 51)
                    Animations = new List<Animation> { new Animation(64, 64, 0, 0, 4, 2, modelGroups, skip: 0) };
                else if (Scenario == 52)
                    Animations = new List<Animation> { new Animation(64, 64, 2, 0, 4, 1, modelGroups, skip: 0) };
                else if (Scenario == 79)
                    Animations = new List<Animation> { new Animation(32, 64, 5, 4, 8, 1, modelGroups, y: 192, skip: 0) };
                else if (Scenario == 107 || Scenario == 108 || Scenario == 136)
                    Animations = new List<Animation> { new Animation(128, 32, 3, 4, 2, 4, modelGroups)
                    { TopDown = true, Reverseable = true, TotalFrameTime = TimeSpan.FromMilliseconds(1000 / 5), PauseAtStart = TimeSpan.FromMilliseconds(500) } };
                else if (Scenario == 147)
                    Animations = new List<Animation> {
                        new Animation(32, 32, 10, 2, 8, 2, modelGroups, y: 192),
                        new Animation(32,32,9,1,1,3, modelGroups,x:96,y:160)
                    };
            }
            Animations?.ForEach(x => x.Update());
        }

        private static byte GetClutId(ushort clut)
        {
            ushort bb = Extended.UshortLittleEndian(clut);
            return (byte)(((bb >> 14) & 0x03) | (bb << 2) & 0x0C);
        }

        private static byte GetTexturePage(byte texturepage) => (byte)(texturepage & 0x0F);

        /// <summary>
        /// Moves sky
        /// </summary>
        /// <param name="vpt"></param>
        private void CreateRotation(GeometryVertexPosition vpt)
        {
            localRotator += (short)skyRotators[Memory.Encounters.Scenario] / 4096f * Memory.gameTime.ElapsedGameTime.Milliseconds;
            if (localRotator <= 0)
                return;
            for (int i = 0; i < vpt.VertexPositionTexture.Length; i++)
                vpt.VertexPositionTexture[i].Position = Vector3.Transform(vpt.VertexPositionTexture[i].Position, Matrix.CreateRotationY(MathHelper.ToRadians(localRotator)));
        }

        /// <summary>
        /// Converts requested Model data (Stage group geometry) into MonoGame VertexPositionTexture
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private GeometryVertexPosition GetVertexBuffer(Model model)
        {
            List<VertexPositionTexture> vptDynamic = new List<VertexPositionTexture>();
            List<GeometryInfoSupplier> bs_renderer_supplier = new List<GeometryInfoSupplier>();
            if (model.vertices == null) return null;
            for (int i = 0; i < model.triangles.Length; i++)
            {
                Vertex A = model.vertices[model.triangles[i].A];
                Vertex B = model.vertices[model.triangles[i].B];
                Vertex C = model.vertices[model.triangles[i].C];
                vptDynamic.Add(new VertexPositionTexture(A,
                    CalculateUV(model.triangles[i].UVs[1], model.triangles[i].TexturePage)));
                vptDynamic.Add(new VertexPositionTexture(B,
                    CalculateUV(model.triangles[i].UVs[2], model.triangles[i].TexturePage)));
                vptDynamic.Add(new VertexPositionTexture(C,
                    CalculateUV(model.triangles[i].UVs[0], model.triangles[i].TexturePage)));
                bs_renderer_supplier.Add(new GeometryInfoSupplier()
                {
                    bQuad = false,
                    clut = model.triangles[i].clut,
                    texPage = model.triangles[i].TexturePage,
                    GPU = model.triangles[i].GPU
                });
            }
            for (int i = 0; i < model.quads.Length; i++)
            {
                //I have to re-trangulate it. Fortunately I had been working on this lately
                Vertex A = model.vertices[model.quads[i].A]; //1
                Vertex B = model.vertices[model.quads[i].B]; //2
                Vertex C = model.vertices[model.quads[i].C]; //4
                Vertex D = model.vertices[model.quads[i].D]; //3

                //triangluation wing-reorder
                //1 2 4
                vptDynamic.Add(new VertexPositionTexture(A,
                    CalculateUV(model.quads[i].UVs[0], model.quads[i].TexturePage)));
                vptDynamic.Add(new VertexPositionTexture(B,
                    CalculateUV(model.quads[i].UVs[1], model.quads[i].TexturePage)));
                vptDynamic.Add(new VertexPositionTexture(D,
                    CalculateUV(model.quads[i].UVs[3], model.quads[i].TexturePage)));

                //1 3 4
                vptDynamic.Add(new VertexPositionTexture(D,
                    CalculateUV(model.quads[i].UVs[3], model.quads[i].TexturePage)));
                vptDynamic.Add(new VertexPositionTexture(C,
                    CalculateUV(model.quads[i].UVs[2], model.quads[i].TexturePage)));
                vptDynamic.Add(new VertexPositionTexture(A,
                    CalculateUV(model.quads[i].UVs[0], model.quads[i].TexturePage)));

                bs_renderer_supplier.Add(new GeometryInfoSupplier()
                {
                    bQuad = true,
                    clut = model.quads[i].clut,
                    texPage = model.quads[i].TexturePage,
                    GPU = model.quads[i].GPU
                });
            }
            return new GeometryVertexPosition(bs_renderer_supplier.ToArray(), vptDynamic.ToArray());
        }

        /// <summary>
        /// Method designed for Stage texture loading.
        /// </summary>
        /// <param name="texturePointer">Absolute pointer to TIM texture header in stageBuffer</param>
        private void ReadTexture(uint texturePointer, BinaryReader br)
        {
            TIM2 textureInterface = new TIM2(br, texturePointer);
            if (Memory.EnableDumpingData || EnableDumpingData)
            {
                IEnumerable<Model> temp = (from mg in modelGroups
                                           from m in mg
                                           select m).Where(x => x.vertices != null && x.triangles != null && x.quads != null);
                //IOrderedEnumerable<byte> cluts = temp.SelectMany(x => x.quads.Select(y => y.clut)).Union(temp.SelectMany(x => x.triangles.Select(y => y.clut))).Distinct().OrderBy(x => x);
                //IOrderedEnumerable<byte> unks = temp.SelectMany(x => x.quads.Select(y => y.UNK)).Union(temp.SelectMany(x => x.triangles.Select(y => y.UNK))).Distinct().OrderBy(x => x);
                //IOrderedEnumerable<byte> hides = temp.SelectMany(x => x.quads.Select(y => y.bHide)).Union(temp.SelectMany(x => x.triangles.Select(y => y.bHide))).Distinct().OrderBy(x => x);
                //IOrderedEnumerable<byte> gpu = temp.SelectMany(x => x.quads.Select(y => y.GPU)).Union(temp.SelectMany(x => x.triangles.Select(y => y.GPU))).Distinct().OrderBy(x => x);
                //IOrderedEnumerable<Color> color = temp.SelectMany(x => x.quads.Select(y => y.Color)).Union(temp.SelectMany(x => x.triangles.Select(y => y.Color))).Distinct().OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B);
                var tuv = (from m in temp
                           where m.triangles != null && m.triangles.Length > 0
                           from t in m.triangles
                           where t != null
                           select new { t.clut, t.TexturePage, t.MinUV, t.MaxUV, t.Rectangle }).Distinct().OrderBy(x => x.TexturePage).ThenBy(x => x.clut).ToList();
                var quv = (from m in temp
                           where m.quads != null && m.quads.Length > 0
                           from q in m.quads
                           where q != null
                           select new { q.clut, q.TexturePage, q.MinUV, q.MaxUV, q.Rectangle }).Distinct().OrderBy(x => x.TexturePage).ThenBy(x => x.clut)/*.Where(x => x.Rectangle.Height > 0 && x.Rectangle.Width > 0)*/.ToList();
                var all = tuv.Union(quv);
                foreach (var tpGroup in all.GroupBy(x => x.TexturePage))
                {
                    byte texturepage = tpGroup.Key;
                    foreach (var clutGroup in tpGroup.GroupBy(x => x.clut))
                    {
                        byte clut = clutGroup.Key;
                        string filename = Path.GetFileNameWithoutExtension(Memory.Encounters.Filename);
                        string p = Path.Combine(Path.GetTempPath(), "Battle Stages", filename, "Reference");
                        Directory.CreateDirectory(p);
                        filename = $"{filename}_{clut}_{texturepage}.png";
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
                                src.Offset(texturepage * 128, 0);
                                Memory.spriteBatch.Draw(tex, dst, src, Color.White);
                            }
                            Memory.SpriteBatchEnd();
                            Memory.graphics.GraphicsDevice.SetRenderTarget(null);
                            using (FileStream fs = new FileStream(Path.Combine(p, filename), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                                tmp.SaveAsPng(fs, 256, 256);
                        }
                    }
                }
            }
            Width = textureInterface.GetWidth;
            Height = textureInterface.GetHeight;
            string path = Path.Combine(Path.GetTempPath(), "Battle Stages", Path.GetFileNameWithoutExtension(Memory.Encounters.Filename));
            Directory.CreateDirectory(path);

            if (Memory.EnableDumpingData || EnableDumpingData)
            {
                string fullpath = Path.Combine(path, $"{Path.GetFileNameWithoutExtension(Memory.Encounters.Filename)}_Clut.png");
                if (!File.Exists(fullpath))
                    textureInterface.SaveCLUT(fullpath);
            }

            textures = new TextureHandler[textureInterface.GetClutCount];
            for (ushort i = 0; i < textureInterface.GetClutCount; i++)
            {
                textures[i] = TextureHandler.Create(Memory.Encounters.Filename, textureInterface, i);
                if (Memory.EnableDumpingData || EnableDumpingData)
                    textures[i].Save(path, false);
            }
        }

        #endregion Methods
    }
}