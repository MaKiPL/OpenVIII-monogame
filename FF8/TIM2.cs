using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    //upgraded TIM class, because that first one is a trash
    public class TIM2
    {
        public struct Texture
        {
            public ushort PaletteX;
            public ushort PaletteY;
            public ushort NumOfColours;
            public ushort NumOfCluts;
            public uint clutSize;
            public byte[] ClutData;
            public ushort ImageOrgX;
            public ushort ImageOrgY;
            public ushort Width;
            public ushort Height;
        }

        private int bpp = -1;

        private Texture texture;
        private uint textureDataPointer;
        private uint timOffset;
        private byte[] buffer;

        public TIM2(byte[] buffer, uint offset = 0)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                this.buffer = buffer;
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                br.BaseStream.Seek(4, SeekOrigin.Current);//clutID
                timOffset = offset;
                byte bppIndicator = br.ReadByte();
                bppIndicator = (byte)(bppIndicator == 0x08 ? 4 :
                    bppIndicator == 0x09 ? 8 :
                    bppIndicator == 0x02 ? 16 :
                    bppIndicator == 0x03 ? 24 : 8);
                bpp = bppIndicator;
                texture = new Texture();
                ReadParameters(br, bppIndicator);
            }
        }

        private void ReadParameters(BinaryReader br, byte _bpp)
        {
            br.BaseStream.Seek(3, SeekOrigin.Current);
            if (_bpp == 4)
            {
                texture.clutSize = br.ReadUInt32() - 12;
                texture.PaletteX = br.ReadUInt16();
                texture.PaletteY = br.ReadUInt16();
                texture.NumOfColours = br.ReadUInt16();
                texture.NumOfCluts = br.ReadUInt16();
                int bppMultiplier = 16;
                if (texture.NumOfColours != 16 || texture.clutSize != (texture.NumOfCluts * bppMultiplier)) //wmsetus uses 4BPP, but sets 256 colours, but actually is 16, but num of clut is 2* 256/16 WTF?
                {
                    texture.NumOfCluts = (ushort)(texture.NumOfColours / 16 * texture.NumOfCluts);
                    bppMultiplier = 32;
                }
                byte[] buffer = new byte[texture.NumOfCluts * bppMultiplier];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = br.ReadByte();
                texture.ClutData = buffer;
                br.BaseStream.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                texture.Width = (ushort)(br.ReadUInt16() * 4);
                texture.Height = br.ReadUInt16();
                textureDataPointer = (uint)br.BaseStream.Position;
                return;
            }
            if (_bpp == 8)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current);
                texture.PaletteX = br.ReadUInt16();
                texture.PaletteY = br.ReadUInt16();
                br.BaseStream.Seek(2, SeekOrigin.Current);
                texture.NumOfCluts = br.ReadUInt16();
                byte[] buffer = new byte[texture.NumOfCluts * 512];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = br.ReadByte();
                texture.ClutData = buffer;
                br.BaseStream.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                texture.Width = (ushort)(br.ReadUInt16() * 2);
                texture.Height = br.ReadUInt16();
                textureDataPointer = (uint)br.BaseStream.Position;
                return;
            }
            if (_bpp == 16)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                texture.Width = br.ReadUInt16();
                texture.Height = br.ReadUInt16();
                textureDataPointer = (uint)br.BaseStream.Position;
                return;
            }
            if (_bpp != 24) return;
            br.BaseStream.Seek(4, SeekOrigin.Current);
            texture.ImageOrgX = br.ReadUInt16();
            texture.ImageOrgY = br.ReadUInt16();
            texture.Width = (ushort)(br.ReadUInt16() / 1.5);
            texture.Height = br.ReadUInt16();
            textureDataPointer = (uint)br.BaseStream.Position;
        }

        private Color[] GetClutColors(BinaryReader br, ushort clut)
        {
            if (clut > texture.NumOfCluts)
                throw new Exception("TIM_v2::GetClutColors::given clut is bigger than texture number of cluts");

                List<Color> colorPixels = new List<Color>(GetWidth * GetHeight);
                if (bpp == 8)
                {
                    br.BaseStream.Seek(timOffset + 20 + (512 * clut), SeekOrigin.Begin);
                    for (int i = 0; i < 512 / 2; i++)
                    {
                        ushort clutPixel = br.ReadUInt16();
                        colorPixels.Add(new Color
                        {
                            R = (byte)MathHelper.Clamp(((clutPixel) & 0x1F) * bpp, 0, 255),
                            G = (byte)MathHelper.Clamp(((clutPixel >> 5) & 0x1F) * bpp, 0, 255),
                            B = (byte)MathHelper.Clamp(((clutPixel >> 10) & 0x1F) * bpp, 0, 255),
                        });
                    }
                }
                else if (bpp == 4)
                {
                    br.BaseStream.Seek(timOffset + 20 + (32 * clut), SeekOrigin.Begin);
                    for (int i = 0; i < 16; i++)
                    {
                        ushort clutPixel = br.ReadUInt16();
                        colorPixels.Add(new Color
                        {
                            R = (byte)MathHelper.Clamp(((clutPixel) & 0x1F) * bpp * 4, 0, 255),
                            G = (byte)MathHelper.Clamp(((clutPixel >> 5) & 0x1F) * bpp * 4, 0, 255),
                            B = (byte)MathHelper.Clamp(((clutPixel >> 10) & 0x1F) * bpp * 4, 0, 255),
                            A = (byte)(clutPixel >> 11 & 1)
                        });
                    }
                }
                else if (bpp > 8) throw new Exception("TIM that has bpp mode higher than 8 has no clut data!");

                return colorPixels.ToArray();
            
        }

        public Texture2D GetTexture(ushort? clut = null, bool bIgnoreSize = false)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                Texture2D image = new Texture2D(Memory.graphics.GraphicsDevice, GetWidth, GetHeight, false, SurfaceFormat.Color);
                image.SetData(CreateImageBuffer(br, clut == null ? null : GetClutColors(br,clut.Value), bIgnoreSize));
                return image;
            }
        }

        private Color[] CreateImageBuffer(BinaryReader br, Color[] palette = null, bool bIgnoreSize = false)
        {
            br.BaseStream.Seek(textureDataPointer, SeekOrigin.Begin);
            Color[] buffer = new Color[texture.Width * texture.Height]; //ARGB
            if (bpp == 8)
            {
                if (!bIgnoreSize)
                    if ((buffer.Length) != br.BaseStream.Length - br.BaseStream.Position)
                        throw new Exception("TIM_v2::CreateImageBuffer::TIM texture buffer has size incosistency.");
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = palette[br.ReadByte()]; //colorkey
                    if (buffer[i] != Color.TransparentBlack)
                        buffer[i].A = 0xFF;
                }
            }
            if (bpp == 4)
            {
                if (!bIgnoreSize)
                    if ((buffer.Length) / 2 != br.BaseStream.Length - br.BaseStream.Position)
                        throw new Exception("TIM_v2::CreateImageBuffer::TIM texture buffer has size incosistency.");
                for (int i = 0; i < buffer.Length; i++)
                {
                    byte colorkey = br.ReadByte();
                    buffer[i] = palette[colorkey & 0xf];
                    buffer[++i] = palette[colorkey >> 4];
                }
            }
            //Then in bs debug where ReadTexture store for all cluts
            //data and then create Texture2D from there. (array of e.g. 15 texture2D)
            return buffer;
        }

        public int GetClutCount => texture.NumOfCluts;

        public int GetWidth => texture.Width;

        public int GetHeight => texture.Height;

        /// <summary>
        /// Splash is 640x400 16BPP typical TIM with palette of ggg bbbbb a rrrrr gg
        /// </summary>
        /// <param name="buffer">raw 16bpp image</param>
        /// <returns>Texture2D</returns>
        /// <remarks>
        /// These files are just the image data with no header and no clut data. So above doesn't
        /// work with this.
        /// </remarks>
        public static Texture2D Overture(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                //var ImageOrgX = BitConverter.ToUInt16(buffer, 0x00);
                //var ImageOrgY = BitConverter.ToUInt16(buffer, 0x02);

                ms.Seek(0x04, SeekOrigin.Begin);
                ushort Width = br.ReadUInt16();
                ushort Height = br.ReadUInt16();
                Texture2D splashTex = new Texture2D(Memory.graphics.GraphicsDevice, Width, Height, false, SurfaceFormat.Color);
                lock (splashTex)
                {
                    Color[] rgbBuffer = new Color[Width * Height];
                    for (int i = 0; i < rgbBuffer.Length && ms.Position + 2 < ms.Length; i++)
                    {
                        ushort pixel = br.ReadUInt16();
                        rgbBuffer[i] = new Color
                        {
                            R = (byte)MathHelper.Clamp(((pixel) & 0x1F) * 8, 0, 255),
                            G = (byte)MathHelper.Clamp(((pixel >> 5) & 0x1F) * 8, 0, 255),
                            B = (byte)MathHelper.Clamp(((pixel >> 10) & 0x1F) * 8, 0, 255),
                            A = 0xFF
                        };
                    }
                    splashTex.SetData(rgbBuffer);
                }
                return splashTex;
            }
        }
    }
}