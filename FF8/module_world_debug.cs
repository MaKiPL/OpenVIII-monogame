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
        private enum _worldState
        {
            _0init,
            _1debugFly
        }


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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst =16)]
            public uint[] blockOffsets;
        }

        private struct Polygon
        {
            public byte F1, F2, F3, N1, N2, N3, U1, V1, U2, V2, U3, V3, TPage_clut, groundtype, unk1, unk2;

            public byte TPage { get => (byte)((TPage_clut>>4)&0xF0);}
            public byte Clut { get => (byte)(TPage_clut & 0x0F);}
            //public byte TPage_clut1 { set => TPage_clut = value; }
        }

        private struct Vertex
        {
            public short X;
            private short Z;
            private short Y;
            private short W;

            public short Z1 { get => (short)(Z*-1); set => Z = value; }
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
            switch(worldState)
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
            Memory.musicIndex = 30;
            init_debugger_Audio.PlayMusic();


            ReadWMX();


            worldState++;
            return;
        }

        private static void ReadWMX()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_WORLD);
            string wmxPath =  aw.GetListOfFiles().Where(x=>x.ToLower().Contains("wmx.obj")).Select(x=>x).First();
            wmx = ArchiveWorker.GetBinaryFile(Memory.Archives.A_WORLD, wmxPath);

            segments = new Segment[835];

            using (MemoryStream ms = new MemoryStream(wmx))
                using (BinaryReader br = new BinaryReader(ms))
                    for(int i = 0; i<segments.Length; i++)
                    {
                    //segHeader test = new segHeader();
                    //test.blockOffsets = new uint[16];
                    //var t = Marshal.SizeOf(typeof(segHeader));
                    //byte[] test2 = br.ReadBytes(68);
                    //GCHandle hwnd = GCHandle.Alloc(test2, GCHandleType.Pinned);
                    //var ttt = Marshal.PtrToStructure<segHeader>(hwnd.AddrOfPinnedObject());
                    //hwnd.Free();
                    ms.Seek(GetSegment(i), SeekOrigin.Begin);
                    segments[i] = new Segment() { segmentId = i, headerData = MakiExtended.ByteArrayToStructure<segHeader>(br.ReadBytes(68)), block =  new Block[16]};
                    ms.Seek(GetSegment(i), SeekOrigin.Begin);
                    for (int n = 0; n < segments[i].block.Length; n++)
                        {
                        ms.Seek(segments[i].headerData.blockOffsets[n] + GetSegment(i), SeekOrigin.Begin);
                        segments[i].block[n] = new Block() { polyCount = br.ReadByte(), vertCount = br.ReadByte(), normalCount = br.ReadByte(), unkPadd = br.ReadByte() };
                        segments[i].block[n].polygons = new Polygon[segments[i].block[n].polyCount];
                        segments[i].block[n].vertices = new Vertex[segments[i].block[n].vertCount];
                        segments[i].block[n].normals = new Normal[segments[i].block[n].normalCount];
                        for (int k = 0; k<segments[i].block[n].polyCount; k++)
                            segments[i].block[n].polygons[k] = MakiExtended.ByteArrayToStructure<Polygon>(br.ReadBytes(16/**segments[i].block[n].polyCount*/));
                        for (int k = 0; k < segments[i].block[n].vertCount; k++)
                            segments[i].block[n].vertices[k] = MakiExtended.ByteArrayToStructure<Vertex>(br.ReadBytes(8 /* *segments[i].block[n].vertCount*/));
                        for (int k = 0; k < segments[i].block[n].normalCount; k++)
                            segments[i].block[n].normals[k] = MakiExtended.ByteArrayToStructure<Normal>(br.ReadBytes(8 /** segments[i].block[n].normalCount*/));
                        segments[i].block[n].unkPadd2 = br.ReadInt32();
                        }
                    }
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

            for (int i = 0; i < 1; i++)
                DrawSegment(i);

            //foreach (var a in modelGroups)
            //    foreach (var b in a.models)
            //    {
            //        var vpt = getVertexBuffer(b);
            //        if (vpt == null) continue;
            //        int localVertexIndex = 0;
            //        for (int i = 0; i < vpt.Item1.Length; i++)
            //        {
            //            ate.Texture = textures[vpt.Item1[i].clut]; //provide texture per-face
            //            foreach (var pass in ate.CurrentTechnique.Passes)
            //            {
            //                pass.Apply();
            //                if (vpt.Item1[i].bQuad)
            //                {
            //                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
            //                    vertexData: vpt.Item2, vertexOffset: localVertexIndex, primitiveCount: 2);
            //                    localVertexIndex += 6;
            //                }
            //                else
            //                {
            //                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
            //                    vertexData: vpt.Item2, vertexOffset: localVertexIndex, primitiveCount: 1);
            //                    localVertexIndex += 3;
            //                }
            //            }
            //        }

                //}

            Memory.SpriteBatchStartAlpha();
            Memory.font.RenderBasicText(Font.CipherDirty($"World Map Debug"), 0, 0, 1, 1, 0, 1);
            //Memory.font.RenderBasicText(Font.CipherDirty($"Camera: {Memory.encounters[Memory.battle_encounter].bCamera}"), 20, 30, 1, 1, 0, 1);
            //Memory.font.RenderBasicText(Font.CipherDirty($"Enemies: {string.Join(",", Memory.encounters[Memory.battle_encounter].BEnemies.Where(x => x != 0x00).Select(x => "0x" + (x - 0x10).ToString("X02")).ToArray())}"), 20, 30 * 2, 1, 1, 0, 1);
            //Memory.font.RenderBasicText(Font.CipherDirty($"Levels: {string.Join(",", Memory.encounters[Memory.battle_encounter].bLevels)}"), 20, 30 * 3, 1, 1, 0, 1);
            //Memory.font.RenderBasicText(Font.CipherDirty($"Loaded enemies: {Convert.ToString(Memory.encounters[Memory.battle_encounter].bLoadedEnemy, 2)}"), 20, 30 * 4, 1, 1, 0, 1);
            Memory.SpriteBatchEnd();
        }

        private static void DrawSegment(int i)
        {
            Segment seg = segments[i];
        }
    }
}
