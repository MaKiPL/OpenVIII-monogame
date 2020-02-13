using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class LZSS
    {
        //NEW LZSS

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

        public static byte[] DecompressAllNew(byte[] data, bool skip = false)
        {
            //Memory.Log.WriteLine($"{nameof(LZSS)}::{nameof(DecompressAllNew)} :: decompressing data");
            byte[] outfilearray; 
            using (MemoryStream infile = new MemoryStream(!skip?data:data.Skip(4).ToArray()))
            {
                Decode(infile, out outfilearray);
            }
            return outfilearray;
        }

        //Code borrowed from Java's implementation of LZSS by antiquechrono
        private static void Decode(MemoryStream infile, out byte[] outfilearray)
        {
            int i, j, k, r, c;

            List<byte> outfile = new List<byte>();

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
            outfilearray = outfile.ToArray();
        }
    }
}