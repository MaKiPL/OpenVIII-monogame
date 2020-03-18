using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Particle Texture
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_PMP"/>
    public sealed class PMP : Texture_Base
    {
        #region Fields

        private byte[] _buffer;
        private Cluts _clut;
        private int _height;

        private int _width;

        #endregion Fields

        #region Constructors

        public PMP(byte[] pmpB) => Load(pmpB);

        #endregion Constructors

        #region Properties

        public override byte GetBytesPerPixel => 4;

        public override int GetClutCount => 16;

        public override int GetClutSize => 16;

        public override int GetColorsCountPerPalette => 16;

        public override int GetHeight => _height;

        public override int GetOrigX => 0;

        public override int GetOrigY => 0;

        public override int GetWidth => _width;

        public byte[] Unknown { get; set; }

        #endregion Properties

        #region Methods

        public override void ForceSetClutColors(ushort newNumOfColors) => throw new System.NotImplementedException();

        public override void ForceSetClutCount(ushort newClut) => throw new System.NotImplementedException();

        public override Color[] GetClutColors(ushort clut) => this._clut[(byte)clut];

        public override Texture2D GetTexture() => GetTexture(0);

        public override Texture2D GetTexture(Color[] colors)
        {
            Texture2D tex = new Texture2D(Memory.graphics.GraphicsDevice, _width, _height);
            TextureBuffer textureBuffer = new TextureBuffer(_width, _height, false);
            int i = 0;
            foreach (byte b in _buffer)
                textureBuffer[i++] = colors[b];
            textureBuffer.SetData(tex);
            return tex;
        }

        public override Texture2D GetTexture(ushort clut) => GetTexture(GetClutColors(clut));

        public override void Load(byte[] buffer, uint offset = 0)
        {
            if (buffer.Length - offset <= 4) return;
            _clut = new Cluts();
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(offset, SeekOrigin.Begin);//unknown
                Unknown = br.ReadBytes(4);
                foreach (int i in Enumerable.Range(0, 16))
                {
                    Color[] colors = Enumerable.Range(0, 16).Select(_ => ABGR1555toRGBA32bit(br.ReadUInt16()))
                        .ToArray();
                    _clut.Add((byte)i, colors);
                }

                long size = ms.Length - ms.Position;
                _height = checked((int)(size / 128));
                _width = checked((int)(size / _height));
                _buffer = br.ReadBytes(checked((int)size));
            }
        }

        public override void Save(string path) => throw new System.NotImplementedException();

        public override void SaveCLUT(string path) => _clut.Save(path);

        #endregion Methods
    }
}