using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Core.World
{
    /// <summary>
    /// Texl.obj is a file containing 20 high-res TIM textures for world map
    /// </summary>
    class texl
    {
        private const int TEX_SIZE = 0x12800;
        private const int TEX_COUNT = 20;
        private Texture2D[][] textures;
        public texl(byte[] texlBuffer)
        {
            textures = new Texture2D[20][];
            using (MemoryStream ms = new MemoryStream(texlBuffer))
            using (BinaryReader br = new BinaryReader(ms))
            for (int i = 0; i < TEX_COUNT; i++)
            {
                    int timOffset = i * TEX_SIZE;
                    TIM2 tim = new TIM2(texlBuffer, (uint)timOffset);
                    textures[i] = new Texture2D[tim.GetClutCount];
                    for (ushort k = 0; k < textures[i].Length; k++)
                        textures[i][k] = tim.GetTexture(k);
            }
        }

        public Texture2D GetTexture(int index, int clut)
            => textures[index][clut];
    }
}
