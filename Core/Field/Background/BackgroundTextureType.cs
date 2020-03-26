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

            public readonly byte Palettes;
            /// <remarks> first 8 are junk and are not used. </remarks>
            private readonly byte _skippedPalettes;
            private readonly byte _texturePages;
            public readonly byte Type;
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

            public static BackgroundTextureType GetTextureType(IReadOnlyCollection<byte> mimBytes)
                => mimBytes == null ? default: TextureTypes.First(x => x.FileSize == mimBytes.Count);

            private BackgroundTextureType(byte palettes, byte texturePages, byte skippedPalettes = 8, byte type = 1) =>
                (Palettes, _texturePages, _skippedPalettes, Type) = (palettes, texturePages, skippedPalettes, type);
            
            private static readonly BackgroundTextureType[] TextureTypes = {
            new BackgroundTextureType(24, 13),
            new BackgroundTextureType(16,12,0,2)};
            #endregion Methods
        }

        #endregion Classes
    }
}
