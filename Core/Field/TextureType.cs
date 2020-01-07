using System.Linq;

namespace OpenVIII
{
    public partial class Background
    {
        //private static int palettes = 24; // or 16;

        #region Fields

        private static _TextureType[] TextureTypes = new _TextureType[]
        {
            new _TextureType {
                Palettes =24,
                TexturePages = 13
            },
            new _TextureType {
                Palettes =16,
                TexturePages = 12,
                SkippedPalettes =0,
                Type = 2
            },
        };

        private _TextureType TextureType;

        #endregion Fields

        #region Classes

        private class _TextureType
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

            public int BytesSkippedPalettes => bytesPerPalette * SkippedPalettes;
            public uint FileSize => checked((uint)(PaletteSectionSize + (Width * THeight)));
            public uint PaletteSectionSize => checked((uint)(bytesPerPalette * Palettes));
            public ushort Width => checked((ushort)(TWidth * TexturePages));

            #endregion Properties

            #region Methods

            // first 8 are junk and are not used.
            public static _TextureType GetTextureType(byte[] mimb)
                => TextureTypes.First(x => x.FileSize == mimb.Length);

            #endregion Methods
        }

        #endregion Classes
    }
}