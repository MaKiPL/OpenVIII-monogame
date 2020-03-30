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

        private readonly Stream _s;

        #endregion Fields

        #region Constructors

        public StreamWithRangeValues(Stream s, long offset, long size, CompressionType compression = 0, int uncompressedSize = 0)
        {
            if (typeof(StreamWithRangeValues) == s.GetType())
            {
                var r = (StreamWithRangeValues)s;
                Debug.Assert(r.Compression == 0);
                offset += r.Offset;
            }
            _s = s;
            Size = size;
            Offset = offset;
            Position = offset;
            Compression = compression;
            UncompressedSize = checked((int)(uncompressedSize == 0 ? size : uncompressedSize));
        }

        #endregion Constructors

        #region Properties

        public override bool CanRead => _s.CanRead;
        public override bool CanSeek => _s.CanSeek;
        public override bool CanWrite => _s.CanWrite;
        public CompressionType Compression { get; }
        public override long Length => _s.Length;
        public long Max => Size + Offset;
        public long Offset { get; }
        public sealed override long Position { get => _s.Position; set => _s.Position = value; }
        public long Size { get; }
        public int UncompressedSize { get; }

        #endregion Properties

        #region Methods

        public override void Flush() => _s.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _s.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _s.Seek(offset, origin);

        public override void SetLength(long value) => _s.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => _s.Write(buffer, offset, count);

        #endregion Methods
    }
}