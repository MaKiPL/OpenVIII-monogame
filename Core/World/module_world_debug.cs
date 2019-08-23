using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Core.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    class Module_world_debug
    {
        private static Matrix projectionMatrix, viewMatrix, worldMatrix;
        private static float degrees, Yshift;
        private static float camDistance = 10.0f;
        private static readonly float renderCamDistance = 1200.0f;
        private static Vector3 camPosition, camTarget;
        private static Vector3 playerPosition = new Vector3(-9105f, 30f, -4466);
        private static Vector3 lastPlayerPosition = playerPosition;
        public static BasicEffect effect;
        public static AlphaTestEffect ate;
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
        private static readonly int renderDistance = 4;
        private static readonly float FOV = 60;

        public static Vector2 segmentPosition;
        private static CharaOne chara;
        private static texl texl;
        private static wmset wmset;
        private static wm2field wm2field;
        private static rail rail;

        private static byte[] wmx;

        static float DEBUGshit = FOV;
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
            /// parsedTriangle is a struct containing pre-calculated values for world map so the calculations are one-time operation
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

            public byte TPage { get => (byte)((TPage_clut >> 4) & 0x0F); }
            public byte Clut { get => (byte)(TPage_clut & 0x0F); }
            private Texflags TexFlags { get => Texflags.TEXFLAGS_ISENTERABLE | Texflags.TEXFLAGS_MISC | Texflags.TEXFLAGS_ROAD | Texflags.TEXFLAGS_SHADOW | Texflags.TEXFLAGS_UNK | Texflags.TEXFLAGS_UNK2 | Texflags.TEXFLAGS_WATER; set => texFlags = value; }
            //public byte TPage_clut1 { set => TPage_clut = value; }
        }
#pragma warning disable 169
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

        #endregion

        /// <summary>
        /// This is index to characters in chara.one file of worldmap
        /// </summary>
        enum worldCharacters
        {
            SquallCasual,
            Ragnarok,
            Chocobo,
            BokoChocobo,
            SquallSeed,
            ZellCasual,
            SelphieCasual
        }

        private static worldCharacters activeCharacter = worldCharacters.SquallCasual;

        private static _worldState worldState;
        private static MiniMapState MapState = MiniMapState.rectangle;

        [Flags]
        enum Texflags : byte
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
        enum VertFlags
        {
            bWalkable = 0b10000000
        }

        const byte TRIFLAGS_COLLIDE = 0b10000000;
        const byte TRIFLAGS_FORESTTEST = 0b01000000;

        private static int GetSegment(int segID) => segID * WM_SEG_SIZE;
        private static void InitWorld()
        {
            Input.OverrideLockMouse = true;
            Input.CurrentMode = Input.MouseLockMode.Center;
            //init renderer
            effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            effect.EnableDefaultLighting();
            camTarget = new Vector3(0, 0f, 0f);
            camPosition = new Vector3(-9100.781f, 108.0096f, -4438.435f);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(60),
                               Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);

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
                            ptd.Add(new ParsedTriangleData()
                            {
                                A = new Vector3(
                                ((segments[i].block[n].vertices[segments[i].block[n].polygons[k].F1].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                                segments[i].block[n].vertices[segments[i].block[n].polygons[k].F1].Z1 / WORLD_SCALE_MODEL,
                                (segments[i].block[n].vertices[segments[i].block[n].polygons[k].F1].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                                uvA = new Vector2(segments[i].block[n].polygons[k].U1 / 256.0f, segments[i].block[n].polygons[k].V1 / 256.0f),
                                parentPolygon = segments[i].block[n].polygons[k],
                                B = new Vector3(
                                ((segments[i].block[n].vertices[segments[i].block[n].polygons[k].F2].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                                segments[i].block[n].vertices[segments[i].block[n].polygons[k].F2].Z1 / WORLD_SCALE_MODEL,
                                (segments[i].block[n].vertices[segments[i].block[n].polygons[k].F2].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                                uvB = new Vector2(segments[i].block[n].polygons[k].U2 / 256.0f, segments[i].block[n].polygons[k].V2 / 256.0f),
                                C = new Vector3(
                                ((segments[i].block[n].vertices[segments[i].block[n].polygons[k].F3].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                                segments[i].block[n].vertices[segments[i].block[n].polygons[k].F3].Z1 / WORLD_SCALE_MODEL,
                                (segments[i].block[n].vertices[segments[i].block[n].polygons[k].F3].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                                uvC = new Vector2(segments[i].block[n].polygons[k].U3 / 256.0f, segments[i].block[n].polygons[k].V3 / 256.0f)
                            });
                            var ptda = ptd[ptd.Count - 1];
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


            if (i >= 768 && i <= 775)
                return 373 + (i - 768);
            if (i >= 776 && i <= 783)
                return 347 + (i - 776);
            if (i >= 784 && i <= 791)
                return 347 + (i - 784);
            if (i >= 792 && i <= 799)
                return 469 + (i - 792);

            if (i >= 800 && i <= 807)
                return 501 + (i - 800);

            if (i >= 808 && i <= 815)
                return 533 + (i - 808);

            if (i >= 816 && i <= 823)
                return 565 + (i - 816);

            if (i >= 824 && i <= 825)
                return 149 + (i - 824);

            if (i == 826)
                return 267;

            if (i == 827) return 274;
            if (i == 828) return 275;

            if (i >= 829)
                return 327;

            if (i >= 830 && i <= 831)
                return 214 + (i - 830);

            if (i >= 832 && i <= 834)
                return 246 + (i - 832);

            if (i >= 834 && i <= 835)
                return 361 + (i - 834);

            return 0;
        }

        public static void Update()
        {
            animationId = 0;
            switch (worldState)
            {
                case _worldState._0init:
                    InitWorld();
                    break;
                case _worldState._1active:
                    OrbitCamera();
                    break;
                case _worldState._9debugFly:
                    FPSCamera();
                    break;
            }

            if (Input.Button(Keys.J))
                MapState = MapState >= MiniMapState.fullscreen ? MapState = 0 : MapState + 1;

            if (Input.Button(Keys.R))
                worldState = _worldState._0init;

            if (Input.Button(Keys.D9))
                worldState = worldState == _worldState._1active ? _worldState._9debugFly : _worldState._1active;

            if (Input.Button(Keys.D8))
                bDebugDisableCollision = !bDebugDisableCollision;

            SimpleInputUpdate(out var bHasMoved); //lastplayerposition = playerposition here
            CollisionUpdate();
            if(bHasMoved)
                EncounterUpdate();
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
            ushort[] AvailableEncounters = wmset.GetEncounters(encPointer); //section4

            //we now have 8 encounters-> 4 casual; 2 mid and 2 rare

            //TODO random + enc.half/none junction + warping to battle
        }

        /// <summary>
        /// Simple input handling- It's mockup of walking forward and backward. It's not the vanilla style input- used for testing collision
        /// </summary>
        private static void SimpleInputUpdate(out bool bHasMoved)
        {
            bHasMoved = false;
            lastPlayerPosition = playerPosition;
            if (Input.Button(Keys.D8))
                playerPosition.X += 1f;
            if (Input.Button(Keys.D2))
                playerPosition.X -= 1f;
            if (Input.Button(Buttons.Up))
            {
                animationId = 1;
                playerPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees)) * 2f;
                playerPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees)) * 2f;
                localMchRotation = (float)(Extended.Radians(-degrees - 90f));
                bHasMoved = true;
            }
            if (Input.Button(Buttons.Down))
            {
                animationId = 1;
                playerPosition.X -= (float)Math.Cos(MathHelper.ToRadians(degrees)) * 2f;
                playerPosition.Z -= (float)Math.Sin(MathHelper.ToRadians(degrees)) * 2f;
                localMchRotation = (float)(Extended.Radians(-degrees + 90f));
                bHasMoved = true;
            }
        }


        /// <summary>
        /// ParsedTriangleData- struct contains all available data paired with found triangle
        /// Vector3 - contains barycentric based on playerPosition
        /// bool - bIsSkyRaycasted - used for sky raycast
        /// </summary>
        private static List<Tuple<ParsedTriangleData, Vector3, bool>> RaycastedTris;

        /// <summary>
        /// This points to polygon structure that is actively used/ character stomps on it
        /// </summary>
        private static Polygon? activeCollidePolygon = null;

        public static int GetRealSegmentId() => (int)(segmentPosition.Y * 32 + segmentPosition.X); //explicit public for wmset and warping sections

        /// <summary>
        /// This method checks for collision- uses raycasting and 3Dintersection to either allow movement, update it and/or warp player. If all checks fails it returns to last known correct player position
        /// </summary>
        private static void CollisionUpdate()
        {
            segmentPosition = new Vector2((int)(playerPosition.X / 512) * -1, (int)(playerPosition.Z / 512) * -1); //needs to be updated on pre-new values of movement
            int realSegmentId = GetRealSegmentId();
            var seg = segments[realSegmentId];
            RaycastedTris = new List<Tuple<ParsedTriangleData, Vector3, bool>>();
            Ray characterRay = new Ray(playerPosition + new Vector3(0, 10f, 0), Vector3.Down); //sets ray origin
            Ray skyRay = new Ray(GetForwardSkyRaycastVector(SKYRAYCAST_FIXEDDISTANCE), Vector3.Down);

            //loop through current block triangles - two rays at the same time. There are only two rays and multi triangles, so iterate triangles and check rays instead of double checking
            for (int i = 0; i < seg.parsedTriangle.Length; i++)
                if (Extended.RayIntersection3D(characterRay, seg.parsedTriangle[i].A, seg.parsedTriangle[i].B, seg.parsedTriangle[i].C, out var characterBarycentric) != 0)
                    RaycastedTris.Add(new Tuple<ParsedTriangleData, Vector3, bool>(seg.parsedTriangle[i], characterBarycentric, false));
                else if (Extended.RayIntersection3D(skyRay, seg.parsedTriangle[i].A, seg.parsedTriangle[i].B, seg.parsedTriangle[i].C, out var skyBarycentric) != 0)
                    RaycastedTris.Add(new Tuple<ParsedTriangleData, Vector3, bool>(seg.parsedTriangle[i], skyBarycentric, true));


            //don't allow walking over non-walkable faces - just because we tested both rays we can make this linq appear only once
            if(!bDebugDisableCollision)
                RaycastedTris = RaycastedTris.Where(x => (x.Item1.parentPolygon.vertFlags & TRIFLAGS_COLLIDE) != 0).ToList();

#if DEBUG
            countofDebugFaces = new Vector2(
                RaycastedTris.Where(x => !x.Item3).Count(),
                RaycastedTris.Where(x => x.Item3).Count()
                );
#endif
            foreach (var prt in RaycastedTris)
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

            foreach (var prt in RaycastedTris)
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

        const float defaultmaxMoveSpeed = 1f;
        const float MoveSpeedChange = 1f;
        static float maxMoveSpeed = defaultmaxMoveSpeed;
        const float maxLookSpeed = 0.25f;
        /// <summary>
        /// This is the relative distance that is added to forward vector of character and then casted from sky to bottom of the level
        /// </summary>
        private const float SKYRAYCAST_FIXEDDISTANCE = 5f;

        public static void FPSCamera()
        {
            camDistance = 10.0f;
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

            if (Input.Button(Buttons.Up))//(Keyboard.GetState().IsKeyDown(Keys.W) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > 0.0f)
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Y -= Yshift / 50;
            }
            if (Input.Button(Buttons.Down))//(Keyboard.GetState().IsKeyDown(Keys.S) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < 0.0f)
            {
                camPosition.X -= (float)Math.Cos(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Z -= (float)Math.Sin(MathHelper.ToRadians(degrees)) * leftdistY / 10;
                camPosition.Y += Yshift / 50;
            }
            if (Input.Button(Buttons.Left))//(Keyboard.GetState().IsKeyDown(Keys.A) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0.0f)
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees - 90)) * leftdistX / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees - 90)) * leftdistX / 10;
            }
            if (Input.Button(Buttons.Right))//(Keyboard.GetState().IsKeyDown(Keys.D) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0.0f)
            {
                camPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees + 90)) * leftdistX / 10;
                camPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees + 90)) * leftdistX / 10;
            }

            camTarget.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Y = camPosition.Y - Yshift / 5;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
        }

        public static void OrbitCamera()
        {
            camDistance = 100f;
            camPosition = new Vector3(
                (float)(playerPosition.X + camDistance * Extended.Cos(degrees - 180f)),
                playerPosition.Y + 50f,
                (float)(playerPosition.Z + camDistance * Extended.Sin(degrees - 180f))
                );
            if (Input.Button(Buttons.Left))
                degrees--;
            if (Input.Button(Buttons.Right))
                degrees++;
            degrees = degrees % 360;
            camTarget = playerPosition;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
        }

        public static int animationTestVariable = 0;

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



            segmentPosition = new Vector2((int)(playerPosition.X / 512) * -1, (int)(playerPosition.Z / 512) * -1);
            for (int i = 0; i < 768; i++)
                DrawSegment(i);

            WrapWaterSegments();

            TeleportPlayerWarp();

            DrawCharacter(activeCharacter);

#if DEBUG
            DrawDebug();
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
                $"Segment Position: ={segmentPosition}\n" +
                $"Press 8 to enable/disable collision: {bDebugDisableCollision}\n" +
                //$"selWalk: =0b{Convert.ToString(bSelectedWalkable,2).PadLeft(8, '0')} of charaRay={countofDebugFaces.X}, skyRay={countofDebugFaces.Y}\n" +
                $"Press 9 to enable debug FPS camera: ={(worldState == _worldState._1active ? "orbit camera" : "FPS debug camera")}\n" +
                $"FPS camera degrees: ={degrees}°\n" +
                $"FOV: ={FOV}", 30, 20, lineSpacing: 5);
            Memory.SpriteBatchEnd();


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
        static bool bDebugDisableCollision = false;

        private static void DrawDebug()
        {
            //DrawDebug_Rays(); //uncomment to enable drawing rays for collision
            //DrawDebug_VehiclePreview(); //uncomment to enable drawing all vehicles in row
            //Debug_DrawRailPaths(); //uncomment to enable draw lines showing rail keypoints
        }

        private static void Debug_DrawRailPaths()
        {
            for (int i = 0; i < rail.GetTrainTrackCount(); i++)
            {
                    List<VertexPositionColor> vpc = new List<VertexPositionColor>();
                for (int n = 0; n < rail.GetTrainTrackFrameCount(i); n++)
                {
                    Vector3 vec = rail.GetTrackFrameVector(i, n) + Vector3.Up*10f;
                    vpc.Add(new VertexPositionColor(vec, Color.Yellow));
                }
                foreach (var pass in ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vpc.ToArray(), 0, vpc.Count/2);
                }
            }
        }

        private static void DrawDebug_VehiclePreview()
        {
            Vector3 localTranslation = playerPosition + new Vector3(20f, 10f, 20f);
            for (int i = 0; i < wmset.GetVehicleModelsCount(); i++)
            {
                Texture2D vehTex = wmset.GetVehicleTexture(i, 0);
                Vector2 originVector = wmset.GetVehicleTextureOriginVector(i, 0);
                var dMod = wmset.GetVehicleGeometry(i, localTranslation+Vector3.Left*50f*i, Quaternion.Identity, new Vector2(vehTex.Width, vehTex.Height), originVector);
                for (int n = 0; n < dMod.Item1.Length; n += 3)
                {
                    ate.Texture = wmset.GetVehicleTexture(i, 0);
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, dMod.Item1, n, 1);
                    }
                }
            }
        }

        private static void DrawDebug_Rays()
        {
            var playerRaycastDownVerts = new[] { new VertexPositionColor(playerPosition, Color.White), new VertexPositionColor(new Vector3(playerPosition.X, -1, playerPosition.Z), Color.White) };
            var skyRaycastDownVerts = GetForwardSkyRaycastVector(SKYRAYCAST_FIXEDDISTANCE);
            var skyVectorDropVerts = new[]
            {
                new VertexPositionColor(skyRaycastDownVerts, Color.White), //draw line from mockup up to the bottom fake infinity
                new VertexPositionColor(new Vector3(skyRaycastDownVerts.X, -5000f, skyRaycastDownVerts.Z), Color.White)
            };

            if (RaycastedTris.Count != 0)
                foreach (var tt in RaycastedTris)
                {
                    var triangle = tt.Item1;
                    var verts2 = new[] {new VertexPositionColor(triangle.A, Color.White),
                new VertexPositionColor(triangle.B, Color.White),

                new VertexPositionColor(triangle.B, Color.White),
                new VertexPositionColor(triangle.C, Color.White),

                new VertexPositionColor(triangle.C, Color.White),
                new VertexPositionColor(triangle.A, Color.White)
                };
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts2, 0, 3);
                    }
                }

            foreach (var pass in ate.CurrentTechnique.Passes)
            {
                pass.Apply();
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, playerRaycastDownVerts, 0, 1);
                Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, skyVectorDropVerts, 0, 1);
            }
        }
#endif

        private static int animationId = 0;
        /// <summary>
        /// translates the world map model so it's vertices are drawn as close to playerPosition vector as possible
        /// </summary>
        static Vector3 localMchTranslation = new Vector3(0, 6f, 0);
        /// <summary>
        /// Rotates the model to work with current coordinate system and for correct movement vectors model translation
        /// </summary>
        static float localMchRotation = -90f;

        private static void DrawCharacter(worldCharacters charaIndex)
        {
            int MchIndex = (int)charaIndex;
            if (animationId >= chara.GetMCH(MchIndex).GetAnimationCount())
                animationId = 0;
            uint testing = chara.GetMCH(MchIndex).GetAnimationFramesCount(animationId);
            animationTestVariable++;
            if (animationTestVariable >= testing)
                animationTestVariable = 0;
            var collectionDebug = chara.GetMCH(MchIndex).GetVertexPositions(playerPosition+ localMchTranslation, Quaternion.CreateFromYawPitchRoll(localMchRotation,0f,0f),animationId, animationTestVariable);

            int textureIndexBase; //chara.one contains textures one-by-one but mch indexes are based from zero for each character. That's why we have to sum texIndexes from previous meshes
            switch (charaIndex)
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

            if (collectionDebug.Item1.Length != 0)
                for (int i = 0; i < collectionDebug.Item1.Length; i += 3)
                {
                    ate.Texture = chara.GetCharaTexture(textureIndexBase+ collectionDebug.Item2[i]);
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, collectionDebug.Item1, i, 1);
                    }
                }
        }

        /// <summary>
        /// This prevents camera/player to get out of playable zone and wraps it to the other side like it's 360o
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
        /// Creates the effect of infinity by creating additional water blocks out-of playable zone
        /// </summary>
        private static void WrapWaterSegments()
        {
            //top water wrap
            if (segmentPosition.Y < 2)
            {
                float baseXseg = 512f * (segmentPosition.X % 32);

                DrawSegment(0, -baseXseg, 512f, true);
                DrawSegment(0, -baseXseg, 1024f, true);

                DrawSegment(0, -baseXseg - 512f, 512f, true);
                DrawSegment(0, -baseXseg - 512f, 1024f, true);

                DrawSegment(0, -baseXseg - 1024f, 512f, true);
                DrawSegment(0, -baseXseg - 1024f, 1024f, true);

                DrawSegment(0, -baseXseg + 512f, 512f, true);
                DrawSegment(0, -baseXseg + 512f, 1024f, true);

                DrawSegment(0, -baseXseg + 1024f, 512f, true);
                DrawSegment(0, -baseXseg + 1024f, 1024f, true);
            }

            //left water wrap
            if (segmentPosition.X < 2)
            {
                float baseYseg = -512f * (segmentPosition.Y % 24);

                DrawSegment(0, 512f, baseYseg, true);
                DrawSegment(0, 1024f, baseYseg, true);

                DrawSegment(0, 512f, baseYseg - 512f, true);
                DrawSegment(0, 1024f, baseYseg - 512f, true);

                DrawSegment(0, 512f, baseYseg - 1024f, true);
                DrawSegment(0, 1024f, baseYseg - 1024, true);

                DrawSegment(0, 512f, baseYseg + 512f, true);
                DrawSegment(0, 1024f, baseYseg + 512f, true);

                DrawSegment(0, 512f, baseYseg + 1024f, true);
                DrawSegment(0, 1024f, baseYseg + 1024, true);
            }

            //bottom water wrap
            if (segmentPosition.Y > 21)
            {
                float baseXseg = 512f * (segmentPosition.X % 32);

                DrawSegment(0, -baseXseg, -12288f, true);
                DrawSegment(0, -baseXseg, -12800f, true);

                DrawSegment(0, -baseXseg - 512f, -12288f, true);
                DrawSegment(0, -baseXseg - 512f, -12800f, true);

                DrawSegment(0, -baseXseg - 1024f, -12288f, true);
                DrawSegment(0, -baseXseg - 1024f, -12800f, true);

                DrawSegment(0, -baseXseg + 512f, -12288f, true);
                DrawSegment(0, -baseXseg + 512f, -12800f, true);

                DrawSegment(0, -baseXseg + 1024f, -12288f, true);
                DrawSegment(0, -baseXseg + 1024f, -12800f, true);
            }

            //right water wrap
            if (segmentPosition.X > 29)
            {
                float baseYseg = -512f * (segmentPosition.Y % 24);

                DrawSegment(0, -16384f, baseYseg, true);
                DrawSegment(0, -16896f, baseYseg, true);

                DrawSegment(0, -16384f, baseYseg - 512f, true);
                DrawSegment(0, -16896f, baseYseg - 512f, true);

                DrawSegment(0, -16384f, baseYseg - 1024f, true);
                DrawSegment(0, -16896f, baseYseg - 1024, true);

                DrawSegment(0, -16384f, baseYseg + 512f, true);
                DrawSegment(0, -16896f, baseYseg + 512f, true);

                DrawSegment(0, -16384f, baseYseg + 1024f, true);
                DrawSegment(0, -16896f, baseYseg + 1024, true);
            }
        }

        /// <summary>
        /// [WIP] Draws clouds in the background
        /// </summary>
        private static void DrawBackgroundClouds()
        {
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            Memory.spriteBatch.Draw(wmset.GetWorldMapTexture(wmset.Section38_textures.clouds, 0), new Rectangle(0, 0, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)), Color.White * .1f);
            Memory.spriteBatch.End();
        }

        private static void DrawRectangleMiniMap()
        {
            float topX = Memory.graphics.GraphicsDevice.Viewport.Width * .6f; //6
            float topY = Memory.graphics.GraphicsDevice.Viewport.Height * .6f;


            float bc = Math.Abs(camPosition.X / 16384.0f);
            topX += Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f * bc;
            bc = Math.Abs(camPosition.Z / 12288f);
            topY += Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f * bc;

            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, Memory.blendState_BasicAdd);
            Memory.spriteBatch.Draw(wmset.GetWorldMapTexture(wmset.Section38_textures.worldmapMinimap, 1), new Rectangle((int)(Memory.graphics.GraphicsDevice.Viewport.Width * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)), Color.White * .7f);
            Memory.spriteBatch.End();


            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive);
            Memory.spriteBatch.Draw(wmset.GetWorldMapTexture(wmset.Section38_textures.minimapPointer, 0), new Rectangle((int)topX, (int)topY, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 32.0f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 32.0f)), null, Color.White * 1f, degrees * 6.3f / 360f + 2.5f, Vector2.Zero, SpriteEffects.None, 1f);
            Memory.SpriteBatchEnd();
        }

        /// <summary>
        /// Determines either to draw the segment or ignore. Example of ignore case is when the distance is bigger than X
        /// </summary>
        /// <param name="baseX"></param>
        /// <param name="baseY"></param>
        /// <param name="seg"></param>
        /// <returns></returns>
        private static bool ShouldDrawSegment(float baseX, float baseY, int seg)
        {
            int ySegment = seg / 32; //2
            int xSegment = seg - ySegment * 32;
            Vector2 currentSegment = new Vector2(xSegment, ySegment);

            for (int i = -1 - renderDistance; i < renderDistance; i++)
                for (int k = 0 - renderDistance; k < renderDistance; k++)
                    if (segmentPosition + new Vector2(i, k) == currentSegment)
                        return true;

            return false;
        }

        private static void DrawSegment(int _i, float? baseXf = null, float? baseYf = null, bool bIsWrapSegment = false)
        {
            float baseX = 0f, baseY = 0f;
            if (baseXf == null || baseYf == null)
            {
                baseX = 512f * (_i % 32);
                baseY = -512f * (_i / 32); //explicit int cast
            }
            else
            {
                baseX = (float)baseXf; //31
                baseY = (float)baseYf; //23
            }
            if (!bIsWrapSegment)
                if (!ShouldDrawSegment((float)baseX, (float)baseY, _i))
                    return;

            effect.TextureEnabled = true;

            if (bIsWrapSegment)
            {
                Segment waterSeg = segments[0]; //get water segment
                for (int k = 0; k < waterSeg.parsedTriangle.Length; k += 3)
                {
                    VertexPositionTexture[] vpc = new VertexPositionTexture[3];
                    vpc[0] = new VertexPositionTexture(
                        waterSeg.parsedTriangle[k].A + new Vector3(baseX, 0, baseY),
                        waterSeg.parsedTriangle[k].uvA);
                    vpc[1] = new VertexPositionTexture(
                        waterSeg.parsedTriangle[k].B + new Vector3(baseX, 0, baseY),
                        waterSeg.parsedTriangle[k].uvB);
                    vpc[2] = new VertexPositionTexture(
                        waterSeg.parsedTriangle[k].C + new Vector3(baseX, 0, baseY),
                        waterSeg.parsedTriangle[k].uvC);
                    ate.Texture = wmset.GetWorldMapTexture(wmset.Section38_textures.waterTex2, 0);
                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpc, 0, 1);
                    }
                }
            }

#region Interchangable zones
            //TODO flags to switch them true or not
            //esthar
            if (Extended.In(_i, 373, 380))
                _i += 395;
            if (Extended.In(_i, 405, 412))
                _i += 371;
            if (Extended.In(_i, 437, 444))
                _i += 347;
            if (Extended.In(_i, 469, 476))
                _i += 323;
            if (Extended.In(_i, 501, 508))
                _i += 299;
            if (Extended.In(_i, 533, 540))
                _i += 275;
            if (Extended.In(_i, 565, 572))
                _i += 251;

            //trabia
            if (Extended.In(_i, 149, 150))
                _i += 675;

            //galbadia
            if (Extended.In(_i, 267, 267))
                _i = 826;

            ////balamb
            if (Extended.In(_i, 274, 275))
                _i += 553;

            ////base
            if (Extended.In(_i, 327, 327))
                _i += 502;

            ////trabia
            if (Extended.In(_i, 214, 215))
                _i += 616;
            if (Extended.In(_i, 246, 247))
                _i += 586;

            ////prison
            if (Extended.In(_i, 361, 361))
                _i += 473;
#endregion



            Segment seg = segments[_i];
            for (int k = 0; k < seg.parsedTriangle.Length; k++)
            {
                if (Extended.Distance3D(camPosition, seg.parsedTriangle[k].A) > renderCamDistance)
                    continue;
                if (CheckFrustrumView(seg.parsedTriangle[k].A.X, seg.parsedTriangle[k].A.Z))
                    continue;

                VertexPositionTexture[] vpc = new VertexPositionTexture[3];
                vpc[0] = new VertexPositionTexture(
                    seg.parsedTriangle[k].A,
                    seg.parsedTriangle[k].uvA);
                vpc[1] = new VertexPositionTexture(
                    seg.parsedTriangle[k].B,
                    seg.parsedTriangle[k].uvB);
                vpc[2] = new VertexPositionTexture(
                    seg.parsedTriangle[k].C,
                    seg.parsedTriangle[k].uvC);
                var poly = seg.parsedTriangle[k].parentPolygon;
                if (poly.texFlags.HasFlag(Texflags.TEXFLAGS_ROAD))
                    ate.Texture = wmset.GetRoadsMiscTextures(wmset.Section39_Textures.asphalt, 0);
                else if (poly.texFlags.HasFlag(Texflags.TEXFLAGS_WATER))
                    ate.Texture = wmset.GetWorldMapTexture(wmset.Section38_textures.waterTex2, 0);
                else
                    ate.Texture = texl.GetTexture(poly.TPage, poly.Clut); //there are two texs, worth looking at other parameters; to reverse! 
                foreach (var pass in ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpc, 0, 1);
                }
            }
        }

        /// <summary>
        /// This method checks if it should not draw some faces based on frustum culling method of checking point in triangle (2D geometry)
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
