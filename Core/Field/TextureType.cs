namespace OpenVIII
{
    public static partial class Module_field_debug
    {
        //private static int palettes = 24; // or 16;

        #region Classes

        private class TextureType
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

            // first 8 are junk and are not used.
        }

        #endregion Classes
    }
}