using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// TIM data for overture textures.
    /// </summary>
    /// <remarks>
    /// The reason for this is the lack of header, And allows compatibility with other objects that
    /// use TIMs.
    /// </remarks>
    public sealed class TIMOverture : TIM2
    {
        #region Constructors

        public TIMOverture(byte[] buffer, uint offset = 0) => _Init(buffer, offset);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public TIMOverture(BinaryReader br, uint offset = 0) => _Init(br, offset);

        #endregion Constructors

        #region Methods

        private new void _Init(byte[] buffer, uint offset)
        {
            this.Buffer = buffer;
            using (var br = new BinaryReader(new MemoryStream(buffer)))
            {
                Init(br, offset);
            }
        }

        private new void _Init(BinaryReader br, uint offset)
        {
            TrimExcess = true;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            Buffer = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
            using (var br2 = new BinaryReader(new MemoryStream(Buffer)))
                Init(br2, 0);
        }

        private new void Init(BinaryReader br, uint offset)
        {
            BPP = 16;
            TIMOffset = offset;
            ReadParameters(br);
        }

        private new void ReadParameters(BinaryReader br)
        {
            br.BaseStream.Seek(TIMOffset, SeekOrigin.Begin);
            Texture.ImageOrgX = br.ReadUInt16();
            Texture.ImageOrgY = br.ReadUInt16();
            Texture.Width = br.ReadUInt16();
            Texture.Height = br.ReadUInt16();
            Texture.ImageSize = (uint)(br.BaseStream.Length - TIMOffset);
            Texture.ImageDataSize = (int)(br.BaseStream.Length - br.BaseStream.Position);
            TextureDataPointer = (uint)br.BaseStream.Position;
            br.BaseStream.Seek(TIMOffset, SeekOrigin.Begin);
            if (TrimExcess)
                Buffer = Buffer.Skip((int)TIMOffset).Take((int)(Texture.ImageDataSize + TextureDataPointer - TIMOffset)).ToArray();
        }

        #endregion Methods

        ///// <summary>
        ///// Splash is 640x400 16BPP typical TIM with palette of ggg bbbbb a rrrrr gg
        ///// </summary>
        ///// <param name="buffer">raw 16bpp image</param>
        ///// <returns>Texture2D</returns>
        ///// <remarks>
        ///// These files are just the image data with no header and no clut data. Tim class doesn't
        ///// handle this.
        ///// </remarks>
        //public static Texture2D Overture(byte[] buffer)
        //{
        //    using (MemoryStream ms = new MemoryStream(buffer))
        //    using (BinaryReader br = new BinaryReader(ms))
        //    {
        //        //var ImageOrgX = BitConverter.ToUInt16(buffer, 0x00);
        //        //var ImageOrgY = BitConverter.ToUInt16(buffer, 0x02);

        //        ms.Seek(0x04, SeekOrigin.Begin);
        //        ushort Width = br.ReadUInt16();
        //        ushort Height = br.ReadUInt16();
        //        Texture2D splashTex = new Texture2D(Memory.graphics.GraphicsDevice, Width, Height, false, SurfaceFormat.Color);
        //        lock (splashTex)
        //        {
        //            Color[] rgbBuffer = new Color[Width * Height];
        //            for (int i = 0; i < rgbBuffer.Length && ms.Position + 2 < ms.Length; i++)
        //            {
        //                rgbBuffer[i] = ABGR1555toRGBA32bit(br.ReadUInt16(), true);
        //            }
        //            splashTex.SetData(rgbBuffer);
        //        }
        //        return splashTex;
        //    }
        //}
    }
}