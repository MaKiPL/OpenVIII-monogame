using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FF8
{
    //I borrowed this from my Rinoa's toolset, but modified to aim for buffer rather than file-work
    internal class TEX
    {
        public Texture TextureData => texture;  //added to get texturedata outside of class.
        private byte[] buffer;

        public byte[] GetBuffer() => buffer;

        private int textureLocator;
        private Texture texture;

        public struct Texture //RawImage after paletteData
        {
            public uint Width; //0x3C
            public uint Height; //0x40
            public byte NumOfPalettes; //0x30
            public byte PaletteFlag; //0x4C
            public uint PaletteSize; //0x58
            public byte[] paletteData; //0xEC
            public byte bytesPerPixel;
        }
        // this struct works keeping it around incase we need a value from it I only ported out bytesperpixel
        ///// <summary>
        ///// TEX Header info
        ///// </summary>
        ///// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/TexFile.cpp"/>
        //[StructLayout(LayoutKind.Sequential)]
        //public struct TexStruct
        //{
        //    // the number on side was me counting bytes. I forgot to count 0 lol.
        //
        //    // Header
        //    /// <summary>
        //    /// 1=FF7 | 2=FF8
        //    /// </summary>
        //    public UInt32 version;//4

        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 unknown1;//8

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] 0 or 1
        //    /// </summary>
        //    public UInt32 hasColorKey;//12

        //    /// <summary>
        //    /// 0 or 1 // hasAlphaBits? related to minAlphaBits
        //    /// </summary>
        //    public UInt32 unknown2;//16

        //    /// <summary>
        //    /// [bpp = 16] 3 [else] Varies (0, 3, 5, 6, 7... 31)
        //    /// </summary>
        //    public UInt32 unknown3;//20

        //    /// <summary>
        //    /// Always 4
        //    /// </summary>
        //    public UInt32 minBitsPerColor;//24

        //    /// <summary>
        //    /// Always 8
        //    /// </summary>
        //    public UInt32 maxBitsPerColor;//28

        //    /// <summary>
        //    /// 0 or 4
        //    /// </summary>
        //    public UInt32 minAlphaBits;//32

        //    /// <summary>
        //    /// Always 8
        //    /// </summary>
        //    public UInt32 maxAlphaBits;//36

        //    /// <summary>
        //    /// [bpp = 16] 32 [else] 8
        //    /// </summary>
        //    public UInt32 minBitsPerPixel;//40

        //    /// <summary>
        //    /// Always 32
        //    /// </summary>
        //    public UInt32 maxBitsPerPixel;//44

        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 unknown4;//48

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] varies (1 -&gt; 31)
        //    /// </summary>
        //    public UInt32 nbPalettes;//52

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] 16 or 256
        //    /// </summary>
        //    public UInt32 nbColorsPerPalette1;//56

        //    /// <summary>
        //    /// [bpp = 16] 16 [bpp = 8] 8 ([bpp = 4] 4)
        //    /// </summary>
        //    public UInt32 bitDepth;//60

        //    /// <summary>
        //    /// Width of image
        //    /// </summary>
        //    public UInt32 imageWidth;//64

        //    /// <summary>
        //    /// Height of image
        //    /// </summary>
        //    public UInt32 imageHeight;//68

        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 pitch;//72

        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 unknown5;//76

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] 1
        //    /// </summary>
        //    public UInt32 hasPal;//80

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] 8
        //    /// </summary>
        //    public UInt32 bitsPerIndex;//84

        //    /// <summary>
        //    /// // [bpp = 16] 0 [else] 1
        //    /// </summary>
        //    public UInt32 indexedTo8bit;//88

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] varies (16, 32, 48 ... 2048)
        //    /// </summary>
        //    public UInt32 paletteSize;//92

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] 16 or 256 // may be 0 sometimes
        //    /// </summary>
        //    public UInt32 nbColorsPerPalette2;//96

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] varies
        //    /// </summary>
        //    public UInt32 runtimeData1;//100

        //    /// <summary>
        //    /// [bpp = 16] 16 [else] 8
        //    /// </summary>
        //    public UInt32 bitsPerPixel;//104

        //    /// <summary>
        //    /// [bpp = 16] 2 [else] 1
        //    /// </summary>
        //    public UInt32 bytesPerPixel;//108

        //    // Pixel format
        //    /// <summary>
        //    /// [bpp = 16] 5 [else] 0
        //    /// </summary>
        //    public UInt32 nbRedBits1;

        //    /// <summary>
        //    /// [bpp = 16] 5 [else] 0
        //    /// </summary>
        //    public UInt32 nbGreenBits1;

        //    /// <summary>
        //    /// [bpp = 16] 5 [else] 0
        //    /// </summary>
        //    public UInt32 nbBlueBits1;

        //    /// <summary>
        //    /// [bpp = 16] 1 [else] 0
        //    /// </summary>
        //    public UInt32 nbAlphaBits1;

        //    /// <summary>
        //    /// [bpp = 16] 31 [else] 0
        //    /// </summary>
        //    public UInt32 redBitmask;

        //    /// <summary>
        //    /// [bpp = 16] 992 [else] 0
        //    /// </summary>
        //    public UInt32 greenBitmask;

        //    /// <summary>
        //    /// [bpp = 16] 31744 [else] 0
        //    /// </summary>
        //    public UInt32 blueBitmask;

        //    /// <summary>
        //    /// [bpp = 16] 32768 [else] 0
        //    /// </summary>
        //    public UInt32 alphaBitmask;

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] 0
        //    /// </summary>
        //    public UInt32 redShift;

        //    /// <summary>
        //    /// [bpp = 16] 5 [else] 0
        //    /// </summary>
        //    public UInt32 greenShift;

        //    /// <summary>
        //    /// [bpp = 16] 10 [else] 0
        //    /// </summary>
        //    public UInt32 blueShift;

        //    /// <summary>
        //    /// [bpp = 16] 15 [else] 0
        //    /// </summary>
        //    public UInt32 alphaShift;

        //    /// <summary>
        //    /// [bpp = 16] 3 [else] 0
        //    /// </summary>
        //    public UInt32 nbRedBits2;

        //    /// <summary>
        //    /// [bpp = 16] 3 [else] 0
        //    /// </summary>
        //    public UInt32 nbGreenBits2;

        //    /// <summary>
        //    /// [bpp = 16] 3 [else] 0
        //    /// </summary>
        //    public UInt32 nbBlueBits2;

        //    /// <summary>
        //    /// [bpp = 16] 7 [else] 0
        //    /// </summary>
        //    public UInt32 nbAlphaBits2;

        //    /// <summary>
        //    /// [bpp = 16] 31 [else] 0
        //    /// </summary>
        //    public UInt32 redMax;

        //    /// <summary>
        //    /// [bpp = 16] 31 [else] 0
        //    /// </summary>
        //    public UInt32 greenMax;

        //    /// <summary>
        //    /// [bpp = 16] 31 [else] 0
        //    /// </summary>
        //    public UInt32 blueMax;

        //    /// <summary>
        //    /// [bpp = 16] 1 [else] 0
        //    /// </summary>
        //    public UInt32 alphaMax;

        //    // /Pixel format
        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 hasColorKeyArray;

        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 runtimeData2;

        //    /// <summary>
        //    /// Always 255
        //    /// </summary>
        //    public UInt32 referenceAlpha;

        //    /// <summary>
        //    /// Always 4
        //    /// </summary>
        //    public UInt32 runtimeData3;

        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 unknown6;

        //    /// <summary>
        //    /// Always 0
        //    /// </summary>
        //    public UInt32 paletteIndex;

        //    /// <summary>
        //    /// Varies, sometimes 0
        //    /// </summary>
        //    public UInt32 runtimeData4;

        //    /// <summary>
        //    /// Varies, sometimes 0
        //    /// </summary>
        //    public UInt32 runtimeData5;

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] Varies (0, 16, 32, 48 ... 768)
        //    /// </summary>
        //    public UInt32 unknown7;

        //    /// <summary>
        //    /// [bpp = 16] 0 [else] Varies (0, 128, 129 ... 511, 512)
        //    /// </summary>
        //    public UInt32 unknown8;

        //    /// <summary>
        //    /// [bpp = 16] 0 or 896 [else] Varies (0, 216 ... 1020)
        //    /// </summary>
        //    public UInt32 unknown9;

        //    /// <summary>
        //    /// Varies (16, 32, 48 ... 960)
        //    /// </summary>
        //    public UInt32 unknown10;

        //    /// <summary>
        //    /// Varies (0, 128, 192 or 256) // only on ff8! (version &gt;= 2)
        //    /// </summary>
        //    public UInt32 unknown11;
        //}

        private struct Color
        {
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte Alpha;
        }

        ///// <summary>
        ///// read the start of byte array into a struct
        ///// </summary>
        ///// <seealso cref="https://stackoverflow.com/questions/2871/reading-a-c-c-data-structure-in-c-sharp-from-a-byte-array"/>

        //public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        //{
        //    T stuff;
        //    GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        //    try
        //    {
        //        stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        //    }
        //    finally
        //    {
        //        handle.Free();
        //    }
        //    return stuff;
        //}

        public TEX(byte[] buffer)
        {
            texture = new Texture();
            this.buffer = buffer;

            ReadParameters();
        }

        public void ReadParameters()
        {
            texture.Width = BitConverter.ToUInt32(buffer, 0x3C);
            texture.Height = BitConverter.ToUInt32(buffer, 0x40);
            texture.bytesPerPixel = buffer[0x68];
            texture.NumOfPalettes = buffer[0x30];
            texture.PaletteFlag = buffer[0x4C];
            texture.PaletteSize = BitConverter.ToUInt32(buffer, 0x58);
            if (texture.PaletteFlag != 0)
            {
                texture.paletteData = new byte[texture.PaletteSize * 4];
                Buffer.BlockCopy(buffer, 0xF0, texture.paletteData, 0, (int)(texture.PaletteSize * 4));
            }
            textureLocator = 0xEC + (int)(texture.PaletteSize * 4) + 4;
        }

        public Texture2D GetTexture(int forcePalette = -1)
        {
            int localTextureLocator = textureLocator;
            Color[] colors;
            if (texture.PaletteFlag != 0)
            {
                if (forcePalette >= texture.NumOfPalettes) //prevents exception for forcing a palette that doesn't exist.
                    return null;

                colors = new Color[texture.paletteData.Length / 4];
                int k = 0;
                for (int i = 0; i < texture.paletteData.Length; i += 4)
                {
                    colors[k].Red = texture.paletteData[i];
                    colors[k].Green = texture.paletteData[i + 1];
                    colors[k].Blue = texture.paletteData[i + 2];
                    colors[k].Alpha = texture.paletteData[i + 3];
                    k++;
                }
                Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, (int)texture.Width, (int)texture.Height, false, SurfaceFormat.Color);

                byte[] convertBuffer = new byte[texture.Width * texture.Height * 4];
                for (int i = 0; i < convertBuffer.Length; i += 4)
                {
                    byte colorkey = buffer[localTextureLocator++];
                    convertBuffer[i] = colors[forcePalette == -1 ? colorkey : (forcePalette * 16) + colorkey].Blue;
                    convertBuffer[i + 1] = colors[forcePalette == -1 ? colorkey : (forcePalette * 16) + colorkey].Green;
                    convertBuffer[i + 2] = colors[forcePalette == -1 ? colorkey : (forcePalette * 16) + colorkey].Red;
                    convertBuffer[i + 3] = colors[forcePalette == -1 ? colorkey : (forcePalette * 16) + colorkey].Alpha;
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
                    using (MemoryStream os = new MemoryStream((int)(texture.Width * texture.Height * 4)))
                    using (BinaryWriter bw = new BinaryWriter(os))
                    {
                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            ms.Seek(localTextureLocator, SeekOrigin.Begin);
                            while (ms.Position < ms.Length)
                                bw.Write(fromPsColor(br.ReadUInt16()), 0, 4);
                        }
                        bmp.SetData(os.GetBuffer(), 0, (int)os.Length);
                    }
                    return bmp;
                }
                if (texture.bytesPerPixel == 3) // not tested but vincent tim had support for it so i guess it's possible RGB or BGR is a thing.
                {
                    //working code
                    Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, (int)texture.Width, (int)texture.Height, false, SurfaceFormat.Color);
                    using (MemoryStream os = new MemoryStream((int)(texture.Width * texture.Height * 4)))
                    using (BinaryWriter bw = new BinaryWriter(os))
                    {
                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            ms.Seek(localTextureLocator, SeekOrigin.Begin);
                            while (ms.Position < ms.Length)
                            {
                                bw.Write(br.ReadBytes(3), 0, 3);
                                bw.Write((byte)255);
                            }
                        }
                        bmp.SetData(os.GetBuffer(), 0, (int)os.Length);
                    }
                    return bmp;
                }

                //not working code
                //Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, (int)texture.Width, (int)texture.Height, false, SurfaceFormat.Bgra5551); //cool, mongame has already BRGA 5551 mode!
                //bmp.SetData(buffer, localTextureLocator, buffer.Length - localTextureLocator);
                //can save image with this but it didn't work with bgra5551
                //using (FileStream fs = File.OpenWrite(Path.GetTempFileName() + ".ff8.png"))
                //{
                //    bmp.SaveAsPng(fs, (int)texture.Width, (int)texture.Height);
                //}
                //return bmp;
                return null;
            }
        }
        /// <summary>
        /// Number needed to convert 16 bit to 32 bit color
        /// </summary>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/PsColor.h"/>
        public const double COEFF_COLOR = (double)255 / 31;

        /// <summary>
        /// converts 16 bit color to 32bit with alpha. alpha needs to be set true manually per pixel unless you know the color value.
        /// </summary>
        /// <param name="color">16 bit color</param>
        /// <param name="useAlpha">area is visable or not</param>
        /// <returns>byte[4] red green blue alpha, i think</returns>
        /// <seealso cref="https://github.com/myst6re/vincent-tim/blob/master/PsColor.cpp"/>
        public static byte[] fromPsColor(UInt16 color, bool useAlpha = false) => new byte[] { (byte)Math.Round((color & 31) * COEFF_COLOR), (byte)Math.Round((color >> 5 & 31) * COEFF_COLOR), (byte)Math.Round((color >> 10 & 31) * COEFF_COLOR), (byte)(color == 0 && useAlpha ? 0 : 255) }; // thought maybe || color == 24667 would make the pink color transparent.
    }
}