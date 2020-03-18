using System.Collections.Generic;
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
            private byte _skippedPalettes = 8;
            private byte _texturePages;
            public byte Type = 1;
            private const ushort OutHeight = 256;
            private const ushort OutWidth = 128;

            #endregion Fields

            #region Properties

            public int BytesSkippedPalettes => BytesPerPalette * _skippedPalettes;
            private uint FileSize => checked((uint)(PaletteSectionSize + (Width * OutHeight)));
            public uint PaletteSectionSize => checked((uint)(BytesPerPalette * Palettes));
            public ushort Width => checked((ushort)(OutWidth * _texturePages));

            #endregion Properties

            #region Methods

            // first 8 are junk and are not used.
            public static BackgroundTextureType GetTextureType(IReadOnlyCollection<byte> mim)
                => mim == null ? default: TextureTypes.First(x => x.FileSize == mim.Count);


            private static readonly BackgroundTextureType[] TextureTypes = {
            new BackgroundTextureType {
                Palettes =24,
                _texturePages = 13
            },
            new BackgroundTextureType {
                Palettes =16,
                _texturePages = 12,
                _skippedPalettes =0,
                Type = 2
            },
            };

            #endregion Methods
        }

        #endregion Classes
    }
}