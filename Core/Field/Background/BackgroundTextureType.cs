using System.Linq;

namespace OpenVIII.Fields
{
    public partial class Background
    {

        #region Classes

        private class BackgroundTextureType
        {
            #region Fields

            public byte Palettes;
            public byte SkippedPalettes = 8;
            public byte TexturePages;
            public byte Type = 1;
            private const ushort THeight = 256;
            private const ushort TWidth = 128;

            #endregion Fields

            #region Properties

            public int BytesSkippedPalettes => BytesPerPalette * SkippedPalettes;
            public uint FileSize => checked((uint)(PaletteSectionSize + (Width * THeight)));
            public uint PaletteSectionSize => checked((uint)(BytesPerPalette * Palettes));
            public ushort Width => checked((ushort)(TWidth * TexturePages));

            #endregion Properties

            #region Methods

            // first 8 are junk and are not used.
            public static BackgroundTextureType GetTextureType(byte[] mimb)
                => mimb == null ? default: TextureTypes.First(x => x.FileSize == mimb.Length);


            private static BackgroundTextureType[] TextureTypes = new BackgroundTextureType[]
            {
            new BackgroundTextureType {
                Palettes =24,
                TexturePages = 13
            },
            new BackgroundTextureType {
                Palettes =16,
                TexturePages = 12,
                SkippedPalettes =0,
                Type = 2
            },
            };

            #endregion Methods
        }

        #endregion Classes
    }
}