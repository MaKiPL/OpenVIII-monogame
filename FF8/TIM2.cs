using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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

        public struct Color
        {
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte Alpha;
        }

        private int bpp = -1;

        private Texture texture;
        private PseudoBufferedStream pbs;
        private uint textureDataPointer;
        private uint timOffset;

        public TIM2(byte[] buffer, uint offset = 0)
        {
            pbs = new PseudoBufferedStream(buffer);
            timOffset = offset;
            pbs.Seek(offset, System.IO.SeekOrigin.Begin);
            pbs.Seek(4, System.IO.SeekOrigin.Current); //clutID
            byte bppIndicator = pbs.ReadByte();
            bppIndicator = (byte)(bppIndicator == 0x08 ? 4 :
                bppIndicator == 0x09 ? 8 :
                bppIndicator == 0x02 ? 16 :
                bppIndicator == 0x03 ? 24 : 8);
            bpp = bppIndicator;
            texture = new Texture();
            ReadParameters(bppIndicator);
        }

        public void KillStreams() => pbs.DisposeAll();

        private void ReadParameters(byte _bpp)
        {
            pbs.Seek(3, System.IO.SeekOrigin.Current);
            if (_bpp == 4)
            {
                texture.clutSize = pbs.ReadUInt() - 12;
                texture.PaletteX = pbs.ReadUShort();
                texture.PaletteY = pbs.ReadUShort();
                texture.NumOfColours = pbs.ReadUShort();
                texture.NumOfCluts = pbs.ReadUShort();
                int bppMultiplier = 16;
                if (texture.NumOfColours != 16 || texture.clutSize != (texture.NumOfCluts * bppMultiplier)) //wmsetus uses 4BPP, but sets 256 colours, but actually is 16, but num of clut is 2* 256/16 WTF?
                {
                    texture.NumOfCluts = (ushort)(texture.NumOfColours / 16 * texture.NumOfCluts);
                    bppMultiplier = 32;
                }
                byte[] buffer = new byte[texture.NumOfCluts * bppMultiplier];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = pbs.ReadByte();
                texture.ClutData = buffer;
                pbs.Seek(4, System.IO.SeekOrigin.Current);
                texture.ImageOrgX = pbs.ReadUShort();
                texture.ImageOrgY = pbs.ReadUShort();
                texture.Width = (ushort)(pbs.ReadUShort() * 4);
                texture.Height = pbs.ReadUShort();
                textureDataPointer = (uint)pbs.Tell();
                return;
            }
            if (_bpp == 8)
            {
                pbs.Seek(4, System.IO.SeekOrigin.Current);
                texture.PaletteX = pbs.ReadUShort();
                texture.PaletteY = pbs.ReadUShort();
                pbs.Seek(2, System.IO.SeekOrigin.Current);
                texture.NumOfCluts = pbs.ReadUShort();
                byte[] buffer = new byte[texture.NumOfCluts * 512];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = pbs.ReadByte();
                texture.ClutData = buffer;
                pbs.Seek(4, System.IO.SeekOrigin.Current);
                texture.ImageOrgX = pbs.ReadUShort();
                texture.ImageOrgY = pbs.ReadUShort();
                texture.Width = (ushort)(pbs.ReadUShort() * 2);
                texture.Height = pbs.ReadUShort();
                textureDataPointer = (uint)pbs.Tell();
                return;
            }
            if (_bpp == 16)
            {
                pbs.Seek(4, System.IO.SeekOrigin.Current);
                texture.ImageOrgX = pbs.ReadUShort();
                texture.ImageOrgY = pbs.ReadUShort();
                texture.Width = pbs.ReadUShort();
                texture.Height = pbs.ReadUShort();
                textureDataPointer = (uint)pbs.Tell();
                return;
            }
            if (_bpp != 24) return;
            pbs.Seek(4, System.IO.SeekOrigin.Current);
            texture.ImageOrgX = pbs.ReadUShort();
            texture.ImageOrgY = pbs.ReadUShort();
            texture.Width = (ushort)(pbs.ReadUShort() / 1.5);
            texture.Height = pbs.ReadUShort();
            textureDataPointer = (uint)pbs.Tell();
        }

        public Color[] GetClutColors(Font.ColorID clut) => GetClutColors((ushort)clut);

        public Color[] GetClutColors(int clut) => GetClutColors((ushort)clut);

        public Color[] GetClutColors(ushort clut)
        {
            if (clut > texture.NumOfCluts)
                throw new Exception("TIM_v2::GetClutColors::given clut is bigger than texture number of cluts");
            List<Color> colorPixels = new List<Color>();
            if (bpp == 8)
            {
                pbs.Seek(timOffset + 20 + (512 * clut), System.IO.SeekOrigin.Begin);
                for (int i = 0; i < 512 / 2; i++)
                {
                    ushort clutPixel = pbs.ReadUShort();
                    byte red = (byte)((clutPixel) & 0x1F);
                    byte green = (byte)((clutPixel >> 5) & 0x1F);
                    byte blue = (byte)((clutPixel >> 10) & 0x1F);
                    red = (byte)MathHelper.Clamp((red * bpp), 0, 255);
                    green = (byte)MathHelper.Clamp((green * bpp), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * bpp), 0, 255);
                    colorPixels.Add(new Color() { Red = red, Green = green, Blue = blue });
                }
            }
            if (bpp == 4)
            {
                pbs.Seek(timOffset + 20 + (32 * clut), System.IO.SeekOrigin.Begin);
                for (int i = 0; i < 16; i++)
                {
                    ushort clutPixel = pbs.ReadUShort();
                    byte red = (byte)((clutPixel) & 0x1F);
                    byte green = (byte)((clutPixel >> 5) & 0x1F);
                    byte blue = (byte)((clutPixel >> 10) & 0x1F);
                    byte alpha = (byte)(clutPixel >> 11 & 1);
                    red = (byte)MathHelper.Clamp((red * bpp * 4), 0, 255);
                    green = (byte)MathHelper.Clamp((green * bpp * 4), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * bpp * 4), 0, 255);
                    colorPixels.Add(new Color { Red = red, Green = green, Blue = blue, Alpha = alpha });
                }
            }
            if (bpp > 8) throw new Exception("TIM that has bpp mode higher than 8 has no clut data!");
            return colorPixels.ToArray();
        }

        public byte[] CreateImageBuffer(Color[] palette = null, bool bIgnoreSize = false)
        {
            pbs.Seek(textureDataPointer, System.IO.SeekOrigin.Begin);
            byte[] buffer = new byte[texture.Width * texture.Height * 4]; //ARGB
            if (bpp == 8)
            {
                if (!bIgnoreSize)
                    if ((buffer.Length) / 4 != pbs.Length - pbs.Tell())
                        throw new Exception("TIM_v2::CreateImageBuffer::TIM texture buffer has size incosistency.");
                for (int i = 0; i < buffer.Length; i++)
                {
                    byte pixel = pbs.ReadByte();
                    Color ColoredPixel = palette[pixel];

                    buffer[i] = ColoredPixel.Red;
                    buffer[++i] = ColoredPixel.Green;
                    buffer[++i] = ColoredPixel.Blue;
                    buffer[++i] = (byte)((ColoredPixel.Red == 0 && ColoredPixel.Green == 0 && ColoredPixel.Blue == 0) ? 0x00 : 0xFF);
                }
            }
            if (bpp == 4)
            {
                if (!bIgnoreSize)
                    if ((buffer.Length) / 8 != pbs.Length - pbs.Tell())
                        throw new Exception("TIM_v2::CreateImageBuffer::TIM texture buffer has size incosistency.");
                for (int i = 0; i < buffer.Length; i++)
                {
                    byte pixel = pbs.ReadByte();
                    Color ColoredPixel = palette[pixel & 0xf];
                    buffer[i] = ColoredPixel.Red;
                    buffer[++i] = ColoredPixel.Green;
                    buffer[++i] = ColoredPixel.Blue;
                    buffer[++i] = ColoredPixel.Alpha;

                    ColoredPixel = palette[pixel >> 4];

                    buffer[++i] = ColoredPixel.Red;
                    buffer[++i] = ColoredPixel.Green;
                    buffer[++i] = ColoredPixel.Blue;
                    buffer[++i] = ColoredPixel.Alpha;
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
        /// <remarks>These files are just the image data with no header and no clut data. So above doesn't work with this.</remarks>
        public static Texture2D Overture(byte[] buffer)
        {
            var ImageOrgX = BitConverter.ToUInt16(buffer, 0x00);
            var ImageOrgY = BitConverter.ToUInt16(buffer, 0x02);
            var Width = BitConverter.ToUInt16(buffer, 0x04);
            var Height = BitConverter.ToUInt16(buffer, 0x06);
            Texture2D splashTex = new Texture2D(Memory.graphics.GraphicsDevice, Width, Height, false, SurfaceFormat.Color);
            lock (splashTex)
            {
                byte[] rgbBuffer = new byte[splashTex.Width * splashTex.Height * 4];
                int innerBufferIndex = 0x08;
                for (int i = 0; i < rgbBuffer.Length && innerBufferIndex < buffer.Length; i += 4)
                {
                    if (innerBufferIndex + 1 >= buffer.Length)
                    {
                        break;
                    }

                    ushort pixel = (ushort)((buffer[innerBufferIndex + 1] << 8) | buffer[innerBufferIndex]);
                    byte red = (byte)((pixel) & 0x1F);
                    byte green = (byte)((pixel >> 5) & 0x1F);
                    byte blue = (byte)((pixel >> 10) & 0x1F);
                    red = (byte)MathHelper.Clamp((red * 8), 0, 255);
                    green = (byte)MathHelper.Clamp((green * 8), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * 8), 0, 255);
                    rgbBuffer[i] = red;
                    rgbBuffer[i + 1] = green;
                    rgbBuffer[i + 2] = blue;
                    rgbBuffer[i + 3] = 255;//(byte)(((pixel >> 7) & 0x1) == 1 ? 255 : 0);
                    innerBufferIndex += 2;
                }
                splashTex.SetData(rgbBuffer);
            }
            return splashTex;
        }
    }
}