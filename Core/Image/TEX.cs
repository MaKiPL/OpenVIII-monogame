using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;
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
    public sealed class TEX : Texture_Base
    {
        #region Fields

        /// <summary>
        /// Raw data of TEX file
        /// </summary>
        private byte[] _buffer;

        private Texture _texture;

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
        //public Texture TextureData => texture;  //added to get texture data outside of class.

        #region Properties

        public bool CLP => _texture.PaletteFlag != 0;
        public override byte GetBytesPerPixel => _texture.BytesPerPixel;
        public override int GetClutCount => _texture.NumOfCluts;
        public override int GetClutSize => (int)_texture.PaletteSize;
        public override int GetColorsCountPerPalette => _texture.NumOfColors;
        public override int GetHeight => _texture.Height;
        public override int GetOrigX => 0;
        public override int GetOrigY => 0;
        public override int GetWidth => _texture.Width;

        /// <summary>
        /// size of header section
        /// </summary>
        private int HeaderSize => _texture.Version <= 1 ? 0xEC : 0xF0;

        /// <summary>
        /// size of palette section
        /// </summary>
        private int PaletteSectionSize => (int)(_texture.PaletteSize * 4);

        /// <summary>
        /// start of texture section
        /// </summary>
        private int TextureLocator => HeaderSize + PaletteSectionSize;

        #endregion Properties

        #region Methods

        public override void ForceSetClutColors(ushort newNumOfColors) => _texture.NumOfColors = newNumOfColors;

        public override void ForceSetClutCount(ushort newClut) => _texture.NumOfCluts = (byte)newClut;

        public override Color[] GetClutColors(ushort clut)
        {
            if (!CLP || _texture.NumOfCluts == 0)
                return null;
            if (clut >= _texture.NumOfCluts)
                throw new Exception($"Desired palette is incorrect use -1 for default or use a smaller number: {clut} > {_texture.NumOfCluts}");

            var colors = new Color[_texture.NumOfColors];
            var k = 0;
            for (var i = clut * _texture.NumOfColors * 4; i < _texture.PaletteData.Length && k < colors.Length; i += 4)
            {
                colors[k].B = _texture.PaletteData[i];
                colors[k].G = _texture.PaletteData[i + 1];
                colors[k].R = _texture.PaletteData[i + 2];
                colors[k].A = _texture.PaletteData[i + 3];
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
        /// Some palettes are 256 but the game only uses 16 colors might need to make the restriction
        /// more lax and allow any size array and only throw errors if the color key is greater than
        /// size of array. Or we could treat any of those bad matches as transparent.
        /// </remarks>
        public override Texture2D GetTexture(Color[] colors)
        {
            if (Memory.Graphics.GraphicsDevice == null) return null;
            if (_texture.PaletteFlag != 0)
            {
                if (colors == null) throw new ArgumentNullException(nameof(colors));
                //if (colors != null && colors.Length != texture.NumOfColors)
                //{
                //    //if (colors.Length > texture.NumOfColors) //truncate colors to the correct amount. in some
                //    //    colors = colors.Take(texture.NumOfColors).ToArray();
                //    //else // might need to expand the array the other way if we get more mismatches.
                //        //Array.Resize(ref colors,texture.NumOfColors);
                //    //throw new Exception($" custom colors parameter set but array size to match palette size: {texture.NumOfColors}");
                //}

                MemoryStream ms;
                using (var br = new BinaryReader(ms = new MemoryStream(_buffer)))
                {
                    ms.Seek(TextureLocator, SeekOrigin.Begin);
                    var convertBuffer = new TextureBuffer(_texture.Width, _texture.Height);
                    for (var i = 0; i < convertBuffer.Length && ms.Position < ms.Length; i++)
                    {
                        var colorKey = br.ReadByte();
                        if (colorKey > colors.Length) continue;
                        convertBuffer[i] = colors[colorKey];
                    }

                    return convertBuffer.GetTexture();
                }
            }

            if (_texture.BytesPerPixel == 2)
            {
                MemoryStream ms;
                using (var br = new BinaryReader(ms = new MemoryStream(_buffer)))
                {
                    ms.Seek(TextureLocator, SeekOrigin.Begin);
                    var convertBuffer = new TextureBuffer(_texture.Width, _texture.Height);
                    for (var i = 0; ms.Position + 2 < ms.Length; i++)
                    {
                        convertBuffer[i] = ABGR1555toRGBA32bit(br.ReadUInt16());
                    }

                    return convertBuffer.GetTexture();
                }
            }

            if (_texture.BytesPerPixel != 3) return null;
            {
                // not tested but vincent tim had support for it so i guess it's possible RGB or BGR
                MemoryStream ms;
                using (var br = new BinaryReader(ms = new MemoryStream(_buffer)))
                {
                    ms.Seek(TextureLocator, SeekOrigin.Begin);
                    var convertBuffer = new TextureBuffer(_texture.Width, _texture.Height);
                    var color = new Color { A = 0xFF };
                    for (var i = 0; ms.Position + 3 < ms.Length; i++)
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

        public override Texture2D GetTexture(ushort clut) => GetTexture(GetClutColors(clut));

        public override void Load(byte[] buffer, uint offset = 0)
        {
            _texture = new Texture();
            this._buffer = buffer;
            ReadParameters();
        }

        /// <summary>
        /// Writes the Tim file to the hard drive.
        /// </summary>
        /// <param name="path">Path where you want file to be saved.</param>
        public override void Save(string path)
        {
            using (var bw = new BinaryWriter(File.Create(path)))
            {
                bw.Write(_buffer);
            }
        }

        public override void SaveCLUT(string path)
        {
            using (var clut = new Texture2D(Memory.Graphics.GraphicsDevice, _texture.NumOfColors, _texture.NumOfCluts))
            {
                for (ushort i = 0; i < _texture.NumOfCluts; i++)
                {
                    clut.SetData(0, new Rectangle(0, i, _texture.NumOfColors, 1), GetClutColors(i), 0, _texture.NumOfColors);
                }
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    clut.SaveAsPng(fs, _texture.NumOfColors, _texture.NumOfCluts);
            }
        }

        /// <summary>
        /// Read header data from TEX file.
        /// </summary>
        /// <see cref="https://github.com/MaKiPL/FF8-Rinoa-s-Toolset/blob/master/SerahToolkit_SharpGL/FF8_Core/TEX.cs"/>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.h"/>
        private void ReadParameters()
        {
            _texture.Version = BitConverter.ToUInt32(_buffer, 0x00);
            _texture.Width = (int)BitConverter.ToUInt32(_buffer, 0x3C); //nothing will be uint size big.
            _texture.Height = (int)BitConverter.ToUInt32(_buffer, 0x40);
            _texture.BytesPerPixel = _buffer[0x68];
            _texture.NumOfCluts = _buffer[0x30];
            _texture.NumOfColors = BitConverter.ToInt32(_buffer, 0x34);
            _texture.BitDepth = BitConverter.ToUInt32(_buffer, 0x38);
            _texture.PaletteFlag = _buffer[0x4C];
            _texture.PaletteSize = BitConverter.ToUInt32(_buffer, 0x58);
            if (_texture.PaletteFlag == 0) return;
            _texture.PaletteData = new byte[PaletteSectionSize];
            Buffer.BlockCopy(_buffer, 0xF0, _texture.PaletteData, 0, PaletteSectionSize);
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
            [SuppressMessage("ReSharper", "NotAccessedField.Local")] public uint BitDepth;

            /// <summary>
            /// 0x68
            /// </summary>
            public byte BytesPerPixel;

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
            public int NumOfColors;

            /// <summary>
            /// 0xF0 for ff8;0xEC for ff7; size = PaletteSize * 4;
            /// </summary>
            public byte[] PaletteData;

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