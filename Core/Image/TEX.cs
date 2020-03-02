using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

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

        public TEX(byte[] buffer) => Load(buffer);

        public TEX()
        {
        }

        #endregion Constructors

        ///// <summary>
        ///// Contains header info and Palette data of TEX file.
        ///// </summary>
        //public Texture TextureData => texture;  //added to get texturedata outside of class.

        #region Properties

        public bool CLP => texture.PaletteFlag != 0;
        public override byte GetBytesPerPixel => texture.bytesPerPixel;
        public override int GetClutCount => texture.NumOfCluts;
        public override int GetClutSize => (int)texture.PaletteSize;
        public override int GetColorsCountPerPalette => texture.NumOfColours;
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

        public override void ForceSetClutColors(ushort newNumOfColours) => texture.NumOfColours = newNumOfColours;

        public override void ForceSetClutCount(ushort newClut) => texture.NumOfCluts = (byte)newClut;

        public override Color[] GetClutColors(ushort clut)
        {
            if (!CLP)
                return null;
            else if (texture.NumOfCluts == 0)
                return null;
            else if (clut >= texture.NumOfCluts)
                throw new Exception($"Desired palette is incorrect use -1 for default or use a smaller number: {clut} > {texture.NumOfCluts}");

            Color[] colors = new Color[texture.NumOfColours];
            int k = 0;
            for (int i = clut * texture.NumOfColours * 4; i < texture.paletteData.Length && k < colors.Length; i += 4)
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
                    if (colors != null && colors.Length != texture.NumOfColours)
                    {
                        if (colors.Length > texture.NumOfColours) //truncate colors to the correct amount. in some
                            colors = colors.Take(texture.NumOfColours).ToArray();
                        else // might need to expand the array the other way if we get more mismatches.
                            throw new Exception($" custom colors parameter set but array size to match palette size: {texture.NumOfColours}");
                    }
                    
                    

                    MemoryStream ms;
                    using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
                    {
                        ms.Seek(TextureLocator, SeekOrigin.Begin);
                        TextureBuffer convertBuffer = new TextureBuffer(texture.Width, texture.Height);
                        for (int i = 0; i < convertBuffer.Length && ms.Position < ms.Length; i++)
                        {
                            convertBuffer[i] = colors[br.ReadByte()]; //colorkey
                        }
                        return convertBuffer.GetTexture();
                    }
                }
                else if (texture.bytesPerPixel == 2)
                {
                    MemoryStream ms;
                    using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
                    {
                        ms.Seek(TextureLocator, SeekOrigin.Begin);
                        TextureBuffer convertBuffer = new TextureBuffer(texture.Width, texture.Height);
                        for (int i = 0; ms.Position + 2 < ms.Length; i++)
                        {
                            convertBuffer[i] = ABGR1555toRGBA32bit(br.ReadUInt16());
                        }
                        return convertBuffer.GetTexture();
                    }
                }
                else if (texture.bytesPerPixel == 3)
                {
                    // not tested but vincent tim had support for it so i guess it's possible RGB or BGR
                    MemoryStream ms;
                    using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
                    {
                        ms.Seek(TextureLocator, SeekOrigin.Begin);
                        TextureBuffer convertBuffer = new TextureBuffer(texture.Width, texture.Height);
                        Color color;
                        color.A = 0xFF;
                        for (int i = 0; ms.Position + 3 < ms.Length; i++)
                        {
                            //RGB or BGR so might need to reorder things to RGB
                            color.B = br.ReadByte();
                            color.G = br.ReadByte();
                            color.R = br.ReadByte();
                            convertBuffer[i] = color;
                        }
                        return convertBuffer.GetTexture();
                    }
                }
            }
            return null;
        }

        public override Texture2D GetTexture(ushort? clut = null) => GetTexture(GetClutColors(clut ?? 0));

        public override Texture2D GetTexture() => GetTexture(0);

        public override void Load(byte[] buffer, uint offset = 0)
        {
            texture = new Texture();
            this.buffer = buffer;
            ReadParameters();
        }

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

        public override void SaveCLUT(string path)
        {
            using (Texture2D CLUT = new Texture2D(Memory.graphics.GraphicsDevice, texture.NumOfColours, texture.NumOfCluts))
            {
                for (ushort i = 0; i < texture.NumOfCluts; i++)
                {
                    CLUT.SetData(0, new Rectangle(0, i, texture.NumOfColours, 1), GetClutColors(i), 0, texture.NumOfColours);
                }
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    CLUT.SaveAsPng(fs, texture.NumOfColours, texture.NumOfCluts);
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
            texture.NumOfCluts = buffer[0x30];
            texture.NumOfColours = BitConverter.ToInt32(buffer, 0x34);
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
            /// 0x30
            /// </summary>
            public byte NumOfCluts;

            /// <summary>
            /// 0x34
            /// </summary>
            public int NumOfColours;

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