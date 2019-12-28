using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace OpenVIII
{
    public class Texture2DWrapper : Texture_Base
{
        Texture2D tex;

        public Texture2DWrapper(Texture2D tex) => this.tex = tex;
        public static implicit operator Texture2DWrapper(Texture2D right) => new Texture2DWrapper(right);
        public static implicit operator Texture2D(Texture2DWrapper right) => right.tex;
        public override int GetClutCount => 1;

        public override byte GetBpp => 24; //shouldn't this be 32? because all our texture2d's are stored in RGBA

        public override int GetClutSize => 0;

        public override int GetColorsCountPerPalette => 0;

        public override int GetHeight => tex?.Height ??0;

        public override int GetOrigX => 0;

        public override int GetOrigY => 0;

        public override int GetWidth => tex?.Width ??0;

        public override Color[] GetClutColors(ushort clut) => null;
        public override void ForceSetClutColors(ushort newNumOfColours)
        {; }
        public override void ForceSetClutCount(ushort newClut)
        {; }
        public override Texture2D GetTexture() => tex;
        public override Texture2D GetTexture(Color[] colors = null) => tex;
        public override Texture2D GetTexture(ushort? clut = null) => tex;
        public override void Save(string path)
        {
            using (FileStream fs = File.Create(path))
                tex.SaveAsPng(fs, tex.Width, tex.Height);
        }

        public override void SaveCLUT(string path)
        { // no clut data.
        }
    }
}