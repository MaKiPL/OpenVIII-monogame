using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static class Module_field_debug
    {
        private static Field_mods mod = 0;
        private static Texture2D tex;
        private static Texture2D texOverlap;
        private static EventEngine eventEngine;
        private static IServices services;
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

        private static int width, height;

        private enum Field_mods
        {
            INIT,
            DEBUGRENDER,
            DISABLED
        };

        public static void Draw()
        {
            switch (mod)
            {
                case Field_mods.INIT:
                    break; //null
                case Field_mods.DEBUGRENDER:
                    DrawDebug();
                    break;
            }
        }

        public static void ResetField() => mod = Field_mods.INIT;

        private static void DrawDebug()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);
            Memory.SpriteBatchStartStencil();
            Rectangle src = new Rectangle(0, 0, tex.Width, tex.Height);
            Rectangle dst = src;
            dst.Size = (dst.Size.ToVector2() * Memory.Scale(tex.Width, tex.Height)).ToPoint();
            //In game I think we'd keep the field from leaving the screen edge but would center on the Squall and the party when it can.
            //I setup scaling after noticing the field didn't size with the screen. I set it to center on screen.
            dst.Offset(Memory.Center.X - dst.Center.X, Memory.Center.Y - dst.Center.Y);
            Memory.spriteBatch.Draw(tex, dst, src, Color.White);
            //new Microsoft.Xna.Framework.Rectangle(0, 0, 1280 + (width - 320), 720 + (height - 224)),
            //new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height)
            Memory.SpriteBatchEnd();
        }

        public static void Update()
        {
#if DEBUG
            // lets you move through all the feilds just holding left or right. it will just loop when it runs out.
            if (Input2.DelayedButton(FF8TextTagKey.Left))
            {
                init_debugger_Audio.PlaySound(0);
                if (Memory.FieldHolder.FieldID > 0)
                    Memory.FieldHolder.FieldID--;
                else
                    Memory.FieldHolder.FieldID = checked((ushort)(Memory.FieldHolder.fields.Length - 1));
                ResetField();
            }
            if (Input2.DelayedButton(FF8TextTagKey.Right))
            {
                init_debugger_Audio.PlaySound(0);
                if (Memory.FieldHolder.FieldID < checked((ushort)(Memory.FieldHolder.fields.Length - 1)))
                    Memory.FieldHolder.FieldID++;
                else
                    Memory.FieldHolder.FieldID = 0;
                ResetField();
            }
#endif
            switch (mod)
            {
                case Field_mods.INIT:
                    Init();
                    break;

                case Field_mods.DEBUGRENDER:
                    UpdateScript();
                    break; //await events here
                case Field_mods.DISABLED:
                    break;
            }
        }

        private static void UpdateScript()
        {
            //We do not know every instruction and it's not possible for now to play field with unknown instruction
            //eventEngine.Update(services);
        }

        private static void Init()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_FIELD);
            string[] test = aw.GetListOfFiles();

            //TODO fix endless look on FieldID 50.
            if (Memory.FieldHolder.FieldID >= Memory.FieldHolder.fields.Length ||
                Memory.FieldHolder.FieldID < 0)
                return;
            string fieldArchiveName = test.FirstOrDefault(x => x.IndexOf(Memory.FieldHolder.fields[Memory.FieldHolder.FieldID], StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fieldArchiveName)) return;
            int fieldLen = fieldArchiveName.Length - 3;
            fieldArchiveName = fieldArchiveName.Substring(0, fieldLen);
            ArchiveWorker fieldArchive = aw.GetArchive(fieldArchiveName);
            string[] filelist = fieldArchive.GetListOfFiles();
            string findstr(string s) =>
                filelist.FirstOrDefault(x => x.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);

            byte[] getfile(string s)
            {
                s = findstr(s);
                if (!string.IsNullOrWhiteSpace(s))
                    return fieldArchive.GetBinaryFile(s);
                else
                    return null;
            }
            string mim = findstr(".mim");
            string map = findstr(".map");
            string s_jsm = findstr(".jsm");
            string s_sy = findstr(".sy");
            if (!string.IsNullOrWhiteSpace(mim) && !string.IsNullOrWhiteSpace(map))
                ParseBackground(fieldArchive.GetBinaryFile(mim), fieldArchive.GetBinaryFile(map));
            //let's start with scripts
            List<Jsm.GameObject> jsmObjects;

            if (!string.IsNullOrWhiteSpace(s_jsm))
            {
                try
                {
                    jsmObjects = Jsm.File.Read(fieldArchive.GetBinaryFile(s_jsm));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    mod = Field_mods.DISABLED;
                    return;
                }

                Sym.GameObjects symObjects;
                if (!string.IsNullOrWhiteSpace(s_sy))
                {
                    symObjects = Sym.Reader.FromBytes(fieldArchive.GetBinaryFile(s_sy));
                }
                else
                    return;
                services = FieldInitializer.GetServices();
                eventEngine = ServiceId.Field[services].Engine;
                eventEngine.Reset();
                for (int objIndex = 0; objIndex < jsmObjects.Count; objIndex++)
                {
                    Jsm.GameObject obj = jsmObjects[objIndex];
                    FieldObject fieldObject = new FieldObject(obj.Id, symObjects.GetObjectOrDefault(objIndex).Name);

                    foreach (Jsm.GameScript script in obj.Scripts)
                        fieldObject.Scripts.Add(script.ScriptId, script.Segment.GetExecuter());

                    eventEngine.RegisterObject(fieldObject);
                }
            }
            else
            {
                mod = Field_mods.DISABLED;
                return;
            }

            //byte[] mchb = getfile(".mch");//Field character models
            //byte[] oneb = getfile(".one");//Field character models container
            //byte[] msdb = getfile(".msd");//dialogs
            //byte[] infb = getfile(".inf");//gateways
            //byte[] idb = getfile(".id");//walkmesh
            //byte[] cab = getfile(".ca");//camera
            //byte[] tdwb = getfile(".tdw");//extra font
            //byte[] mskb = getfile(".msk");//movie cam
            //byte[] ratb = getfile(".rat");//battle on field
            //byte[] pmdb = getfile(".pmd");//particle info
            //byte[] sfxb = getfile(".sfx");//sound effects

            mod++;
            return;
        }

        private static void ParseBackground(byte[] mimb, byte[] mapb)
        {
            if (mimb == null || mapb == null)
                return;

            int type1Width = 1664;

            tiles = new List<Tile>();
            int palettees = 24;
            //128x256
            //using (BinaryReader pbsmim = new BinaryReader(new MemoryStream(mimb))
            using (BinaryReader pbsmap = new BinaryReader(new MemoryStream(mapb)))
                while (pbsmap.BaseStream.Position + 16 < pbsmap.BaseStream.Length)
                {
                    Tile tile = new Tile { x = pbsmap.ReadInt16() };
                    if (tile.x == 0x7FFF)
                        break;
                    tile.y = pbsmap.ReadInt16();
                    tile.z = pbsmap.ReadUInt16();// (ushort)(4096 - pbsmap.ReadUShort());
                    byte texIdBuffer = pbsmap.ReadByte();
                    tile.texID = (byte)(texIdBuffer & 0xF);
                    pbsmap.BaseStream.Seek(1, SeekOrigin.Current);
                    //short testz = pbsmap.ReadShort();
                    //testz = (short)(testz >> 6);
                    //testz &= 0xF;
                    tile.pallID = (byte)((pbsmap.ReadInt16() >> 6) & 0xF);
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
            Color[] finalImage = new Color[height * width]; //ARGB;
            Color[] finalOverlapImage = new Color[height * width];
            tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            texOverlap = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            byte MaximumLayer = tiles.Max(x => x.layId);
            byte MinimumLayer = tiles.Min(x => x.layId);

            List<ushort> BufferDepth = tiles.GroupBy(x => x.z).Select(group => group.First()).Select(x => x.z).ToList();
            BufferDepth.Sort();

            for (int LayerId = 1; LayerId <= MaximumLayer + 1; LayerId++)
            {
                foreach (Tile tile in tiles)
                {
                    if (LayerId != MaximumLayer + 1)
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
                    int sourceImagePointer = 512 * palettees;

                    int realX = Math.Abs(lowestX) + tile.x; //baseX
                    int realY = Math.Abs(lowestY) + tile.y; //*width
                    int realDestinationPixel = ((realY * width) + realX);
                    if (tile.blend2 >= 4)
                    {
                        int startPixel = sourceImagePointer + tile.srcx + 128 * tile.texID + (type1Width * tile.srcy);
                        for (int y = 0; y < 16; y++)
                            for (int x = 0; x < 16; x++)
                            {
                                int pos = realDestinationPixel + (x) + (y * width);
                                byte pixel = mimb[startPixel + x + (y * 1664)];
                                ushort color16bit = BitConverter.ToUInt16(mimb, 2 * pixel + palettePointer);
                                if (color16bit == 0) // 0 is 100 % transparent so not blending that pixel
                                    continue;
                                Color color = Texture_Base.ABGR1555toRGBA32bit(color16bit);
                                //bool stp = Texture_Base.GetSTP(color16bit);
                                if (tile.blendType < 4 /*&& stp*/)
                                {
                                    if (color == Color.Black)
                                        continue;
                                    //stp should be set for pixel to blend
                                    //seems stp isn't being used in fields.
                                    //normally if black and stp enabled black isn't transparent.
                                    //but I think blendtype might override that.
                                    //since black put into blend modes won't do anything might
                                    // as well skip it.
                                    // might be spots that should be black that aren't and will need
                                    // to revisit this.
                                    if (true)//!bSaveToOverlapBuffer)
                                        Blend(ref finalImage[pos], color, tile);
                                    else
                                        Blend(ref finalOverlapImage[pos], color, tile);
                                }
                                else
                                {
                                    if (true)//!bSaveToOverlapBuffer)
                                    {
                                        finalImage[pos] = color;
                                        finalImage[pos].A = 0xFF;
                                    }
                                    else
                                    {
                                        finalOverlapImage[pos] = color;
                                        finalOverlapImage[pos].A = 0xFF;
                                    }
                                }
                            }
                    }
                    else
                    {
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
        }

        private static void DrawEntities() => throw new NotImplementedException();

        /// <summary>
        /// Blend the colors depending on tile.blendmode
        /// </summary>
        /// <param name="finalImageColor"></param>
        /// <param name="color"></param>
        /// <param name="tile"></param>
        /// <returns>Color</returns>
        /// <see cref="http://www.raphnet.net/electronique/psx_adaptor/Playstation.txt"/>
        /// <seealso cref="http://www.psxdev.net/forum/viewtopic.php?t=953"/>
        /// <seealso cref="//http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_MAP"/>
        private static Color Blend(ref Color finalImageColor, Color color, Tile tile)
        {
            //• Semi Transparency
            //When semi transparency is set for a pixel, the GPU first reads the pixel it wants to write to, and then calculates
            //the color it will write from the 2 pixels according to the semi - transparency mode selected.Processing speed is lower
            //in this mode because additional reading and calculating are necessary.There are 4 semi - transparency modes in the
            //GPU.
            //B = the pixel read from the image in the frame buffer, F = the half transparent pixel
            //• 1.0 x B + 0.5 x F
            //• 1.0 x B + 1.0 x F
            //• 1.0 x B - 1.0 x F
            //• 1.0 x B + 0.25 x F
            //color must not be black
            Color baseColor = finalImageColor;
            Color blend0()
            {
                Color r;

                r.R = (byte)MathHelper.Clamp(baseColor.R + color.R / 2, 0, 255);
                r.G = (byte)MathHelper.Clamp(baseColor.R + color.G / 2, 0, 255);
                r.B = (byte)MathHelper.Clamp(baseColor.B + color.B / 2, 0, 255);

                //r.R = (byte)((baseColor.R + color.R) / 2);
                //r.G = (byte)((baseColor.R + color.G) / 2);
                //r.B = (byte)((baseColor.B + color.B) / 2);
                return r;
            }
            Color blend1()
            {
                Color r;
                r.R = (byte)MathHelper.Clamp(baseColor.R + color.R, 0, 255);
                r.G = (byte)MathHelper.Clamp(baseColor.G + color.G, 0, 255);
                r.B = (byte)MathHelper.Clamp(baseColor.B + color.B, 0, 255);
                return r;
            }
            Color blend2()
            {
                Color r;
                r.R = (byte)MathHelper.Clamp(baseColor.R - color.R, 0, 255);
                r.G = (byte)MathHelper.Clamp(baseColor.G - color.G, 0, 255);
                r.B = (byte)MathHelper.Clamp(baseColor.B - color.B, 0, 255);
                return r;
            }
            Color blend3()
            {
                Color r;
                r.R = (byte)MathHelper.Clamp((byte)(baseColor.R + (0.25 * color.R)), 0, 255);
                r.G = (byte)MathHelper.Clamp((byte)(baseColor.G + (0.25 * color.G)), 0, 255);
                r.B = (byte)MathHelper.Clamp((byte)(baseColor.B + (0.25 * color.B)), 0, 255);
                //r.R = (byte)MathHelper.Clamp((byte)(baseColor.R + (0.25 * color.R)), 0, 255);
                //r.G = (byte)MathHelper.Clamp((byte)(baseColor.G + (0.25 * color.G)), 0, 255);
                //r.B = (byte)MathHelper.Clamp((byte)(baseColor.B + (0.25 * color.B)), 0, 255);
                return r;
            }
            switch (tile.blendType)
            {
                case 0:
                    return finalImageColor = blend0();

                case 1:
                    return finalImageColor = blend1();

                case 2:
                    return finalImageColor = blend2();

                case 3:
                    //break;
                    return finalImageColor = blend3();
            }
            throw new Exception($"Blendtype is {tile.blendType}: There are only 4 blend modes, 0-3, 4+ are drawn directly.");
        }
    }
}