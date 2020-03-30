using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public sealed partial class ArchiveZzz
    {
        #region Classes

        public static class FileData
        {
            #region Methods

            /// <summary>
            /// Convert the FileData for ZZZ to a String and FI pair
            /// </summary>
            /// <param name="br">Binary reader with raw data.</param>
            /// <returns>String and FI pair</returns>
            public static KeyValuePair<string, FI> Load(BinaryReader br)
            {
                var filenameLength = br.ReadInt32();
                var filenameBytes = br.ReadBytes(filenameLength);
                return new KeyValuePair<string, FI>(ConvertFilename(filenameBytes), new FI(checked((int)br.ReadInt64()), checked((int)br.ReadUInt32())));
            }

            /// <summary>
            /// Decode/Encode the filename string as bytes.
            /// </summary>
            /// <remarks>
            /// Could be Ascii or UTF8, I see no special characters and the first like 127 of UTF8 is the
            /// same as Ascii.
            /// </remarks>
            private static string ConvertFilename(byte[] filenameBytes) => System.Text.Encoding.UTF8.GetString(filenameBytes);

            #endregion Methods
        }

        #endregion Classes
    }
}