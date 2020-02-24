using System.Diagnostics;
using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// Starts at offset position and has a offset and size value so you know how far you can go.
    /// </summary>
    public class StreamWithRangeValues : Stream
    {
        #region Fields

        private Stream s;

        #endregion Fields

        #region Constructors

        public StreamWithRangeValues(Stream s, long offset, long size, CompressionType compression = 0, int uncompressedsize = 0)
        {
            if (typeof(StreamWithRangeValues) == s.GetType())
            {
                StreamWithRangeValues swrv = (StreamWithRangeValues)s;
                Debug.Assert(swrv.Compression == 0);
                offset += swrv.Offset;
            }
            this.s = s;
            Size = size;
            Offset = offset;
            Position = offset;
            Compression = compression;
            UncompressedSize = checked((int)(uncompressedsize == 0 ? size : uncompressedsize));
        }

        #endregion Constructors

        #region Properties

        public override bool CanRead => s.CanRead;
        public override bool CanSeek => s.CanSeek;
        public override bool CanWrite => s.CanWrite;
        public CompressionType Compression { get; }
        public override long Length => s.Length;
        public long Max => Size + Offset;
        public long Offset { get; }
        public override long Position { get => s.Position; set => s.Position = value; }
        public long Size { get; }
        public int UncompressedSize { get; }

        #endregion Properties

        #region Methods

        public override void Flush() => s.Flush();

        public override int Read(byte[] buffer, int offset, int count) => s.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => s.Seek(offset, origin);

        public override void SetLength(long value) => s.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => s.Write(buffer, offset, count);

        #endregion Methods
    }
}