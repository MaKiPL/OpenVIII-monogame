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
    public class PMP : Texture_Base
    {
        #region Fields

        private byte[] buffer;
        private Cluts clut;
        private int height;

        private int width;

        #endregion Fields

        #region Constructors

        public PMP(byte[] pmpb) => Load(pmpb, 0);

        #endregion Constructors

        #region Properties

        public override byte GetBytesPerPixel => 4;

        public override int GetClutCount => 16;

        public override int GetClutSize => 16;

        public override int GetColorsCountPerPalette => 16;

        public override int GetHeight => height;

        public override int GetOrigX => 0;

        public override int GetOrigY => 0;

        public override int GetWidth => width;

        public byte[] Unknown { get;  }

        #endregion Properties

        #region Methods

        public override void ForceSetClutColors(ushort newNumOfColors) => throw new System.NotImplementedException();

        public override void ForceSetClutCount(ushort newClut) => throw new System.NotImplementedException();

        public override Color[] GetClutColors(ushort clut) => this.clut[(byte)clut];

        public override Texture2D GetTexture() => GetTexture(0);

        public override Texture2D GetTexture(Color[] colors)
        {
            Texture2D tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            TextureBuffer texbuff = new TextureBuffer(width, height, false);
            int i = 0;
            foreach (byte b in buffer)
                texbuff[i++] = colors[b];
            texbuff.SetData(tex);
            return tex;
        }

        public override Texture2D GetTexture(ushort clut) => GetTexture(GetClutColors(clut));

        public override void Load(byte[] buffer, uint offset = 0)
        {
            if (buffer.Length - offset <= 4) return;
            clut = new Cluts();
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                ms.Seek(offset, SeekOrigin.Begin);//unknown
                Unknown = br.ReadBytes(4);
                foreach (byte i in Enumerable.Range(0, 16))
                {
                    Color[] colors = new Color[16];
                    foreach (byte c in Enumerable.Range(0, 16))
                        colors[c] = ABGR1555toRGBA32bit(br.ReadUInt16());
                    clut.Add(i, colors);
                }

                long size = ms.Length - ms.Position;
                height = checked((int)(size / 128));
                width = checked((int)(size / height));
                this.buffer = br.ReadBytes(checked((int)size));
            }
        }

        public override void Save(string path) => throw new System.NotImplementedException();

        public override void SaveCLUT(string path) => clut.Save(path);

        #endregion Methods
    }
}