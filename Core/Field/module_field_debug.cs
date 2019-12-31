using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static class Module_field_debug
    {
        public enum BlendModes : byte
        {
            halfadd,
            add,
            subtract,
            quarteradd,
            none,
        }

        private static Field_mods mod = 0;

        //private static Texture2D tex;
        //private static Texture2D texOverlap;
        private static ConcurrentDictionary<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<BlendModes, Texture2D>>> Textures;

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
            public BlendModes blendType;
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

        private static List<KeyValuePair<BlendModes, Texture2D>> drawtextures() => Textures.OrderByDescending(x => x.Key).SelectMany(x => x.Value.OrderBy(y => y.Key).SelectMany(y => y.Value)).ToList();

        private static void DrawDebug()
        {
            Memory.graphics.GraphicsDevice.Clear(Color.Black);

            List<KeyValuePair<BlendModes, Texture2D>> _drawtextures = drawtextures();
            bool open = false;
            BlendModes lastbm = BlendModes.none;
            float alpha = 1f;
            foreach (KeyValuePair<BlendModes, Texture2D> kvp in _drawtextures)
            {
                if (!open || lastbm != kvp.Key)
                {
                    if (open)
                        Memory.SpriteBatchEnd();
                    open = true;
                    alpha = 1f;
                    switch (kvp.Key)
                    {
                        default:
                            Memory.SpriteBatchStartAlpha();
                            break;

                        case BlendModes.halfadd:
                            //alpha = .5f; // not right but may work okay lets see.
                            Memory.SpriteBatchStart(bs: Memory.blendState_Add);
                            break;

                        case BlendModes.quarteradd:
                            //alpha = .25f; //not right but may work okay lets see.
                            Memory.SpriteBatchStart(bs: Memory.blendState_Add);
                            break;

                        case BlendModes.add:
                            Memory.SpriteBatchStart(bs: Memory.blendState_Add);
                            break;

                        case BlendModes.subtract:
                            //open = false;
                            //continue; // isn't working. unsure why.
                            alpha = .9f;
                            Memory.SpriteBatchStart(bs: Memory.blendState_Subtract);
                            break;
                    }
                    lastbm = kvp.Key;
                }
                Texture2D tex = kvp.Value;
                Rectangle src = new Rectangle(0, 0, tex.Width, tex.Height);
                Rectangle dst = src;
                dst.Size = (dst.Size.ToVector2() * Memory.Scale(tex.Width, tex.Height, Memory.ScaleMode.FitBoth)).ToPoint();
                //In game I think we'd keep the field from leaving the screen edge but would center on the Squall and the party when it can.
                //I setup scaling after noticing the field didn't size with the screen. I set it to center on screen.
                dst.Offset(Memory.Center.X - dst.Center.X, Memory.Center.Y - dst.Center.Y);
                Memory.spriteBatch.Draw(tex, dst, src, Color.White * alpha);
                //new Microsoft.Xna.Framework.Rectangle(0, 0, 1280 + (width - 320), 720 + (height - 224)),
                //new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height)
            }
            if (open)
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
            ArchiveWorker fieldArchive = aw.GetArchive(fieldArchiveName);
            string[] filelist = fieldArchive.GetListOfFiles();
            string findstr(string s) =>
                filelist.FirstOrDefault(x => x.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);

#pragma warning disable CS8321 // Local function is declared but never used
            byte[] getfile(string s)
#pragma warning restore CS8321 // Local function is declared but never used
            {
                s = findstr(s);
                if (!string.IsNullOrWhiteSpace(s))
                    return fieldArchive.GetBinaryFile(s);
                else
                    return null;
            }
            string s_jsm = findstr(".jsm");
            string s_sy = findstr(".sy");
            if (!ParseBackground(getfile(".mim"), getfile(".map")))
                return;
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

        private static bool ParseBackground(byte[] mimb, byte[] mapb)
        {
            if (mimb == null || mapb == null)
                return false;

            int type1Width = 1664;

            tiles = new List<Tile>();
            int palettees = 24;
            //128x256
            //using (BinaryReader pbsmim = new BinaryReader(new MemoryStream(mimb))
            const int tilesize = 16;
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
                    tile.blendType = (BlendModes)pbsmap.ReadByte();
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

            height = Math.Abs(lowestY) + maximumY + tilesize; //224
            width = Math.Abs(lowestX) + maximumX + tilesize; //320
            //Color[] finalImage = new Color[height * width]; //ARGB;
            //Color[] finalOverlapImage = new Color[height * width];
            //tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            //texOverlap = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            IOrderedEnumerable<byte> layers = tiles.Select(x => x.layId).Distinct().OrderBy(x => x);
            byte MaximumLayer = layers.Max();
            byte MinimumLayer = layers.Min();
            IOrderedEnumerable<ushort> BufferDepth = tiles.Select(x => x.z).Distinct().OrderByDescending(x => x); // larger number is farther away.

            if (Textures != null)
            {
                foreach (Texture2D tex in drawtextures().Select(x => x.Value))
                    tex.Dispose();
            }
            Textures = new ConcurrentDictionary<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<BlendModes, Texture2D>>>();
            foreach (ushort z in BufferDepth)
            {
                ConcurrentDictionary<byte, ConcurrentDictionary<BlendModes, Texture2D>> dictlayers = Textures.GetOrAdd(z, new ConcurrentDictionary<byte, ConcurrentDictionary<BlendModes, Texture2D>>());
                foreach (byte layerID in layers)
                // i saw atleast one field look better checking the z and not layerid.
                //maybe once we start drawing this in 3d space it'll make more sense.
                // z is def important. I think layerID might be more for turning things on and off or moving things around.
                // So maybe in the end we can have things grouped by layer but drawn in z index order
                // I think the layers more control what we are to see like animation frames of elevators and such.
                // like all layers aren't ment to be drawn at all times.
                // Sky texture is drawing differently for dollet bridge and such like i guess it's supposed to be stretched.
                // or maybe the bridge just moves infront of the sky so it doesn't need to fill the space.
                {
                    ConcurrentDictionary<BlendModes, Texture2D> dictblend = dictlayers.GetOrAdd(layerID, new ConcurrentDictionary<BlendModes, Texture2D>());
                    foreach (Tile tile in tiles.Where(x => x.z == z && x.layId == layerID))
                    {
                        //if (tile.z != z)
                        //    continue;
                        //if (tile.z != BufferDepth[LayerId])
                        //    continue;
                        //else
                        //if (LayerId != 0)
                        //    continue;

                        int palettePointer = 4096 + ((tile.pallID) * 512);
                        int sourceImagePointer = 512 * palettees;

                        int realX = Math.Abs(lowestX) + tile.x; //baseX
                        int realY = Math.Abs(lowestY) + tile.y; //*width
                        int realDestinationPixel = ((realY * width) + realX);
                        
                        if (tile.blend2 >= 4)
                        {
                            Color[] colors = new Color[tilesize * tilesize];
                            Rectangle dst = new Rectangle(realX, realY, tilesize, tilesize);
                            int startPixel = sourceImagePointer + tile.srcx + 128 * tile.texID + (type1Width * tile.srcy);
                            bool hasColor = false;
                            for (int y = 0; y < tilesize; y++)
                                for (int x = 0; x < tilesize; x++)
                                {
                                    int pos = x + (y * tilesize); //realDestinationPixel + (x) + (y * width);
                                    byte pixel = mimb[startPixel + x + (y * 1664)];
                                    ushort color16bit = BitConverter.ToUInt16(mimb, 2 * pixel + palettePointer);
                                    if (color16bit == 0) // 0 is 100 % transparent so not blending that pixel
                                        continue;
                                    Color color = Texture_Base.ABGR1555toRGBA32bit(color16bit);
                                    //bool stp = Texture_Base.GetSTP(color16bit);
                                    if (tile.blendType < BlendModes.none)
                                    {
                                        if (color == Color.Black)
                                            continue;
                                        if (tile.blendType == BlendModes.halfadd)
                                            colors[pos] = Color.Multiply(color, .5f);
                                        else if (tile.blendType == BlendModes.quarteradd)
                                            colors[pos] = Color.Multiply(color, .25f);
                                        else
                                            colors[pos] = color;
                                        colors[pos].A = 0xFF;
                                        hasColor = true;
                                        //stp should be set for pixel to blend
                                        //seems stp isn't being used in fields.
                                        //normally if black and stp enabled black isn't transparent.
                                        //but I think blendtype might override that.
                                        //since black put into blend modes won't do anything might
                                        // as well skip it.
                                        // might be spots that should be black that aren't and will need
                                        // to revisit this.
                                        //if (true)//!bSaveToOverlapBuffer)
                                        //    BlendColors(ref colors[pos], color, tile);
                                        //else
                                        //    BlendColors(ref finalOverlapImage[pos], color, tile);
                                    }
                                    else
                                    {
                                        if (true)//!bSaveToOverlapBuffer)
                                        {
                                            colors[pos] = color;
                                            colors[pos].A = 0xFF;
                                            hasColor = true;
                                        }
                                        //else
                                        //{
                                        //    finalOverlapImage[pos] = color;
                                        //    finalOverlapImage[pos].A = 0xFF;
                                        //}
                                    }
                                }
                            if (!hasColor)
                                continue;
                            Texture2D tex = dictblend.GetOrAdd(tile.blendType, new Texture2D(Memory.graphics.GraphicsDevice, width, height));
                            //using (RenderTarget2D rt = new RenderTarget2D(Memory.graphics.GraphicsDevice, tex.Width, tex.Height))
                            //{
                            //    Memory.graphics.GraphicsDevice.SetRenderTarget(rt);
                            //    Memory.SpriteBatchStartAlpha();
                            //    tex.SetData(0, dst, colors, 0, colors.Length);
                            //    Memory.spriteBatch.Draw(tex, Vector2.Zero, Color.White);
                            //    using (Texture2D t = new Texture2D(Memory.graphics.GraphicsDevice, tilesize, tilesize))
                            //    {
                            //        t.SetData(colors);
                            //        Memory.spriteBatch.Draw(t, dst, Color.White);
                            //    }
                            //    Memory.SpriteBatchEnd();
                            //    Memory.graphics.GraphicsDevice.SetRenderTarget(null);
                            //    rt.GetData(0, dst, colors, 0, colors.Length);
                            //    tex.SetData(0, dst, colors, 0, colors.Length);
                            //}
                            if (tile.blendType < BlendModes.none)
                            {
                                Color[] texColors = new Color[colors.Length];
                                tex.GetData(0, dst, texColors, 0, colors.Length);

                                for (int i = 0; i < texColors.Length; i++)
                                {
                                    Color tc = texColors[i];
                                    Color c = colors[i];
                                    if (tc != Color.TransparentBlack && c != Color.TransparentBlack)
                                    //check for overlapping pixels by checking if both are not transparent.
                                    {
                                        switch (tile.blendType)
                                        {
                                            case BlendModes.add:
                                            default:
                                                colors[i] = blend1(tc, c);
                                                break;

                                            case BlendModes.subtract:
                                                colors[i] = blend2(tc, c);
                                                break;

                                        }
                                    }
                                }
                            }
                            for (int i = 0; i < colors.Length; i++)
                            {
                                Rectangle r = new Rectangle(dst.X + i % dst.Width, dst.Y + i / dst.Width, 1, 1);
                                Color c = colors[i];
                                // skip transparent pixels. as this sets the value of the pixel to transparent.
                                if (c != Color.TransparentBlack)
                                    tex.SetData(0, r, colors, i, 1);
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
            }
            //tex.SetData(finalImage);
            //texOverlap.SetData(finalOverlapImage);
            if (Memory.EnableDumpingData || true)
            {
                //List<KeyValuePair<BlendModes, Texture2D>> _drawtextures = drawtextures();
                foreach (KeyValuePair<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<BlendModes, Texture2D>>> z in Textures)
                    foreach (KeyValuePair<byte, ConcurrentDictionary<BlendModes, Texture2D>> layer in z.Value)
                        foreach (KeyValuePair<BlendModes, Texture2D> kvp in layer.Value)
                        {
                            kvp.Value.SaveAsPng(
                                new FileStream(Path.Combine(Path.GetTempPath(),
                                $"Field.{Memory.FieldHolder.FieldID}.{z.Key.ToString("D4")}.{layer.Key.ToString("D2")}.{(int)kvp.Key}.png"),
                                FileMode.Create, FileAccess.Write, FileShare.ReadWrite),
                                kvp.Value.Width, kvp.Value.Height);
                        }
            }
            return true;
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
        private static Color BlendColors(ref Color finalImageColor, Color color, Tile tile)
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
            //BlendState blendmode1 = new BlendState
            //{
            //    ColorSourceBlend = Blend.SourceColor,
            //    ColorDestinationBlend = Blend.DestinationColor,
            //    ColorBlendFunction = BlendFunction.Add
            //};
            //BlendState blendmode2 = new BlendState
            //{
            //    ColorSourceBlend = Blend.SourceColor,
            //    ColorDestinationBlend = Blend.DestinationColor,
            //    ColorBlendFunction = BlendFunction.Subtract
            //};
            //BlendState blendmode3 = new BlendState
            //{
            //    BlendFactor =
            //    ColorSourceBlend = Blend.SourceColor,
            //    ColorDestinationBlend = Blend.DestinationColor,
            //    ColorBlendFunction = BlendFunction.Subtract
            //};

            switch (tile.blendType)
            {
                case BlendModes.halfadd:
                    return finalImageColor = blend0(baseColor, color);

                case BlendModes.add:
                    return finalImageColor = blend1(baseColor, color);

                case BlendModes.subtract:
                    return finalImageColor = blend2(baseColor, color);

                case BlendModes.quarteradd:
                    //break;
                    return finalImageColor = blend3(baseColor, color);
            }
            throw new Exception($"Blendtype is {tile.blendType}: There are only 4 blend modes, 0-3, 4+ are drawn directly.");
        }

        private static Color blend0(Color baseColor, Color color)
        {
            Color r;

            r.R = (byte)MathHelper.Clamp(baseColor.R + color.R / 2, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.R + color.G / 2, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B + color.B / 2, 0, 255);
            r.A = (byte)MathHelper.Clamp(baseColor.A + color.A, 0, 255);

            //r.R = (byte)((baseColor.R + color.R) / 2);
            //r.G = (byte)((baseColor.R + color.G) / 2);
            //r.B = (byte)((baseColor.B + color.B) / 2);
            return r;
        }

        private static Color blend1(Color baseColor, Color color)
        {
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R + color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G + color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B + color.B, 0, 255);
            r.A = (byte)MathHelper.Clamp(baseColor.A + color.A, 0, 255);
            return r;
        }

        private static Color blend2(Color baseColor, Color color)
        {
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R - color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G - color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B - color.B, 0, 255);
            r.A = (byte)MathHelper.Clamp(baseColor.A + color.A, 0, 255);
            return r;
        }

        private static Color blend3(Color baseColor, Color color)
        {
            Color r;
            r.R = (byte)MathHelper.Clamp((byte)(baseColor.R + (0.25 * color.R)), 0, 255);
            r.G = (byte)MathHelper.Clamp((byte)(baseColor.G + (0.25 * color.G)), 0, 255);
            r.B = (byte)MathHelper.Clamp((byte)(baseColor.B + (0.25 * color.B)), 0, 255);
            r.A = (byte)MathHelper.Clamp(baseColor.A + color.A, 0, 255);
            //r.R = (byte)MathHelper.Clamp((byte)(baseColor.R + (0.25 * color.R)), 0, 255);
            //r.G = (byte)MathHelper.Clamp((byte)(baseColor.G + (0.25 * color.G)), 0, 255);
            //r.B = (byte)MathHelper.Clamp((byte)(baseColor.B + (0.25 * color.B)), 0, 255);
            return r;
        }
    }
}