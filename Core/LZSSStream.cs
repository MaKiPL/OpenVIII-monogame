using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Attempt to make the LZSS into a stream that we can read from on the fly...
    /// putting on back burner as I feel like it's taking too much time. This class does not work. So discard or ignore.
    /// </summary>
    public class LZSSStream : Stream
    {

        #region License

        /**************************************************************
        LZSS.C -- A Data Compression Program
        (tab = 4 spaces)
        ***************************************************************
        4/6/1989 Haruhiko Okumura
        Use, distribute, and modify this program freely.
        Please send me your improved versions.
        PC-VAN		SCIENCE
        NIFTY-Serve	PAF01022
        CompuServe	74050,1022
        **************************************************************/

        #endregion License

        private static readonly int N = 4096;
        private static readonly int F = 18;
        private static readonly int THRESHOLD = 2;
        private static readonly int EOF = -1;
        private long _length;
        private List<byte> outfile;

        public LZSSStream(Stream src, long infilepos, long position) {
            infile = src;
            _infilepos = infilepos;
            Position = position;
            //_length = length;
            outfile = new List<byte>();
            infile.Seek(infilepos, SeekOrigin.Begin);
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length { get => _length; }

        private Stream infile;
        private long _infilepos;

        public override long Position { get; set; }

        ///<remarks>Code borrowed from Java's implementation of LZSS by antiquechrono</remarks>
        private byte[] Decode(int offset, int count)
        {
            int i, j, k, r, c;
            
            int[] text_buf = new int[N + F - 1];    /* ring buffer of size N, with extra F-1 bytes to facilitate string comparison */
                                                    //unsigned int  flags;
            int flags;

            for (i = 0; i < N - F; i++) text_buf[i] = 0;
            r = N - F; flags = 0;
            for (; ; )
            {
                if (((flags >>= 1) & 256) == 0)
                {
                    if ((c = infile.ReadByte()) == EOF) break;
                    flags = c | 0xff00;     /* uses higher byte cleverly */
                }                           /* to count eight */
                                            // if (flags & 1) {
                if ((flags & 1) == 1)
                {
                    if ((c = infile.ReadByte()) == EOF) break;
                    outfile.Add((byte)c);
                    text_buf[r++] = c;
                    r &= (N - 1);
                }
                else
                {
                    if ((i = infile.ReadByte()) == EOF) break;
                    if ((j = infile.ReadByte()) == EOF) break;
                    i |= ((j & 0xf0) << 4); j = (j & 0x0f) + THRESHOLD;
                    for (k = 0; k <= j; k++)
                    {
                        c = text_buf[(i + k) & (N - 1)];
                        outfile.Add((byte)c);
                        text_buf[r++] = c;
                        r &= (N - 1);
                    }
                }
            }
            try
            {
                return outfile.Skip(offset).Take(count).ToArray();
            }
            finally
            {
                //outfile.RemoveRange(0,offset + count);
            }
        }

        public override void Flush() => throw new NotImplementedException();
        public override int Read(byte[] buffer, int offset, int count)
        {
            if(CanRead)
            {
                var decbuffer = Decode(offset, count);
                for (int a = 0; a < buffer.Length && a<count && a < decbuffer.Length; a++)
                {
                    buffer[a] = decbuffer[a];
                }
                return Math.Min(Math.Min(count, buffer.Length), decbuffer.Length);
            }
            return 0;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            if(CanSeek)
            {
                switch(origin)
                {
                    case SeekOrigin.Begin:
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        Position += offset;
                        break;
                    case SeekOrigin.End:
                        Position = _length + offset;
                        break;
                }
            }
            return Position;
        }

        public override void SetLength(long value) => _length = value;
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }
}