using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.AV
{
    public static partial class Sound
    {
        #region Classes

        /// <summary>
        /// Entry in FMT file
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_FMT"/>
        public class Entry
        {
            #region Fields

            /// <summary>
            /// Header Data to be prepended to sound data for ffmpeg or saving to drive.
            /// </summary>
            public readonly byte[] HeaderData;

            /// <summary>
            /// Offset of wav file in the dat.
            /// </summary>
            public readonly uint Offset;

            /// <summary>
            /// Size of wav file in the dat.
            /// </summary>
            public readonly uint Size;

            /// <summary>
            /// Size the header data in the FMT file. Rest has to be appended via Fill Header.
            /// </summary>
            private const uint OutputHeaderSize = 50;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Generate Entry
            /// </summary>
            /// <param name="br">Source of data</param>
            /// <param name="size">size entry, this is checked for 0 before loading.</param>
            private Entry(BinaryReader br, uint size)
            {
                Size = size;
                Offset = br.ReadUInt32();
                //Unknown. If first byte is 0, the other 11 are always 0. Probably some kind of looping metadata.
                br.BaseStream.Seek(12, SeekOrigin.Current);
                HeaderData = FillHeader(br);
            }

            #endregion Constructors

            #region Properties

            private uint OutputDataSize => Size;

            /// <summary>
            /// Total size including the header
            /// </summary>
            private uint OutputTotalSize => Size + 70;

            #endregion Properties

            #region Methods

            public static implicit operator BufferData(Entry value) => new BufferData
            {
                DataSeekLoc = value.Offset,
                DataSize = value.Size,
                HeaderSize = checked((uint)value.HeaderData.Length),
                Target = BufferData.TargetFile.SoundDat
            };

            public static IReadOnlyList<Entry> Read(Stream s)
            {
                using (var br = new BinaryReader(s))
                {
                    //The FMT is headed by 4 bytes representing the number of sound file headers
                    //in the rest of the FMT file, then by a 36 byte header that isn't interesting.
                    var count = br.ReadInt32();
                    s.Seek(36, SeekOrigin.Current);
                    return Enumerable.Range(0, count).Select(_ => CreateInstance(br)).Where(x => x != null).ToList()
                        .AsReadOnly();
                }
            }

            private static Entry CreateInstance(BinaryReader br)
            {
                var size = br.ReadUInt32();
                if (size != 0) return new Entry(br, size);
                br.BaseStream.Seek(34, SeekOrigin.Current);
                return null;
            }

            private byte[] FillHeader(BinaryReader br) => FillHeader(br.ReadBytes(checked((int)OutputHeaderSize)));

            private byte[] FillHeader(byte[] headerBytes)
            {
                var headerData = new byte[OutputHeaderSize + 28];
                using (var bw = new BinaryWriter(new MemoryStream(headerData)))
                {
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                    bw.Write(OutputTotalSize);
                    // ReSharper disable once StringLiteralTypo
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                    bw.Write(OutputHeaderSize);
                    bw.Write(headerBytes);
                    bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                    bw.Write(OutputDataSize);
                }
                return headerData;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}