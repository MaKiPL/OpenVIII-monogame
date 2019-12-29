using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        protected sbyte bpp = -1;

        /// <summary>
        /// Raw Data buffer
        /// </summary>
        protected byte[] buffer;

        /// <summary>
        /// Image has a CLUT
        /// </summary>
        protected bool CLP;

        protected bool ignorealpha = false;

        /// <summary>
        /// Texture Data
        /// </summary>
        protected Texture texture;

        /// <summary>
        /// Start of Image Data
        /// </summary>
        protected uint textureDataPointer;

        protected bool throwexc = true;

        /// <summary>
        /// Start of Tim Data
        /// </summary>
        protected uint timOffset;

        protected bool trimExcess = false;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialize TIM class
        /// </summary>
        /// <param name="buffer">Raw Data buffer</param>
        /// <param name="offset">Start of Tim Data</param>
        public TIM2(byte[] buffer, uint offset = 0, bool noExc = false)
        {
            throwexc = !noExc;
            _Init(buffer, offset);
        }

        /// <summary> <summary> Initialize TIM class </summary> <param name="br">BinaryReader
        /// pointing to the file data</param> <param name="offset">Start of Tim Data</param>
        public TIM2(BinaryReader br, uint offset = 0, bool noExec = false)
        {
            throwexc = !noExec;
            _Init(br, offset);
        }

        protected TIM2()
        {
        }

        #endregion Constructors

        #region Enums

        /// <summary>
        /// BPP indicator
        /// <para>4 BPP is default</para>
        /// <para>If 8 and 16 are set then it's 24</para>
        /// <para>CLP should always be set for 4 and 8</para>
        /// </summary>
        [Flags]
        public enum Bppflag : byte
        {
            /// <summary>
            /// <para>4 BPP</para>
            /// <para>This is 0 so it will show as unset.</para>
            /// </summary>
            _4bpp = 0b0,

            /// <summary>
            /// <para>8 BPP</para>
            /// <para>if _8bpp and _16bpp are set then it's 24 bit</para>
            /// </summary>
            _8bpp = 0b1,

            /// <summary>
            /// <para>16 BPP</para>
            /// <para>if _8bpp and _16bpp are set then it's 24 bit</para>
            /// </summary>
            _16bpp = 0b10,

            /// <summary>
            /// <para>24 BPP</para>
            /// <para>Both flags must be set for this to be right</para>
            /// </summary>
            _24bpp = _8bpp | _16bpp,

            /// <summary>
            /// Color Lookup table Present
            /// </summary>
            CLP = 0b1000,
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets Bits per pixel
        /// </summary>
        public override byte GetBpp => (byte)bpp;

        /// <summary>
        /// Number of clut color palettes
        /// </summary>
        public override int GetClutCount => texture.NumOfCluts;

        /// <summary>
        /// Gets size of clut data as in TIM file
        /// </summary>
        public override int GetClutSize => texture.clutdataSize;

        /// <summary>
        /// Gets number of colours per palette
        /// </summary>
        public override int GetColorsCountPerPalette => texture.NumOfColours;

        /// <summary>
        /// Height
        /// </summary>
        public override int GetHeight => texture.Height;

        /// <summary>
        /// Gets origin texture coordinate X for VRAM buffer
        /// </summary>
        public override int GetOrigX => texture.ImageOrgX;

        /// <summary>
        /// Gets origin texture coordinate Y for VRAM buffer
        /// </summary>
        public override int GetOrigY => texture.ImageOrgY;

        /// <summary>
        /// Width
        /// </summary>
        public override int GetWidth => texture.Width;

        public bool IgnoreAlpha { get => ignorealpha; set => ignorealpha = value; }

        public bool NOT_TIM { get; protected set; }

        #endregion Properties

        #region Methods

        public bool Assert(bool a)
        {
            if (!a)
            {
                NOT_TIM = true;
                if (throwexc)
                {
                    throw new InvalidDataException($"Invalid TIM File");
                }
                //else
                //    Debug.Assert(a);
            }
            return !a;
        }

        public override void ForceSetClutColors(ushort newNumOfColours) => texture.NumOfColours = newNumOfColours;

        public override void ForceSetClutCount(ushort newClut) => texture.NumOfCluts = newClut;

        public override Color[] GetClutColors(ushort clut)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
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
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
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
        public override Texture2D GetTexture(ushort? clut = null)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                return GetTexture(br, clut == null || !CLP ? null : GetClutColors(br, clut.Value));
            }
        }

        public override Texture2D GetTexture()
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                return GetTexture(br, !CLP ? null : GetClutColors(br, 0));
            }
        }

        public override Texture2D GetTexture(Color[] colors = null)
        {
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                if (Assert(CLP) || Assert(colors.Length == texture.NumOfColours))
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
            Bppflag b = (Bppflag)br.ReadByte();
            timOffset = offset;
            if ((b & (Bppflag._24bpp)) >= (Bppflag._24bpp)) bpp = 24;
            else if ((b & Bppflag._16bpp) != 0) bpp = 16;
            else if ((b & Bppflag._8bpp) != 0) bpp = 8;
            else bpp = 4;
            CLP = (b & Bppflag.CLP) != 0;
            if (Assert(((bpp == 4 || bpp == 8) && CLP) || ((bpp == 16 || bpp == 24) && !CLP)))
                return;
            ReadParameters(br);
        }

        /// <summary>
        /// Writes the Tim file to the hard drive.
        /// </summary>
        /// <param name="path">Path where you want file to be saved.</param>
        public override void Save(string path)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Create(path)))
            {
                if (trimExcess)
                    bw.Write(buffer);
                else
                    bw.Write(buffer.Skip((int)timOffset).Take((int)(texture.ImageDataSize + textureDataPointer)).ToArray());
            }
        }

        protected void _Init(byte[] buffer, uint offset = 0)
        {
            this.buffer = buffer;
            using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
            {
                Init(br, offset);
            }
        }

        protected void _Init(BinaryReader br, uint offset)
        {
            trimExcess = true;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            buffer = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
            using (BinaryReader br2 = new BinaryReader(new MemoryStream(buffer)))
            {
                Init(br2, 0);
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
        protected Color[] CreateImageBuffer(BinaryReader br, Color[] palette = null)
        {
            br.BaseStream.Seek(textureDataPointer, SeekOrigin.Begin);
            Color[] buffer = new Color[texture.Width * texture.Height]; //ARGB
            if (Assert((buffer.Length / bpp) <= br.BaseStream.Length - br.BaseStream.Position)) //make sure the buffer is large enough
                return null;
            if (bpp == 8)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    byte colorkey = br.ReadByte();
                    if (colorkey < texture.NumOfColours)
                        buffer[i] = palette[colorkey]; //colorkey
                    else
                        buffer[i] = Color.TransparentBlack; // trying something out of oridinary.
                }
            }
            else if (bpp == 4)
            {
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
                    buffer[i] = ABGR1555toRGBA32bit(br.ReadUInt16(), ignorealpha);
            }
            else if (bpp == 24) //could be wrong. // assuming it is BGR
            {
                for (int i = 0; i < buffer.Length && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                {
                    byte[] pixel = br.ReadBytes(3);
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
                throw new Exception($"TIM_v2::CreateImageBuffer::TIM unsupported bits per pixel = {bpp}");
            //Then in bs debug where ReadTexture store for all cluts
            //data and then create Texture2D from there. (array of e.g. 15 texture2D)
            return buffer;
        }

        public override void SaveCLUT(string path)
        {
            if (CLP)
                using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
                {
                    using (Texture2D CLUT = new Texture2D(Memory.graphics.GraphicsDevice, texture.NumOfColours, texture.NumOfCluts))
                    {
                        for (ushort i = 0; i < texture.NumOfCluts; i++)
                        {
                            CLUT.SetData(0, new Rectangle(0, i, texture.NumOfColours, 1), GetClutColors(br, i), 0, texture.NumOfColours);
                        }
                        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                            CLUT.SaveAsPng(fs, texture.NumOfColours, texture.NumOfCluts);
                    }
                }
        }

        /// <summary>
        /// Get clut color palette
        /// </summary>
        /// <param name="br">Binaryreader pointing to memorystream of data.</param>
        /// <param name="clut">Active clut data</param>
        /// <returns>Color[]</returns>
        protected Color[] GetClutColors(BinaryReader br, ushort clut)
        {
            if (clut >= texture.NumOfCluts)
                throw new Exception($"TIM_v2::GetClutColors::given clut {clut} is >= texture number of cluts {texture.NumOfCluts}");

            if (CLP)
            {
                Color[] colorPixels = new Color[texture.NumOfColours];
                br.BaseStream.Seek(timOffset + 20 + (texture.NumOfColours * 2 * clut), SeekOrigin.Begin);
                for (int i = 0; i < texture.NumOfColours; i++)
                    colorPixels[i] = ABGR1555toRGBA32bit(br.ReadUInt16(), ignorealpha);
                return colorPixels;
            }
            else throw new Exception($"TIM that has {bpp} bpp mode and has no clut data!");
        }

        protected Texture2D GetTexture(BinaryReader br, Color[] colors)
        {
            Texture2D image = new Texture2D(Memory.graphics.GraphicsDevice, GetWidth, GetHeight, false, SurfaceFormat.Color);
            image.SetData(CreateImageBuffer(br, colors));
            return image;
        }

        /// <summary>
        /// Populate Texture structure
        /// </summary>
        /// <param name="br">Binaryreader pointing to memorystream of data.</param>
        /// <param name="_bpp">bits per pixel</param>
        protected void ReadParameters(BinaryReader br)
        {
            texture = new Texture();
            texture.Read(br, (byte)bpp, CLP, !throwexc);
            if (Assert(!texture.NOT_TIM)) return;
            textureDataPointer = (uint)br.BaseStream.Position;
            if (trimExcess)
                buffer = buffer.Skip((int)timOffset).Take((int)(texture.ImageDataSize + textureDataPointer - timOffset)).ToArray();
        }

        public override void Load(byte[] buffer, uint offset = 0) => _Init(buffer, offset);

        #endregion Methods

        #region Structs

        protected struct Texture
        {
            #region Fields

            public byte[] ClutData;

            public int clutdataSize;

            /// <summary>
            /// length, in bytes, of the entire CLUT block (including the header)
            /// </summary>
            public uint clutSize;

            public ushort Height;
            public int ImageDataSize;
            public ushort ImageOrgX;
            public ushort ImageOrgY;
            public uint ImageSize;
            public ushort NumOfCluts;
            public ushort NumOfColours;
            public ushort PaletteX;
            public ushort PaletteY;
            public bool throwexc;
            public ushort Width;

            #endregion Fields

            #region Properties

            public bool NOT_TIM { get; private set; }

            #endregion Properties

            #region Methods

            public bool Assert(bool a)
            {
                if (!a)
                {
                    NOT_TIM = true;
                    if (throwexc)
                    {
                        throw new InvalidDataException($"Invalid TIM File");
                    }
                    //else
                    //    Debug.Assert(a);
                }
                return !a;
            }

            /// <summary>
            /// Populate Texture structure
            /// </summary>
            /// <param name="br">Binaryreader pointing to memorystream of data.</param>
            /// <param name="_bpp">bits per pixel</param>
            public void Read(BinaryReader br, byte _bpp, bool clp, bool noExec = false)
            {
                throwexc = !noExec;
                br.BaseStream.Seek(3, SeekOrigin.Current);
                if (clp)
                {
                    //long start = br.BaseStream.Position;
                    clutSize = br.ReadUInt32();
                    PaletteX = br.ReadUInt16();
                    PaletteY = br.ReadUInt16();
                    NumOfColours = br.ReadUInt16(); //width of clut
                    NumOfCluts = br.ReadUInt16(); //height of clut
                    clutdataSize = (int)(clutSize - 12);//(NumOfColours * NumOfCluts*2);
                    if (Assert(clutdataSize == NumOfColours * NumOfCluts * 2 || clutdataSize == NumOfColours * NumOfCluts) ||
                    Assert(PaletteX % 16 == 0) ||
                    Assert(PaletteY >= 0 && PaletteY <= 511))
                        return;
                    if (_bpp == 4 && NumOfColours > 16)
                    {
                        // timviewer was overriding the read number of colors per pixel to 16
                        // and this makes sense because you cannot read more than 16 colors with only a 4 bit value.
                        // though this might break stuff.
                        NumOfColours = 16;
                        NumOfCluts = checked((ushort)(clutdataSize / (NumOfColours * 2)));
                    }
                    Assert(_bpp == 8 && NumOfColours <= 256 || _bpp != 8);
                    ClutData = br.ReadBytes(clutdataSize);
                    //br.BaseStream.Seek(start+clutSize, SeekOrigin.Begin);
                }
                //wmsetus uses 4BPP, but sets 256 colours, but actually is 16, but num of clut is 2* 256/16 WTF?

                ImageSize = br.ReadUInt32(); // image size + header in bytes
                ImageOrgX = br.ReadUInt16();
                ImageOrgY = br.ReadUInt16();
                Width = br.ReadUInt16();
                Height = br.ReadUInt16();
                ImageDataSize = (int)(ImageSize - 12);//(NumOfColours * NumOfCluts*2);
                Assert(ImageDataSize == Width * Height * 2);

                // Pixel data is stored in uint16 spots. So you use this to convert that to the
                // correct color / CLUT colorkey If 32 bit existed it'd be 1:2 24 bit is 1:1.5 16 bit
                // is 1:1 8 bit is 2:1 4 bit is 4:1

                if (_bpp == 4)
                    Width = (ushort)(Width * 4);
                if (_bpp == 8)
                    Width = (ushort)(Width * 2);
                //if (_bpp == 16)
                //    Width = Width;
                if (_bpp == 24)
                {
                    Width = (ushort)(Width / 1.5);
                    // The below i'm unsure if any of this effects TIM files. So I commented them out.
                    // http://www.elisanet.fi/6581/PSX/doc/Playstation_Hardware.pdf
                    // http://www.elisanet.fi/6581/PSX/doc/psx.pdf
                    //24 bit color mode has different restrictions. I guess you bybass the psx gpu to draw these
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