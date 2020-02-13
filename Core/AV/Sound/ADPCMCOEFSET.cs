using System.Runtime.InteropServices;

namespace OpenVIII.AV
{
    public static partial class Sound
    {
        #region Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct ADPCMCOEFSET
        {
            public short iCoef1;
            public short iCoef2;
        };

        #endregion Structs
    }
}