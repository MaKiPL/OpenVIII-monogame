using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    /// <summary>
    /// Texl.obj is a file containing 20 high-res TIM textures for world map
    /// </summary>
    class texl
    {
        private const int TEX_SIZE = 0x12800;
        private const int TEX_COUNT = 20;
        private TextureHandler[][] textures;
        public texl(byte[] texlBuffer)
        {
            textures = new TextureHandler[20][];
            using (var br = new BinaryReader(new MemoryStream(texlBuffer)))
            for (var i = 0; i < TEX_COUNT; i++)
            {
                    var timOffset = i * TEX_SIZE;
                    var tim = new TIM2(texlBuffer, (uint)timOffset);
                    textures[i] = new TextureHandler[tim.GetClutCount];
                    for (ushort k = 0; k < textures[i].Length; k++)
                        textures[i][k] = TextureHandler.Create($"texl_tim{(i + 1).ToString("D2")}.tim", tim, k, null);
                    //todo detect if mods aren't using palettes.
                }
        }

        public TextureHandler GetTexture(int index, int clut)
            => textures[index][clut];
    }
}
