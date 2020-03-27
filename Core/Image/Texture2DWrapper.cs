using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace OpenVIII
{
    public class Texture2DWrapper : Texture_Base
    {
        #region Fields

        private Texture2D _tex;

        #endregion Fields

        #region Constructors

        public Texture2DWrapper(Texture2D tex) => _tex = tex;

        #endregion Constructors

        #region Properties

        public override byte GetBytesPerPixel => 4;

        public override int GetClutCount => 0;

        public override int GetClutSize => 0;

        public override int GetColorsCountPerPalette => 0;

        public override int GetHeight => _tex?.Height ?? 0;

        public override int GetOrigX => 0;

        public override int GetOrigY => 0;

        public override int GetWidth => _tex?.Width ?? 0;

        #endregion Properties

        #region Methods

        public static implicit operator Texture2D(Texture2DWrapper right) => right._tex;

        public static implicit operator Texture2DWrapper(Texture2D right) => new Texture2DWrapper(right);

        public override void ForceSetClutColors(ushort newNumOfColors)
        {
        }

        public override void ForceSetClutCount(ushort newClut)
        {
        }

        public override Color[] GetClutColors(ushort clut) => null;

        public override Texture2D GetTexture() => _tex;

        public override Texture2D GetTexture(Color[] colors) => _tex;

        public override Texture2D GetTexture(ushort clut) => _tex;

        public override void Load(byte[] buffer, uint offset = 0) => throw new System.NotImplementedException("This class must use Load(Texture2D)");

        public void Load(Texture2D inputTex) => _tex = inputTex;

        public override void Save(string path)
        {
            using (var fs = File.Create(path))
                _tex.SaveAsPng(fs, _tex.Width, _tex.Height);
        }

        public override void SaveCLUT(string path)
        { // no clut data.
        }

        #endregion Methods
    }
}