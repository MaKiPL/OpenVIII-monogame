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

        private static Vector2 segmentPosition;
        private static CharaOne chara;
        private static texl texl;
        private static wmset wmset;


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

        #endregion

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

        const byte TRIFLAGS_COLLIDE =    0b10000000;
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

            ReadWMX();
            worldState++;
            return;
        }

        private static void ReadWMX()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_WORLD);
            string wmxPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains("wmx.obj")).Select(x => x).First();
            string texlPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains("texl.obj")).Select(x => x).First();
            string wmPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains($"wmset{Extended.GetLanguageShort(true)}.obj")).Select(x => x).First();
            string charaOne = aw.GetListOfFiles().Where(x => x.ToLower().Contains("chara.one")).Select(x => x).First();
            wmx = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmxPath);
            texl = new texl(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, texlPath));
            chara = new CharaOne(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, charaOne));
            wmset = new wmset(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmPath));

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
                    }
                    segments[i].parsedTriangle = ptd.ToArray();
                }
        }

        /// <summary>
        /// This method returns the origX and origY coordinates for segment replacement for pre-parsing
        /// </summary>
        /// <param name="i"></param>
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

            SimpleInputUpdate(); //lastplayerposition = playerposition here
            CollisionUpdate();
        }

        /// <summary>
        /// Simple input handling- it doesn't work really with collision and lastPlayerPosition for non-walkable areas
        /// </summary>
        private static void SimpleInputUpdate()
        {
            lastPlayerPosition = playerPosition;
            if (Input.Button(Keys.D8))
                playerPosition.X += 1f;
            if (Input.Button(Keys.D2))
                playerPosition.X -= 1f;
            if (Input.Button(Buttons.Up))
            {
                animationId = 1;
                playerPosition.X += (float)Math.Cos(MathHelper.ToRadians(degrees));
                playerPosition.Z += (float)Math.Sin(MathHelper.ToRadians(degrees));
                localMchRotation = (float)(Extended.Radians(-degrees - 90f));
            }
            if(Input.Button(Buttons.Down))
            {
                animationId = 1;
                playerPosition.X -= (float)Math.Cos(MathHelper.ToRadians(degrees));
                playerPosition.Z -= (float)Math.Sin(MathHelper.ToRadians(degrees));
                localMchRotation = (float)(Extended.Radians(-degrees + 90f));
            }
        }

        private static byte bSelectedWalkable = 0;
        private static int countofDebugFaces = 0;

        private static void CollisionUpdate()
        {
            int realSegmentId = (int)(segmentPosition.Y * 32 + segmentPosition.X);
            var seg = segments[realSegmentId];
            List<Tuple<ParsedTriangleData, Extended.Barycentric>> ii = new List<Tuple<ParsedTriangleData, Extended.Barycentric>>();
            for (int i = 0; i < seg.parsedTriangle.Length; i++)
            {
                Extended.Barycentric bary = new Extended.Barycentric(seg.parsedTriangle[i].A, seg.parsedTriangle[i].B, seg.parsedTriangle[i].C,
                     playerPosition);
                if (bary.IsInside)
                    ii.Add(new Tuple<ParsedTriangleData, Extended.Barycentric>(seg.parsedTriangle[i], bary));
            }
            if (ii.Count == 0)
                return;
            countofDebugFaces = ii.Count;
            foreach (var bart in ii)
            {
                //if (ii.Count != 1)
                //{
                    float minimumX = (new float[] { bart.Item1.A.X, bart.Item1.B.X, bart.Item1.C.X }).Min();
                    float minimumZ = (new float[] { bart.Item1.A.Z, bart.Item1.B.Z, bart.Item1.C.Z }).Min();
                    float maximumX = (new float[] { bart.Item1.A.X, bart.Item1.B.X, bart.Item1.C.X }).Max();
                    float maximumZ = (new float[] { bart.Item1.A.Z, bart.Item1.B.Z, bart.Item1.C.Z }).Max();
                    if (!Extended.In(playerPosition.X, minimumX, maximumX))
                        continue;
                    if (!Extended.In(playerPosition.Z, minimumZ, maximumZ))
                        continue;
                //}
                Vector3 squaPos = bart.Item2.Interpolate(bart.Item1.A, bart.Item1.B, bart.Item1.C);
                bSelectedWalkable = (byte)bart.Item1.parentPolygon.vertFlags;
                //tunnels debug

                //if ((bSelectedWalkable & TRIFLAGS_FORESTTEST)!=0)
                //    if (Math.Abs(squaPos.Y - playerPosition.Y) > 10f)
                //        continue;

                //tunnels debug end
                if ((byte)(bSelectedWalkable&TRIFLAGS_COLLIDE) == 0)
                    continue;
                playerPosition.Y = squaPos.Y;
                return;
            }
            //out of loop- failed to obtain collision or abandon move
            playerPosition = lastPlayerPosition;
        }

        const float defaultmaxMoveSpeed = 1f;
        const float MoveSpeedChange = 1f;
        static float maxMoveSpeed = defaultmaxMoveSpeed;
        const float maxLookSpeed = 0.25f;
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
                (float)(playerPosition.X + camDistance * Extended.Cos(degrees-180f)),
                playerPosition.Y + 50f,
                (float)(playerPosition.Z + camDistance * Extended.Sin(degrees-180f))
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

            DrawCharacter(0);

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
                $"selWalk: =0b{Convert.ToString(bSelectedWalkable,2).PadLeft(8, '0')} of {countofDebugFaces}\n" +
                $"Press 9 to enable debug FPS camera: ={(worldState== _worldState._1active ? "orbit camera" : "FPS debug camera")}\n" +
                $"FPS camera degrees: ={degrees}° (WARNING! Frustum cull disabled!)\n" +
                $"FOV: ={FOV}", 30, 20, lineSpacing: 5);
            Memory.SpriteBatchEnd();


        }

        private static int animationId = 0;
        static Vector3 localMchTranslation = new Vector3(0, 6f, 0);

        static float localMchRotation = -90f;

        private static void DrawCharacter(int charaIndex)
        {
            int MchIndex = charaIndex;
            uint testing = chara.GetMCH(MchIndex).GetAnimationFramesCount(animationId);
            animationTestVariable++;
            if (animationTestVariable >= testing)
                animationTestVariable = 0;
            var collectionDebug = chara.GetMCH(MchIndex).GetVertexPositions(playerPosition+ localMchTranslation, Quaternion.CreateFromYawPitchRoll(localMchRotation,0f,0f),animationId, animationTestVariable);
            if (collectionDebug.Item1.Length != 0)
                for (int i = 0; i < collectionDebug.Item1.Length; i += 3)
                {
                    ate.Texture = chara.GetCharaTexture(collectionDebug.Item2[i]);
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

        private static bool CheckFrustrumView(float px, float py)
        {
            return false; //ENABLE LATER!!!
            float ax, ay, d1, d2, d3;
            ax = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * -100f;
            ay = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * -100f;

            Vector3 left = Vector3.Zero, right = Vector3.Zero;
            left.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(Extended.ClampOverload(degrees - FOV, 0, 359))) * renderCamDistance * 2;
            left.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(Extended.ClampOverload(degrees - FOV, 0, 359))) * renderCamDistance * 2;
            right.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(Extended.ClampOverload(degrees + FOV, 0, 359))) * renderCamDistance * 2;
            right.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(Extended.ClampOverload(degrees + FOV, 0, 359))) * renderCamDistance * 2;

            d1 = px * (ay - left.Z) + py * (left.X - ax) + (ax * left.Z - ay * left.X);
            d2 = px * (left.Z - right.Z) + py * (right.X - left.X) + (left.X * right.Z - left.Z * right.X);
            d3 = px * (right.Z - ay) + py * (ax - right.X) + (right.X * ay - right.Z * ax);

            return ((d1 > 0 || d2 > 0 || d3 > 0) && (d1 < 0 || d2 < 0 || d3 < 0));
        }
    }
}
