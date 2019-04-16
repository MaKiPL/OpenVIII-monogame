using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    //I borrowed this from my Rinoa's toolset, but modified to aim for buffer rather than file-work
    class TEX
    {
        public Texture TextureData { get => texture; } //added to get texturedata outside of class.
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
        }
        /// <summary>
        /// TEX Header info
        /// </summary>
        /// <seealso cref="https://github.com/myst6re/vincent-tim"/>
        [StructLayout(LayoutKind.Sequential)]
        public struct TexStruct
        {
            // Header
            /// <summary>
            /// 1=FF7 | 2=FF8
            /// </summary>
            UInt32 version;
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 unknown1;
            /// <summary>
            /// [bpp = 16] 0 [else] 0 or 1
            /// </summary>
            UInt32 hasColorKey;
            /// <summary>
            /// 0 or 1 // hasAlphaBits? related to minAlphaBits
            /// </summary>
            UInt32 unknown2;
            /// <summary>
            /// [bpp = 16] 3 [else] Varies (0, 3, 5, 6, 7... 31)
            /// </summary>
            UInt32 unknown3;
            /// <summary>
            /// Always 4
            /// </summary>
            UInt32 minBitsPerColor;
            /// <summary>
            /// Always 8
            /// </summary>
            UInt32 maxBitsPerColor;
            /// <summary>
            /// 0 or 4
            /// </summary>
            UInt32 minAlphaBits;
            /// <summary>
            /// Always 8
            /// </summary>
            UInt32 maxAlphaBits;
            /// <summary>
            /// [bpp = 16] 32 [else] 8
            /// </summary>
            UInt32 minBitsPerPixel;
            /// <summary>
            /// Always 32
            /// </summary>
            UInt32 maxBitsPerPixel;
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 unknown4;
            /// <summary>
            /// [bpp = 16] 0 [else] varies (1 -> 31)
            /// </summary>
            UInt32 nbPalettes;
            /// <summary>
            /// [bpp = 16] 0 [else] 16 or 256
            /// </summary>
            UInt32 nbColorsPerPalette1;
            /// <summary>
            /// [bpp = 16] 16 [bpp = 8] 8 ([bpp = 4] 4)
            /// </summary>
            UInt32 bitDepth;
            /// <summary>
            /// Width of image
            /// </summary>
            UInt32 imageWidth;
            /// <summary>
            /// Height of image
            /// </summary>
            UInt32 imageHeight;
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 pitch;
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 unknown5;
            /// <summary>
            /// [bpp = 16] 0 [else] 1
            /// </summary>
            UInt32 hasPal;
            /// <summary>
            /// [bpp = 16] 0 [else] 8
            /// </summary>
            UInt32 bitsPerIndex;
            /// <summary>
            /// // [bpp = 16] 0 [else] 1
            /// </summary>
            UInt32 indexedTo8bit;
            /// <summary>
            /// [bpp = 16] 0 [else] varies (16, 32, 48 ... 2048)
            /// </summary>
            UInt32 paletteSize;
            /// <summary>
            /// [bpp = 16] 0 [else] 16 or 256 // may be 0 sometimes
            /// </summary>
            UInt32 nbColorsPerPalette2;
            /// <summary>
            /// [bpp = 16] 0 [else] varies
            /// </summary>
            UInt32 runtimeData1;
            /// <summary>
            /// [bpp = 16] 16 [else] 8
            /// </summary>
            UInt32 bitsPerPixel;
            /// <summary>
            /// [bpp = 16] 2 [else] 1
            /// </summary>
            UInt32 bytesPerPixel;
            // Pixel format
            /// <summary>
            /// [bpp = 16] 5 [else] 0
            /// </summary>
            UInt32 nbRedBits1;
            /// <summary>
            /// [bpp = 16] 5 [else] 0
            /// </summary>
            UInt32 nbGreenBits1;
            /// <summary>
            /// [bpp = 16] 5 [else] 0
            /// </summary>
            UInt32 nbBlueBits1;
            /// <summary>
            /// [bpp = 16] 1 [else] 0
            /// </summary>
            UInt32 nbAlphaBits1;
            /// <summary>
            /// [bpp = 16] 31 [else] 0
            /// </summary>
            UInt32 redBitmask;
            /// <summary>
            /// [bpp = 16] 992 [else] 0
            /// </summary>
            UInt32 greenBitmask;
            /// <summary>
            /// [bpp = 16] 31744 [else] 0
            /// </summary>
            UInt32 blueBitmask;
            /// <summary>
            /// [bpp = 16] 32768 [else] 0
            /// </summary>
            UInt32 alphaBitmask;
            /// <summary>
            /// [bpp = 16] 0 [else] 0
            /// </summary>
            UInt32 redShift;
            /// <summary>
            /// [bpp = 16] 5 [else] 0
            /// </summary>
            UInt32 greenShift;
            /// <summary>
            /// [bpp = 16] 10 [else] 0
            /// </summary>
            UInt32 blueShift;
            /// <summary>
            /// [bpp = 16] 15 [else] 0
            /// </summary>
            UInt32 alphaShift;
            /// <summary>
            /// [bpp = 16] 3 [else] 0
            /// </summary>
            UInt32 nbRedBits2;
            /// <summary>
            /// [bpp = 16] 3 [else] 0
            /// </summary>
            UInt32 nbGreenBits2;
            /// <summary>
            /// [bpp = 16] 3 [else] 0
            /// </summary>
            UInt32 nbBlueBits2;
            /// <summary>
            /// [bpp = 16] 7 [else] 0
            /// </summary>
            UInt32 nbAlphaBits2;
            /// <summary>
            /// [bpp = 16] 31 [else] 0
            /// </summary>
            UInt32 redMax;
            /// <summary>
            /// [bpp = 16] 31 [else] 0
            /// </summary>
            UInt32 greenMax;
            /// <summary>
            /// [bpp = 16] 31 [else] 0
            /// </summary>
            UInt32 blueMax;
            /// <summary>
            /// [bpp = 16] 1 [else] 0
            /// </summary>
            UInt32 alphaMax;
            // /Pixel format
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 hasColorKeyArray;
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 runtimeData2;
            /// <summary>
            /// Always 255
            /// </summary>
            UInt32 referenceAlpha;
            /// <summary>
            /// Always 4
            /// </summary>
            UInt32 runtimeData3;
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 unknown6;
            /// <summary>
            /// Always 0
            /// </summary>
            UInt32 paletteIndex;
            /// <summary>
            /// Varies, sometimes 0
            /// </summary>
            UInt32 runtimeData4;
            /// <summary>
            /// Varies, sometimes 0
            /// </summary>
            UInt32 runtimeData5;
            /// <summary>
            /// [bpp = 16] 0 [else] Varies (0, 16, 32, 48 ... 768)
            /// </summary>
            UInt32 unknown7;
            /// <summary>
            /// [bpp = 16] 0 [else] Varies (0, 128, 129 ... 511, 512)
            /// </summary>
            UInt32 unknown8;
            /// <summary>
            /// [bpp = 16] 0 or 896 [else] Varies (0, 216 ... 1020)
            /// </summary>
            UInt32 unknown9;
            /// <summary>
            /// Varies (16, 32, 48 ... 960)
            /// </summary>
            UInt32 unknown10;
            /// <summary>
            /// Varies (0, 128, 192 or 256) // only on ff8! (version >= 2)
            /// </summary>
            UInt32 unknown11;
    }

        struct Color
        {
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte Alpha;
        }
        /// <summary>
        /// read the start of byte array into a struct
        /// </summary>
        /// <seealso cref="https://stackoverflow.com/questions/2871/reading-a-c-c-data-structure-in-c-sharp-from-a-byte-array"/>
        public TexStruct header;
        T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            T stuff;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return stuff;
        }
        public TEX(byte[] buffer)
        {
            texture = new Texture();
            header = ByteArrayToStructure<TexStruct>(buffer);
            this.buffer = buffer;

            ReadParameters();
        }

        public void ReadParameters()
        {

            texture.Width = BitConverter.ToUInt32(buffer, 0x3C);
            texture.Height = BitConverter.ToUInt32(buffer, 0x40);
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
                Texture2D bmp = new Texture2D(Memory.graphics.GraphicsDevice, (int)texture.Width, (int)texture.Height, false, SurfaceFormat.Bgra5551); //cool, mongame has already BRGA 5551 mode!                
                bmp.SetData(buffer, localTextureLocator, buffer.Length - localTextureLocator);

                return bmp;
            }
        }
    }
}
