using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    //I borrowed this from my Rinoa's toolset, but modified to aim for buffer rather than file-work
    class TEX
    {
        public Texture TextureData { get => texture; } //added to get texturedata outside of class.
        private byte[] buffer;
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

        struct Color
        {
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte Alpha;
        }


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
            if (forcePalette >= texture.NumOfPalettes) //prevents exception for forcing a palette that doesn't exist.
                return null;
            int localTextureLocator = textureLocator;
            Color[] colors;
            if (texture.PaletteFlag != 0)
            {
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
