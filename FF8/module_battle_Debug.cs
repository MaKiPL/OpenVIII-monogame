using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FF8
{
    class Module_battle_debug
    {
        private static uint bs_cameraPointer;
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static float degrees, Yshift;
        private static readonly float camDistance = 10.0f;
        private static Vector3 camPosition, camTarget;
        private static TIM2 textureInterface;
        private static Texture2D[] textures;


        //skyRotating floats are hardcoded
        private static readonly ushort[] skyRotators = { 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x10, 0x10, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x2, 0x0, 0x0, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x4, 0x0, 0x8, 0x0, 0x4, 0x4, 0x0, 0x4, 0x0, 0x4, 0xfffc, 0x8, 0xfffc, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x0, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x8, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x4, 0x4, 0x8, 0xfffc, 0x4, 0x4, 0x4, 0x4, 0x8, 0x8, 0x4, 0xfffc, 0xfffc, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0xfffc, 0x0, 0x0, 0x0, 0x0, 0x8, 0x8, 0x0, 0x8, 0xfffc, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x0, 0x0, 0x4, 0x4, 0x4, 0x4, 0x4, 0x0, 0x0, 0x8, 0x0, 0x8, 0x8 };
        static float localRotator = 0.0f;



        public static BasicEffect effect;
        public static AlphaTestEffect ate;
        private static BattleCamera battleCamera;

        private static string battlename = "a0stg000.x";
        private static byte[] stageBuffer;

        private static int battleModule = 0;

        private const int BATTLEMODULE_INIT = 0;
        private const int BATTLEMODULE_READDATA = 1;
        private const int BATTLEMODULE_DRAWGEOMETRY = 2;
        private const int BATTLEMODULE_CAMERAINTRO = 3;
        private const int BATTLEMODULE_ACTIVE = 4;
        private const float FPS = 1000.0f / 15f;

        private struct BS_RENDERER_ADD
        {
            public bool bQuad;
            public byte clut;
            public byte texPage;
        }


        private struct BattleCamera
        {
            public BattleCameraSettings battleCameraSettings;
            public BattleCameraCollection battleCameraCollection;
        }

        private struct BattleCameraSettings
        {
            public byte[] unk;
        }

        private struct BattleCameraCollection
        {
            public uint cAnimCollectionCount;
            public uint pCameraEOF;
            public BattleCameraSet[] battleCameraSet;
        }

        private struct BattleCameraSet
        {
            public uint[] animPointers;
            public uint globalSetPointer;
            public CameraAnimation[] cameraAnimation;
        }

        private struct CameraAnimation
        {
            public ushort header;

        }

        private struct Triangle
        {
            public ushort A;
            public ushort B;
            public ushort C;
            public byte U1;
            public byte V1;
            public byte U2;
            public byte V2;
            public byte clut;
            public byte U3;
            public byte V3;
            public byte TexturePage;
            public byte bHide;
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte GPU;
        }

        private struct Quad
        {
            public ushort A;
            public ushort B;
            public ushort C;
            public ushort D;
            public byte U1;
            public byte V1;
            public byte clut;
            public byte U2;
            public byte V2;
            public byte TexturePage;
            public byte bHide;
            public byte U3;
            public byte V3;
            public byte U4;
            public byte V4;
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte GPU;
        }

        private struct MainGeometrySection
        {
            public uint Group1Pointer;
            public uint Group2Pointer;
            public uint Group3Pointer;
            public uint Group4Pointer;
            public uint TextureUNUSEDPointer;
            public uint TexturePointer;
            public uint EOF;
        }

        private struct ObjectsGroup
        {
            public uint numberOfSections;
            public uint settings1Pointer;
            public uint objectListPointer;
            public uint settings2Pointer;
            public uint relativeEOF;
        }

        private struct Vertex
        {
            public short X;
            public short Y;
            public short Z;
        }

        private struct Model
        {
            public Vertex[] vertices;
            public Triangle[] triangles;
            public Quad[] quads;
        }

        private struct ModelGroup
        {
            public Model[] models;
        }

        private static ModelGroup[] modelGroups;

        private static Debug_battleDat[] monstersData;
        private static CharacterData[] charactersData;

        private struct CharacterData
        {
            public Debug_battleDat character, weapon;
        };

        private static PseudoBufferedStream pbs;

        private static byte GetTexturePage(byte texturepage) => (byte)(texturepage & 0x0F);

        private static byte GetClutId(ushort clut)
        {
            ushort bb = MakiExtended.UshortLittleEndian(clut);
            return (byte)(((bb >> 14) & 0x03) | (bb << 2) & 0x0C);
        }

        public static void ResetState() { battleModule = BATTLEMODULE_INIT; }

        public static void Update()
        {
            switch (battleModule)
            {
                case BATTLEMODULE_INIT:
                    InitBattle();
                    break;
                case BATTLEMODULE_READDATA:
                    ReadData();
                    break;
                case BATTLEMODULE_DRAWGEOMETRY:
                    if (Input.GetInputDelayed(Keys.OemTilde))
                        DEBUGframe++;
                    if (Input.GetInputDelayed(Keys.Tab))
                        DEBUGframe--;
                    FPSCamera();
                    break;
            }
        }

        public static void Draw()
        {
            switch (battleModule)
            {
                case BATTLEMODULE_DRAWGEOMETRY:
                    DrawGeometry();
                    DrawMonsters();
                    DrawCharactersWeapons();
                    break;

            }
        }

        /// <summary>
        /// Method to render characters and weapons for them
        /// </summary>
        private static void DrawCharactersWeapons()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;
            effect.TextureEnabled = true;

            for (int n = 0; n < charactersData.Length; n++)
            {
                frame[n+monstersData.Length] = frame[n+monstersData.Length] == charactersData[n].character.animHeader.animations[0].cFrames ? 0 : frame[n];
                for (int i = 0; i < charactersData[n].character.geometry.cObjects; i++)
                {
                    var a = charactersData[n].character.GetVertexPositions(i, new Vector3(-20 + n * 10, 10, 0),0, frame[n], frameperFPS / FPS); //DEBUG
                    if (a == null || a.Length == 0)
                        return;
                    ate.Texture = charactersData[n].character.textures.textures[0];
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                        vertexData: a, vertexOffset: 0, primitiveCount: a.Length / 3);
                    }
                }
            }
            for (int n = 0; n < charactersData.Length; n++)
            {
                frame[n + monstersData.Length] = frame[n + monstersData.Length] == charactersData[n].weapon.animHeader.animations[0].cFrames ? 0 : frame[n];
                for (int i = 0; i < charactersData[n].weapon.geometry.cObjects; i++)
                {
                    var a = charactersData[n].weapon.GetVertexPositions(i, new Vector3(-20+n*10,10, 0),0, frame[n], frameperFPS / FPS); //DEBUG
                    if (a == null || a.Length == 0)
                        return;
                    ate.Texture = charactersData[n].weapon.textures.textures[0];
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                        vertexData: a, vertexOffset: 0, primitiveCount: a.Length / 3);
                    }
                }
            }
        }

        private static int[] frame;
        private static int DEBUGframe = 0;
        private static void DrawMonsters()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;
            effect.TextureEnabled = true;

            float tick = Memory.gameTime.ElapsedGameTime.Milliseconds;
                frameperFPS += tick;
                if (frameperFPS > FPS)
                {
                for (int x = 0; x < frame.Length; x++)
                    frame[x] += (int)(frameperFPS/FPS);
                frameperFPS = 0.0f;
                }
            for (int n = 0; n < monstersData.Length; n++)
            {
                if(monstersData[n].GetId == 127)
                {
                    //TODO;
                    continue;
                }
                frame[n] = frame[n] == monstersData[n].animHeader.animations[0].cFrames ? 0 : frame[n];
                for (int i = 0; i < monstersData[n].geometry.cObjects; i++)
                {
                    var a = monstersData[n].GetVertexPositions(i, new Vector3(-50+(n%12)*10, 50, (n/12)*10), 0, frame[n],frameperFPS/FPS); //DEBUG
                    if (a == null || a.Length == 0)
                        return;
                    ate.Texture = monstersData[n].textures.textures[0];
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                        vertexData: a, vertexOffset: 0, primitiveCount: a.Length / 3);
                    }
                }
            }
        }

        const float defaultmaxMoveSpeed = 1f;
        const float MoveSpeedChange = 1f;
        static float maxMoveSpeed = defaultmaxMoveSpeed;
        const float maxLookSpeed = 0.25f;
        public static void FPSCamera()
        {
            #region FPScamera
            float x_shift = 0.0f, y_shift = 0.0f, leftdistX = 0.0f, leftdistY = 0.0f;

            //speedcontrols
            //+ to increase
            //- to decrease
            //* to reset            
            if (Input.Button(Keys.OemPlus) || Input.Button(Keys.Add))
            {
                maxMoveSpeed += MoveSpeedChange;
            }
            if (Input.Button(Keys.OemMinus) || Input.Button(Keys.Subtract))
            {
                maxMoveSpeed -= MoveSpeedChange;
                if (maxMoveSpeed < defaultmaxMoveSpeed) maxMoveSpeed = defaultmaxMoveSpeed;
            }
            if (Input.Button(Keys.Multiply)) maxMoveSpeed = defaultmaxMoveSpeed;

            //speed is effected by the milliseconds between frames. so alittle goes a long way. :P
            x_shift = Input.Distance(Buttons.MouseXjoy, maxLookSpeed);
            y_shift = Input.Distance(Buttons.MouseYjoy, maxLookSpeed);
            leftdistX = Math.Abs(Input.Distance(Buttons.LeftStickX, maxMoveSpeed));
            leftdistY = Math.Abs(Input.Distance(Buttons.LeftStickY, maxMoveSpeed));
            x_shift += Input.Distance(Buttons.RightStickX, maxLookSpeed);
            y_shift += Input.Distance(Buttons.RightStickY, maxLookSpeed);
            Yshift -= y_shift;
            degrees = (degrees + (int)x_shift) % 360;
            Yshift = MathHelper.Clamp(Yshift, -80, 80);
            if (leftdistY == 0)
            {
                leftdistY = Input.Distance(maxMoveSpeed);
            }
            if (leftdistX == 0)
            {
                leftdistX = Input.Distance(maxMoveSpeed);
            }

            if (Input.Button(Buttons.Up))
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Y -= Yshift / 50;
            }
            if (Input.Button(Buttons.Down))
            {
                camPosition.X -= (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Z -= (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Y += Yshift / 50;
            }
            if (Input.Button(Buttons.Left))
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees - 90)) * leftdistX / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees - 90)) * leftdistX / 10;
            }
            if (Input.Button(Buttons.Right))
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees + 90)) * leftdistX / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees + 90)) * leftdistX / 10;
            }

            //Input.LockMouse();

            camTarget.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Y = camPosition.Y - Yshift / 5;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            #endregion
        }
        private static void DrawGeometry()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);

            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;




            effect.TextureEnabled = true;
            for(int n = 0; n < modelGroups.Length; n++)
                foreach (var b in modelGroups[n].models)
                {
                    var vpt = GetVertexBuffer(b);
                    if (n == 3 && skyRotators[Memory.encounters[Memory.battle_encounter].bScenario] != 0)
                        CreateRotation(vpt);
                    if (vpt == null) continue;
                    int localVertexIndex = 0;
                    for (int i = 0; i < vpt.Item1.Length; i++)
                    {
                        ate.Texture = textures[vpt.Item1[i].clut]; //provide texture per-face
                        foreach (var pass in ate.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            if (vpt.Item1[i].bQuad)
                            {
                                Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                                vertexData: vpt.Item2, vertexOffset: localVertexIndex, primitiveCount: 2);
                                localVertexIndex += 6;
                            }
                            else
                            {
                                Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                                vertexData: vpt.Item2, vertexOffset: localVertexIndex, primitiveCount: 1);
                                localVertexIndex += 3;
                            }
                        }
                    }

                }

            Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(Font.CipherDirty($"Encounter ready at: {Memory.battle_encounter}"), 0, 0, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Camera: {Memory.encounters[Memory.battle_encounter].bCamera}"), 20, 30, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Enemies: {string.Join(",", Memory.encounters[Memory.battle_encounter].BEnemies.Where(x => x != 0x00).Select(x => "0x" + (x - 0x10).ToString("X02")).ToArray())}"), 20, 30 * 2, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Levels: {string.Join(",", Memory.encounters[Memory.battle_encounter].bLevels)}"), 20, 30 * 3, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Loaded enemies: {Convert.ToString(Memory.encounters[Memory.battle_encounter].bLoadedEnemy, 2)}"), 20, 30 * 4, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Debug frame: {DEBUGframe}"), 20, 30 * 5, 1, 1, 0, 1);
            Memory.SpriteBatchEnd();
        }

        private static void CreateRotation(Tuple<BS_RENDERER_ADD[], VertexPositionTexture[]> vpt)
        {
            localRotator += (short)skyRotators[Memory.encounters[Memory.battle_encounter].bScenario]/4096f * Memory.gameTime.ElapsedGameTime.Milliseconds;
            if (localRotator <= 0)
                return;
            for (int i = 0; i < vpt.Item2.Length; i++)
                vpt.Item2[i].Position = Vector3.Transform(vpt.Item2[i].Position, Matrix.CreateRotationY(MathHelper.ToRadians(localRotator)));
        }

        private static Tuple<BS_RENDERER_ADD[], VertexPositionTexture[]> GetVertexBuffer(Model model)
        {
            //draw model triangles
            //every triangle have three vertices, so...
            List<VertexPositionTexture> vptDynamic = new List<VertexPositionTexture>();
            List<BS_RENDERER_ADD> bs_renderer_supplier = new List<BS_RENDERER_ADD>();
            if (model.vertices == null) return null;
            for (int i = 0; i < model.triangles.Length; i++)
            {
                Vertex A = model.vertices[model.triangles[i].A];
                Vertex B = model.vertices[model.triangles[i].B];
                Vertex C = model.vertices[model.triangles[i].C];
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.triangles[i].U2, model.triangles[i].V2, model.triangles[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)B.X / 100, (float)B.Y / 100, (float)B.Z / 100),
                    CalculateUV(model.triangles[i].U3, model.triangles[i].V3, model.triangles[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)C.X / 100, (float)C.Y / 100, (float)C.Z / 100),
                    CalculateUV(model.triangles[i].U1, model.triangles[i].V1, model.triangles[i].TexturePage, textureInterface.GetWidth)));
                bs_renderer_supplier.Add(new BS_RENDERER_ADD()
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
                    CalculateUV(model.quads[i].U1, model.quads[i].V1, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)B.X / 100, (float)B.Y / 100, (float)B.Z / 100),
                    CalculateUV(model.quads[i].U2, model.quads[i].V2, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)D.X / 100, (float)D.Y / 100, (float)D.Z / 100),
                    CalculateUV(model.quads[i].U4, model.quads[i].V4, model.quads[i].TexturePage, textureInterface.GetWidth)));

                //1 3 4
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X / 100, (float)A.Y / 100, (float)A.Z / 100),
                    CalculateUV(model.quads[i].U1, model.quads[i].V1, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)C.X / 100, (float)C.Y / 100, (float)C.Z / 100),
                    CalculateUV(model.quads[i].U3, model.quads[i].V3, model.quads[i].TexturePage, textureInterface.GetWidth)));
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)D.X / 100, (float)D.Y / 100, (float)D.Z / 100),
                    CalculateUV(model.quads[i].U4, model.quads[i].V4, model.quads[i].TexturePage, textureInterface.GetWidth)));

                bs_renderer_supplier.Add(new BS_RENDERER_ADD()
                {
                    bQuad = true,
                    clut = model.quads[i].clut,
                    texPage = model.quads[i].TexturePage
                });
            }
            return new Tuple<BS_RENDERER_ADD[], VertexPositionTexture[]>
                (bs_renderer_supplier.ToArray(), vptDynamic.ToArray());
        }

        private static Vector2 CalculateUV(byte U, byte V, byte texPage, int texWidth)
        {
            //old code from my wiki page
            //Float U = (float)U_Byte / (float)(TIM_Texture_Width * 2) + ((float)Texture_Page / (TIM_Texture_Width * 2));
            float fU = (float)U / texWidth + (((float)texPage * 128) / texWidth);
            float fV = V / 256.0f;
            return new Vector2(fU, fV);

        }

        private static void InitBattle()
        {
            Input.OverrideLockMouse=true;
            Input.CurrentMode = Input.MouseLockMode.Center;

            Init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            int stage = enc.bScenario;
            battlename = $"a0stg{stage.ToString("000")}.x";
            Console.WriteLine($"BS_DEBUG: Loading stage {battlename}");
            Console.WriteLine($"BS_DEBUG/ENC: Encounter: {Memory.battle_encounter}\t cEnemies: {enc.bNumOfEnemies}\t Enemies: {string.Join(",", enc.BEnemies.Where(x => x != 0x00).Select(x => $"0x{(x - 0x10).ToString("X02")}").ToArray())}");


            //init renderer
            effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            camTarget = new Vector3(0, 0f, 0f);
            camPosition = new Vector3(0f, 50f, -100f);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f),
                               Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);
            battleModule++;
            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            {
                Projection = projectionMatrix,
                View = viewMatrix,
                World = worldMatrix
            };
            return;
        }

        #region fileParsing

        private static void ReadData()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string[] test = aw.GetListOfFiles();
            battlename = test.First(x => x.ToLower().Contains(battlename));
            stageBuffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, battlename);
            pbs = new PseudoBufferedStream(stageBuffer);
            bs_cameraPointer = GetCameraPointer();
            pbs.Seek(bs_cameraPointer, 0);
            ReadCamera();
            uint sectionCounter = pbs.ReadUInt();
            if (sectionCounter != 6)
            {
                Console.WriteLine($"BS_PARSER_PRE_OBJECTSECTION: Main geometry section has no 6 pointers at: {pbs.Tell()}");
                battleModule++;
                return;
            }
            MainGeometrySection MainSection = ReadObjectGroupPointers();
            ObjectsGroup[] objectsGroups = new ObjectsGroup[4]
            {
                ReadObjectsGroup(MainSection.Group1Pointer),
                ReadObjectsGroup(MainSection.Group2Pointer),
                ReadObjectsGroup(MainSection.Group3Pointer),
                ReadObjectsGroup(MainSection.Group4Pointer)
            };

            modelGroups = new ModelGroup[4]
            {
                ReadModelGroup(objectsGroups[0].objectListPointer),
                ReadModelGroup(objectsGroups[1].objectListPointer),
                ReadModelGroup(objectsGroups[2].objectListPointer),
                ReadModelGroup(objectsGroups[3].objectListPointer)
            };

            ReadTexture(MainSection.TexturePointer);

            ReadCharacters();
            ReadMonster();

            //for frames indexes monsters are first, then after n monsters characters appear with weapons
            frame = new int[monstersData.Length + charactersData.Length];

            battleModule++;
        }

        public static int DEBUG = 0;
        private static float frameperFPS = 0.0f;

        private static void ReadCharacters()
        {
            //DEBUG - party provider here
            charactersData = new CharacterData[2];
            charactersData[0] = new CharacterData
            {
                character = new Debug_battleDat(0, Debug_battleDat.EntityType.Character, 0),
                weapon = new Debug_battleDat(0, Debug_battleDat.EntityType.Weapon, 0)
            };
            charactersData[1] = new CharacterData
            {
                character = new Debug_battleDat(2, Debug_battleDat.EntityType.Character, 6),
                weapon = new Debug_battleDat(2, Debug_battleDat.EntityType.Weapon, 13)
            };
        }

        private static void ReadMonster()
        {
            Init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            if (enc.bNumOfEnemies == 0)
                return;
            //DEBUG BELOW; I just want to draw any model
            monstersData = new Debug_battleDat[1];
            monstersData[0] = new Debug_battleDat(26, Debug_battleDat.EntityType.Monster);
            //for (int n = 26; n <= monstersData.Length; n++)
            //    monstersData[n] = new Debug_battleDat(n, Debug_battleDat.EntityType.Monster);
            //END OF DEBUG
        }

        private static void ReadTexture(uint texturePointer)
        {
            textureInterface = new TIM2(stageBuffer, texturePointer);
            textures = new Texture2D[textureInterface.GetClutCount];
            for (int i = 0; i < textureInterface.GetClutCount; i++)
            {
                byte[] b = textureInterface.CreateImageBuffer(textureInterface.GetClutColors(i));
                Texture2D tex = new Texture2D(Memory.spriteBatch.GraphicsDevice,
                    textureInterface.GetWidth, textureInterface.GetHeight, false, SurfaceFormat.Color);
                tex.SetData(b);
                textures[i] = tex;
            }
        }

        private static ModelGroup ReadModelGroup(uint pointer)
        {
            pbs.Seek(pointer, System.IO.SeekOrigin.Begin);
            uint modelsCount = pbs.ReadUInt();
            Model[] models = new Model[modelsCount];
            uint[] modelPointers = new uint[modelsCount];
            for (int i = 0; i < modelsCount; i++)
                modelPointers[i] = pointer + pbs.ReadUInt();
            for (int i = 0; i < modelsCount; i++)
                models[i] = ReadModel(modelPointers[i]);
            return new ModelGroup() { models = models };
        }

        private static Model ReadModel(uint pointer)
        {
            bool bSpecial = false;
            pbs.Seek(pointer, System.IO.SeekOrigin.Begin);
            uint header = MakiExtended.UintLittleEndian(pbs.ReadUInt());
            if (header != 0x01000100)
            {
                Console.WriteLine("WARNING- THIS STAGE IS DIFFERENT! It has weird object section. INTERESTING, TO REVERSE!");
                bSpecial = true;
            }
            ushort verticesCount = pbs.ReadUShort();
            Vertex[] vertices = new Vertex[verticesCount];
            for (int i = 0; i < verticesCount; i++)
                vertices[i] = ReadVertex();
            if (bSpecial && Memory.encounters[Memory.battle_encounter].bScenario == 20)
                return new Model();
            pbs.Seek((pbs.Tell() % 4) + 4, System.IO.SeekOrigin.Current);
            ushort trianglesCount = pbs.ReadUShort();
            ushort quadsCount = pbs.ReadUShort();
            pbs.Seek(4, System.IO.SeekOrigin.Current);
            Triangle[] triangles = new Triangle[trianglesCount];
            Quad[] quads = new Quad[quadsCount];
            if (trianglesCount > 0)
                for (int i = 0; i < trianglesCount; i++)
                    triangles[i] = ReadTriangle();
            if (quadsCount > 0)
                for (int i = 0; i < quadsCount; i++)
                    quads[i] = ReadQuad();
            return new Model()
            {
                vertices = vertices,
                triangles = triangles,
                quads = quads
            };
        }

        private static Triangle ReadTriangle()
        =>
            new Triangle()
            {
                A = pbs.ReadUShort(),
                B = pbs.ReadUShort(),
                C = pbs.ReadUShort(),
                U1 = pbs.ReadByte(),
                V1 = pbs.ReadByte(),
                U2 = pbs.ReadByte(),
                V2 = pbs.ReadByte(),
                clut = GetClutId(pbs.ReadUShort()),
                U3 = pbs.ReadByte(),
                V3 = pbs.ReadByte(),
                TexturePage = GetTexturePage(pbs.ReadByte()),
                bHide = pbs.ReadByte(),
                Red = pbs.ReadByte(),
                Green = pbs.ReadByte(),
                Blue = pbs.ReadByte(),
                GPU = pbs.ReadByte()
            };

        private static Quad ReadQuad()
        => new Quad()
        {
            A = pbs.ReadUShort(),
            B = pbs.ReadUShort(),
            C = pbs.ReadUShort(),
            D = pbs.ReadUShort(),
            U1 = pbs.ReadByte(),
            V1 = pbs.ReadByte(),
            clut = GetClutId(pbs.ReadUShort()),
            U2 = pbs.ReadByte(),
            V2 = pbs.ReadByte(),
            TexturePage = GetTexturePage(pbs.ReadByte()),
            bHide = pbs.ReadByte(),
            U3 = pbs.ReadByte(),
            V3 = pbs.ReadByte(),
            U4 = pbs.ReadByte(),
            V4 = pbs.ReadByte(),
            Red = pbs.ReadByte(),
            Green = pbs.ReadByte(),
            Blue = pbs.ReadByte(),
            GPU = pbs.ReadByte()
        };

        private static Vertex ReadVertex()
        => new Vertex()
        {
            X = pbs.ReadShort(),
            Y = pbs.ReadShort(),
            Z = pbs.ReadShort()
        };


        private static ObjectsGroup ReadObjectsGroup(uint pointer)
        {
            pbs.Seek(pointer, System.IO.SeekOrigin.Begin);
            return new ObjectsGroup()
            {
                numberOfSections = pbs.ReadUInt(),
                settings1Pointer = pointer + pbs.ReadUInt(),
                objectListPointer = pointer + pbs.ReadUInt(),
                settings2Pointer = pointer + pbs.ReadUInt(),
                relativeEOF = pointer + pbs.ReadUInt()
            };
        }

        private static MainGeometrySection ReadObjectGroupPointers()
        {
            int basePointer = (int)pbs.Tell() - 4;
            uint objectGroup_1 = (uint)basePointer + pbs.ReadUInt();
            uint objectGroup_2 = (uint)basePointer + pbs.ReadUInt();
            uint objectGroup_3 = (uint)basePointer + pbs.ReadUInt();
            uint objectGroup_4 = (uint)basePointer + pbs.ReadUInt();
            uint TextureUnused = (uint)basePointer + pbs.ReadUInt();
            uint Texture = (uint)basePointer + pbs.ReadUInt();
            uint EOF = (uint)basePointer + pbs.ReadUInt();
            //if (pbs.Length != (pbs.Tell() - 6 * 4) + EOF) 
            if (EOF != pbs.Length) //I though EOF is relative EOF, not global, lol
                throw new Exception("BS_PARSER_ERROR_LENGTH: Geometry EOF pointer is other than buffered filesize");

            return new MainGeometrySection()
            {
                Group1Pointer = objectGroup_1,
                Group2Pointer = objectGroup_2,
                Group3Pointer = objectGroup_3,
                Group4Pointer = objectGroup_4,
                TextureUNUSEDPointer = TextureUnused,
                TexturePointer = Texture,
                EOF = EOF
            }; //EOF = EOF; beauty of language
        }

        private static uint GetCameraPointer()
        {
            int[] _x5D4 = {4,5,9,12,13,14,15,21,22,23,24,26,
29,32,33,34,35,36,39,40,50,53,55,61,62,63,64,65,66,67,68,69,70,
71,72,73,75,78,82,83,85,86,87,88,89,90,91,94,96,97,98,99,100,105,
106,121,122,123,124,125,126,127,135,138,141,144,145,148,149,150,
151,158,160};

            int[] _x5D8 = {
0,1,2,3,6,7,10,11,17,18,25,27,28,38,41,42,43,47,49,57,58,59,60,74,
76,77,80,81,84,93,95,101,102,103,104,109,110,111,112,113,114,115,116,
117,118,119,120,128,129,130,131,132,133,134,139,140,143,146,152,153,154,
155,156,159,161,162};

            int _5d4 = _x5D4.Count(x => x == Memory.encounters[Memory.battle_encounter].bScenario);
            int _5d8 = _x5D8.Count(x => x == Memory.encounters[Memory.battle_encounter].bScenario);
            if (_5d4 > 0) return 0x5D4;
            if (_5d8 > 0) return 0x5D8;
            switch (Memory.encounters[Memory.battle_encounter].bScenario)
            {
                case 8:
                case 48:
                case 79:
                    return 0x618;
                case 16:
                    return 0x628;
                case 19:
                    return 0x644;
                case 20:
                    return 0x61c;
                case 30:
                case 31:
                    return 0x934;
                case 37:
                    return 0xcc0;
                case 44:
                case 45:
                case 46:
                    return 0x9A4;
                case 51:
                case 52:
                case 107:
                case 108:
                    return 0x600;
                case 54:
                case 56:
                    return 0x620;
                case 92:
                    return 0x83c;
                case 136:
                    return 0x5fc;
                case 137:
                    return 0xFDC; 
                case 142:
                    return 0x183C;
                case 147:
                    return 0xa0c;
                case 157:
                    return 0x638;

            }
            throw new Exception("0xFFF, unknown pointer!");
        }

        private static void ReadCamera()
        {
            Memory.BS_CameraStruct = new Memory.VIII_cameraMemoryStruct();
            uint cCameraHeaderSector = pbs.ReadUShort();
            //if (cCameraHeaderSector != 0x2)
            //    ; //error handler?
            uint pCameraSetting = pbs.ReadUShort();
            uint pCameraAnimationCollection = pbs.ReadUShort();
            uint sCameraDataSize = pbs.ReadUShort();

            //Camera settings parsing?
            BattleCameraSettings bcs = new BattleCameraSettings() { unk = pbs.ReadBytes(24) };
            //end of camera settings parsing



            pbs.Seek(bs_cameraPointer, 0);
            pbs.Seek(pCameraAnimationCollection, System.IO.SeekOrigin.Current);
            BattleCameraCollection bcc = new BattleCameraCollection { cAnimCollectionCount = pbs.ReadUShort() };
            BattleCameraSet[] bcset = new BattleCameraSet[bcc.cAnimCollectionCount];
            bcc.battleCameraSet = bcset;
            for (int i = 0; i < bcc.cAnimCollectionCount; i++)
                bcset[i] = new BattleCameraSet() { globalSetPointer = (uint)(pbs.Tell() + pbs.ReadUShort() - i * 2 - 2) };
            bcc.pCameraEOF = pbs.ReadUShort();

            for (int i = 0; i < bcc.cAnimCollectionCount; i++)
            {
                pbs.Seek(bcc.battleCameraSet[i].globalSetPointer, 0);
                bcc.battleCameraSet[i].animPointers = new uint[8];
                for (int n = 0; n < bcc.battleCameraSet[i].animPointers.Length; n++)
                    bcc.battleCameraSet[i].animPointers[n] = (uint)(pbs.Tell() + pbs.ReadUShort() * 2 - n * 2);
                bcc.battleCameraSet[i].cameraAnimation = new CameraAnimation[bcc.battleCameraSet[i].animPointers.Length];
                for (int n = 0; n < bcc.battleCameraSet[i].animPointers.Length; n++)
                {
                    pbs.Seek(bcc.battleCameraSet[i].animPointers[n], 0);
                    bcc.battleCameraSet[i].cameraAnimation[n] = new CameraAnimation() { header = pbs.ReadUShort() };
                }
            }

            battleCamera = new BattleCamera() { battleCameraCollection = bcc, battleCameraSettings = bcs };

            //DEBUG DELETE ME
            ReadAnimation(7);
            //END OF DEBUG

            pbs.Seek(bs_cameraPointer + sCameraDataSize, 0); //debug out
        }


        //WIP debug only, used for reverse engineering
        private static void ReadAnimation(int animId)
        {
            Memory.BS_CameraStruct.camAnimId = (byte)animId;
            if ((animId >> 4) >= battleCamera.battleCameraCollection.cAnimCollectionCount)
                return;
            var pointer = battleCamera.battleCameraCollection.battleCameraSet[animId >> 4].animPointers[animId & 0xF];
            pbs.Seek(pointer, 0);
            ushort eax = pbs.ReadUShort();
            Memory.BS_CameraStruct.mainController = eax; //[esi+2], ax 
            if (eax == 0xFFFF)
                return;
            ushort ebx = eax;
            eax = (ushort)((eax >> 6) & 3);
            eax--;
            if (eax == 0)
            {
                eax = 0x200;
                Memory.BS_CameraStruct.thirdWordController = Memory.BS_CameraStruct.secondWordController = eax;
                goto structFullfiled; 
            }
            eax--;
            if (eax == 0)
            {
                eax = pbs.ReadUShort();
                Memory.BS_CameraStruct.thirdWordController = Memory.BS_CameraStruct.secondWordController = eax;
                goto structFullfiled;
            }
            eax--;
            if (eax != 0)
                goto structFullfiled;
            Memory.BS_CameraStruct.secondWordController = pbs.ReadUShort(); //esi+4
            Memory.BS_CameraStruct.thirdWordController = pbs.ReadUShort(); //esi+6 


        structFullfiled:
            eax = ebx;
            eax = (ushort)((eax >> 8) & 3);
            switch (eax)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }

            //default here
            //there's now some operations to copy next vars
            //see BS_Camera_ReadAnimation+CC 00103B8C
        }


        #endregion
    }
}
