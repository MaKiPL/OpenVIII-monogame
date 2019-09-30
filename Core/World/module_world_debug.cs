using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Core.World;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    public class Module_world_debug
    {
        private static bool bUseCustomShaderTest = true; //enable for testing the shader- mostly learning stuff

        private static FPS_Camera fps_camera;
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static float degrees;
        private static float camDistance = 10.0f;
        private static float renderCamDistance = 1200f;
        private static Vector3 camPosition, camTarget;
        private static Vector3 playerPosition = new Vector3(-9105f, 30f, -4466);
        private static Vector3 lastPlayerPosition = playerPosition;
        public static BasicEffect effect;
        public static AlphaTestEffect ate;
        public static Effect worldShaderModel;
        private const float BEND_VALUE = 1.4f;
        private const float BEND_DISTANCE = 350.0f;
        private readonly static Vector3 BEND_VECTOR = new Vector3(0, -0.01f, 0);
        private readonly static Vector4 FOG_COLOR = new Vector4(2f, 2f, 2f, 0f);

        private enum _worldState
        {
            _0init,
            _1active,
            _9debugFly
        }

        private enum MiniMapState
        {
            noMinimap,
            planet,
            rectangle,
            fullscreen
        }

        //DEBUG
        private const float WORLD_SCALE_MODEL = 16f;

        private static readonly float FOV = 60;

        public static Vector2 segmentPosition;
        private static CharaOne chara;
        private static texl texl;
        private static wmset wmset;
        private static wm2field wm2field;
        private static rail rail;

        private static byte[] wmx;
        private static float DEBUGshit = 1f;
        private const int WM_SEG_SIZE = 0x9000; //World map segment size in file
        private const int WM_SEGMENTS_COUNT = 835;

        #region structures

        private static Segment[] segments;

        private struct Segment
        {
            public int segmentId;
            public SegHeader headerData;
            public Block[] block;

            /// <summary>
            /// parsedTriangle is a struct containing pre-calculated values for world map so the
            /// calculations are one-time operation
            /// </summary>
            public ParsedTriangleData[] parsedTriangle;
        }

        private struct ParsedTriangleData
        {
            public Vector3 A;
            public Vector3 B;
            public Vector3 C;
            public Vector2 uvA;
            public Vector2 uvB;
            public Vector2 uvC;
            public Polygon parentPolygon;
            public BoundingBox boundingBox;
        }

        private struct Block
        {
            public byte polyCount;
            public byte vertCount;
            public byte normalCount;
            public byte unkPadd;
            public Polygon[] polygons;
            public Vertex[] vertices;
            public Normal[] normals;
            public int unkPadd2;
        }

        [StructLayout(LayoutKind.Sequential, Size = 68, Pack = 1)]
        private struct SegHeader
        {
            public uint groupId;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public uint[] blockOffsets;
        }

#pragma warning disable 0649 //Yes, we know- it's expected here

        private struct Polygon
        {
            public byte F1, F2, F3, N1, N2, N3, U1, V1, U2, V2, U3, V3, TPage_clut, groundtype;
            //private byte texSwitch, flags;

            public Texflags texFlags;
            public byte vertFlags;

            public byte TPage => (byte)((TPage_clut >> 4) & 0x0F);
            public byte Clut => (byte)(TPage_clut & 0x0F);
            private Texflags TexFlags { get => Texflags.TEXFLAGS_ISENTERABLE | Texflags.TEXFLAGS_MISC | Texflags.TEXFLAGS_ROAD | Texflags.TEXFLAGS_SHADOW | Texflags.TEXFLAGS_UNK | Texflags.TEXFLAGS_UNK2 | Texflags.TEXFLAGS_WATER; set => texFlags = value; }
            //public byte TPage_clut1 { set => TPage_clut = value; }

            public override string ToString()
            {
                return $"GP={groundtype.ToString()} TP={TPage.ToString()} Clut={Clut.ToString()} TexFlags={Convert.ToString((byte)texFlags, 2).PadLeft(8, '0')} vertFlags={Convert.ToString(vertFlags, 2).PadLeft(8, '0')} UV={U1.ToString()} {V1.ToString()} {U2.ToString()} {V2.ToString()} {U3.ToString()} {V3.ToString()}";
            }
        }

        private struct Vertex
        {
            public short X;
            private short Z;
            public short Y;
            private short W;

            public short Z1 { get => (short)(Z * -1); set => Z = value; }
        }

        private struct Normal /*: Vertex we can't inherit struct in C#*/
        {
            public short X;
            private short Z;
            private short Y;
            private short W;

            public short Z1 { get => (short)(Z * -1); set => Z = value; }
        }

#pragma warning restore 169
#pragma warning restore 0649

        #endregion structures

        /// <summary>
        /// This is index to characters in chara.one file of worldmap
        /// </summary>
        private enum worldCharacters
        {
            SquallCasual,
            Ragnarok,
            Chocobo,
            BokoChocobo,
            SquallSeed,
            ZellCasual,
            SelphieCasual
        }

        private static worldCharacterInstance[] worldCharacterInstances = new worldCharacterInstance[8];

        private struct worldCharacterInstance
        {
            public worldCharacters activeCharacter;
            public Vector3 worldPosition;
            public float localRotation;
            public int currentAnimationId;
            public int currentAnimFrame;
            public float animationDeltaTime;
            public bool bDraw;
        }


        private static _worldState worldState;
        private static MiniMapState MapState = MiniMapState.rectangle;

        [Flags]
        private enum Texflags : byte
        {
            TEXFLAGS_SHADOW = 0b11,
            TEXFLAGS_UNK = 0b100,
            TEXFLAGS_ISENTERABLE = 0b00001000,
            TEXFLAGS_UNK2 = 0b00010000,
            TEXFLAGS_ROAD = 0b00100000,
            TEXFLAGS_WATER = 0b01000000,
            TEXFLAGS_MISC = 0b10000000
        }

        [Flags]
        private enum VertFlags
        {
            /// <summary>
            /// Player can walk on selected face (and only player)
            /// </summary>
            bWalkable = 0b10000000,
            /// <summary>
            /// Available exclusive to forests and tunnels
            /// </summary>
            bTreeZone = 0b01000000,
            /// <summary>
            /// Marked on faces that are NOT walkable by Player, but are by Chocobo- a thin water for example
            /// </summary>
            bWalkableByChocobo = 0b00010000
              
        }

        private const byte TRIFLAGS_COLLIDE = 0b10000000;
        private const byte TRIFLAGS_FORESTTEST = 0b01000000;

        private static int GetSegment(int segID) => segID * WM_SEG_SIZE;

        private static void InitWorld()
        {
            fps_camera = new FPS_Camera();
            //init renderer
            effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            camTarget = new Vector3(0, 0f, 0f);
            camPosition = new Vector3(-9100.781f, 108.0096f, -4438.435f);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(60),
                               Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            //worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
            //              Forward, Vector3.Up);
            worldMatrix = Matrix.CreateTranslation(0, 0, 0);

            if (bUseCustomShaderTest)
            {

                worldShaderModel = Memory.content.Load<Effect>("testShader");
                worldShaderModel.Parameters["World"].SetValue(worldMatrix);
                worldShaderModel.Parameters["View"].SetValue(viewMatrix);
                worldShaderModel.Parameters["Projection"].SetValue(projectionMatrix);
                worldShaderModel.Parameters["bendValue"].SetValue(BEND_VALUE);
                worldShaderModel.Parameters["bendDistance"].SetValue(BEND_DISTANCE);
                worldShaderModel.Parameters["bendVector"].SetValue(BEND_VECTOR);
                worldShaderModel.Parameters["Projection"].SetValue(projectionMatrix);
                worldShaderModel.Parameters["fogColor"].SetValue(FOG_COLOR);
            }
            //temporarily disabling this, because I'm getting more and more tired of this music playing over and over when debugging
            //Memory.musicIndex = 30;
            //init_debugger_Audio.PlayMusic();
            ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice)
            {
                Projection = projectionMatrix,
                View = viewMatrix,
                World = worldMatrix,
                FogEnabled = true,
                FogColor = Color.CornflowerBlue.ToVector3(),
                FogStart = 9.75f,
                FogEnd = 1000.00f
            };

            ReadWorldMapFiles();
            worldCharacterInstances[0] = new worldCharacterInstance() { activeCharacter = worldCharacters.SquallCasual, worldPosition = playerPosition, localRotation = -90f, currentAnimationId = 0, currentAnimFrame = 0 };
            worldState++;
            return;
        }

        private static void ReadWorldMapFiles()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_WORLD);
            ArchiveWorker awMain = new ArchiveWorker(Memory.Archives.A_MAIN);

            string wmxPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains("wmx.obj")).Select(x => x).First();
            string texlPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains("texl.obj")).Select(x => x).First();
            string wmPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains($"wmset{Extended.GetLanguageShort(true)}.obj")).Select(x => x).First();
            string charaOne = aw.GetListOfFiles().Where(x => x.ToLower().Contains("chara.one")).Select(x => x).First();
            string railFile = aw.GetListOfFiles().Where(x => x.ToLower().Contains("rail.obj")).Select(x => x).First();

            wmx = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmxPath);
            texl = new texl(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, texlPath));
            chara = new CharaOne(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, charaOne));
            wmset = new wmset(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmPath));
            rail = new rail(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, railFile));

            string wm2fieldPath = awMain.GetListOfFiles().Where(x => x.ToLower().Contains("wm2field.tbl")).Select(x => x).First();

            wm2field = new wm2field(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MAIN, wm2fieldPath));

            //let's update chara texture indexes due to worldmap VRAM tex atlas behaviour
            chara.AssignTextureSizesForMchInstance(0, new int[] { 0, 1 }); //naturally
            chara.AssignTextureSizesForMchInstance(1, new int[] { 2, 3, 4, 5 }); //ragnarok uses 4 textures!
            for (int i = 2; i < Enum.GetNames(typeof(worldCharacters)).Length; i++)
                chara.AssignTextureSizesForMchInstance(i, new int[] { i * 2 + 2, i * 2 + 3 }); //after ragnarok casual two textures per mesh + two additional due to ragnarok

            segments = new Segment[WM_SEGMENTS_COUNT];

            using (MemoryStream ms = new MemoryStream(wmx))
            using (BinaryReader br = new BinaryReader(ms))
                for (int i = 0; i < segments.Length; i++)
                {
                    ms.Seek(GetSegment(i), SeekOrigin.Begin);
                    segments[i] = new Segment { segmentId = i, headerData = Extended.ByteArrayToStructure<SegHeader>(br.ReadBytes(68)), block = new Block[16] };
                    ms.Seek(GetSegment(i), SeekOrigin.Begin);
                    for (int n = 0; n < segments[i].block.Length; n++)
                    {
                        ms.Seek(segments[i].headerData.blockOffsets[n] + GetSegment(i), SeekOrigin.Begin);
                        segments[i].block[n] = new Block { polyCount = br.ReadByte(), vertCount = br.ReadByte(), normalCount = br.ReadByte(), unkPadd = br.ReadByte() };
                        segments[i].block[n].polygons = new Polygon[segments[i].block[n].polyCount];
                        segments[i].block[n].vertices = new Vertex[segments[i].block[n].vertCount];
                        segments[i].block[n].normals = new Normal[segments[i].block[n].normalCount];
                        for (int k = 0; k < segments[i].block[n].polyCount; k++)
                            segments[i].block[n].polygons[k] = Extended.ByteArrayToStructure<Polygon>(br.ReadBytes(16));
                        for (int k = 0; k < segments[i].block[n].vertCount; k++)
                            segments[i].block[n].vertices[k] = Extended.ByteArrayToStructure<Vertex>(br.ReadBytes(8));
                        for (int k = 0; k < segments[i].block[n].normalCount; k++)
                            segments[i].block[n].normals[k] = Extended.ByteArrayToStructure<Normal>(br.ReadBytes(8));
                        segments[i].block[n].unkPadd2 = br.ReadInt32();
                    }
                    List<ParsedTriangleData> ptd = new List<ParsedTriangleData>();
                    int interI = GetInterchangableSegmentReplacementIndex(i);
                    float baseX = 512f * (interI % 32);
                    float baseY = -512f * (interI / 32);
                    for (int n = 0; n < segments[i].block.Length; n++)
                    {
                        float localX = 2048 * (n % 4);
                        float localZ = -2048 * (n / 4);
                        for (int k = 0; k < segments[i].block[n].polyCount; k++)
                        {
                            Vector2[] uvs = {
                                new Vector2(segments[i].block[n].polygons[k].U1 / 256.0f, segments[i].block[n].polygons[k].V1 / 256.0f),
                                new Vector2(segments[i].block[n].polygons[k].U2 / 256.0f, segments[i].block[n].polygons[k].V2 / 256.0f),
                                new Vector2(segments[i].block[n].polygons[k].U3 / 256.0f, segments[i].block[n].polygons[k].V3 / 256.0f)
                                            };
                            if(segments[i].block[n].polygons[k].texFlags.HasFlag(Texflags.TEXFLAGS_ROAD)) //this is roads UV fix
                            {
                                uvs[0] += new Vector2(0f, 0.002f);
                                uvs[1] += new Vector2(0f, 0.002f);
                                uvs[2] += new Vector2(0f, 0.002f);
                            }
                            ptd.Add(new ParsedTriangleData()
                            {
                                A = new Vector3(
                                ((segments[i].block[n].vertices[segments[i].block[n].polygons[k].F1].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                                segments[i].block[n].vertices[segments[i].block[n].polygons[k].F1].Z1 / WORLD_SCALE_MODEL,
                                (segments[i].block[n].vertices[segments[i].block[n].polygons[k].F1].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                                uvA = uvs[0],
                                parentPolygon = segments[i].block[n].polygons[k],
                                B = new Vector3(
                                ((segments[i].block[n].vertices[segments[i].block[n].polygons[k].F2].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                                segments[i].block[n].vertices[segments[i].block[n].polygons[k].F2].Z1 / WORLD_SCALE_MODEL,
                                (segments[i].block[n].vertices[segments[i].block[n].polygons[k].F2].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                                uvB = uvs[1],
                                C = new Vector3(
                                ((segments[i].block[n].vertices[segments[i].block[n].polygons[k].F3].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                                segments[i].block[n].vertices[segments[i].block[n].polygons[k].F3].Z1 / WORLD_SCALE_MODEL,
                                (segments[i].block[n].vertices[segments[i].block[n].polygons[k].F3].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                                uvC = uvs[2]
                            });
                            ParsedTriangleData ptda = ptd[ptd.Count - 1];
                            ptda.boundingBox = Extended.GetBoundingBox(ptda.A, ptda.B, ptda.C);
                            ptd[ptd.Count - 1] = ptda;
                        }
                    }
                    segments[i].parsedTriangle = ptd.ToArray();
                }
        }

        /// <summary>
        /// This method returns the origX and origY coordinates for segment replacement for pre-parsing
        /// </summary>
        /// <param name="i">index of wm block</param>
        /// <returns></returns>
        private static int GetInterchangableSegmentReplacementIndex(int i)
        {
            if (i < 768)
                return i;

            if (i == (int)interZone.prisonNormal) //correct
                return (int)interZone.prisonGround;

            if (i == (int)interZone.missileBaseNormal)
                return (int)interZone.missileBaseDestroyed;

            if (i == (int)interZone.balambGardenE_static) //correct;
                return (int)interZone.balambGardenE_mobile+1;
            if (i == (int)interZone.balambGardenW_static)
                return (int)interZone.balambGardenW_mobile-1;

            if (i == (int)interZone.galbadiaGarden_static)
                return (int)interZone.galbadiaGarden_mobile;

            if (i == (int)interZone.trabiaGardenE_state0)
                return (int)interZone.trabiaGardenE_state1;
            if (i == (int)interZone.trabiaGardenW_state0)
                return (int)interZone.trabiaGardenW_state1;

            if (i == (int)interZone.lunarCryCraterE_state0)
                return (int)interZone.lunarCryCraterE_state1;
            if (i == (int)interZone.lunarCryCraterW_state0)
                return (int)interZone.lunarCryCraterW_state1;

            if (i == (int)interZone.lunarCryCreaterN_state0)
                return (int)interZone.lunarCryCraterN_state1;
            if (i == (int)interZone.lunarCryCraterS_state0)
                return (int)interZone.lunarCryCraterS_state1;

            if (Extended.In(i, 768, 775))
                return i - 395;
            if (Extended.In(i, 776, 783))
                return i - 371;
            if (Extended.In(i, 784, 791))
                return i - 347;
            if (Extended.In(i, 792, 799))
                return i - 323;
            if (Extended.In(i, 800, 807))
                return i - 299;
            if (Extended.In(i, 808, 815))
                return i - 275;
            if (Extended.In(i, 816, 823))
                return i - 251;

            return 0;
        }

        public static bool bHasMoved = false;
        private static int currentControllableEntity = 0;

        public static void Update(GameTime deltaTime)
        {
            UpdateTextureAnimation();
            switch (worldState)
            {
                case _worldState._0init:
                    InitWorld();
                    break;

                case _worldState._1active:
                    OrbitCamera();
                    break;

                case _worldState._9debugFly:
                    viewMatrix = fps_camera.Update(ref camPosition, ref camTarget, ref degrees);
                    break;
            }

            if (Input2.DelayedButton(Keys.J) || Input2.DelayedButton(FF8TextTagKey.Select))
                MapState = MapState >= MiniMapState.fullscreen ? MapState = 0 : MapState + 1;

            if (Input2.DelayedButton(Keys.R))
                worldState = _worldState._0init;

            if (Input2.Button(Keys.D9))
                worldState = worldState == _worldState._1active ? _worldState._9debugFly : _worldState._1active;

            if (Input2.Button(Keys.D8))
                bDebugDisableCollision = !bDebugDisableCollision;

            InputUpdate(out var localRotator);
            CollisionUpdate();
            AnimationUpdate();
            if (bHasMoved)
            {
                worldCharacterInstances[currentControllableEntity].worldPosition = playerPosition;
                worldCharacterInstances[currentControllableEntity].localRotation = localRotator;
                EncounterUpdate();
            }
        }

        private static void AnimationUpdate()
        {
            if (bHasMoved)
            {
                if (worldCharacterInstances[currentControllableEntity].activeCharacter == worldCharacters.Ragnarok)
                {
                    //some other anim system
                }
                else
                {
                    worldCharacterInstances[currentControllableEntity].currentAnimationId = 1;
                }
            }
            else
                worldCharacterInstances[currentControllableEntity].currentAnimationId = 0;

            for(int i = 0; i<worldCharacterInstances.Length; i++)
            {
                worldCharacterInstance instance = worldCharacterInstances[i];
                if (!(instance as worldCharacterInstance?).HasValue)
                    continue;

                if (instance.animationDeltaTime >= (1000 / 60.0))
                {
                    instance.currentAnimFrame++;
                    instance.animationDeltaTime = 0;
                }
                else instance.animationDeltaTime += Memory.gameTime.ElapsedGameTime.Milliseconds;

                uint framesCount = chara.GetMCH((int)instance.activeCharacter).GetAnimationFramesCount(instance.currentAnimationId);
                if (instance.currentAnimFrame > framesCount-1)
                    instance.currentAnimFrame = 0;
                worldCharacterInstances[i] = instance;
            }

        }

        private static void UpdateTextureAnimation()
        {
            if (wmset == null)
                return;
            wmset.textureAnimation[] beachAnims = wmset.BeachAnimations;
            wmset.textureAnimation[] waterAnims = wmset.WaterAnimations;
            float elapsedTime = Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            UpdateTextureAnimation_SelectedStruct(ref beachAnims, elapsedTime);
            UpdateTextureAnimation_SelectedStruct(ref waterAnims, elapsedTime, true);
            wmset.BeachAnimations = beachAnims;
            wmset.WaterAnimations = waterAnims;
        }

        private static void UpdateTextureAnimation_SelectedStruct(ref wmset.textureAnimation[] beachAnims, float elapsedTime, bool bWater = false)
        {
            for (int i = 0; i < beachAnims.Length; i++)
            {
                float totalMaxValue = (15.625f * beachAnims[i].animTimeout)/1000.0f; //1 is 15.625 milliseconds, because 0x20 is 500 milliseconds
                beachAnims[i].deltaTime += elapsedTime;
                if (beachAnims[i].deltaTime > totalMaxValue)
                {
                    if (beachAnims[i].bIncrementing)
                        beachAnims[i].currentAnimationIndex++;
                    else
                        beachAnims[i].currentAnimationIndex--;
                    beachAnims[i].deltaTime = 0f;
                if (beachAnims[i].currentAnimationIndex >= beachAnims[i].framesCount)
                    if (beachAnims[i].bLooping > 0)
                    {
                        beachAnims[i].bIncrementing = !beachAnims[i].bIncrementing;
                        beachAnims[i].currentAnimationIndex = beachAnims[i].framesCount - 2;
                    }
                    else
                        beachAnims[i].currentAnimationIndex = 0;
                if (beachAnims[i].currentAnimationIndex < 0)
                {
                    beachAnims[i].currentAnimationIndex = 1;
                    beachAnims[i].bIncrementing = !beachAnims[i].bIncrementing;
                }
                    if (bWater)
                        wmset.UpdateWorldMapWaterTexturePaletteForAnimation(i, wmset.GetWaterAnimationPalettes(i, beachAnims[i].currentAnimationIndex));
                }
            }
        }

        /// <summary>
        /// If player moved then check for available encounters and if we should play it
        /// </summary>
        private static void EncounterUpdate()
        {
            //RE: if ((world_currentVehicle < 0 || world_currentVehicle > 9) && world_currentVehicle != 128 || !isStateOfMovement   //Naturally, we don't want encounters if in vehicle
            int regionId = wmset.GetWorldRegionBySegmentPosition((int)segmentPosition.X, (int)segmentPosition.Y); //section2
            if (activeCollidePolygon == null)
                return;
            int groundId = activeCollidePolygon.Value.groundtype;
            int encPointer = wmset.GetEncounterHelperPointer(regionId, groundId); //section1
            if (encPointer == 0xffff)
                return;
            ushort[] AvailableEncounters = wmset.GetEncounters(encPointer); //section4

            //we now have 8 encounters-> 4 casual; 2 mid and 2 rare

            //TODO random + enc.half/none junction + warping to battle
        }

        /// <summary>
        /// Provides 4-axis support for input of currently controlled entity
        /// TODO: extend to 360/ fix diagonal double speed/ calculate local degrees based on sticks
        /// </summary>
        /// <param name="localRotation"></param>
        private static void InputUpdate(out float localRotation)
        {
            localRotation = 0f;
            bHasMoved = false;
            lastPlayerPosition = playerPosition;

#if DEBUG
            if (Input2.Button(Keys.OemPlus))
                DEBUGshit += 4f;
            if (Input2.Button(Keys.OemMinus))
                DEBUGshit -= 4f;

            if (Input2.Button(Keys.NumPad9))
                DEBUGshit += 0.1f;
            if (Input2.Button(Keys.NumPad6))
                DEBUGshit -= 0.1f;

            if (Input2.Button(Keys.NumPad8))
                DEBUGshit += 1f;
            if (Input2.Button(Keys.NumPad5))
                DEBUGshit -= 1f;

            if (Input2.Button(Keys.NumPad7))
                DEBUGshit += 10f;
            if (Input2.Button(Keys.NumPad4))
                DEBUGshit -= 10f;
#endif

            if (worldState != _worldState._9debugFly)
            {
                if (Input2.Button(FF8TextTagKey.Up))
                {
                    playerPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees));
                    playerPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees));
                    localRotation = (float)Extended.Radians(-degrees - 90f);
                    bHasMoved = true;
                }
                if (Input2.Button(FF8TextTagKey.Down))
                {
                    playerPosition.X -= (float)Math.Cos(MathHelper.ToRadians(degrees));
                    playerPosition.Z -= (float)Math.Sin(MathHelper.ToRadians(degrees));
                    localRotation = (float)Extended.Radians(-degrees + 90f);
                    bHasMoved = true;
                }
                if (Input2.Button(FF8TextTagKey.Left))
                {
                    playerPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees-90f));
                    playerPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees-90f));
                    localRotation = (float)Extended.Radians(-degrees);
                    bHasMoved = true;
                }
                if (Input2.Button(FF8TextTagKey.Right))
                {
                    playerPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees + 90f));
                    playerPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees + 90f));
                    localRotation = (float)Extended.Radians(180f - degrees);
                    bHasMoved = true;
                }
            }
        }

        /// <summary>
        /// ParsedTriangleData- struct contains all available data paired with found triangle Vector3
        /// - contains barycentric based on playerPosition bool - bIsSkyRaycasted - used for sky raycast
        /// </summary>
        private static List<Tuple<ParsedTriangleData, Vector3, bool>> RaycastedTris;

        /// <summary>

        /// This method checks for collision- uses raycasting and 3Dintersection to either allow
        /// movement, update it and/or warp player. If all checks fails it returns to last known
        /// correct player position
        /// This points to polygon structure that is actively used/ character stomps on it
        /// </summary>
        private static Polygon? activeCollidePolygon = null;

        public static int GetRealSegmentId() => (int)(segmentPosition.Y * 32 + segmentPosition.X); //explicit public for wmset and warping sections
        public static int GetRealSegmentId(float x, float y) => (int)((y<0 ? 24+y:y) * 32 + (x<0 ? 32+x : x)); //explicit public for wmset and warping sections

        /// <summary>
        /// This method checks for collision- uses raycasting and 3Dintersection to either allow movement, update it and/or warp player. If all checks fails it returns to last known correct player position

        /// </summary>
        private static void CollisionUpdate()
        {
            segmentPosition = new Vector2((int)(playerPosition.X / 512) * -1, (int)(playerPosition.Z / 512) * -1); //needs to be updated on pre-new values of movement
            int realSegmentId = GetRealSegmentId();
            realSegmentId = SetInterchangeableZone(realSegmentId);
            var seg = segments[realSegmentId];
            RaycastedTris = new List<Tuple<ParsedTriangleData, Vector3, bool>>();
            Ray characterRay = new Ray(playerPosition + new Vector3(0, 10f, 0), Vector3.Down); //sets ray origin
            Ray skyRay = new Ray(GetForwardSkyRaycastVector(SKYRAYCAST_FIXEDDISTANCE), Vector3.Down);

            //loop through current block triangles - two rays at the same time. There are only two rays and multi triangles, so iterate triangles and check rays instead of double checking
            for (int i = 0; i < seg.parsedTriangle.Length; i++)
                if (Extended.RayIntersection3D(characterRay, seg.parsedTriangle[i].A, seg.parsedTriangle[i].B, seg.parsedTriangle[i].C, out Vector3 characterBarycentric) != 0)
                    RaycastedTris.Add(new Tuple<ParsedTriangleData, Vector3, bool>(seg.parsedTriangle[i], characterBarycentric, false));
                else if (Extended.RayIntersection3D(skyRay, seg.parsedTriangle[i].A, seg.parsedTriangle[i].B, seg.parsedTriangle[i].C, out Vector3 skyBarycentric) != 0)
                    RaycastedTris.Add(new Tuple<ParsedTriangleData, Vector3, bool>(seg.parsedTriangle[i], skyBarycentric, true));

            //don't allow walking over non-walkable faces - just because we tested both rays we can make this linq appear only once
            if (!bDebugDisableCollision)
                RaycastedTris = RaycastedTris.Where(x => (x.Item1.parentPolygon.vertFlags & TRIFLAGS_COLLIDE) != 0 && x.Item2 != Vector3.Zero).ToList();

            

#if DEBUG
            countofDebugFaces = new Vector2(
                RaycastedTris.Where(x => !x.Item3).Count(),
                RaycastedTris.Where(x => x.Item3).Count()
                );
#endif
            foreach (Tuple<ParsedTriangleData, Vector3, bool> prt in RaycastedTris)
            {
                if (prt.Item3) //we do not want skyRaycasts here, iterate only characterRay
                    continue;
                Vector3 distance = playerPosition - prt.Item2;
                float distY = Math.Abs(distance.Y);
                if ((prt.Item1.parentPolygon.vertFlags & TRIFLAGS_FORESTTEST) == 0)
                    if (distY > 10f)
                        continue;
                Vector3 squaPos = prt.Item2;
                playerPosition.Y = squaPos.Y;
                activeCollidePolygon = prt.Item1.parentPolygon;
#if DEBUG
                bSelectedWalkable = prt.Item1.parentPolygon.vertFlags;
#endif
                return;
            }
            //out of loop- failed to obtain collision or abandon move - we need to check now if player wanted to get to forest

            foreach (Tuple<ParsedTriangleData, Vector3, bool> prt in RaycastedTris)
            {
                if (!prt.Item3) //we do not want skyRaycasts here, iterate only characterRay
                    continue;
                //we do not want to check for Y here
                if ((prt.Item1.parentPolygon.vertFlags & TRIFLAGS_FORESTTEST) != 0) //this opts out non-forest faces
                    continue;
                Vector3 squaPos = prt.Item2;
                playerPosition.Y = squaPos.Y;
                activeCollidePolygon = prt.Item1.parentPolygon;
#if DEBUG
                bSelectedWalkable = prt.Item1.parentPolygon.vertFlags;
#endif
                return;
            }

            playerPosition = lastPlayerPosition;
        }

        private const float defaultmaxMoveSpeed = 1f;
        private const float MoveSpeedChange = 1f;
        private static float maxMoveSpeed = defaultmaxMoveSpeed;
        private const float maxLookSpeed = 0.25f;

        /// <summary>
        /// This is the relative distance that is added to forward vector of character and then
        /// casted from sky to bottom of the level
        /// </summary>
        private const float SKYRAYCAST_FIXEDDISTANCE = 5f;

        public static void OrbitCamera()
        {
            camDistance = 100f;
            camPosition = new Vector3(
                (float)(playerPosition.X + camDistance * Extended.Cos(degrees - 180f)),
                playerPosition.Y + 50f,
                (float)(playerPosition.Z + camDistance * Extended.Sin(degrees - 180f))
                );
            if (Input2.Button(FF8TextTagKey.RotateLeft))
                degrees--;
            if (Input2.Button(FF8TextTagKey.RotateRight))
                degrees++;
            if (degrees < 0)
                degrees = 359;
            if (degrees > 359)
                degrees = 0;
            camTarget = playerPosition;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
        }

        private double RadianAngleFromVector3s(Vector3 a, Vector3 b) => Math.Acos(Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b)));

        public static void Draw()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawBackgroundClouds();

            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix;
            ate.View = viewMatrix;
            ate.World = worldMatrix;

            if (bUseCustomShaderTest)
            {
                worldShaderModel.Parameters["Projection"].SetValue(ate.Projection);
                worldShaderModel.Parameters["View"].SetValue(ate.View);
                worldShaderModel.Parameters["World"].SetValue(ate.World);
                worldShaderModel.Parameters["camWorld"].SetValue(camPosition);
            }

            segmentPosition = new Vector2((int)(playerPosition.X / 512) * -1, (int)(playerPosition.Z / 512) * -1);

            //let's get segments ids for cube 
            /*
             * SEG0 SEG1 SEG2
             * SEG3 CURR SEG5
             * SEG6 SEG7 SEG8 */

            DrawSegment(-1, -1); //SEG0 
            DrawSegment(0, -1); //SEG1
            DrawSegment(1, -1); //SEG2
            DrawSegment(-1, 0); //SEG3
            DrawSegment(0,0); //draws current walkable segment
            DrawSegment(+1, 0); //SEG5
            DrawSegment(-1, +1); //SEG6
            DrawSegment(0, +1); //SEG7
            DrawSegment(+1, +1); //SEG8

            if (degrees < 90 || degrees > 270)
            {
                DrawSegment(-2, 0);
                DrawSegment(-2, 1);
                DrawSegment(-2, -1);
            }
            if (degrees < 360 && degrees > 180)
            {
                DrawSegment(0, 2);
                DrawSegment(-1, 2);
                DrawSegment(1, 2);
            }
            if (degrees < 180 && degrees > 0)
            {
                DrawSegment(0, -2);
                DrawSegment(-1, -2);
                DrawSegment(+1, -2);
            }
            if (degrees < 270 || degrees > 90)
            {
                DrawSegment(2, 0);
                DrawSegment(2, 1);
                DrawSegment(2, -1);
            }

            TeleportPlayerWarp();

            foreach(worldCharacterInstance charaInstance in worldCharacterInstances)
                DrawCharacter(charaInstance);
#if DEBUG
            DrawDebug();
            DrawCharacterShadow();
#endif

            switch (MapState)
            {
                case MiniMapState.noMinimap:
                    break;

                case MiniMapState.planet:
                    break;

                case MiniMapState.rectangle:
                    DrawRectangleMiniMap();
                    break;

                case MiniMapState.fullscreen:
                    break;
            }

            Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(
                $"World map MapState: {MapState}\n" +
                $"World Map Camera: ={camPosition}\n" +
                $"Player position: ={playerPosition}\n" +
                $"Segment Position: ={segmentPosition} ({GetSegmentVectorPlayerPosition()})\n" +
                $"Press 8 to enable/disable collision: {bDebugDisableCollision}\n" +
                $"selWalk: =0b{Convert.ToString(bSelectedWalkable, 2).PadLeft(8, '0')} of charaRay={countofDebugFaces.X}, skyRay={countofDebugFaces.Y}\n" +
                $"selWalk2: {(activeCollidePolygon.HasValue ? activeCollidePolygon.Value.ToString() : "N/A")}\n" +
                $"debugshit= {DEBUGshit}\n" +
                $"Press 9 to enable debug FPS camera: ={(worldState == _worldState._1active ? "orbit camera" : "FPS debug camera")}\n" +
                $"FPS camera degrees: ={degrees}°\n" +
                $"FOV: ={FOV}", 30, 20, 1f, 2f, lineSpacing: 5);
            Memory.SpriteBatchEnd();
        }

        private static float GetSegmentVectorPlayerPosition() => segmentPosition.Y * 32 + segmentPosition.X;

        /// <summary>
        /// Takes care of drawing shadows and additional FX when needed (like in forest).
        /// [WIP]
        /// </summary>
        private static void DrawCharacterShadow()
        {
            if (activeCollidePolygon == null)
                return;
            if((activeCollidePolygon.Value.vertFlags & TRIFLAGS_FORESTTEST) > 0)
            {
                var shadowGeom = Extended.GetShadowPlane(playerPosition + new Vector3(-2.2f, .1f, -2.2f), 4f);
                ate.Texture = (Texture2D)wmset.GetWorldMapTexture(wmset.Section38_textures.shadowBig, 0);
                ate.Alpha = .25f;
                foreach(var pass in ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    ate.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, shadowGeom, 0, shadowGeom.Length / 3);
                }
                ate.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                ate.Alpha = 1f;
            }
            else if ((activeCollidePolygon.Value.vertFlags & TRIFLAGS_FORESTTEST) == 0)
            {
                if(bHasMoved)
                {
                    var shadowGeom = Extended.GetShadowPlane(playerPosition + new Vector3(-2.2f, .1f, -2.2f), 4f);
                    ate.Texture = (Texture2D)wmset.GetWorldMapTexture(wmset.Section38_textures.wmfx_bush, 0);
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        ate.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, shadowGeom, 0, shadowGeom.Length / 3);
                    }
                    ate.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                }
            }
        }

        /// <summary>
        /// Gets the vector3 position of the raycast that drops from sky and is used for forest
        /// </summary>
        /// <returns></returns>
        private static Vector3 GetForwardSkyRaycastVector(float distance = 15f)
        {
            float degreesRadians = (float)Extended.Radians(-degrees + 90f); //gets radians value of current degrees
            Vector3 relativeTranslation = new Vector3(
                (float)Math.Sin(degreesRadians) * distance,
                5000f,
                (float)Math.Cos(degreesRadians) * distance
                );
            return new Vector3(playerPosition.X + relativeTranslation.X,
                relativeTranslation.Y,
                playerPosition.Z + relativeTranslation.Z);
        }

#if DEBUG

        private static byte bSelectedWalkable = 0;
        private static Vector2 countofDebugFaces = Vector2.Zero;
        private static bool bDebugDisableCollision = false;

        private static void DrawDebug()
        {
            if(Memory.currentGraphicMode != Memory.GraphicModes.DirectX) //looks like strict DX shaders can't simply accept SV_POSITION, COLOR0 or something?
                DrawDebug_Rays(); //uncomment to enable drawing rays for collision

            //DrawDebug_VehiclePreview(); //uncomment to enable drawing all vehicles in row

            //if(Memory.currentGraphicMode != Memory.graphicModes.DirectX)
                //Debug_DrawRailPaths(); //uncomment to enable draw lines showing rail keypoints
        }

        private static void Debug_DrawRailPaths()
        {
            for (int i = 0; i < rail.GetTrainTrackCount(); i++)
            {
                List<VertexPositionColor> vpc = new List<VertexPositionColor>();
                for (int n = 0; n < rail.GetTrainTrackFrameCount(i); n++)
                {
                    Vector3 vec = rail.GetTrackFrameVector(i, n) + Vector3.Up * 10f;
                    vpc.Add(new VertexPositionColor(vec, Color.Yellow));
                }
                foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vpc.ToArray(), 0, vpc.Count / 2);
                }
            }
        }

        private static void DrawDebug_VehiclePreview()
        {
            Vector3 localTranslation = playerPosition + new Vector3(20f, 10f, 20f);
            for (int i = 0; i < wmset.GetVehicleModelsCount(); i++)
            {
                Texture2D vehTex = (Texture2D)wmset.GetVehicleTexture(i, 0);
                Vector2 originVector = wmset.GetVehicleTextureOriginVector(i, 0);
                Tuple<VertexPositionTexture[], byte[]> dMod = wmset.GetVehicleGeometry(i, localTranslation + Vector3.Left * 50f * i, Quaternion.Identity, new Vector2(vehTex.Width, vehTex.Height), originVector);
                for (int n = 0; n < dMod.Item1.Length; n += 3)
                {
                    ate.Texture = (Texture2D)wmset.GetVehicleTexture(i, 0);
                    foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, dMod.Item1, n, 1);
                    }
                }
            }
        }

        private static void DrawDebug_Rays()
        {
            VertexPositionColor[] playerRaycastDownVerts = new[] { new VertexPositionColor(playerPosition, Color.White), new VertexPositionColor(new Vector3(playerPosition.X, -1, playerPosition.Z), Color.White) };
            Vector3 skyRaycastDownVerts = GetForwardSkyRaycastVector(SKYRAYCAST_FIXEDDISTANCE);
            VertexPositionColor[] skyVectorDropVerts = new[]
            {
                new VertexPositionColor(skyRaycastDownVerts, Color.White), //draw line from mockup up to the bottom fake infinity
                new VertexPositionColor(new Vector3(skyRaycastDownVerts.X, -5000f, skyRaycastDownVerts.Z), Color.White)
            };

            if (RaycastedTris.Count != 0)
                foreach (Tuple<ParsedTriangleData, Vector3, bool> tt in RaycastedTris)
                {
                    ParsedTriangleData triangle = tt.Item1;
                    VertexPositionColor[] verts2 = new[] {new VertexPositionColor(triangle.A, Color.White),
                new VertexPositionColor(triangle.B, Color.White),

                new VertexPositionColor(triangle.B, Color.White),
                new VertexPositionColor(triangle.C, Color.White),

                new VertexPositionColor(triangle.C, Color.White),
                new VertexPositionColor(triangle.A, Color.White)
                };
                    foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts2, 0, 3);
                    }
                }

            foreach (EffectPass pass in ate.CurrentTechnique.Passes)
            {
                pass.Apply();
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, playerRaycastDownVerts, 0, 1);
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, skyVectorDropVerts, 0, 1);
            }
        }

#endif

        /// <summary>
        /// translates the world map model so it's vertices are drawn as close to playerPosition
        /// vector as possible
        /// </summary>
        private static Vector3 localMchTranslation = new Vector3(0, 6f, 0);

        private static Vector2 Scale;

        private static void DrawCharacter(worldCharacterInstance? charaInstance_)
        {
            if (charaInstance_ == null)
                return;
            worldCharacterInstance charaInstance = charaInstance_.Value;
            int MchIndex = (int)charaInstance.activeCharacter;
            if (charaInstance.currentAnimationId >= chara.GetMCH(MchIndex).GetAnimationCount())
                charaInstance.currentAnimationId = 0;
            Tuple<VertexPositionColorTexture[], byte[]> charaCollection = chara.GetMCH(MchIndex).GetVertexPositions(charaInstance.worldPosition + localMchTranslation, Quaternion.CreateFromYawPitchRoll(charaInstance.localRotation, 0f, 0f), charaInstance.currentAnimationId, charaInstance.currentAnimFrame);

            int textureIndexBase; //chara.one contains textures one-by-one but mch indexes are based from zero for each character. That's why we have to sum texIndexes from previous meshes
            switch (charaInstance.activeCharacter)
            {
                case worldCharacters.Ragnarok:
                    textureIndexBase = 2;
                    break;

                case worldCharacters.Chocobo:
                    textureIndexBase = 6;
                    break;

                case worldCharacters.BokoChocobo:
                    textureIndexBase = 8;
                    break;

                case worldCharacters.SquallSeed:
                    textureIndexBase = 10;
                    break;

                case worldCharacters.ZellCasual:
                    textureIndexBase = 12;
                    break;

                case worldCharacters.SelphieCasual:
                    textureIndexBase = 14;
                    break;

                case worldCharacters.SquallCasual:
                default:
                    textureIndexBase = 0;
                    break;
            }

            Dictionary<Texture2D, List<VertexPositionColorTexture>> vptCollection = new Dictionary<Texture2D, List<VertexPositionColorTexture>>();
            for(int i = 0; i<charaCollection.Item2.Length; i+=3)
            {
                var charaTexture = chara.GetCharaTexture(textureIndexBase + charaCollection.Item2[i]);
                if (!vptCollection.ContainsKey(charaTexture))
                    vptCollection.Add(charaTexture, new List<VertexPositionColorTexture>());
                vptCollection[charaTexture].AddRange(charaCollection.Item1.Skip(i).Take(3).ToArray());
            }

            foreach(KeyValuePair<Texture2D, List<VertexPositionColorTexture>> kvp in vptCollection)
            {
                ate.Texture = kvp.Key;
                if (bUseCustomShaderTest)
                {
                    worldShaderModel.Parameters["ModelTexture"].SetValue(ate.Texture);
                    worldShaderModel.CurrentTechnique = worldShaderModel.Techniques["Texture_fog_bend"];
                }
                foreach (EffectPass pass in bUseCustomShaderTest ? worldShaderModel.CurrentTechnique.Passes : ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, kvp.Value.ToArray(), 0, kvp.Value.Count/3);
                }
            }
        }

        /// <summary>
        /// This prevents camera/player to get out of playable zone and wraps it to the other side
        /// like it's 360o
        /// </summary>
        private static void TeleportPlayerWarp()
        {
            if (playerPosition.X > 0)
                playerPosition.X = 32 * 512 * -1;
            if (playerPosition.X < 32 * 512 * -1)
                playerPosition.X = 0;

            if (playerPosition.Z > 0)
                playerPosition.Z = 24 * 512 * -1;
            if (playerPosition.Z < 24 * 512 * -1)
                playerPosition.Z = 0;
        }

        /// <summary>
        /// [WIP] Draws clouds in the background
        /// </summary>
        private static void DrawBackgroundClouds()
        {
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            wmset.GetWorldMapTexture(wmset.Section38_textures.clouds, 0).Draw(new Rectangle(0, 0, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)), Color.White * .1f);
            Memory.spriteBatch.End();
        }

        private static void DrawRectangleMiniMap()
        {
            Rectangle src = new Rectangle(Point.Zero, wmset.GetWorldMapTexture(wmset.Section38_textures.worldmapMinimap, 1).Size.ToPoint());
            Scale = Memory.Scale(src.Width, src.Height, Memory.ScaleMode.FitBoth);
            src.Width = (int)(src.Width * Scale.X);
            src.Height = (int)(src.Height * Scale.Y);
            src.Height /= 2;
            src.Width /= 2;
            Rectangle dst =
                new Rectangle(
                    Memory.graphics.GraphicsDevice.Viewport.Width - (src.Width) - 50,
                    Memory.graphics.GraphicsDevice.Viewport.Height - (src.Height) - 50,
                    src.Width,
                    src.Height);

            float bc = Math.Abs(camPosition.X / 16384.0f);
            float topX = dst.X + (dst.Width * bc);
            bc = Math.Abs(camPosition.Z / 12288f);
            float topY = dst.Y + (dst.Height * bc);

            //Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, Memory.blendState_BasicAdd);
            Memory.SpriteBatchStartAlpha(sortMode: SpriteSortMode.BackToFront);
            wmset.GetWorldMapTexture(wmset.Section38_textures.worldmapMinimap, 1).Draw(dst, Color.White * .7f);
            Memory.spriteBatch.End();

            src = new Rectangle(Point.Zero, wmset.GetWorldMapTexture(wmset.Section38_textures.minimapPointer, 0).Size.ToPoint());
            Scale = Memory.Scale(src.Width, src.Height, Memory.ScaleMode.FitBoth);
            src.Height = (int)((src.Width * Scale.X) / 30);
            src.Width = (int)((src.Height * Scale.Y) / 30);
            dst = new Rectangle(
                (int)topX,
                (int)topY,
                src.Width,
                src.Height);

            //Memory.SpriteBatchStartAlpha(sortMode: SpriteSortMode.BackToFront);
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            wmset.GetWorldMapTexture(wmset.Section38_textures.minimapPointer, 0).Draw(dst, Color.White * 1f, degrees * 6.3f / 360f + 2.5f, Vector2.Zero, SpriteEffects.None, 1f);
            Memory.SpriteBatchEnd();

            //float topX = Memory.graphics.GraphicsDevice.Viewport.Width * .6f; //6
            //float topY = Memory.graphics.GraphicsDevice.Viewport.Height * .6f;

            //float bc = Math.Abs(camPosition.X / 16384.0f);
            //topX += Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f * bc;
            //bc = Math.Abs(camPosition.Z / 12288f);
            //topY += Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f * bc;

            ////Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, Memory.blendState_BasicAdd);
            //Memory.SpriteBatchStartAlpha(sortMode: SpriteSortMode.BackToFront);
            //wmset.GetWorldMapTexture(wmset.Section38_textures.worldmapMinimap, 1).Draw(new Rectangle((int)(Memory.graphics.GraphicsDevice.Viewport.Width * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)), Color.White * .7f);
            //Memory.spriteBatch.End();

            //Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            //wmset.GetWorldMapTexture(wmset.Section38_textures.minimapPointer, 0).Draw(new Rectangle((int)topX, (int)topY, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 32.0f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 32.0f)), Color.White * 1f, degrees * 6.3f / 360f + 2.5f, Vector2.Zero, SpriteEffects.None, 1f);
            //Memory.SpriteBatchEnd();
        }

        private static void DrawSegment(int xTranslation, int yTranslation)
        {
            effect.TextureEnabled = true;
            int _i = GetRealSegmentId((segmentPosition.X + xTranslation) % 32, (segmentPosition.Y + yTranslation) % 24);

            _i = SetInterchangeableZone(_i);

            Segment seg = segments[_i];
            Vector3 translationVector = Vector3.Zero;
            Vector2 playerSegmentVector = segmentPosition;

            if(playerSegmentVector.X+xTranslation < 0)
                translationVector = new Vector3(32 * 512, 0, 0); //LEFT
            if (playerSegmentVector.Y + yTranslation < 0)
                translationVector = new Vector3(0,0,24 * 512); //UP

            if (playerSegmentVector.X + xTranslation > 31)
                translationVector = new Vector3(32 * -512, 0, 0); //RIGHT
            if (playerSegmentVector.Y + yTranslation > 23)
                translationVector = new Vector3(0, 0, 24 * -512); //BOTTOM

            if(playerSegmentVector.X +xTranslation<0 && playerSegmentVector.Y+yTranslation < 0 && xTranslation<0 && yTranslation <0) //UL diagonal wrap
                translationVector = new Vector3(32 * 512, 0, 24 * 512);
            if (playerSegmentVector.X + xTranslation > 31 && playerSegmentVector.Y + yTranslation < 0 && xTranslation > 0 && yTranslation < 0) //UR diagonal wrap
                translationVector = new Vector3(32 * -512, 0, 24 * 512);
            if (playerSegmentVector.X + xTranslation > 31 && playerSegmentVector.Y + yTranslation > 23 && xTranslation > 0 && yTranslation > 0) //BR diagonal wrap
                translationVector = new Vector3(32 * -512, 0, 24 * -512);
            if (playerSegmentVector.X + xTranslation < 0 && playerSegmentVector.Y + yTranslation > 23 && xTranslation < 0 && yTranslation > 0) //BL diagonal wrap
                translationVector = new Vector3(32 * 512, 0, 24 * -512);

            Dictionary<Texture2D, Tuple<List<VertexPositionTexture>, bool>> groupedPolygons = new Dictionary<Texture2D, Tuple<List<VertexPositionTexture>, bool>>();


            for (int k = 0; k < seg.parsedTriangle.Length; k++)
            {
                Vector3 firstEdge = seg.parsedTriangle[k].A + translationVector;
                double faceDistance = Extended.Distance3D(playerPosition, firstEdge);
                if (faceDistance > renderCamDistance) //this face is beyond the rendering zone; ignore whole segment!
                    continue;
                if (CheckFrustrumView(firstEdge.X, firstEdge.Z))
                    continue;

                Vector3 parsedTriangleB = seg.parsedTriangle[k].B + translationVector;
                Vector3 parsedTriangleC = seg.parsedTriangle[k].C + translationVector;
                bool bIsWaterBlock = false;

                VertexPositionTexture[] vpc = new VertexPositionTexture[3];
                vpc[0] = new VertexPositionTexture(
                    firstEdge,
                    seg.parsedTriangle[k].uvA);
                vpc[1] = new VertexPositionTexture(
                    parsedTriangleB,
                    seg.parsedTriangle[k].uvB);
                vpc[2] = new VertexPositionTexture(
                    parsedTriangleC,
                    seg.parsedTriangle[k].uvC);
                Polygon poly = seg.parsedTriangle[k].parentPolygon;
                if (poly.texFlags.HasFlag(Texflags.TEXFLAGS_ROAD))
                    ate.Texture = wmset.GetRoadsMiscTextures();
                else if (poly.texFlags.HasFlag(Texflags.TEXFLAGS_WATER))
                {
                    SetWaterAnimationTexture(seg, k, vpc, poly);
                    bIsWaterBlock = true;
                }
                else
                    ate.Texture = (Texture2D)texl.GetTexture(poly.TPage, poly.Clut);
                if (groupedPolygons.ContainsKey(ate.Texture))
                    groupedPolygons[ate.Texture].Item1.AddRange(vpc);
                else
                    groupedPolygons.Add(ate.Texture, new Tuple<List<VertexPositionTexture>, bool>(new List<VertexPositionTexture>() {vpc[0], vpc[1],vpc[2]}, bIsWaterBlock));
                
                //if (poly.texFlags.HasFlag(Texflags.TEXFLAGS_WATER) && Extended.In(poly.groundtype, 32, 34))
                //{
                //    Memory.graphics.GraphicsDevice.BlendState = BlendState.Additive;
                //    ate.Texture = wmset.GetWaterAnimationTextureFrame(0, wmset.WaterAnimations[0].currentAnimationIndex);
                //    foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                //    {
                //        pass.Apply();
                //        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpc, 0, 1);
                //    }
                //    Memory.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                //}
            }

            foreach(KeyValuePair<Texture2D, Tuple<List<VertexPositionTexture>, bool>> kvp in groupedPolygons)
            {
                ate.Texture = kvp.Key;
                var vptFinal = kvp.Value.Item1.ToArray();
                if(bUseCustomShaderTest)
                    worldShaderModel.Parameters["ModelTexture"].SetValue(ate.Texture);

                if (kvp.Value.Item2 && bUseCustomShaderTest)
                    worldShaderModel.CurrentTechnique = worldShaderModel.Techniques["Texture_fog_bend_waterAnim"];
                else if (bUseCustomShaderTest)
                    worldShaderModel.CurrentTechnique = worldShaderModel.Techniques["Texture_fog_bend"];
                    
                foreach (EffectPass pass in bUseCustomShaderTest ? worldShaderModel.CurrentTechnique.Passes : ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vptFinal, 0, vptFinal.Length/3);
                }
            }            
        }

        enum interZone : int
        {
            prisonNormal = 834,
            prisonGround = 361,

            missileBaseNormal = 829,
            missileBaseDestroyed = 327,

            balambGardenW_static = 827,
            balambGardenE_static = 828,
            balambGardenE_mobile = 274,
            balambGardenW_mobile = 275,

            galbadiaGarden_static = 826,
            galbadiaGarden_mobile = 267,

            trabiaGardenE_state0 = 824,
            trabiaGardenW_state0 = 825,
            trabiaGardenE_state1 = 149,
            trabiaGardenW_state1 = 150,

            lunarCryCraterE_state0 = 830,
            lunarCryCraterW_state0 = 831,
            lunarCryCraterE_state1 = 214,
            lunarCryCraterW_state1 = 215,

            lunarCryCraterN_state1 = 246,
            lunarCryCraterS_state1 = 247,
            lunarCryCreaterN_state0 = 832,
            lunarCryCraterS_state0 = 833
        };

        /// <summary>
        /// This method changes zone i index to show segment that has to be drawn based on actual worldmap save state
        /// </summary>
        /// <param name="_i">index of current wmx segment</param>
        /// <param name="bfixCollision">use only with collision- due to inverted Balamb there's collision issue</param>
        /// <returns></returns>
        private static int SetInterchangeableZone(int _i)
        {
            //if(true) means unreversed world flags
            switch((interZone)_i)
            {
                case interZone.prisonGround:
                    if (true)
                        return (int)interZone.prisonNormal;

                case interZone.missileBaseDestroyed:
                    if (true)
                        return (int)interZone.missileBaseNormal;

                case interZone.balambGardenE_mobile:
                    if (true)
                        return (int)interZone.balambGardenE_static -1;
                case interZone.balambGardenW_mobile:
                    if (true)
                        return (int)interZone.balambGardenW_static + 1;

                case interZone.galbadiaGarden_mobile:
                    if (true)
                        return (int)interZone.galbadiaGarden_static;

                case interZone.trabiaGardenE_state1:
                    if (true)
                        return (int)interZone.trabiaGardenE_state0;
                case interZone.trabiaGardenW_state1:
                    if (true)
                        return (int)interZone.trabiaGardenW_state0;

                case interZone.lunarCryCraterE_state1:
                    if (true)
                        return (int)interZone.lunarCryCraterE_state0;
                case interZone.lunarCryCraterW_state1:
                    if (true)
                        return (int)interZone.lunarCryCraterW_state0;

                case interZone.lunarCryCraterN_state1:
                    if (true)
                        return (int)interZone.lunarCryCreaterN_state0;
                case interZone.lunarCryCraterS_state1:
                    if (true)
                        return (int)interZone.lunarCryCraterS_state0;

                default:
                    return setEstharZones(_i);
            }
        }

        private static int setEstharZones(int i)
        {
            if (true) // esthar replace flag
            {
                if (Extended.In(i, 373, 380))
                    return i + 395;
                if (Extended.In(i, 405, 412))
                    return i + 371;
                if (Extended.In(i, 437, 444))
                    return i + 347;
                if (Extended.In(i, 469, 476))
                    return i + 323;
                if (Extended.In(i, 501, 508))
                    return i + 299;
                if (Extended.In(i, 533, 540))
                    return i + 275;
                if (Extended.In(i, 565, 572))
                    return i + 251;
            }
            return i; //compiler sake
        }

        private static void SetWaterAnimationTexture(Segment seg, int k, VertexPositionTexture[] vpc, Polygon poly)
        {
            /*
             *  GP=10 - Beach [ANIMATED]
                GP=31/CLUT0 - river endflow
                GP=31/CLUT6 - waterfall [ANIMATED]
                GP=31/CLUT3 - river [ANIMATED]
                GP=32 - Thin water- walkable with chocobo
                GP=33 - transition between thin water to ocean
                GP=34 - ocean
                */
            var waterAtlas = wmset.GetWorldMapWaterTexture();
            if (poly.groundtype == 10 || poly.groundtype == 32 || (poly.groundtype == 31 && poly.Clut == 3)) //BEACH + flat water + river flowing down
            {
                var @as = seg.parsedTriangle[k].parentPolygon;
                int animationIdPointer = 1; //beach corner
                if (@as.Clut == 2)
                    animationIdPointer = 0; //beach atlas
                if (@as.Clut == 3 && poly.groundtype == 31)
                    animationIdPointer = 2; //river anim

                var texx = wmset.GetBeachAnimationTextureFrame(animationIdPointer, wmset.BeachAnimations[animationIdPointer].currentAnimationIndex);
                float Ucoorder = @as.Clut == 2 ? 128f : 192;
                float Vcoorder = @as.Clut == 3 && poly.groundtype == 31 ? 32f : 0f;
                if (poly.groundtype == 10 || (poly.groundtype == 32 && poly.Clut == 2) || (poly.groundtype == 31 && poly.Clut == 3))
                {
                    vpc[0].TextureCoordinate = new Vector2((@as.U1 - Ucoorder) / texx.Width, (@as.V1 - Vcoorder) / (float)texx.Height);
                    vpc[1].TextureCoordinate = new Vector2((@as.U2 - Ucoorder) / texx.Width, (@as.V2 - Vcoorder) / (float)texx.Height);
                    vpc[2].TextureCoordinate = new Vector2((@as.U3 - Ucoorder) / texx.Width, (@as.V3 - Vcoorder) / (float)texx.Height);
                }

                if (poly.groundtype == 10 || (poly.groundtype == 32 && poly.Clut == 2) || (poly.groundtype == 31 && poly.Clut == 3))
                    ate.Texture = wmset.GetBeachAnimationTextureFrame(animationIdPointer, wmset.BeachAnimations[animationIdPointer].currentAnimationIndex);
                else if (poly.groundtype == 32)
                {
                    vpc[0].TextureCoordinate = new Vector2(@as.U1 / (float)waterAtlas.Width, @as.V1 / (float)waterAtlas.Height);
                    vpc[1].TextureCoordinate = new Vector2(@as.U2 / (float)waterAtlas.Width, @as.V2 / (float)waterAtlas.Height);
                    vpc[2].TextureCoordinate = new Vector2(@as.U3 / (float)waterAtlas.Width, @as.V3 / (float)waterAtlas.Height);
                    ate.Texture = waterAtlas;
                }
            }
            else if (Extended.In(poly.groundtype, 31, 34))
            {
                var @as = seg.parsedTriangle[k].parentPolygon;
                vpc[0].TextureCoordinate = new Vector2(@as.U1 / (float)waterAtlas.Width, @as.V1 / (float)waterAtlas.Height);
                vpc[1].TextureCoordinate = new Vector2(@as.U2 / (float)waterAtlas.Width, @as.V2 / (float)waterAtlas.Height);
                vpc[2].TextureCoordinate = new Vector2(@as.U3 / (float)waterAtlas.Width, @as.V3 / (float)waterAtlas.Height);
                ate.Texture = waterAtlas;
            }
            else
                ate.Texture = (Texture2D)wmset.GetWorldMapTexture(wmset.Section38_textures.waterTex2, 0); //FAIL- should not be used (I think)
        }

        /// <summary>
        /// This method checks if it should not draw some faces based on frustum culling method of
        /// checking point in triangle (2D geometry)
        /// </summary>
        /// <param name="pointX">X coordinate</param>
        /// <param name="pointY">Y (actually Z from 3D) coordinate</param>
        /// <returns></returns>
        private static bool CheckFrustrumView(float pointX, float pointY)
        {
            float ax, ay, d1, d2, d3;
            ax = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * -100f;
            ay = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * -100f;

            Vector3 left = Vector3.Zero, right = Vector3.Zero;
            left.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(Extended.ClampOverload(degrees - FOV, 0, 359))) * renderCamDistance * 2;
            left.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(Extended.ClampOverload(degrees - FOV, 0, 359))) * renderCamDistance * 2;
            right.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(Extended.ClampOverload(degrees + FOV, 0, 359))) * renderCamDistance * 2;
            right.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(Extended.ClampOverload(degrees + FOV, 0, 359))) * renderCamDistance * 2;

            d1 = pointX * (ay - left.Z) + pointY * (left.X - ax) + (ax * left.Z - ay * left.X);
            d2 = pointX * (left.Z - right.Z) + pointY * (right.X - left.X) + (left.X * right.Z - left.Z * right.X);
            d3 = pointX * (right.Z - ay) + pointY * (ax - right.X) + (right.X * ay - right.Z * ax);

            return ((d1 > 0 || d2 > 0 || d3 > 0) && (d1 < 0 || d2 < 0 || d3 < 0));
        }
    }
}
