using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    //upgraded TIM class, because that first one is a trash
    class TIM2
    {
        public struct Texture
        {
            public ushort PaletteX;
            public ushort PaletteY;
            public ushort NumOfCluts;
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

        private static int bpp = -1;

        private static Texture texture;
        private static PseudoBufferedStream pbs;
        private static uint textureDataPointer = 0;
        private static uint timOffset;

        public TIM2(byte[] buffer, uint offset = 0)
        {
            pbs = new PseudoBufferedStream(buffer);
            timOffset = offset;
            pbs.Seek(offset, PseudoBufferedStream.SEEK_BEGIN);
            pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT); //clutID
            byte bppIndicator = pbs.ReadByte();
            bppIndicator = (byte)(bppIndicator == 0x08 ? 4 : 
                bppIndicator == 0x09 ? 8 : 
                bppIndicator == 0x02 ? 16 :
                bppIndicator == 0x03 ? 24 : 8);
            bpp = bppIndicator;
            texture = new Texture();
            ReadParameters(bppIndicator);
        }

        private void ReadParameters(byte bpp) //in TIM v1 this was as sbyte, why? xD
        {
            pbs.Seek(3, PseudoBufferedStream.SEEK_CURRENT);
            if (bpp == 4)
            {
                pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT);
                texture.PaletteX = pbs.ReadUShort();
                texture.PaletteY = pbs.ReadUShort();
                pbs.Seek(2, PseudoBufferedStream.SEEK_CURRENT);
                texture.NumOfCluts = pbs.ReadUShort();
                byte[] buffer = new byte[texture.NumOfCluts * 16];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = pbs.ReadByte();
                texture.ClutData = buffer;
                pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT);
                texture.ImageOrgX = pbs.ReadUShort();
                texture.ImageOrgY = pbs.ReadUShort();
                texture.Width = (ushort)(pbs.ReadUShort() * 4);
                texture.Height = pbs.ReadUShort();
                textureDataPointer = (uint)pbs.Tell();
                return;
            }
            if (bpp == 8)
            {
                pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT);
                texture.PaletteX = pbs.ReadUShort();
                texture.PaletteY = pbs.ReadUShort();
                pbs.Seek(2, PseudoBufferedStream.SEEK_CURRENT);
                texture.NumOfCluts = pbs.ReadUShort();
                byte[] buffer = new byte[texture.NumOfCluts * 512];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = pbs.ReadByte();
                texture.ClutData = buffer;
                pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT);
                texture.ImageOrgX = pbs.ReadUShort();
                texture.ImageOrgY = pbs.ReadUShort();
                texture.Width = (ushort)(pbs.ReadUShort() * 2);
                texture.Height = pbs.ReadUShort();
                textureDataPointer = (uint)pbs.Tell();
                return;
            }
            if (bpp == 16)
            {
                pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT);
                texture.ImageOrgX = pbs.ReadUShort();
                texture.ImageOrgY = pbs.ReadUShort();
                texture.Width = pbs.ReadUShort();
                texture.Height = pbs.ReadUShort();
                textureDataPointer = (uint)pbs.Tell();
                return;
            }
            if (bpp != 24) return;
            pbs.Seek(4, PseudoBufferedStream.SEEK_CURRENT);
            texture.ImageOrgX = pbs.ReadUShort();
            texture.ImageOrgY = pbs.ReadUShort();
            texture.Width = (ushort)(pbs.ReadUShort() / 1.5);
            texture.Height = pbs.ReadUShort();
            textureDataPointer = (uint)pbs.Tell();
        }

        public Color[] GetClutColors(int clut)
        {
            if (clut > texture.NumOfCluts)
                throw new Exception("TIM_v2::GetClutColors::given clut is bigger than texture number of cluts");
            //if(bpp != )
            List<Color> colorPixels = new List<Color>();
            if (bpp == 8)
            {
                pbs.Seek(timOffset + 20 + (512 * clut), PseudoBufferedStream.SEEK_BEGIN);
                for (int i = 0; i < 512 / 2; i++)
                {
                    ushort clutPixel =  pbs.ReadUShort();
                    //byte red = (byte)(clutPixel >> 11);
                    //byte green = (byte)((clutPixel >> 6) & 0x1F);
                    //byte blue = (byte)((clutPixel >> 1) & 0x1F);
                    //byte alpha = (byte)(clutPixel & 0x01);
                    byte red = (byte)((clutPixel ) & 0x1F);
                    byte green = (byte)((clutPixel>>5) & 0x1F);
                    byte blue = (byte)((clutPixel >> 10) & 0x1F);
                    red = (byte)MathHelper.Clamp((red * bpp), 0, 255);
                    green = (byte)MathHelper.Clamp((green * bpp), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * bpp), 0, 255);
                    colorPixels.Add(new Color() { Red = red, Green = green, Blue = blue });
                }
            }
            if (bpp == 4)
            {
                pbs.Seek(timOffset + 20 + (32 * clut), PseudoBufferedStream.SEEK_BEGIN);
                for (int i = 0; i < 16; i++)
                {
                    ushort clutPixel = pbs.ReadUShort();
                    byte red = (byte)((clutPixel) & 0x1F);
                    byte green = (byte)((clutPixel >> 5) & 0x1F);
                    byte blue = (byte)((clutPixel >> 10) & 0x1F);
                    byte alpha = (byte)(clutPixel >> 11 & 1);
                    red = (byte)MathHelper.Clamp((red * bpp*4), 0, 255);
                    green = (byte)MathHelper.Clamp((green * bpp*4), 0, 255);
                    blue = (byte)MathHelper.Clamp((blue * bpp*4), 0, 255);
                    colorPixels.Add(new Color() { Red = red, Green = green, Blue = blue, Alpha=alpha });
                }
            }
            if (bpp > 8) throw new Exception("TIM that has bpp mode higher than 8 has no clut data!");
            return colorPixels.ToArray();
        }

        public byte[] CreateImageBuffer(Color[] palette = null, bool bIgnoreSize = false)
        {
            pbs.Seek(textureDataPointer, PseudoBufferedStream.SEEK_BEGIN);
            byte[] buffer = new byte[texture.Width * texture.Height * 4]; //ARGB
            if (bpp == 8)
            {
                if(!bIgnoreSize)
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
            if(bpp==4)
            {
                if ((buffer.Length) / 8 != pbs.Length - pbs.Tell())
                    throw new Exception("TIM_v2::CreateImageBuffer::TIM texture buffer has size incosistency.");
                for (int i = 0; i < buffer.Length; i++)
                {
                    byte pixel = pbs.ReadByte();
                    //Color ColoredPixel = palette[(pixel >> 4)&0xF];
                    Color ColoredPixel = palette[pixel&0xf];
                    buffer[i] = ColoredPixel.Red;
                    buffer[++i] = ColoredPixel.Green;
                    buffer[++i] = ColoredPixel.Blue;
                    buffer[++i] = ColoredPixel.Alpha;

                    ColoredPixel = palette[pixel>>4];

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

        private static ushort ushortLittleEndian(ushort ushort_)
    => (ushort)((ushort_ << 8) | (ushort_ >> 8));

        private static short shortLittleEndian(short ushort_)
            => (short)((ushort_ << 8) | (ushort_ >> 8));

        private static uint uintLittleEndian(uint uint_)
            => (uint_ << 24) | ((uint_ << 8) & 0x00FF0000) |
            ((uint_ >> 8) & 0x0000FF00) | (uint_ >> 24);

        private static int uintLittleEndian(int uint_)
            => (uint_ << 24) | ((uint_ << 8) & 0x00FF0000) |
            ((uint_ >> 8) & 0x0000FF00) | (uint_ >> 24);
    }
}
