using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Fields

        /// <summary>
        /// skyRotating floats are hardcoded
        /// </summary>
        private static readonly float[] skyRotators = { 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x10, 0x10, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x2, 0x0, 0x0, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x4, 0x0, 0x8, 0x0, 0x4, 0x4, 0x0, 0x4, 0x0, 0x4, 0xfffc, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x8, 0xfffc, 0x4, 0x4, 0x4, 0x4, 0x8, 0x8, 0x4, 0xfffc, 0xfffc, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x8, 0x8, 0x0, 0x8, 0xfffc, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x8, 0x0, 0x8, 0x8 };

        /// <summary>
        /// a rotator is a float that holds current axis rotation for sky. May be malformed by skyRotators or TimeCompression magic
        /// </summary>
        private float localRotator = 0.0f;

        private ModelGroup[] modelGroups;

        #endregion Fields

        #region Constructors

        public Stage()
        { }

        #endregion Constructors

        #region Properties

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
            string filename = Memory.Encounters.Current().Filename;
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

            s.modelGroups = new ModelGroup[4]
            {
                    ModelGroup.Read(objectsGroups[0].objectListPointer,br),
                    ModelGroup.Read(objectsGroups[1].objectListPointer,br),
                    ModelGroup.Read(objectsGroups[2].objectListPointer,br),
                    ModelGroup.Read(objectsGroups[3].objectListPointer,br)
            };
            s.ReadTexture(MainSection.TexturePointer, br);
            return s;
        }

        public void Draw()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            using (AlphaTestEffect ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            {
                Projection = projectionMatrix,
                View = viewMatrix,
                World = worldMatrix
            })
            using (BasicEffect effect = new BasicEffect(Memory.graphics.GraphicsDevice)
            {
                TextureEnabled = true
            })
            {
                for (int n = 0; n < (modelGroups?.Length ?? 0); n++)
                    foreach (Model b in modelGroups[n])
                    {
                        GeometryVertexPosition vpt = GetVertexBuffer(b);
                        if (n == 3 && skyRotators[Memory.Encounters.Current().Scenario] != 0)
                            CreateRotation(vpt);
                        if (vpt == null) continue;
                        int localVertexIndex = 0;
                        for (int i = 0; i < vpt.GeometryInfoSupplier.Length; i++)
                        {
                            ate.Texture = (Texture2D)textures[vpt.GeometryInfoSupplier[i].clut]; //provide texture per-face
                            foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                            {
                                pass.Apply();
                                if (vpt.GeometryInfoSupplier[i].bQuad)
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
        { }

        private static Vector2 CalculateUV(byte U, byte V, byte texPage, int texWidth)
        {
            //old code from my wiki page
            //Float U = (float)U_Byte / (float)(TIM_Texture_Width * 2) + ((float)Texture_Page / (TIM_Texture_Width * 2));
            float fU = (float)U / texWidth + (((float)texPage * 128) / texWidth);
            float fV = V / 256.0f;
            return new Vector2(fU, fV);
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
            localRotator += (short)skyRotators[Memory.Encounters.Current().Scenario] / 4096f * Memory.gameTime.ElapsedGameTime.Milliseconds;
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
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.triangles[i].U2, model.triangles[i].V2, model.triangles[i].TexturePage, Width)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)B.X / 100, (float)B.Y / 100, (float)B.Z / 100),
                    CalculateUV(model.triangles[i].U3, model.triangles[i].V3, model.triangles[i].TexturePage, Width)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)C.X / 100, (float)C.Y / 100, (float)C.Z / 100),
                    CalculateUV(model.triangles[i].U1, model.triangles[i].V1, model.triangles[i].TexturePage, Width)));
                bs_renderer_supplier.Add(new GeometryInfoSupplier()
                {
                    bQuad = false,
                    clut = model.triangles[i].clut,
                    texPage = model.triangles[i].TexturePage
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
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.quads[i].U1, model.quads[i].V1, model.quads[i].TexturePage, Width)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)B.X / 100, (float)B.Y / 100, (float)B.Z / 100),
                    CalculateUV(model.quads[i].U2, model.quads[i].V2, model.quads[i].TexturePage, Width)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)D.X / 100, (float)D.Y / 100, (float)D.Z / 100),
                    CalculateUV(model.quads[i].U4, model.quads[i].V4, model.quads[i].TexturePage, Width)));

                //1 3 4
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)D.X / 100, (float)D.Y / 100, (float)D.Z / 100),
                    CalculateUV(model.quads[i].U4, model.quads[i].V4, model.quads[i].TexturePage, Width)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)C.X / 100, (float)C.Y / 100, (float)C.Z / 100),
                    CalculateUV(model.quads[i].U3, model.quads[i].V3, model.quads[i].TexturePage, Width)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.quads[i].U1, model.quads[i].V1, model.quads[i].TexturePage, Width)));

                bs_renderer_supplier.Add(new GeometryInfoSupplier()
                {
                    bQuad = true,
                    clut = model.quads[i].clut,
                    texPage = model.quads[i].TexturePage
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
            Width = textureInterface.GetWidth;
            Height = textureInterface.GetHeight;
            string path = Path.Combine(Path.GetTempPath(), "Battle Stages");
            Directory.CreateDirectory(path);
            textureInterface.SaveCLUT(Path.Combine(path, $"{Path.GetFileNameWithoutExtension(Memory.Encounters.Current().Filename)}_Clut.png"));
            textures = new TextureHandler[textureInterface.GetClutCount];
            for (ushort i = 0; i < textureInterface.GetClutCount; i++)
            {
                textures[i] = TextureHandler.Create(Memory.Encounters.Current().Filename, textureInterface, i);
                textures[i].Save(path);
            }
        }

        #endregion Methods
    }
}