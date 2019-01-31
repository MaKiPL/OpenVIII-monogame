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

namespace FF8
{
    class module_world_debug
    {
        private static Matrix projectionMatrix;
        private static Matrix viewMatrix;
        private static Matrix worldMatrix;
        private static float degrees = 0;
        private static float Yshift = 0;
        private static float camDistance = 10.0f;
        private static Vector3 camPosition;
        private static Vector3 camTarget;
        public static BasicEffect effect;
        public static AlphaTestEffect ate;
        private enum _worldState
        {
            _0init,
            _1debugFly
        }

        //DEBUG
        private static int nk = 0;

        private static List<Texture2D[]> textures;
        private static List<Texture2D[]> wm38textures;

        private static byte[] wmx;

        private static int GetSegment(int segID) => segID * 0x9000;

        private static Segment[] segments;

        private struct Segment
        {
            public int segmentId;
            public segHeader headerData;
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
        private struct segHeader
        {
            public uint groupId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public uint[] blockOffsets;
        }

        private struct Polygon
        {
            public byte F1, F2, F3, N1, N2, N3, U1, V1, U2, V2, U3, V3, TPage_clut, groundtype, unk1, unk2;

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

        public static void Update()
        {
            switch (worldState)
            {
                case _worldState._0init:
                    InitWorld();
                    break;
                case _worldState._1debugFly:
                    break;
            }
        }

        private static void InitWorld()
        {
            //init renderer
            effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            effect.EnableDefaultLighting();
            camTarget = new Vector3(0, 0f, 0f);
            camPosition = new Vector3(0f, 50f, -100f);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f),
                               Memory.graphics.GraphicsDevice.DisplayMode.AspectRatio,
                1f, 10000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);

            //temporarily disabling this, because I'm getting more and more tired of this music playing over and over when debugging
            //Memory.musicIndex = 30;
            //init_debugger_Audio.PlayMusic();

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
            wmx = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmxPath);
            byte[] texl = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, texlPath);
            ReadWmSet(ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmPath));

            ReadTextures(texl);
            segments = new Segment[835];

            using (MemoryStream ms = new MemoryStream(wmx))
            using (BinaryReader br = new BinaryReader(ms))
                for (int i = 0; i < segments.Length; i++)
                {
                    ms.Seek(GetSegment(i), SeekOrigin.Begin);
                    segments[i] = new Segment() { segmentId = i, headerData = MakiExtended.ByteArrayToStructure<segHeader>(br.ReadBytes(68)), block = new Block[16] };
                    ms.Seek(GetSegment(i), SeekOrigin.Begin);
                    for (int n = 0; n < segments[i].block.Length; n++)
                    {
                        ms.Seek(segments[i].headerData.blockOffsets[n] + GetSegment(i), SeekOrigin.Begin);
                        segments[i].block[n] = new Block() { polyCount = br.ReadByte(), vertCount = br.ReadByte(), normalCount = br.ReadByte(), unkPadd = br.ReadByte() };
                        segments[i].block[n].polygons = new Polygon[segments[i].block[n].polyCount];
                        segments[i].block[n].vertices = new Vertex[segments[i].block[n].vertCount];
                        segments[i].block[n].normals = new Normal[segments[i].block[n].normalCount];
                        for (int k = 0; k < segments[i].block[n].polyCount; k++)
                            segments[i].block[n].polygons[k] = MakiExtended.ByteArrayToStructure<Polygon>(br.ReadBytes(16));
                        for (int k = 0; k < segments[i].block[n].vertCount; k++)
                            segments[i].block[n].vertices[k] = MakiExtended.ByteArrayToStructure<Vertex>(br.ReadBytes(8));
                        for (int k = 0; k < segments[i].block[n].normalCount; k++)
                            segments[i].block[n].normals[k] = MakiExtended.ByteArrayToStructure<Normal>(br.ReadBytes(8));
                        segments[i].block[n].unkPadd2 = br.ReadInt32();
                    }
                }
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

                wm_s38(ms, br, sections, v);
            }
        }

        private static void wm_s38(MemoryStream ms,BinaryReader br, int[] sec, byte[] v)
        {
            ms.Seek(sec[38-1], SeekOrigin.Begin);
            List<int> wm38sections = new List<int>();
            int eof = -1;
            while((eof = br.ReadInt32()) != 0)
                wm38sections.Add(eof);
            wm38textures = new List<Texture2D[]>();

            for(int i = 0; i<wm38sections.Count; i++)
            {
                TIM2 tim = new TIM2(v, (uint)(sec[38 - 1] + wm38sections[i]));
                wm38textures.Add(new Texture2D[tim.GetClutCount]);
                for(int k= 0; k<wm38textures[i].Length; k++)
                {
                    wm38textures[i][k] = new Texture2D(Memory.graphics.GraphicsDevice, tim.GetWidth, tim.GetHeight, false, SurfaceFormat.Color);
                    wm38textures[i][k].SetData(tim.CreateImageBuffer(tim.GetClutColors(k), true));
                }
            }
        }

        #endregion

        private static void ReadTextures(byte[] texl)
        {
            MemoryStream ms = new MemoryStream(texl);
            BinaryReader br = new BinaryReader(ms);
            textures = new List<Texture2D[]>(); //20

            for(int i = 0; i<20; i++)
            {
                int timOffset = i * 0x12800;
                TIM2 tim = new TIM2(texl, (uint)timOffset);
                textures.Add(new Texture2D[tim.GetClutCount]);
                for(int k=0; k<textures[i].Length; k++)
                {
                    textures[i][k] = new Texture2D(Memory.graphics.GraphicsDevice, tim.GetWidth, tim.GetHeight, false, SurfaceFormat.Color);
                    textures[i][k].SetData(tim.CreateImageBuffer(tim.GetClutColors(k), true));
                }
            }

            br.Close();
            ms.Close();
            br.Dispose();
            ms.Dispose();
        }

        public static void Draw()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Memory.graphics.GraphicsDevice.RasterizerState = rasterizerState;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice);
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
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance / 1;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance / 1;
                camPosition.Y -= Yshift / 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < 0.0f)
            {
                camPosition.X -= (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance / 1;
                camPosition.Z -= (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance / 1;
                camPosition.Y += Yshift / 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0.0f)
            {
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees - 90)) * camDistance / 1;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees - 90)) * camDistance / 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0.0f)
            {
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees + 90)) * camDistance / 1;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees + 90)) * camDistance / 1;
            }

            Mouse.SetPosition(200, 200);

            camTarget.X = camPosition.X + (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Z = camPosition.Z + (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Y = camPosition.Y - Yshift / 5;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            #endregion

            //334 debug
            //for (int i = 333; i < 334; i++)
            //    DrawSegment(i);

            if (Input.GetInputDelayed(Keys.P))
                nk++;

            //0,1,2,3,4,5,6,7
            var DEBUG_segGroupHeaders = segments.GroupBy(x=> x.headerData.groupId).Select(h=>h.First().headerData.groupId).ToArray();

            DrawSegment(nk);
            //Console.WriteLine($"DEBUG: nk {nk}\tgpId: {segments[nk].headerData.groupId}");

            Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(Font.CipherDirty($"World Map Debug: nk={nk}"), 0, 0, 1, 1, 0, 1);
            Memory.SpriteBatchEnd();
        }

        private static void DrawSegment(int _i)
        {
            float baseX, baseY;
            if(_i> 768) //TODO http://forums.qhimm.com/index.php?topic=16230.msg230004#msg230004 
            {
                baseX = 0f; baseY = 0f;
            }
            else
            {
                //baseX = (2048f/4) * (_i % 32);
                //baseY = (2048f/4) * (int)(_i / 32); //explicit int cast
                baseX = 0f;
                baseY = 0f;
            }


            effect.TextureEnabled = true;
            Segment seg = segments[_i];
            float localX = 0;//_i * 2048;
            for(int i = 0; i<seg.block.Length; i++)
            {
                localX = 2048 * (i % 4);
                float localZ = -2048 * (i / 4);





                VertexPositionTexture[] vpc = new VertexPositionTexture[seg.block[i].polygons.Length*3];
                for(int k=0; k<seg.block[i].polyCount*3; k+=3)
                {
                    vpc[k] = new VertexPositionTexture(
                        new Vector3((seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].X + localX) / 1f + baseX,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].Z1 / 1f,
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].Y + localZ) / 1f + baseY), 
                        new Vector2(seg.block[i].polygons[k/3].U1 /256.0f , seg.block[i].polygons[k / 3].V1 / 256.0f));
                    vpc[k + 1] = new VertexPositionTexture(
                        new Vector3((seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].X + localX) / 1f + baseX,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].Z1 / 1f,
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].Y + localZ) / 1f + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U2 / 256.0f, seg.block[i].polygons[k / 3].V2 / 256.0f));
                    vpc[k + 2] = new VertexPositionTexture(
                        new Vector3((seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].X + localX) / 1f + baseX,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].Z1 / 1f,
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].Y + localZ) / 1f + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U3 / 256.0f, seg.block[i].polygons[k / 3].V3 / 256.0f));

                    if (seg.headerData.groupId > 20)
                    {
                        //TODO;
                        ate.Texture = wm38textures[16][0];
                    }
                    else
                        ate.Texture = textures[(int)seg.headerData.groupId][seg.block[i].polygons[k/3].Clut]; //there are two texs, worth looking at other parameters; to reverse! 

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
