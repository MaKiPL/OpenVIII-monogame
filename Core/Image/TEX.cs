using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// TEX file handler class. TEX files are packages of textures and Palettes.
    /// </summary>
    /// <remarks>
    /// I borrowed this from my Rinoa's toolset, but modified to aim for buffer rather than file-work
    /// </remarks>
    /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TEX.cs"/>
    /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.cpp"/>
    public class TEX : Texture_Base
    {
        #region Fields

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

        ///// <summary>
        ///// Contains header info and Palette data of TEX file.
        ///// </summary>
        //public Texture TextureData => texture;  //added to get texturedata outside of class.

        #region Properties

        public bool CLP => texture.PaletteFlag != 0;
        public override int GetClutCount => texture.NumOfPalettes;
        public override int GetClutSize => (int)texture.PaletteSize;
        public override byte GetBpp => texture.bytesPerPixel;
        public override int GetColorsCountPerPalette => (int)texture.NumOfColorsPerPalette;
        public override int GetHeight => texture.Height;
        public override int GetOrigX => 0;
        public override int GetOrigY => 0;
        public override int GetWidth => texture.Width;
        /// <summary>
        /// size of header section
        /// </summary>
        private int Headersize => texture.Version <= 1 ? 0xEC : 0xF0;

        /// <summary>
        /// size of palette section
        /// </summary>
        private int PaletteSectionSize => (int)(texture.PaletteSize * 4);

        /// <summary>
        /// start of texture section
        /// </summary>
        private int TextureLocator => Headersize + PaletteSectionSize;

        #endregion Properties

        #region Methods

        public override void ForceSetClutColors(ushort newNumOfColours)
        {
            texture.NumOfColorsPerPalette = newNumOfColours;
        }

        public override void ForceSetClutCount(ushort newClut)
        {
            texture.NumOfPalettes = (byte)newClut;
        }

        public override Color[] GetClutColors(ushort clut)
        {
            if (!CLP) return null;
            else if (clut >= texture.NumOfPalettes)
                throw new Exception($"Desired palette is incorrect use -1 for default or use a smaller number: {clut} > {texture.NumOfPalettes}");

            Color[] colors = new Color[texture.NumOfColorsPerPalette];
            int k = 0;
            for (uint i = clut * texture.NumOfColorsPerPalette * 4; i < texture.paletteData.Length && k < colors.Length; i += 4)
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
        /// <param name="colors">Override colors of palette; Array size must match texture.NumOfColorsPerPalette</param>
        /// <returns>32bit Texture2D</returns>
        /// <remarks>
        /// Some paletts are 256 but the game only uses 16 colors might need to make the restriction
        /// more lax and allow any size array and only throw errors if the colorkey is greater than
        /// size of array. Or we could treat any of those bad matches as transparent.
        /// </remarks>
        public override Texture2D GetTexture(Color[] colors = null)
        {
            if (Memory.graphics.GraphicsDevice != null)
            {
                if (texture.PaletteFlag != 0)
                {
                    if (colors != null && colors.Length != texture.NumOfColorsPerPalette)
                        throw new Exception($" custom colors parameter set but array size to match palette size: {texture.NumOfColorsPerPalette}");
                    using (MemoryStream ms = new MemoryStream(buffer))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        try
                        {
                            ms.Seek(TextureLocator, SeekOrigin.Begin);
                            Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, texture.Width, texture.Height, false, SurfaceFormat.Color);
                            Color[] convertBuffer = new Color[texture.Width * texture.Height];
                            for (int i = 0; i < convertBuffer.Length && ms.Position < ms.Length; i++)
                            {
                                convertBuffer[i] = colors[br.ReadByte()]; //colorkey
                            }
                            bmp.SetData(convertBuffer);
                            return bmp;
                        }
                        catch (NullReferenceException)
                        {
                            return null;
                        }
                        catch (ArgumentNullException)
                        {
                            return null;
                        }
                    }
                }
                else if (texture.bytesPerPixel == 2)
                {
                    using (MemoryStream ms = new MemoryStream(buffer))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        try
                        {
                            ms.Seek(TextureLocator, SeekOrigin.Begin);
                            Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, texture.Width, texture.Height, false, SurfaceFormat.Color);
                            colors = new Color[texture.Width * texture.Height];
                            for (int i = 0; i < colors.Length && ms.Position + 2 < ms.Length; i++)
                            {
                                colors[i] = ABGR1555toRGBA32bit(br.ReadUInt16());
                            }
                            bmp.SetData(colors);
                            return bmp;
                        }
                        catch (NullReferenceException)
                        {
                            return null;
                        }
                        catch (ArgumentNullException)
                        {
                            return null;
                        }
                    }
                }
                else if (texture.bytesPerPixel == 3)
                {
                    // not tested but vincent tim had support for it so i guess it's possible RGB or BGR
                    using (MemoryStream ms = new MemoryStream(buffer))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        ms.Seek(TextureLocator, SeekOrigin.Begin);
                        Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, texture.Width, texture.Height, false, SurfaceFormat.Color);
                        colors = new Color[texture.Width * texture.Height];
                        for (int i = 0; i < colors.Length && ms.Position + 3 < ms.Length; i++)
                        {
                            //RGB or BGR so might need to reorder things to RGB
                            colors[i].B = br.ReadByte();
                            colors[i].G = br.ReadByte();
                            colors[i].R = br.ReadByte();
                            colors[i].A = 0xFF;
                        }
                        bmp.SetData(colors);
                        return bmp;
                    }
                }
            }
            return null;
        }

        public override Texture2D GetTexture(ushort? clut = null) => GetTexture(GetClutColors(clut ?? 0));

        public override Texture2D GetTexture() => GetTexture(0);

        /// <summary>
        /// Writes the Tim file to the hard drive.
        /// </summary>
        /// <param name="path">Path where you want file to be saved.</param>
        public override void Save(string path)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Create(path)))
            {
                bw.Write(buffer);
            }
        }

        /// <summary>
        /// Read header data from TEX file.
        /// </summary>
        /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TEX.cs"/>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.h"/>
        private void ReadParameters()
        {
            texture.Version = BitConverter.ToUInt32(buffer, 0x00);
            texture.Width = (int)BitConverter.ToUInt32(buffer, 0x3C); //nothing will be uint size big.
            texture.Height = (int)BitConverter.ToUInt32(buffer, 0x40);
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

        #endregion Methods

        #region Structs

        /// <summary>
        /// Contains Header info and Palette data of TEX file.
        /// </summary>
        /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TEX.cs"/>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.h"/>
        private struct Texture
        {
            #region Fields

            /// <summary>
            /// 0x38
            /// </summary>
            public uint bitDepth;

            /// <summary>
            /// 0x68
            /// </summary>
            public byte bytesPerPixel;

            /// <summary>
            /// 0x40
            /// </summary>
            public int Height;

            /// <summary>
            /// 0x34
            /// </summary>
            public uint NumOfColorsPerPalette;

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
            /// 0x00; 1=FF7 | 2=FF8
            /// </summary>
            public uint Version;

            /// <summary>
            /// 0x3C
            /// </summary>
            public int Width;

            #endregion Fields
        }

        #endregion Structs

        //public struct Color
        //{
        //    #region Fields

        // public byte Alpha; public byte Blue; public byte Green; public byte Red;

        //    #endregion Fields
        //}
    }
}