using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class ArchiveZZZ
    {
        public static class FileData
        {
            //public string Filename { get; set; }
            //public long Offset { get; set; }
            //public uint Size { get; set; }

            /// <summary>
            /// Decode/Encode the filename string as bytes.
            /// </summary>
            /// <remarks>
            /// Could be Ascii or UTF8, I see no special characters and the first like 127 of UTF8 is the
            /// same as Ascii.
            /// </remarks>
            private static string ConvertFilename(byte[] filenamebytes) => System.Text.Encoding.UTF8.GetString(filenamebytes);

            public static KeyValuePair<string,FI> Load(BinaryReader br)
            {
                int FilenameLength = br.ReadInt32();
                byte[] Filenamebytes = br.ReadBytes(FilenameLength);
                return new KeyValuePair<string, FI>(ConvertFilename(Filenamebytes), new FI(checked((int)br.ReadInt64()), checked((int)br.ReadUInt32())));
            }
        }
    }
}