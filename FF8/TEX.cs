using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace FF8
{
    /// <summary>
    /// TEX file handler class. TEX files are packages of textures and Palettes.
    /// </summary>
    /// <remarks>
    /// I borrowed this from my Rinoa's toolset, but modified to aim for buffer rather than file-work
    /// </remarks>
    /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TEX.cs"/>
    /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.cpp"/>
    public class TEX
    {
        #region Fields

        /// <summary>
        /// Ratio needed to convert 16 bit to 32 bit color
        /// </summary>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/PsColor.h"/>
        public const double COEFF_COLOR = (double)255 / 31;

        /// <summary>
        /// Raw data of TEX file
        /// </summary>
        private byte[] buffer;

        private Texture texture;

        #endregion Fields

        #region Constructors

        public TEX(byte[] buffer)
        {
            texture = new Texture();
            this.buffer = buffer;
            ReadParameters();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Contains header info and Palette data of TEX file.
        /// </summary>
        public Texture TextureData => texture;  //added to get texturedata outside of class.

        /// <summary>
        /// start of texture section
        /// </summary>
        private int TextureLocator => Headersize + PaletteSectionSize;

        /// <summary>
        /// size of palette section
        /// </summary>
        private int PaletteSectionSize => (int)(texture.PaletteSize * 4);

        /// <summary>
        /// size of header section
        /// </summary>
        private int Headersize => texture.Version <= 1 ? 0xEC : 0xF0;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Read header data from TEX file.
        /// </summary>
        /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TEX.cs"/>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.h"/>
        private void ReadParameters()
        {
            texture.Version = BitConverter.ToUInt32(buffer, 0x00);
            texture.Width = BitConverter.ToUInt32(buffer, 0x3C);
            texture.Height = BitConverter.ToUInt32(buffer, 0x40);
            texture.bytesPerPixel = buffer[0x68];
            texture.NumOfPalettes = buffer[0x30];
            texture.NumOfColorsPerPalette = BitConverter.ToUInt32(buffer, 0x34);
            texture.bitDepth = BitConverter.ToUInt32(buffer, 0x38);
            texture.PaletteFlag = buffer[0x4C];
            texture.PaletteSize = BitConverter.ToUInt32(buffer, 0x58);
            if (texture.PaletteFlag != 0)
            {
                texture.paletteData = new byte[PaletteSectionSize];
                Buffer.BlockCopy(buffer, 0xF0, texture.paletteData, 0, PaletteSectionSize);
            }
        }

        public Color[] GetPallets(int forcePalette)
        {
            if (forcePalette < 0) forcePalette = 0;

            Color[] colors = new Color[texture.NumOfColorsPerPalette];
            int k = 0;
            for (uint i = (uint)(forcePalette * texture.NumOfColorsPerPalette * 4); i < texture.paletteData.Length && k < colors.Length; i += 4)
            {
                colors[k].B = texture.paletteData[i];
                colors[k].G = texture.paletteData[i + 1];
                colors[k].R = texture.paletteData[i + 2];
                colors[k].A = texture.paletteData[i + 3];
                k++;
            }
            return colors;
        }

        /// <summary>
        /// Get Texture2D converted to 32bit color
        /// </summary>
        /// <param name="forcePalette">Desired Palette, see texture.NumOfPalettes. -1 is default.</param>
        /// <param name="colors">Overide colors of pallet; Array size must match texture.NumOfColorsPerPalette</param>
        /// <returns>32bit Texture2D</returns>
        public Texture2D GetTexture(int forcePalette = -1, Color[] colors = null)
        {
            uint ImageSizeBytes = texture.Width * texture.Height * 4;
            if (texture.PaletteFlag != 0)
            {
                if (colors != null && colors.Length != texture.NumOfColorsPerPalette)
                    throw new Exception($" custom colors parameter set but array size to match pallet size: {texture.NumOfColorsPerPalette}");
                else if (colors == null)
                {
                    if (forcePalette >= texture.NumOfPalettes) //prevents exception for forcing a palette that doesn't exist.
                        throw new Exception($"Desired pallet is incorrect use -1 for default or use a smaller number: {forcePalette} > {texture.NumOfPalettes}");
                    colors = GetPallets(forcePalette);
                }
                Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, (int)texture.Width, (int)texture.Height, false, SurfaceFormat.Color);
                int localTextureLocator = TextureLocator;
                Color[] convertBuffer = new Color[ImageSizeBytes / 4];
                for (int i = 0; i < convertBuffer.Length; i++)
                {
                    byte colorkey = buffer[localTextureLocator++];
                    convertBuffer[i] = colors[colorkey];
                }
                bmp.SetData(convertBuffer);
                return bmp;
            }
            else
            {
                if (texture.bytesPerPixel == 2)
                {
                    //working code
                    Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, (int)texture.Width, (int)texture.Height, false, SurfaceFormat.Color);
                    using (MemoryStream os = new MemoryStream((int)(ImageSizeBytes)))
                    using (BinaryWriter bw = new BinaryWriter(os))
                    {
                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            ms.Seek(TextureLocator, SeekOrigin.Begin);
                            while (ms.Position < ms.Length)
                                bw.Write(FromPsColor(br.ReadUInt16()), 0, 4);
                        }
                        bmp.SetData(os.GetBuffer(), 0, (int)os.Length);
                    }
                    return bmp;
                }
                if (texture.bytesPerPixel == 3)
                {
                    // not tested but vincent tim had support for it so i guess it's possible RGB or
                    // BGR is a thing.
                    Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, (int)texture.Width, (int)texture.Height, false, SurfaceFormat.Color);
                    using (MemoryStream os = new MemoryStream((int)(ImageSizeBytes)))
                    using (BinaryWriter bw = new BinaryWriter(os))
                    {
                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            ms.Seek(TextureLocator, SeekOrigin.Begin);
                            while (ms.Position < ms.Length)
                            {
                                //RGB or BGR so might need to reorder things to RGB
                                bw.Write(br.ReadBytes(3), 0, 3);
                                //ALPHA
                                bw.Write((byte)0xFF);
                            }
                        }
                        bmp.SetData(os.GetBuffer(), 0, (int)os.Length);
                    }
                    return bmp;
                }

                return null;
            }
        }

        /// <summary>
        /// converts 16 bit color to 32bit with alpha. alpha needs to be set true manually per pixel
        /// unless you know the color value.
        /// </summary>
        /// <param name="color">16 bit color</param>
        /// <param name="useAlpha">area is visable or not</param>
        /// <returns>byte[4] red green blue alpha, i think</returns>
        /// <see cref="https://github.com/myst6re/vincent-tim/blob/master/PsColor.cpp"/>
        public static byte[] FromPsColor(ushort color, bool useAlpha = false) => new byte[] { (byte)Math.Round((color & 31) * COEFF_COLOR), (byte)Math.Round(((color >> 5) & 31) * COEFF_COLOR), (byte)Math.Round(((color >> 10) & 31) * COEFF_COLOR), (byte)(color == 0 && useAlpha ? 0 : 255) };

        #endregion Methods

        #region Structs

        /// <summary>
        /// Contains Header info and Palette data of TEX file.
        /// </summary>
        /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TEX.cs"/>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.h"/>
        public struct Texture
        {
            #region Fields

            /// <summary>
            /// 0x68
            /// </summary>
            public byte bytesPerPixel;

            /// <summary>
            /// 0x40
            /// </summary>
            public uint Height;

            /// <summary>
            /// 0x30
            /// </summary>
            public byte NumOfPalettes;

            /// <summary>
            /// 0xF0 for ff8;0xEC for ff7; size = PaletteSize * 4;
            /// </summary>
            public byte[] paletteData;

            /// <summary>
            /// 0x4C
            /// </summary>
            public byte PaletteFlag;

            /// <summary>
            /// 0x58
            /// </summary>
            public uint PaletteSize;

            /// <summary>
            /// 0x3C
            /// </summary>
            public uint Width;

            /// <summary>
            /// 0x38
            /// </summary>
            public uint bitDepth;

            /// <summary>
            /// 0x34
            /// </summary>
            public uint NumOfColorsPerPalette;

            /// <summary>
            /// 0x00; 1=FF7 | 2=FF8
            /// </summary>
            public uint Version;

            #endregion Fields
        }

        //public struct Color
        //{
        //    #region Fields

        // public byte Alpha; public byte Blue; public byte Green; public byte Red;

        //    #endregion Fields
        //}

        #endregion Structs
    }
}