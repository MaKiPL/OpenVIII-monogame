using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private static readonly float camDistance = 10.0f;
        private static readonly float renderCamDistance = 1200.0f;
        private static Vector3 camPosition, camTarget;
        public static BasicEffect effect;
        public static AlphaTestEffect ate;
        private enum _worldState
        {
            _0init,
            _1debugFly
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

        private static List<Texture2D[]> textures;
        private static List<Texture2D[]> wm38textures;
        private static List<Texture2D[]> wm39textures;
        private static List<Texture2D[]> charaOneTextures;
        private static readonly int renderDistance = 4;
        private static readonly float FOV = 60;

        private static Vector2 segmentPosition;

        private static Debug_MCH[] mchEntities;

        private static byte[] wmx;

        static float DEBUGshit = FOV;

        private static int GetSegment(int segID) => segID * 0x9000;

        private static Segment[] segments;

        private struct Segment
        {
            public int segmentId;
            public SegHeader headerData;
            public Block[] block;
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
            public byte F1, F2, F3, N1, N2, N3, U1, V1, U2, V2, U3, V3, TPage_clut, groundtype, TextureSwitch, unk2;

            public byte TPage { get => (byte)((TPage_clut >> 4) & 0x0F); }
            public byte Clut { get => (byte)(TPage_clut & 0x0F); }
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

        private static _worldState worldState;
        private static MiniMapState MapState = MiniMapState.rectangle;

        public static void Update()
        {
            switch (worldState)
            {
                case _worldState._0init:
                    InitWorld();
                    break;
                case _worldState._1debugFly:
                    FPSCamera();
                    break;
            }
        }

        private static void InitWorld()
        {
            Input.OverrideLockMouse = true;
            Input.CurrentMode = Input.MouseLockMode.Center;
            //init renderer
            effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            effect.EnableDefaultLighting();
            camTarget = new Vector3(0, 0f, 0f);
            camPosition = new Vector3(-9166f, 112f, -4570f);
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
            string wmPath = aw.GetListOfFiles().Where(x => x.ToLower().Contains("wmsetus.obj")).Select(x => x).First();
            string charaOne = aw.GetListOfFiles().Where(x => x.ToLower().Contains("chara.one")).Select(x => x).First();
            wmx = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmxPath);
            byte[] texl = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, texlPath);
            byte[] charaOneB = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, charaOne);
            ReadCharaOne(charaOneB);
            charaOneB = null; //GC
            ReadWmSet(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmPath));

            ReadTextures(texl);
            texl = null; //GC
            segments = new Segment[835];

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
                }
        }

        //TODO - so parsing is done
        private static void ReadCharaOne(byte[] charaOneB)
        {
            List<Debug_MCH> mchs = new List<Debug_MCH>();
            using (MemoryStream ms = new MemoryStream(charaOneB))
            using (BinaryReader br = new BinaryReader(ms))
            {

                uint eof = br.ReadUInt32();
                TIM2 tim;
                while (ms.CanRead)
                    if (ms.Position > ms.Length)
                        break;
                    else if (BitConverter.ToUInt16(charaOneB, (int)ms.Position) == 0)
                        ms.Seek(2, SeekOrigin.Current);
                    else if (br.ReadUInt64() == 0x0000000800000010)
                    {
                        ms.Seek(-8, SeekOrigin.Current);
                        tim = new TIM2(charaOneB, (uint)ms.Position);
                        ms.Seek(tim.GetHeight * tim.GetWidth / 2 + 64, SeekOrigin.Current); //i.e. 64*20=1280/2=640 + 64= 704 + eof
                        if (charaOneTextures == null)
                            charaOneTextures = new List<Texture2D[]>();
                        Texture2D[] _2d = { tim.GetTexture(0,true) };

                        charaOneTextures.Add(_2d);
                    }
                    else //is geometry structure
                    {
                        ms.Seek(-8, SeekOrigin.Current);
                        mchs.Add(new Debug_MCH(ms, br));
                    }
            }
            mchEntities = mchs.ToArray();
        }

        #region wmset

        private static void ReadWmSet(byte[] v)
        {
            int[] sections = new int[48];
            using (MemoryStream ms = new MemoryStream(v))
            using (BinaryReader br = new BinaryReader(ms))
            {
                for (int i = 0; i < sections.Length; i++)
                    sections[i] = br.ReadInt32();

                Wm_s38(ms, br, sections, v);
                Wm_s39(ms, br, sections, v);
            }
        }

        private static void Wm_s39(MemoryStream ms, BinaryReader br, int[] sec, byte[] v)
        {
            ms.Seek(sec[39 - 1], SeekOrigin.Begin);
            List<int> wm39sections = new List<int>();
            int eof = -1;
            while ((eof = br.ReadInt32()) != 0)
                wm39sections.Add(eof);
            wm39textures = new List<Texture2D[]>();

            for (int i = 0; i < wm39sections.Count; i++)
            {
                TIM2 tim = new TIM2(v, (uint)(sec[39 - 1] + wm39sections[i]));
                wm39textures.Add(new Texture2D[tim.GetClutCount]);
                for (ushort k = 0; k < wm39textures[i].Length; k++)
                {
                    wm39textures[i][k] = tim.GetTexture(k, true);
                }
            }
        }

        private static void Wm_s38(MemoryStream ms, BinaryReader br, int[] sec, byte[] v)
        {
            ms.Seek(sec[38 - 1], SeekOrigin.Begin);
            List<int> wm38sections = new List<int>();
            int eof = -1;
            while ((eof = br.ReadInt32()) != 0)
                wm38sections.Add(eof);
            wm38textures = new List<Texture2D[]>();

            for (int i = 0; i < wm38sections.Count; i++)
            {
                TIM2 tim = new TIM2(v, (uint)(sec[38 - 1] + wm38sections[i]));
                wm38textures.Add(new Texture2D[tim.GetClutCount]);
                for (ushort k = 0; k < wm38textures[i].Length; k++)
                {
                    wm38textures[i][k] = tim.GetTexture(k, true);
                }
            }
        }

        #endregion

        private static void ReadTextures(byte[] texl)
        {
            MemoryStream ms = new MemoryStream(texl);
            BinaryReader br = new BinaryReader(ms);
            textures = new List<Texture2D[]>(); //20

            for (int i = 0; i < 20; i++)
            {
                int timOffset = i * 0x12800;
                TIM2 tim = new TIM2(texl, (uint)timOffset);
                textures.Add(new Texture2D[tim.GetClutCount]);
                for (ushort k = 0; k < textures[i].Length; k++)
                {
                    textures[i][k] = tim.GetTexture(k,true);
                }
            }

            br.Close();
            ms.Close();
            br.Dispose();
            ms.Dispose();
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
            #endregion
        }

        public static void Draw()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            Memory.spriteBatch.Draw(wm38textures[10][0], new Rectangle(0, 0, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)), Color.White * .1f);
            Memory.spriteBatch.End();



            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix;
            ate.View = viewMatrix;
            ate.World = worldMatrix;

            //334 debug
            for (int i = 0; i < 768; i++)
                DrawSegment(i);

            if (true)
            {
                var collectionDebug = mchEntities[0].GetVertexPositions(-500, 100, -500);
                ate.Texture = charaOneTextures[1][0];
                foreach (var pass in ate.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, collectionDebug.Item1, 0, collectionDebug.Item1.Length / 3);
                }
            }

            if (Input.GetInputDelayed(Keys.J))
                MapState = MapState >= MiniMapState.fullscreen ? MapState = 0 : MapState + 1;

            //if (Input.GetInputDelayed(Keys.F1))
            //    ;




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
                $"Segment Position: ={segmentPosition}\n" +
                $"FPS camera degrees: ={degrees}°\n" +
                $"FOV: ={FOV}", 30,20,lineSpacing:5);
            Memory.SpriteBatchEnd();


        }

        private static void DrawRectangleMiniMap()
        {
            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, Memory.blendState_BasicAdd);
            Memory.spriteBatch.Draw(wm38textures[11][1], new Rectangle((int)(Memory.graphics.GraphicsDevice.Viewport.Width * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)), Color.White * .7f);
            Memory.spriteBatch.End();

            //cursor is wm38[24][0] and -16384 is max X, where 12288 is max Y

            float topX = Memory.graphics.GraphicsDevice.Viewport.Width * .6f; //6
            float topY = Memory.graphics.GraphicsDevice.Viewport.Height * .6f;


            float bc = Math.Abs(camPosition.X / 16384.0f);
            topX += Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f * bc;
            bc = Math.Abs(camPosition.Z / 12288f);
            topY += Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f * bc;

            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(wm38textures[24][0], new Rectangle((int)topX, (int)topY, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 32.0f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 32.0f)), null, Color.White * 1f, degrees * 6.3f / 360f + 2.5f, Vector2.Zero, SpriteEffects.None, 1f);
            Memory.SpriteBatchEnd();
        }

        private static bool ShouldDrawSegment(float baseX, float baseY, int seg)
        {
            segmentPosition = new Vector2((int)(camPosition.X / 512) * -1, (int)(camPosition.Z / 512) * -1);

            if (camPosition.X > 0)
                camPosition.X = 32 * 512 * -1;
            if (camPosition.X < 32 * 512 * -1)
                camPosition.X = 0;

            if (camPosition.Z > 0)
                camPosition.Z = 24 * 512 * -1;
            if (camPosition.Z < 24 * 512 * -1)
                camPosition.Z = 0;

            int ySegment = seg / 32; //2
            int xSegment = seg - ySegment * 32;
            Vector2 currentSegment = new Vector2(xSegment, ySegment);

            for (int i = -1 - renderDistance; i < renderDistance; i++)
                for (int k = 0 - renderDistance; k < renderDistance; k++)
                    if (segmentPosition + new Vector2(i, k) == currentSegment)
                        return true;

            return false;
        }

        private static void DrawSegment(int _i)
        {
            float baseX, baseY;
            //TODO http://forums.qhimm.com/index.php?topic=16230.msg230004#msg230004 
            baseX = 2048f / 4 * (_i % 32);
            baseY = -(2048f / 4) * (_i / 32); //explicit int cast

            if (!ShouldDrawSegment(baseX, baseY, _i))
                return;

            #region Interchangable zones
            //esthar
            if (Extended.In(_i, 373, 380))
                _i += 395;
            if (Extended.In(_i, 405, 412))
                _i += 371;
            if (Extended.In(_i, 437, 444))
                _i += 785 - 438;
            if (Extended.In(_i, 469, 476))
                _i += 793 - 470;
            if (Extended.In(_i, 501, 508))
                _i += 801 - 502;
            if (Extended.In(_i, 533, 540))
                _i += 809 - 534;
            if (Extended.In(_i, 565, 572))
                _i += 817 - 566;

            //trabia
            if (Extended.In(_i, 149, 150))
                _i += 825 - 150;

            //galbadia
            if (Extended.In(_i, 267, 267))
                _i = 826;

            ////balamb
            if (Extended.In(_i, 274, 275))
                _i += 828 - 275;

            ////base
            if (Extended.In(_i, 327, 327))
                _i += 830 - 328;

            ////trabia
            if (Extended.In(_i, 214, 215))
                _i += 831 - 215;
            if (Extended.In(_i, 246, 247))
                _i += 833 - 247;

            ////prison
            if (Extended.In(_i, 361, 361))
                _i += 835 - 362;
            #endregion


            effect.TextureEnabled = true;
            Segment seg = segments[_i];
            float localX = 0;//_i * 2048;
            for (int i = 0; i < seg.block.Length; i++)
            {
                localX = 2048 * (i % 4);
                float localZ = -2048 * (i / 4);





                VertexPositionTexture[] vpc = new VertexPositionTexture[seg.block[i].polygons.Length * 3];
                for (int k = 0; k < seg.block[i].polyCount * 3; k += 3)
                {
                    vpc[k] = new VertexPositionTexture(
                        new Vector3(((seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].Z1 / WORLD_SCALE_MODEL,
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U1 / 256.0f, seg.block[i].polygons[k / 3].V1 / 256.0f));
                    vpc[k + 1] = new VertexPositionTexture(
                        new Vector3(((seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].Z1 / WORLD_SCALE_MODEL,
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U2 / 256.0f, seg.block[i].polygons[k / 3].V2 / 256.0f));
                    vpc[k + 2] = new VertexPositionTexture(
                        new Vector3(((seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].X + localX) / WORLD_SCALE_MODEL + baseX) * -1f,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].Z1 / WORLD_SCALE_MODEL,
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].Y + localZ) / WORLD_SCALE_MODEL + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U3 / 256.0f, seg.block[i].polygons[k / 3].V3 / 256.0f));

                    if (Extended.Distance3D(camPosition, vpc[k].Position) > renderCamDistance)
                        continue;

                    float ax, ay, px, py, d1, d2, d3;

                    px = vpc[k].Position.X;
                    py = vpc[k].Position.Z;
                    ax = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(degrees)) * -50f;
                    ay = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(degrees)) * -50f;

                    Vector3 left=Vector3.Zero,right = Vector3.Zero;
                    left.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(Extended.ClampOverload(degrees-FOV, 0, 359))) * renderCamDistance*2;
                    left.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(Extended.ClampOverload(degrees-FOV, 0, 359))) * renderCamDistance*2;
                    right.X = camPosition.X + (float)Math.Cos(MathHelper.ToRadians(Extended.ClampOverload(degrees + FOV, 0, 359))) * renderCamDistance*2;
                    right.Z = camPosition.Z + (float)Math.Sin(MathHelper.ToRadians(Extended.ClampOverload(degrees + FOV, 0, 359))) * renderCamDistance*2;

                    d1 = px * (ay - left.Z) + py * (left.X - ax) + (ax * left.Z - ay * left.X);
                    d2 = px * (left.Z - right.Z) + py * (right.X - left.X) + (left.X * right.Z - left.Z * right.X);
                    d3 = px * (right.Z - ay) + py * (ax - right.X) + (right.X * ay - right.Z * ax);

                    if ((d1 > 0 || d2 > 0 || d3 > 0) && (d1 < 0 || d2 < 0 || d3 < 0))
                        continue;

                    if (seg.block[i].polygons[k / 3].TextureSwitch == 0x40 ||
                        seg.block[i].polygons[k / 3].TextureSwitch == 0xC0)
                    {
                        ate.Texture = wm38textures[16][0];
                    }   
                    else switch (seg.block[i].polygons[k / 3].TextureSwitch)
                        {
                            case 0x60:
                                ate.Texture = wm39textures[0][0];
                                break;
                            case 0xE0:
                                ate.Texture = wm39textures[5][0];
                                break;
                            default:
                                ate.Texture = textures[seg.block[i].polygons[k / 3].TPage][seg.block[i].polygons[k / 3].Clut]; //there are two texs, worth looking at other parameters; to reverse! 
                                break;
                        } //there are two texs, worth looking at other parameters; to reverse! 



                    foreach (var pass in ate.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        Memory.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpc, k, 1);
                    }
                }
            }
        }
    }
}
