using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class Cards : Faces
    {
        private const int TextureCount = 10;
        private const string TextureFilename = "mc{0:00}.tex";
        private const int TextureStartOffset = 0;
        private const string IndexFilename = "cardanm.sp2";


        private static Texture2D[] textures;

        private static Dictionary<Enum, Entry> entries;

        public Cards() => Process(TextureCount, TextureFilename, TextureStartOffset, IndexFilename,ref entries,ref textures);
    }
}
