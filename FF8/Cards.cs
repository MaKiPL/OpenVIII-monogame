using Microsoft.Xna.Framework;
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


        protected new static Texture2D[] textures;

        protected new static Dictionary<Enum, Entry> entries;

        public Cards() => Process(TextureCount, TextureFilename, TextureStartOffset, IndexFilename,ref entries,ref textures);

        public override void Draw(Enum id, Rectangle dst, float fade = 1) => base.Draw(id, dst, fade,ref entries, ref textures);
        public override void Draw(int id, Rectangle dst, float fade = 1) => base.Draw(id, dst, fade,ref entries, ref textures);
        public override Entry GetEntry(Enum id) => base.GetEntry(id, ref entries);
        public override Entry GetEntry(int id) => base.GetEntry(id, ref entries);
    }
}
