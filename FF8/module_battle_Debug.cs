using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FF8
{
    class module_battle_debug
    {

        //debug battle mode treats encounterID as scene ID and therefore skips
        //character, mechanism and everything loading just for testing purpouses

        //it proceeds with initiating battle as usual with INIT->READDATA->DrawGeometry
        //but skips character + music + camera + mechanism + script + encounter loading
        //after going into state 4 you are free to debug many mechanisms of the battle
        //yet everything remains clearly logic.

        private static uint bs_cameraPointer = 0x00;
        private static Matrix projectionMatrix;
        private static Matrix viewMatrix;
        private static Matrix worldMatrix;
        private static float degrees = 0;
        private static float Yshift = 0;
        private static float camDistance = 10.0f;
        private static Vector3 camPosition;
        private static Vector3 camTarget;
        private static TIM2 textureInterface;
        private static Texture2D[] textures;


        public static BasicEffect effect;

        private static string battlename = "a0stg000.x";
        private static byte[] stageBuffer;

        private static int battleModule = 0;

        private const int BATTLEMODULE_INIT = 0;
        private const int BATTLEMODULE_READDATA = 1;
        private const int BATTLEMODULE_DRAWGEOMETRY = 2;
        private const int BATTLEMODULE_CAMERAINTRO = 3;
        private const int BATTLEMODULE_ACTIVE = 4;

        private struct BS_RENDERER_ADD
        {
            public bool bQuad;
            public byte clut;
            public byte texPage;
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



        private static PseudoBufferedStream pbs;

        private static byte GetTexturePage(byte texturepage) => (byte)(texturepage & 0x0F);

        private static byte GetClutId(ushort clut)
        {
            ushort bb = ushortLittleEndian(clut);
           byte b = (byte)(((bb >> 14) & 0x03) | (bb<<2) & 0x0C);
            return b;
        }

        public static void ResetState() { battleModule = BATTLEMODULE_INIT; }

        public static void Update()
        {
            switch(battleModule)
            {
                case BATTLEMODULE_INIT:
                    InitBattle();
                    break;
                case BATTLEMODULE_READDATA:
                    ReadData();
                    break;
                case BATTLEMODULE_DRAWGEOMETRY:
                    if (Keyboard.GetState().IsKeyDown(Keys.F1)) Memory.module = Memory.MODULE_MOVIETEST;
                    break;
                default:
                    break;
            }
        }

        public static void Draw()
        {
            switch(battleModule)
            {
                case BATTLEMODULE_DRAWGEOMETRY:
                    DrawGeometry();
                    break;

            }
        }

        private static void DrawGeometry()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Memory.graphics.GraphicsDevice.RasterizerState = rasterizerState;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            AlphaTestEffect ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice);
            ate.Projection = projectionMatrix;
            ate.View = viewMatrix;
            ate.World = worldMatrix;

            #region FPScamera

            float x_shift = Mouse.GetState().X - 200;
            float y_shift = 200 - Mouse.GetState().Y;
            x_shift += GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X * 2f;
            Yshift -= y_shift;
            Yshift -= GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y * 2f;
            degrees += (int)x_shift;
            if (degrees < 0)
                degrees = 359;
            if (degrees > 359)
                degrees = 0;
            Yshift = MathHelper.Clamp(Yshift, -80, 80);

            if (Keyboard.GetState().IsKeyDown(Keys.W) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > 0.0f)
            {
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance / 10;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance / 10;
                camPosition.Y -= Yshift / 50;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < 0.0f)
            {
                camPosition.X -= (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance / 10;
                camPosition.Z -= (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance / 10;
                camPosition.Y += Yshift / 50;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0.0f)
            {
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees - 90)) * camDistance / 10;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees - 90)) * camDistance / 10;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0.0f)
            {
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees + 90)) * camDistance / 10;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees + 90)) * camDistance / 10;
            }

            Mouse.SetPosition(200, 200);

            camTarget.X = camPosition.X + (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Z = camPosition.Z + (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Y = camPosition.Y - Yshift / 5;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            #endregion




            effect.TextureEnabled = true;
            foreach (var a in modelGroups)
                foreach (var b in a.models)
                {
                    var vpt = getVertexBuffer(b);
                    if (vpt == null) continue;
                    int localVertexIndex = 0;
                    for (int i = 0; i < vpt.Item1.Length; i++)
                    {
                        ate.Texture = textures[vpt.Item1[i].clut]; //provide texture per-face
                        foreach (var pass in ate.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            if(vpt.Item1[i].bQuad)
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

            //vertices -> set this one to VertexPositionTexture
            //vertices -> texturecoordinate
            //floorVerts = new VertexPositionTexture[6];

            //effect -> View = Matrix.CreateLookAt (
            //cameraPosition, cameraLookAtVector, cameraUpVector);

            //effect.textureenabled=  true;
            //effect.texture

            //graphicsdevice.drawuserprimitives
        }

        private static Tuple<BS_RENDERER_ADD[], VertexPositionTexture[]> getVertexBuffer(Model model)
        {
            //draw model triangles
            //every triangle have three vertices, so...
            List<VertexPositionTexture> vptDynamic = new List<VertexPositionTexture>();
            List<BS_RENDERER_ADD> bs_renderer_supplier = new List<BS_RENDERER_ADD>();
            if (model.vertices == null) return null;
            for (int i = 0; i<model.triangles.Length; i++)
            {
                Vertex A = model.vertices[model.triangles[i].A];
                Vertex B = model.vertices[model.triangles[i].B];
                Vertex C = model.vertices[model.triangles[i].C];
                vptDynamic.Add(new VertexPositionTexture(new Vector3((float)A.X/100, (float)A.Y/100, (float)A.Z / 100),
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
            for(int i = 0; i<model.quads.Length; i++)
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
                (bs_renderer_supplier.ToArray(),vptDynamic.ToArray());
        }

        private static Vector2 CalculateUV(byte U, byte V, byte texPage, int texWidth)
        {
            //old code from my wiki page
            //Float U = (float)U_Byte / (float)(TIM_Texture_Width * 2) + ((float)Texture_Page / (TIM_Texture_Width * 2));
            float fU = (float)U / texWidth + (((float)texPage*128) / texWidth);
            float fV = V / 256.0f;
            return new Vector2(fU, fV);

        }

        private static void InitBattle()
        {
            //DEBUG
            init_debugger_battle.Encounter enc = Memory.encounters[Memory.battle_encounter];
            int stage = enc.bScenario;
            battlename = $"a0stg{stage.ToString("000")}.x";
            Console.WriteLine($"BS_DEBUG: Loading stage {battlename}");

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
            //DEBUG- stop here;
            battleModule++;
            return;
        }

        #region fileParsing

        private static void ReadData()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_BATTLE);
            string[] test = aw.GetListOfFiles();
            battlename = test.Where(x => x.ToLower().Contains(battlename)).First();
            stageBuffer = ArchiveWorker.GetBinaryFile(Memory.Archives.A_BATTLE, battlename);
            pbs = new PseudoBufferedStream(stageBuffer);
            bs_cameraPointer = GetCameraPointer();
            pbs.Seek(bs_cameraPointer, PseudoBufferedStream.SEEK_BEGIN);
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
            battleModule++;
        }

        private static void ReadTexture(uint texturePointer)
        {
            textureInterface = new TIM2(stageBuffer, texturePointer);
            textures = new Texture2D[textureInterface.GetClutCount];
            for(int i = 0; i<textureInterface.GetClutCount; i++)
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
            pbs.Seek(pointer, PseudoBufferedStream.SEEK_BEGIN);
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
            pbs.Seek(pointer, PseudoBufferedStream.SEEK_BEGIN);
            uint header = uintLittleEndian(pbs.ReadUInt());
            if (header != 0x01000100)
            {
                Console.WriteLine("WARNING- THIS STAGE IS DIFFERENT! It has weird object section. INTERESTING, TO REVERSE!");
                bSpecial = true;
            }
            ushort verticesCount = pbs.ReadUShort();
            Vertex[] vertices = new Vertex[verticesCount];
            for (int i = 0; i < verticesCount; i++)
                vertices[i] = ReadVertex();
            if (bSpecial && Memory.battle_encounter == 20)
                return new Model();
            pbs.Seek((pbs.Tell() % 4) + 4, PseudoBufferedStream.SEEK_CURRENT);
            ushort trianglesCount = pbs.ReadUShort();
            ushort quadsCount = pbs.ReadUShort();
            pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT);
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
            pbs.Seek(pointer, PseudoBufferedStream.SEEK_BEGIN);
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
            int basePointer = pbs.Tell() - 4;
            uint objectGroup_1 = (uint)basePointer + pbs.ReadUInt();
            uint objectGroup_2 = (uint)basePointer + pbs.ReadUInt();
            uint objectGroup_3 = (uint)basePointer + pbs.ReadUInt();
            uint objectGroup_4 = (uint)basePointer + pbs.ReadUInt();
            uint TextureUnused = (uint)basePointer + pbs.ReadUInt();
            uint Texture = (uint)basePointer + pbs.ReadUInt();
            uint EOF = (uint)basePointer + pbs.ReadUInt();
            //if (pbs.Length != (pbs.Tell() - 6 * 4) + EOF) 
            if(EOF != pbs.Length) //I though EOF is relative EOF, not global, lol
            throw new Exception("BS_PARSER_ERROR_LENGTH: Geometry EOF pointer is other than buffered filesize");

            return new MainGeometrySection() {Group1Pointer = objectGroup_1,
            Group2Pointer = objectGroup_2, Group3Pointer = objectGroup_3,
            Group4Pointer = objectGroup_4, TextureUNUSEDPointer = TextureUnused,
            TexturePointer = Texture, EOF = EOF}; //EOF = EOF; beauty of language
        }

        //normal way of GetCameraPointer() instead of 163 switches. Why Eidos?
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

            int _5d4 = _x5D4.Count(x => x== Memory.battle_encounter);
            int _5d8 = _x5D8.Count(x => x == Memory.battle_encounter);
            if (_5d4 > 0) return 0x5D4;
            if (_5d8 > 0) return 0x5D8;
            switch (Memory.battle_encounter)
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
                    return 0xFDC; //That one is really giant, what is it? //It's a witch stage, worth to see at MIPS
                case 142:
                    return 0x183C; //That one won! xD //It's a final battle
                case 147:
                    return 0xa0c;
                case 157:
                    return 0x638;

            }
            throw new Exception("0xFFF, unknown pointer!");
        }

        private static void ReadCamera()
        {
            //sub_509970

            uint eax = bs_cameraPointer;
            pbs.ReadUShort(); //null
            ushort cx = pbs.ReadUShort(); //eax+2
            ushort dx = pbs.ReadUShort(); //eax+4
            uint camSettingsPointer = cx + eax;
            uint camAnimPointer = dx + eax;
            uint cameraSize = pbs.ReadUShort();

            //==========DELETE ME AFTER INVESRTIGATING CAMERA
            pbs.Seek(-8, PseudoBufferedStream.SEEK_CURRENT);
            pbs.Seek(bs_cameraPointer + cameraSize, PseudoBufferedStream.SEEK_BEGIN);
            return;
            //==========END OF DELETE ME

            //FF8::509898
            //EAX - monster presentation camera;


            //sub_503520

            pbs.Seek(camAnimPointer, PseudoBufferedStream.SEEK_BEGIN);
            int DebugCameraEAX = 7; //This is relevant to fought monster, which camera animation to use
            //int v2 = (DebugCameraEAX >> 4) & 0xF; //not sure about this, doesn't make sense as higher AL of camera anim mode is lower than available cameras?????
            //ushort availableCameras = pbs.ReadUShort();
            //if (v2 > availableCameras)
            //    throw new Exception("NOT_EXCEPTION: Cameras is bigger, use fixed camera/ no camera animation");
            //int v3 = v2 & 7; //maximum 7 cameras;
            //uint keypointPointer = (uint)(camAnimPointer + (v3 * 2) + 2);
            //pbs.Seek(keypointPointer, PseudoBufferedStream.SEEK_BEGIN);
            //ushort v5 = pbs.ReadUShort();
            //uint anotherPointer = v5 * (uint)2 + keypointPointer;

            DebugCameraEAX &= 0x7;
            pbs.ReadUShort();
            pbs.Seek((DebugCameraEAX - 1) * 2, PseudoBufferedStream.SEEK_BEGIN);
            ushort animationPointer = pbs.ReadUShort();
            pbs.Seek(animationPointer + camAnimPointer, PseudoBufferedStream.SEEK_BEGIN);



            //another pointer is saved then to edx+10 and used at: sub_5035E0->005035F6


            //pbs.Seek(2, PseudoBufferedStream.SEEK_CURRENT);
            //int v2 = pbs.ReadInt();
            //v2 = (v2 >> 4) & 0xF; //?

            //pbs.Seek(camAnimPointer, PseudoBufferedStream.SEEK_BEGIN);
            //ushort a1 = pbs.ReadUShort();
            //if(v2 < a1)
            //{
            //    int v3 = v2 & 7;
            //    uint keypointPointer = (uint)(camAnimPointer + (v3 * 2) + 2);
            //    eax = 0x10000;
            //    eax &= 0xFFFF;

            //}


            //dummy float/ skip camera data
            ushort cameraConst = pbs.ReadUShort();
            if (ushortLittleEndian(cameraConst) != 0x0200) throw new Exception($"BS_PARSER: 0x0200 const not found at {pbs.Tell()}");
            ushort pCameraSetting = pbs.ReadUShort();
            ushort pCameraAnimation = pbs.ReadUShort();
            ushort sCameraDataSize = pbs.ReadUShort();

            //debug camera read
            byte[] cameraSettings = pbs.ReadBytes(24);
            //char cameraAnimMode = (char)pbs.ReadByte();
            //byte stopEnemyBeforeAnim = pbs.ReadByte();

            ushort numOfCameras = pbs.ReadUShort();

            ushort relativePointerToCamera = pbs.ReadUShort();
            ushort relativeEOF = pbs.ReadUShort();
            ushort padding = pbs.ReadUShort();

            List<byte[]> Animations = new List<byte[]>();
            //FF8_EN.exe+103534
            ushort animCount = pbs.ReadUShort();
            ushort[] pointersTest = new ushort[animCount];
            for (int i = 0; i < animCount; i++)
                pointersTest[i] = pbs.ReadUShort();


            //DEBUG MODE, skips camera
            pbs.Seek(-8, PseudoBufferedStream.SEEK_CURRENT);
            pbs.Seek(sCameraDataSize, PseudoBufferedStream.SEEK_CURRENT);
        }

        #endregion

        private static ushort ushortLittleEndian(ushort ushort_)
            => (ushort)((ushort_ << 8) | (ushort_ >> 8));

        private static short shortLittleEndian(short ushort_)
            => (short)((ushort_ << 8) | (ushort_ >> 8));

        private static uint uintLittleEndian(uint uint_)
            => (uint_ << 24) | ((uint_ << 8) & 0x00FF0000) |
            ((uint_ >> 8) & 0x0000FF00) | (uint_ >> 24);

        private static int uintLittleEndian(int uint_)
            => (uint_ << 24) | ((uint_ << 8) & 0x00FF0000) |
            ((uint_ >> 8) & 0x0000FF00) | (uint_ >> 24);
    }
}
