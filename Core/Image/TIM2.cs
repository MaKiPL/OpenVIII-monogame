using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Playstation TIM Texture format.
    /// </summary>
    /// <see cref="http://www.raphnet.net/electronique/psx_adaptor/Playstation.txt"/>
    /// <seealso cref="http://www.psxdev.net/forum/viewtopic.php?t=109"/>
    /// <seealso cref="https://mrclick.zophar.net/TilEd/download/timgfx.txt"/>
    /// <seealso cref="http://www.elisanet.fi/6581/PSX/doc/Playstation_Hardware.pdf"/>
    /// <seealso cref="http://www.elisanet.fi/6581/PSX/doc/psx.pdf"/>
    /// <remarks>upgraded TIM class, because that first one is a trash</remarks>
    public class TIM2 : Texture_Base
    {
        #region Fields

        /// <summary>
        /// Bits per pixel
        /// </summary>
        protected sbyte BPP = -1;

        /// <summary>
        /// Raw Data buffer
        /// </summary>
        protected byte[] Buffer;

        /// <summary>
        /// Image has a CLUT
        /// </summary>
        protected bool CLP;

        /// <summary>
        /// Texture Data
        /// </summary>
        protected TextureData Texture;

        /// <summary>
        /// Start of Image Data
        /// </summary>
        protected uint TextureDataPointer;

        protected bool ThrowExec = true;

        /// <summary>
        /// Start of Tim Data
        /// </summary>
        protected uint TIMOffset;

        protected bool TrimExcess;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize TIM class
        /// </summary>
        /// <param name="buffer">Raw Data buffer</param>
        /// <param name="offset">Start of Tim Data</param>
        public TIM2(byte[] buffer, uint offset = 0, bool noExc = false)
        {
            ThrowExec = !noExc;
            _Init(buffer, offset);
        }

        /// <summary> <summary> Initialize TIM class </summary> <param name="br">BinaryReader
        /// pointing to the file data</param> <param name="offset">Start of Tim Data</param>
        public TIM2(BinaryReader br, uint offset = 0, bool noExec = false)
        {
            ThrowExec = !noExec;
            _Init(br, offset);
        }

        protected TIM2()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets Bits per pixel
        /// </summary>
        public override byte GetBytesPerPixel => (byte)BPP;

        /// <summary>
        /// Number of clut color palettes
        /// </summary>
        public override int GetClutCount => Texture.NumOfCluts;

        /// <summary>
        /// Gets size of clut data as in TIM file
        /// </summary>
        public override int GetClutSize => Texture.ClutDataSize;

        /// <summary>
        /// Gets number of colors per palette
        /// </summary>
        public override int GetColorsCountPerPalette => Texture.NumOfColors;

        /// <summary>
        /// Height
        /// </summary>
        public override int GetHeight => Texture.Height;

        /// <summary>
        /// Gets origin texture coordinate X for VRAM buffer
        /// </summary>
        public override int GetOrigX => Texture.ImageOrgX;

        /// <summary>
        /// Gets origin texture coordinate Y for VRAM buffer
        /// </summary>
        public override int GetOrigY => Texture.ImageOrgY;

        /// <summary>
        /// Width
        /// </summary>
        public override int GetWidth => Texture.Width;

        public bool IgnoreAlpha { get; set; }

        public bool NotTIM { get; protected set; }

        #endregion Properties

        #region Methods

        public bool Assert(bool a)
        {
            if (!a)
            {
                NotTIM = true;
                if (ThrowExec)
                {
                    throw new InvalidDataException("Invalid TIM File");
                }
                //else
                //    Debug.Assert(a);
            }
            return !a;
        }

        public override void ForceSetClutColors(ushort newNumOfColors) => Texture.NumOfColors = newNumOfColors;

        public override void ForceSetClutCount(ushort newClut) => Texture.NumOfCluts = newClut;

        public override Color[] GetClutColors(ushort clut)
        {
            using (var br = new BinaryReader(new MemoryStream(Buffer)))
            {
                return GetClutColors(br, clut);
            }
        }

        /// <summary>
        /// Gets Color[] palette from TIM image data
        /// </summary>
        /// <param name="clut">clut index</param>
        /// <returns></returns>
        public Color[] GetPalette(ushort clut = 0)
        {
            Color[] colors;
            using (var br = new BinaryReader(new MemoryStream(Buffer)))
                colors = GetClutColors(br, clut);
            return colors;
        }

        /// <summary>
        /// Create Texture from Tim image data.
        /// </summary>
        /// <param name="clut">Active clut data</param>
        /// <param name="bIgnoreSize">
        /// If true skip size check useful for files with more than just Tim
        /// </param>
        /// <returns>Texture2D</returns>
        public override Texture2D GetTexture(ushort clut)
        {
            using (var br = new BinaryReader(new MemoryStream(Buffer)))
            {
                return GetTexture(br, !CLP ? null : GetClutColors(br, clut));
            }
        }

        public override Texture2D GetTexture()
        {
            using (var br = new BinaryReader(new MemoryStream(Buffer)))
            {
                return GetTexture(br, !CLP ? null : GetClutColors(br, 0));
            }
        }

        public override Texture2D GetTexture(Color[] colors)
        {
            using (var br = new BinaryReader(new MemoryStream(Buffer)))
            {
                if (Assert(CLP) || Assert(colors.Length == Texture.NumOfColors))
                    return null;
                return GetTexture(br, colors);
            }
        }

        /// <summary>
        /// Initialize TIM class
        /// </summary>
        /// <param name="br">BinaryReader pointing to the file data</param>
        /// <param name="offset">Start of Tim Data</param>
        public void Init(BinaryReader br, uint offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            if (Assert(br.ReadByte() == 0x10) || //tag
            Assert(br.ReadByte() == 0)) // version
                return;
            br.BaseStream.Seek(2, SeekOrigin.Current);
            var b = (Bppflag)br.ReadByte();
            TIMOffset = offset;
            if ((b & (Bppflag._24bpp)) >= (Bppflag._24bpp)) BPP = 24;
            else if ((b & Bppflag._16bpp) != 0) BPP = 16;
            else if ((b & Bppflag._8bpp) != 0) BPP = 8;
            else BPP = 4;
            CLP = (b & Bppflag.CLP) != 0;
            if (Assert(((BPP == 4 || BPP == 8) && CLP) || ((BPP == 16 || BPP == 24) && !CLP)))
                return;
            ReadParameters(br);
        }

        public override void Load(byte[] buffer, uint offset = 0) => _Init(buffer, offset);

        /// <summary>
        /// Writes the Tim file to the hard drive.
        /// </summary>
        /// <param name="path">Path where you want file to be saved.</param>
        public override void Save(string path)
        {
            using (var bw = new BinaryWriter(File.Create(path)))
            {
                bw.Write(TrimExcess
                    ? Buffer
                    : Buffer.Skip((int)TIMOffset).Take((int)(Texture.ImageDataSize + TextureDataPointer)).ToArray());
            }
        }

        public override void SaveCLUT(string path)
        {
            if (!CLP) return;
            using (var br = new BinaryReader(new MemoryStream(Buffer)))
            using (var clut = new Texture2D(Memory.graphics.GraphicsDevice, Texture.NumOfColors, Texture.NumOfCluts))
            {
                for (ushort i = 0; i < Texture.NumOfCluts; i++)
                {
                    clut.SetData(0, new Rectangle(0, i, Texture.NumOfColors, 1), GetClutColors(br, i), 0, Texture.NumOfColors);
                }
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    clut.SaveAsPng(fs, Texture.NumOfColors, Texture.NumOfCluts);
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public override void SavePNG(string path, short clut = -1)
        {
            if (clut == -1)
            {
                if (Texture.NumOfCluts > 0)
                {
                    Enumerable.Range(0, Texture.NumOfCluts).ForEach(x => SavePNG(path, (short)x));
                    return;
                }

                clut = 0;
            }

            using (var br = new BinaryReader(new MemoryStream(Buffer)))
            using (var tex = GetTexture(br, !CLP ? null : GetClutColors(br, (ushort)clut)))
            using (var fs = new FileStream($"{path}{(CLP && Texture.NumOfCluts > 1 ? "_" + clut.ToString("D") : "")}.png", FileMode.Create,
                FileAccess.Write, FileShare.ReadWrite))
            {
                tex.SaveAsPng(fs, GetWidth, GetHeight);
            }
        }

        protected void _Init(byte[] buffer, uint offset = 0)
        {
            Buffer = buffer;
            using (var br = new BinaryReader(new MemoryStream(buffer)))
            {
                Init(br, offset);
            }
        }

        protected void _Init(BinaryReader br, uint offset)
        {
            TrimExcess = true;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            Buffer = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
            using (var br2 = new BinaryReader(new MemoryStream(Buffer)))
            {
                Init(br2, 0);
            }
        }

        /// <summary>
        /// Output 32 bit Color data for image.
        /// </summary>
        /// <param name="br">Binary reader pointing to memory stream of data.</param>
        /// <param name="palette">Color[] palette</param>
        /// <param name="bIgnoreSize">
        /// If true skip size check useful for files with more than just Tim
        /// </param>
        /// <returns>Color[]</returns>
        /// <remarks>
        /// This allows null palette but it doesn't seem to handle the palette being null
        /// </remarks>
        protected TextureBuffer CreateImageBuffer(BinaryReader br, Color[] palette = null)
        {
            br.BaseStream.Seek(TextureDataPointer, SeekOrigin.Begin);
            var buffer = new TextureBuffer(Texture.Width, Texture.Height); //ARGB
            if (Assert((buffer.Length / BPP) <= br.BaseStream.Length - br.BaseStream.Position)) //make sure the buffer is large enough
                return null;
            if (BPP == 8)
            {
                for (var i = 0; i < buffer.Length; i++)
                {
                    var colorKey = br.ReadByte();
                    if (colorKey < Texture.NumOfColors)
                        buffer[i] = palette[colorKey]; //color key
                    //else
                    //    buffer[i] = Color.TransparentBlack; // trying something out of ordinary.
                }
            }
            else if (BPP == 4)
            {
                for (var i = 0; i < buffer.Length; i++)
                {
                    var colorKey = br.ReadByte();
                    buffer[i] = palette[colorKey & 0xf];
                    buffer[++i] = palette[colorKey >> 4];
                }
            }
            else if (BPP == 16) //copied from overture
            {
                for (var i = 0; i < buffer.Length && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                    buffer[i] = ABGR1555toRGBA32bit(br.ReadUInt16(), IgnoreAlpha);
            }
            else if (BPP == 24) //could be wrong. // assuming it is BGR
            {
                for (var i = 0; i < buffer.Length && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                {
                    var pixel = br.ReadBytes(3);
                    buffer[i] = new Color
                    {
                        R = pixel[2],
                        G = pixel[1],
                        B = pixel[0],
                        A = 0xFF
                    };
                }
            }
            else
                throw new Exception($"TIM_v2::CreateImageBuffer::TIM unsupported bits per pixel = {BPP}");
            //Then in bs debug where ReadTexture store for all cluts
            //data and then create Texture2D from there. (array of e.g. 15 texture2D)
            return buffer;
        }

        /// <summary>
        /// Get clut color palette
        /// </summary>
        /// <param name="br">Binary reader pointing to memory stream of data.</param>
        /// <param name="clut">Active clut data</param>
        /// <returns>Color[]</returns>
        protected Color[] GetClutColors(BinaryReader br, ushort clut)
        {
            if (clut >= Texture.NumOfCluts)
                throw new Exception($"TIM_v2::GetClutColors::given clut {clut} is >= texture number of cluts {Texture.NumOfCluts}");

            if (CLP)
            {
                var colorPixels = new Color[Texture.NumOfColors];
                br.BaseStream.Seek(TIMOffset + 20 + (Texture.NumOfColors * 2 * clut), SeekOrigin.Begin);
                for (var i = 0; i < Texture.NumOfColors; i++)
                    colorPixels[i] = ABGR1555toRGBA32bit(br.ReadUInt16(), IgnoreAlpha);
                return colorPixels;
            }
            else throw new Exception($"TIM that has {BPP} bpp mode and has no clut data!");
        }

        protected Texture2D GetTexture(BinaryReader br, Color[] colors) => CreateImageBuffer(br, colors).GetTexture();

        /// <summary>
        /// Populate Texture structure
        /// </summary>
        /// <param name="br">Binary reader pointing to memory stream of data.</param>
        /// <param name="_bpp">bits per pixel</param>
        protected void ReadParameters(BinaryReader br)
        {
            Texture = new TextureData();
            Texture.Read(br, (byte)BPP, CLP, !ThrowExec);
            if (Assert(!Texture.NotTIM)) return;
            TextureDataPointer = (uint)br.BaseStream.Position;
            if (TrimExcess)
                Buffer = Buffer.Skip((int)TIMOffset).Take((int)(Texture.ImageDataSize + TextureDataPointer - TIMOffset)).ToArray();
        }

        #endregion Methods

        #region Structs

        protected struct TextureData
        {
            #region Fields

            public byte[] ClutData;

            public int ClutDataSize;

            /// <summary>
            /// length, in bytes, of the entire CLUT block (including the header)
            /// </summary>
            public uint ClutSize;

            public ushort Height;
            public int ImageDataSize;
            public ushort ImageOrgX;
            public ushort ImageOrgY;
            public uint ImageSize;
            public ushort NumOfCluts;
            public ushort NumOfColors;
            public ushort PaletteX;
            public ushort PaletteY;
            public bool ThrowExec;
            public ushort Width;

            #endregion Fields

            #region Properties

            public bool NotTIM { get; private set; }

            #endregion Properties

            #region Methods

            public bool Assert(bool a)
            {
                if (!a)
                {
                    NotTIM = true;
                    if (ThrowExec)
                    {
                        throw new InvalidDataException("Invalid TIM File");
                    }
                    //else
                    //    Debug.Assert(a);
                }
                return !a;
            }

            /// <summary>
            /// Populate Texture structure
            /// </summary>
            /// <param name="br">Binary reader pointing to memory stream of data.</param>
            /// <param name="bpp">bits per pixel</param>
            public void Read(BinaryReader br, byte bpp, bool clp, bool noExec = false)
            {
                ThrowExec = !noExec;
                br.BaseStream.Seek(3, SeekOrigin.Current);
                if (clp)
                {
                    //long start = br.BaseStream.Position;
                    ClutSize = br.ReadUInt32();
                    PaletteX = br.ReadUInt16();
                    PaletteY = br.ReadUInt16();
                    NumOfColors = br.ReadUInt16(); //width of clut
                    NumOfCluts = br.ReadUInt16(); //height of clut
                    ClutDataSize = (int)(ClutSize - 12);//(NumOfColors * NumOfCluts*2);
                    if (Assert(ClutDataSize == NumOfColors * NumOfCluts * 2 || ClutDataSize == NumOfColors * NumOfCluts) ||
                    Assert(PaletteX % 16 == 0) ||
                    Assert(PaletteY <= 511))
                        return;
                    if (bpp == 4 && NumOfColors > 16)
                    {
                        // tim viewer was overriding the read number of colors per pixel to 16
                        // and this makes sense because you cannot read more than 16 colors with only a 4 bit value.
                        // though this might break stuff.
                        NumOfColors = 16;
                        NumOfCluts = checked((ushort)(ClutDataSize / (NumOfColors * 2)));
                    }
                    Assert(bpp == 8 && NumOfColors <= 256 || bpp != 8);
                    ClutData = br.ReadBytes(ClutDataSize);
                    //br.BaseStream.Seek(start+clutSize, SeekOrigin.Begin);
                }
                //wmsetus uses 4BPP, but sets 256 colors, but actually is 16, but num of clut is 2* 256/16 WTF?

                ImageSize = br.ReadUInt32(); // image size + header in bytes
                ImageOrgX = br.ReadUInt16();
                ImageOrgY = br.ReadUInt16();
                Width = br.ReadUInt16();
                Height = br.ReadUInt16();
                ImageDataSize = (int)(ImageSize - 12);//(NumOfColors * NumOfCluts*2);
                Assert(ImageDataSize == Width * Height * 2);

                // Pixel data is stored in uint16 spots. So you use this to convert that to the
                // correct color / CLUT color key If 32 bit existed it'd be 1:2 24 bit is 1:1.5 16 bit
                // is 1:1 8 bit is 2:1 4 bit is 4:1

                if (bpp == 4)
                    Width = (ushort)(Width * 4);
                if (bpp == 8)
                    Width = (ushort)(Width * 2);
                //if (_bpp == 16)
                //    Width = Width;
                if (bpp == 24)
                {
                    Width = (ushort)(Width / 1.5);
                    // The below i'm unsure if any of this effects TIM files. So I commented them out.
                    // http://www.elisanet.fi/6581/PSX/doc/Playstation_Hardware.pdf
                    // http://www.elisanet.fi/6581/PSX/doc/psx.pdf
                    //24 bit color mode has different restrictions. I guess you bypass the psx gpu to draw these
                    //Assert(Width >= 8 && Height >= 2);// &&  // not sure if the multiple of 8 for width is right.
                }

                //Assert(Width <= 256 && Height <= 256 && Width > 0 && Height > 0); // sprite max size 256x256 and min size 1x1

                //Assert(Width % 8 == 0 && Height % 8 == 0); // maybe texture must be multiple of 8.
            }

            #endregion Methods
        }

        #endregion Structs
    }
}