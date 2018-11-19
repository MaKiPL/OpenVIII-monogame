using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace FF8
{
    internal class module_field_debug
    {
        private static field_mods mod = 0;
        private static Texture2D tex;
        private static Texture2D texOverlap;

        private static List<Tile> tiles;

        private struct Tile
        {
            public short x, y;
            public ushort z;
            public byte texID; // 4 bits
            public byte pallID; //[6-10]
            public byte srcx, srcy;
            public byte layId;
            public byte blendType;
            public byte state;
            public byte parameter;
            public byte blend1;
            public byte blend2;
        }

        private static byte param_ = 0;
        private static byte state_ = 0;

        private static int width = 0;
        private static int height = 0;


        private enum field_mods
        {
            INIT,
            DEBUGRENDER
        };

        internal static void Draw()
        {
            switch (mod)
            {
                case field_mods.INIT:
                    break; //null
                case field_mods.DEBUGRENDER:
                    DrawDebug();
                    break;
            }
        }

        public static void ResetField()
            => mod = field_mods.INIT;

        private static void DrawDebug()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            Memory.SpriteBatchStartStencil();
            Memory.spriteBatch.Draw(tex,
                new Microsoft.Xna.Framework.Rectangle(0, 0, 1280+(width-320), 720+(height-224)),
                new Microsoft.Xna.Framework.Rectangle(0,0,tex.Width, tex.Height)
                , Microsoft.Xna.Framework.Color.White);
            Memory.SpriteBatchEnd();
        }

        internal static void Update()
        {
            switch (mod)
            {
                case field_mods.INIT:
                    Init();
                    break;
                case field_mods.DEBUGRENDER:
                    break; //await events here
            }
        }

        private static void Init()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.FF8DIR + "field.fs");
            string[] test = aw.GetListOfFiles();
            if (Memory.FieldHolder.FieldID >= Memory.FieldHolder.fields.Length ||
                Memory.FieldHolder.FieldID < 0)
                return;
            var CollectionEntry = test.Where(x => x.ToLower().Contains(Memory.FieldHolder.fields[Memory.FieldHolder.FieldID]));
            if (CollectionEntry.Count() == 0) return;
            string fieldArchive = CollectionEntry.First();
            int fieldLen = fieldArchive.Length - 2;
            fieldArchive = fieldArchive.Substring(0, fieldLen);
            byte[] fs = ArchiveWorker.GetBinaryFile(Memory.FF8DIR + "field", $"{fieldArchive}fs");
            byte[] fi = ArchiveWorker.GetBinaryFile(Memory.FF8DIR + "field", $"{fieldArchive}fi");
            byte[] fl = ArchiveWorker.GetBinaryFile(Memory.FF8DIR + "field", $"{fieldArchive}fl");
            if (fs == null || fi == null || fl == null) return;
            string[] test_ = ArchiveWorker.GetBinaryFileList(fl);
            string mim = test_.Where(x => x.ToLower().Contains(".mim")).First();
            string map = test_.Where(x => x.ToLower().Contains(".map")).First();
            byte[] mimb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, mim);
            byte[] mapb = ArchiveWorker.FileInTwoArchives(fi, fs, fl, map);
            if (mimb == null || mapb == null) return;

            int type1Width = 1664;

            tiles = new List<Tile>();
            int palletes = 24;
            //128x256
            PseudoBufferedStream pbsmap = new PseudoBufferedStream(mapb);
            PseudoBufferedStream pbsmim = new PseudoBufferedStream(mimb);
            while (pbsmap.Tell() + 16 < pbsmap.Length)
            {
                Tile tile = new Tile();
                tile.x = pbsmap.ReadShort();
                if (tile.x == 0x7FFF)
                    break;
                tile.y = pbsmap.ReadShort();
                tile.z = pbsmap.ReadUShort();// (ushort)(4096 - pbsmap.ReadUShort());
                byte texIdBuffer = pbsmap.ReadByte();
                tile.texID = (byte)(texIdBuffer & 0xF);
                pbsmap.Seek(1, PseudoBufferedStream.SEEK_CURRENT);
                //short testz = pbsmap.ReadShort();
                //testz = (short)(testz >> 6);
                //testz &= 0xF;
                tile.pallID = (byte)((pbsmap.ReadShort() >> 6) & 0xF);
                tile.srcx = pbsmap.ReadByte();
                tile.srcy = pbsmap.ReadByte();
                tile.layId = (byte)(pbsmap.ReadByte() & 0x7F);
                tile.blendType = pbsmap.ReadByte();
                tile.parameter = pbsmap.ReadByte();
                tile.state = pbsmap.ReadByte();
                tile.blend1 = (byte)((texIdBuffer >> 4) & 0x1);
                tile.blend2 = (byte)(texIdBuffer >> 5);
                tiles.Add(tile);
                //srcY = srcX == texID * 128 + srcX;
            }

            int lowestY = tiles.Min(x => x.y);
            int maximumY = tiles.Max(x => x.y);
            int lowestX = tiles.Min(x => x.x); //-160;
            int maximumX = tiles.Max(x => x.x);

            height = Math.Abs(lowestY) + maximumY + 16; //224
            width = Math.Abs(lowestX) + maximumX + 16; //320
            byte[] finalImage = new byte[height * width * 4]; //ARGB;
            byte[] finalOverlapImage = new byte[height * width * 4];
            tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            texOverlap = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            var MaximumLayer = tiles.Max(x => x.layId);
            var MinimumLayer = tiles.Min(x => x.layId);
            
            List<ushort> BufferDepth = tiles.GroupBy(x => x.z).Select(group => group.First()).Select(x => x.z).ToList();
            BufferDepth.Sort();
            
            for (int LayerId = 1; LayerId <= MaximumLayer+1; LayerId++)
            {
                foreach (Tile tile in tiles)
                {
                    if (LayerId != MaximumLayer+1)
                    {
                        if (tile.layId != LayerId)
                            continue;
                        //if (tile.z != BufferDepth[LayerId])
                        //    continue;
                    }
                    else
                        if (tile.layId != 0)
                            continue;
                    
                    int palettePointer = 4096 + ((tile.pallID) * 512);
                    int sourceImagePointer = 512 * palletes;

                    int realX = Math.Abs(lowestX) + tile.x; //baseX
                    int realY = Math.Abs(lowestY) + tile.y; //*width
                    int realDestinationPixel = ((realY * width) + realX) * 4;
                    if (tile.blend2 >= 4)
                    {
                        int startPixel = sourceImagePointer + tile.srcx + 128 * tile.texID + (type1Width * tile.srcy);
                        for (int y = 0; y < 16; y++)
                            for (int x = 0; x < 16; x++)
                            {
                                byte pixel = mimb[startPixel + x + (y * 1664)];
                                ushort pixels = BitConverter.ToUInt16(mimb, 2 * pixel + palettePointer);
                                if (pixels == 00)
                                    continue;
                                byte red = (byte)((pixels) & 0x1F);
                                byte green = (byte)((pixels >> 5) & 0x1F);
                                byte blue = (byte)((pixels >> 10) & 0x1F);
                                red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                                green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                                blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                                if (tile.blendType < 4)
                                {
                                    if (true)//!bSaveToOverlapBuffer)
                                    {

                                        byte baseColorR = finalImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                                        byte baseColorG = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                                        byte baseColorB = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                                        Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalImage, realDestinationPixel, x, y);
                                    }
                                    else
                                    {
                                        byte baseColorR = finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                                        byte baseColorG = finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                                        byte baseColorB = finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                                        Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalOverlapImage, realDestinationPixel, x, y);
                                    }
                                }
                                else
                                {
                                    if (true)//!bSaveToOverlapBuffer)
                                    {
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = red;
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = green;
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = blue;
                                        finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 3] = 0xFF;
                                    }
                                    else
                                    {
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4)] = red;
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = green;
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = blue;
                                        finalOverlapImage[realDestinationPixel + (x * 4) + (y * width * 4) + 3] = 0xFF;
                                    }
                                }
                            }
                    }
                    else
                    {
                        ;
                        //    int startPixel = sourceImagePointer + tile.srcx / 2 + 128 * tile.texID + (type1Width * tile.srcy);
                        //    for (int y = 0; y < 16; y++)
                        //        for (int x = 0; x < 16; x++)
                        //        {
                        //            byte index = mimb[startPixel + x + (y * 1664)];
                        //            ushort pixels = BitConverter.ToUInt16(mimb, 2 * (index & 0xF) + palettePointer);
                        //            byte red = (byte)((pixels) & 0x1F);
                        //            byte green = (byte)((pixels >> 5) & 0x1F);
                        //            byte blue = (byte)((pixels >> 10) & 0x1F);
                        //            red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                        //            green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                        //            blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                        //            if (pixels != 0)
                        //            {

                        //                if (tile.blendType < 4)
                        //                {
                        //                    byte baseColorR = finalImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                        //                    byte baseColorG = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                        //                    byte baseColorB = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                        //                    Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalImage, realDestinationPixel, x, y);
                        //                }
                        //            }
                        //            pixels = BitConverter.ToUInt16(mimb, 2 * (index >> 4) + palettePointer);
                        //            red = (byte)((pixels) & 0x1F);
                        //            green = (byte)((pixels >> 5) & 0x1F);
                        //            blue = (byte)((pixels >> 10) & 0x1F);
                        //            red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                        //            green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                        //            blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                        //            if (pixels != 0)
                        //            {

                        //                if (tile.blendType < 4)
                        //                {
                        //                    byte baseColorR = finalImage[realDestinationPixel + (x * 4) + (y * width * 4)];
                        //                    byte baseColorG = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1];
                        //                    byte baseColorB = finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2];
                        //                    Blend(baseColorR, baseColorG, baseColorB, red, green, blue, tile, finalImage, realDestinationPixel, x, y);
                        //                }
                        //            }
                        //            else
                        //            {
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = red;
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = green;
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = blue;
                        //                finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 3] = 0xFF;
                        //            }
                        //        }
                    }
                }
            }
            tex.SetData(finalImage);
            texOverlap.SetData(finalOverlapImage);


            mod++;
            return;
        }

        private static void DrawEntities()
        {
            throw new NotImplementedException();
        }

        private static void Blend(byte baseColorR, byte baseColorG, byte baseColorB, byte red, byte green, byte blue, Tile tile, byte[] finalImage, int realDestinationPixel, int x, int y)
        {
            switch (tile.blendType)
            {
                case 0:
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)((baseColorR + red) / 2);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)((baseColorG + green) / 2);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)((baseColorB + blue) / 2);
                    break;
                case 1:
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)MathHelper.Clamp(baseColorR + red, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)MathHelper.Clamp(baseColorG + green, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)MathHelper.Clamp(baseColorB + blue, 0, 255);
                    break;
                case 2:
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)MathHelper.Clamp(baseColorR - red, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)MathHelper.Clamp(baseColorG - green, 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)MathHelper.Clamp(baseColorB - blue, 0, 255);
                    break;
                case 3:
                    break;
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4)] = (byte)MathHelper.Clamp((byte)(baseColorR + (0.25 * red)), 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 1] = (byte)MathHelper.Clamp((byte)(baseColorG + (0.25 * green)), 0, 255);
                    finalImage[realDestinationPixel + (x * 4) + (y * width * 4) + 2] = (byte)MathHelper.Clamp((byte)(baseColorB + (0.25 * blue)), 0, 255);
                    break;
            }
        }
    }
}