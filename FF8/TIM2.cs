using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace FF8
{
    //upgraded TIM class, because that first one is a trash
    public class TIM2
    {

        #region Fields

        /// <summary>
        /// Bits per pixel
        /// </summary>
        private int bpp = -1;

        /// <summary>
        /// Raw Data buffer
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// Texture Data
        /// </summary>
        private Texture texture;

        /// <summary>
        /// Start of Image Data
        /// </summary>
        private uint textureDataPointer;

        /// <summary>
        /// Start of Tim Data
        /// </summary>
        private uint timOffset;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize TIM class
        /// </summary>
        /// <param name="buffer">Raw Data buffer</param>
        /// <param name="offset">Start of Tim Data</param>
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

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Number of cult color palettes
        /// </summary>
        public int GetClutCount => texture.NumOfCluts;

        /// <summary>
        /// Height
        /// </summary>
        public int GetHeight => texture.Height;

        /// <summary>
        /// Width
        /// </summary>
        public int GetWidth => texture.Width;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Splash is 640x400 16BPP typical TIM with palette of ggg bbbbb a rrrrr gg
        /// </summary>
        /// <param name="buffer">raw 16bpp image</param>
        /// <returns>Texture2D</returns>
        /// <remarks>
        /// These files are just the image data with no header and no clut data. Tim class doesn't
        /// handle this.
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

        /// <summary>
        /// Create Texture from Tim image data.
        /// </summary>
        /// <param name="clut">Active clut data</param>
        /// <param name="bIgnoreSize">
        /// If true skip size check useful for files with more than just Tim
        /// </param>
        /// <returns>Texture2D</returns>
        public Texture2D GetTexture(ushort? clut = null, bool bIgnoreSize = false)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                Texture2D image = new Texture2D(Memory.graphics.GraphicsDevice, GetWidth, GetHeight, false, SurfaceFormat.Color);
                image.SetData(CreateImageBuffer(br, clut == null ? null : GetClutColors(br, clut.Value), bIgnoreSize));
                return image;
            }
        }

        /// <summary>
        /// Output 32 bit Color data for image.
        /// </summary>
        /// <param name="br">Binaryreader pointing to memorystream of data.</param>
        /// <param name="palette">Color[] palette</param>
        /// <param name="bIgnoreSize">
        /// If true skip size check useful for files with more than just Tim
        /// </param>
        /// <returns>Color[]</returns>
        /// <remarks>
        /// This allows null palette but it doesn't seem to handle the palette being null
        /// </remarks>
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
                    buffer[i] = palette[br.ReadByte()]; //colorkey
            }
            else if (bpp == 4)
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
            else if (bpp == 16) //copied from overture
            {
                for (int i = 0; i < buffer.Length && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                {
                    ushort pixel = br.ReadUInt16();
                    buffer[i] = new Color
                    {
                        R = (byte)MathHelper.Clamp(((pixel) & 0x1F) * 8, 0, 255),
                        G = (byte)MathHelper.Clamp(((pixel >> 5) & 0x1F) * 8, 0, 255),
                        B = (byte)MathHelper.Clamp(((pixel >> 10) & 0x1F) * 8, 0, 255),
                        A = 0xFF
                    };
                }
            }
            else if (bpp == 24) //could be wrong.
            {
                for (int i = 0; i < buffer.Length && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                {
                    byte[] pixel = br.ReadBytes(3);
                    buffer[i] = new Color
                    {
                        R = pixel[0],
                        G = pixel[1],
                        B = pixel[2],
                        A = 0xFF
                    };
                }
            }
            else
                throw new Exception($"TIM_v2::CreateImageBuffer::TIM unsupported bits per pixel = {bpp}");
            //Then in bs debug where ReadTexture store for all cluts
            //data and then create Texture2D from there. (array of e.g. 15 texture2D)
            return buffer;
        }

        /// <summary>
        /// Get clut color palette
        /// </summary>
        /// <param name="br">Binaryreader pointing to memorystream of data.</param>
        /// <param name="clut">Active clut data</param>
        /// <returns>Color[]</returns>
        private Color[] GetClutColors(BinaryReader br, ushort clut)
        {
            if (clut > texture.NumOfCluts)
                throw new Exception("TIM_v2::GetClutColors::given clut is bigger than texture number of cluts");

            Color[] colorPixels = new Color[GetWidth * GetHeight];
            if (bpp == 8)
            {
                br.BaseStream.Seek(timOffset + 20 + (512 * clut), SeekOrigin.Begin);
                for (int i = 0; i < 512 / 2; i++)
                {
                    ushort clutPixel = br.ReadUInt16();
                    colorPixels[i].R = (byte)MathHelper.Clamp(((clutPixel) & 0x1F) * bpp, 0, 255);
                    colorPixels[i].G = (byte)MathHelper.Clamp(((clutPixel >> 5) & 0x1F) * bpp, 0, 255);
                    colorPixels[i].B = (byte)MathHelper.Clamp(((clutPixel >> 10) & 0x1F) * bpp, 0, 255);
                    if (colorPixels[i] != Color.TransparentBlack)
                        colorPixels[i].A = 0xFF;
                }
            }
            else if (bpp == 4)
            {
                br.BaseStream.Seek(timOffset + 20 + (32 * clut), SeekOrigin.Begin);
                for (int i = 0; i < 16; i++)
                {
                    ushort clutPixel = br.ReadUInt16();
                    colorPixels[i].R = (byte)MathHelper.Clamp(((clutPixel) & 0x1F) * bpp * 4, 0, 255);
                    colorPixels[i].G = (byte)MathHelper.Clamp(((clutPixel >> 5) & 0x1F) * bpp * 4, 0, 255);
                    colorPixels[i].B = (byte)MathHelper.Clamp(((clutPixel >> 10) & 0x1F) * bpp * 4, 0, 255);
                    colorPixels[i].A = (byte)(clutPixel >> 11 & 1);
                }
            }
            else if (bpp > 8) throw new Exception("TIM that has bpp mode higher than 8 has no clut data!");

            return colorPixels;
        }

        /// <summary>
        /// Populate Texture structure
        /// </summary>
        /// <param name="br">Binaryreader pointing to memorystream of data.</param>
        /// <param name="_bpp">bits per pixel</param>
        private void ReadParameters(BinaryReader br, byte _bpp)
        {
            texture.Read(br, _bpp);
            textureDataPointer = (uint)br.BaseStream.Position;
        }

        #endregion Methods

        #region Structs

        private struct Texture
        {

            #region Fields

            public byte[] ClutData;
            public uint clutSize;
            public ushort Height;
            public ushort ImageOrgX;
            public ushort ImageOrgY;
            public ushort NumOfCluts;
            public ushort NumOfColours;
            public ushort PaletteX;
            public ushort PaletteY;
            public ushort Width;

            #endregion Fields

            #region Methods

            /// <summary>
            /// Populate Texture structure
            /// </summary>
            /// <param name="br">Binaryreader pointing to memorystream of data.</param>
            /// <param name="_bpp">bits per pixel</param>
            public void Read(BinaryReader br, byte _bpp)
            {
                br.BaseStream.Seek(3, SeekOrigin.Current);
                if (_bpp == 4)
                {
                    clutSize = br.ReadUInt32() - 12;
                    PaletteX = br.ReadUInt16();
                    PaletteY = br.ReadUInt16();
                    NumOfColours = br.ReadUInt16();
                    NumOfCluts = br.ReadUInt16();
                    int bppMultiplier = 16;
                    if (NumOfColours != 16 || clutSize != (NumOfCluts * bppMultiplier)) //wmsetus uses 4BPP, but sets 256 colours, but actually is 16, but num of clut is 2* 256/16 WTF?
                    {
                        NumOfCluts = (ushort)(NumOfColours / 16 * NumOfCluts);
                        bppMultiplier = 32;
                    }
                    byte[] buffer = new byte[NumOfCluts * bppMultiplier];
                    for (int i = 0; i != buffer.Length; i++)
                        buffer[i] = br.ReadByte();
                    ClutData = buffer;
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                    ImageOrgX = br.ReadUInt16();
                    ImageOrgY = br.ReadUInt16();
                    Width = (ushort)(br.ReadUInt16() * 4);
                    Height = br.ReadUInt16();
                    return;
                }
                if (_bpp == 8)
                {
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                    PaletteX = br.ReadUInt16();
                    PaletteY = br.ReadUInt16();
                    br.BaseStream.Seek(2, SeekOrigin.Current);
                    NumOfCluts = br.ReadUInt16();
                    byte[] buffer = new byte[NumOfCluts * 512];
                    for (int i = 0; i != buffer.Length; i++)
                        buffer[i] = br.ReadByte();
                    ClutData = buffer;
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                    ImageOrgX = br.ReadUInt16();
                    ImageOrgY = br.ReadUInt16();
                    Width = (ushort)(br.ReadUInt16() * 2);
                    Height = br.ReadUInt16();
                    return;
                }
                if (_bpp == 16)
                {
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                    ImageOrgX = br.ReadUInt16();
                    ImageOrgY = br.ReadUInt16();
                    Width = br.ReadUInt16();
                    Height = br.ReadUInt16();
                    return;
                }
                if (_bpp != 24) return;
                br.BaseStream.Seek(4, SeekOrigin.Current);
                ImageOrgX = br.ReadUInt16();
                ImageOrgY = br.ReadUInt16();
                Width = (ushort)(br.ReadUInt16() / 1.5);
                Height = br.ReadUInt16();
            }

            #endregion Methods
        }

        #endregion Structs

    }
}