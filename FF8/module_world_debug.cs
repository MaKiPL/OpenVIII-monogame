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
        private const float WORLD_SCALE_MODEL = 16f;

        private static List<Texture2D[]> textures;
        private static List<Texture2D[]> wm38textures;
        private static List<Texture2D[]> wm39textures;
        private static List<Texture2D[]> charaOneTextures;
        private static readonly int renderDistance = 2;

        private static Vector2 segmentPosition;

        private static byte[] wmx;

        static float DEBUGshit = 0.0f;

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
            camPosition = new Vector3(50f, 50f, 50f);
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

        private static void ReadCharaOne(byte[] charaOneB)
        {
            using (MemoryStream ms = new MemoryStream(charaOneB))
            using (BinaryReader br = new BinaryReader(ms))
            {

                uint eof = br.ReadUInt32();
                TIM2 tim;
                while(ms.CanRead)
                if (br.ReadUInt32() == 16 && br.ReadUInt32() == 8)
                {
                    ms.Seek(-8, SeekOrigin.Current);
                    tim = new TIM2(charaOneB, (uint)ms.Position);
                    ms.Seek(tim.GetHeight *tim.GetWidth/2 + 64, SeekOrigin.Current); //i.e. 64*20=1280/2=640 + 64= 704 + eof
                        if (charaOneTextures == null)
                            charaOneTextures = new List<Texture2D[]>();
                        charaOneTextures.Add(new Texture2D[1] { new Texture2D(Memory.graphics.GraphicsDevice, tim.GetWidth, tim.GetHeight, false, SurfaceFormat.Color) });
                        charaOneTextures.Last()[0].SetData(tim.CreateImageBuffer(tim.GetClutColors(0), true));
                }
                else //is geometry structure
                    {
                        ms.Seek(-8, SeekOrigin.Current);
                        uint esi10h = BitConverter.ToUInt32(charaOneB, (int)ms.Position + 0x10);
                        uint esi14h = BitConverter.ToUInt32(charaOneB, (int)ms.Position + 0x14);
                        uint u45 = BitConverter.ToUInt32(charaOneB, (int)ms.Position + 56) + 2; //+2?
                        uint v29 = BitConverter.ToUInt32(charaOneB, (int)ms.Position) << 6; //bone section?
                        uint d250516c = BitConverter.ToUInt32(charaOneB, (int)ms.Position+4) * 8; //unk?
                        uint v25 = BitConverter.ToUInt32(charaOneB, (int)ms.Position + 8); //unk size?

                        ms.Seek(u45, SeekOrigin.Begin);

                        ushort v8 = br.ReadUInt16();
                        ushort v9 = br.ReadUInt16();


                        

                        return; //TODO
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

                Wm_s38(ms, br, sections, v);
                wm_s39(ms, br, sections, v);
            }
        }

        private static void wm_s39(MemoryStream ms, BinaryReader br, int[] sec, byte[] v)
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
                for (int k = 0; k < wm39textures[i].Length; k++)
                {
                    wm39textures[i][k] = new Texture2D(Memory.graphics.GraphicsDevice, tim.GetWidth, tim.GetHeight, false, SurfaceFormat.Color);
                    wm39textures[i][k].SetData(tim.CreateImageBuffer(tim.GetClutColors(k), true));
                }
            }
        }

        private static void Wm_s38(MemoryStream ms,BinaryReader br, int[] sec, byte[] v)
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
            Memory.spriteBatch.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            Memory.spriteBatch.Draw(wm38textures[10][0], new Rectangle(0, 0, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)), Color.White * .1f);
            Memory.spriteBatch.End();



            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
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
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance * 5 / 5;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance*5 / 5;
                camPosition.Y -= Yshift / 10;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < 0.0f)
            {
                camPosition.X -= (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance*5 / 5;
                camPosition.Z -= (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance * 5 / 5;
                camPosition.Y += Yshift / 10;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0.0f)
            {
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees - 90)) * camDistance * 5 / 5;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees - 90)) * camDistance * 5 / 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0.0f)
            {
                camPosition.X += (float)System.Math.Cos(MathHelper.ToRadians(degrees + 90)) * camDistance * 5 / 5;
                camPosition.Z += (float)System.Math.Sin(MathHelper.ToRadians(degrees + 90)) * camDistance * 5 / 5;
            }

            Mouse.SetPosition(200, 200);

            camTarget.X = camPosition.X + (float)System.Math.Cos(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Z = camPosition.Z + (float)System.Math.Sin(MathHelper.ToRadians(degrees)) * camDistance;
            camTarget.Y = camPosition.Y - Yshift / 5;
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            #endregion

            if (Keyboard.GetState().IsKeyDown(Keys.P))
                DEBUGshit += .01f;


            //334 debug
            for (int i = 0; i < 768; i++)
                DrawSegment(i);

            if (Input.GetInputDelayed(Keys.P))
                ;
            //worldScaleModel+=0.10f;

            //0,1,2,3,4,5,6,7
            //byte[] aa = (from s in segments from blok in s.block from bk in blok.polygons select bk.isWater).ToArray();
            //string[] cc = (from s in aa.Distinct() orderby s select $"{s}:  {Convert.ToString(s, 2).PadLeft(8, '0')}").ToArray();
            //DrawSegment(nk);
            //Console.WriteLine($"DEBUG: nk {nk}\tgpId: {segments[nk].headerData.groupId}");

            Memory.spriteBatch.Begin(SpriteSortMode.BackToFront, Memory.blendState_BasicAdd);
            Memory.spriteBatch.Draw(wm38textures[11][1], new Rectangle((int)(Memory.graphics.GraphicsDevice.Viewport.Width * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height * 0.60f), (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f)),Color.White*.7f);
            Memory.spriteBatch.End();

            //cursor is wm38[24][0] and -16384 is max X, where 12288 is max Y

            float topX = Memory.graphics.GraphicsDevice.Viewport.Width * .6f; //6
            float topY = Memory.graphics.GraphicsDevice.Viewport.Height * .6f;


            float bc = Math.Abs(camPosition.X / 16384.0f);
            topX += (Memory.graphics.GraphicsDevice.Viewport.Width / 2.8f * bc);
            bc = Math.Abs(camPosition.Z / 12288f);
            topY += (Memory.graphics.GraphicsDevice.Viewport.Height / 2.8f * bc);


            Memory.SpriteBatchStartAlpha();
            Memory.spriteBatch.Draw(wm38textures[24][0], new Rectangle((int)topX, (int)topY, (int)(Memory.graphics.GraphicsDevice.Viewport.Width / 32.0f), (int)(Memory.graphics.GraphicsDevice.Viewport.Height / 32.0f)),null,  Color.White * 1f, degrees * 6.3f / 360f + 2.5f , Vector2.Zero, SpriteEffects.None, 1f);
            Memory.font.RenderBasicText(Font.CipherDirty($"World Map Debug: nk={WORLD_SCALE_MODEL}"), 0, 0, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"World Map Camera: X={camPosition}"), 0, 30, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"Segment Position: ={segmentPosition}"), 0, 30 * 2, 1, 1, 0, 1);
            Memory.font.RenderBasicText(Font.CipherDirty($"FPS camera deegress: ={DEBUGshit}"), 0, 30 * 3, 1, 1, 0, 1);
            Memory.SpriteBatchEnd();

            
        }

        private static bool ShouldDrawSegment(float baseX, float baseY, int seg)
        {
            segmentPosition = new Vector2((int)(camPosition.X / 512) *-1, (int)(camPosition.Z / 512) * -1);

            if (camPosition.X > 0)
                camPosition.X = 32*512*-1;
            if (camPosition.X < 32 * 512 * -1)
                camPosition.X = 0;

            if (camPosition.Z > 0)
                camPosition.Z = 24 * 512 * -1;
            if (camPosition.Z < 24 * 512 * -1)
                camPosition.Z = 0;


            int ySegment = seg / 32; //2
            int xSegment = seg- ySegment*32;
            Vector2 currentSegment = new Vector2(xSegment, ySegment);


            //Vector2 topLeft = segmentPosition - new Vector2(renderDistance, renderDistance);
            //Vector2 bottomRight = segmentPosition + new Vector2(renderDistance, renderDistance);

            for (int i = -1-renderDistance; i<renderDistance; i++)
                for(int k = 0-renderDistance; k<renderDistance; k++)
                    if (segmentPosition + new Vector2(i, k) == currentSegment)
                        return true;

            return false;
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
                baseX = (2048f / 4) * (_i % 32);
                baseY = -(2048f / 4) * (int)(_i / 32); //explicit int cast
                //baseX = _i*4f;
                //baseY = _i*4;
            }

            if (!ShouldDrawSegment(baseX, baseY, _i))
                return;


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
                        new Vector3(((seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].X + localX) / (WORLD_SCALE_MODEL) + baseX) *-1f,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].Z1 / (WORLD_SCALE_MODEL),
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F1].Y + localZ) / (WORLD_SCALE_MODEL) + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U1 / 256.0f, seg.block[i].polygons[k / 3].V1 / 256.0f));
                    vpc[k + 1] = new VertexPositionTexture(
                        new Vector3(((seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].X + localX) / (WORLD_SCALE_MODEL) + baseX)*-1f,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].Z1 / (WORLD_SCALE_MODEL),
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F2].Y + localZ) / (WORLD_SCALE_MODEL) + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U2 / 256.0f, seg.block[i].polygons[k / 3].V2 / 256.0f));
                    vpc[k + 2] = new VertexPositionTexture(
                        new Vector3(((seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].X + localX) / (WORLD_SCALE_MODEL) + baseX)*-1f,
                        seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].Z1 / (WORLD_SCALE_MODEL),
                        (seg.block[i].vertices[seg.block[i].polygons[k / 3].F3].Y + localZ) / (WORLD_SCALE_MODEL) + baseY),
                        new Vector2(seg.block[i].polygons[k / 3].U3 / 256.0f, seg.block[i].polygons[k / 3].V3 / 256.0f));

                    if (seg.block[i].polygons[k / 3].TextureSwitch == 0x40 ||
                        seg.block[i].polygons[k / 3].TextureSwitch == 0xC0)
                    {
                        ate.Texture = wm38textures[16][0];
                    }
                    else if (seg.block[i].polygons[k / 3].TextureSwitch == 0x60)
                        ate.Texture = wm39textures[0][0];
                    else if (seg.block[i].polygons[k / 3].TextureSwitch == 0xE0)
                        ate.Texture = wm39textures[5][0];
                    else
                        ate.Texture = textures[seg.block[i].polygons[k / 3].TPage][seg.block[i].polygons[k / 3].Clut]; //there are two texs, worth looking at other parameters; to reverse! 

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
