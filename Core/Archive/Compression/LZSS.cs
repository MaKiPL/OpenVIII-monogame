using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace OpenVIII
{
    /// <summary>
    /// <para>NEW LZSS</para>
    /// <para>LZSS.C -- A Data Compression Program</para>
    /// <para>(tab = 4 spaces)</para>
    /// <para>4/6/1989 Haruhiko Okumura</para>
    /// <para>Use, distribute, and modify this program freely.</para>
    /// <para>Please send me your improved versions.</para>
    /// <para>PC-VAN SCIENCE</para>
    /// <para>NIFTY-Serve PAF01022</para>
    /// <para>CompuServe  74050,1022</para>
    /// </summary>
    public class LZSS
    {
        #region Fields

        private const int EOF = -1;
        private const int F = 18;
        private const int N = 4096;
        private const int THRESHOLD = 2;

        #endregion Fields

        #region Methods

        public static byte[] DecompressAllNew(byte[] data, int uncompressedSize, bool skip = false)
        {
            if (uncompressedSize < 0) throw new ArgumentOutOfRangeException(nameof(uncompressedSize)); // if 0 ignore checks.
            //Memory.Log.WriteLine($"{nameof(LZSS)}::{nameof(DecompressAllNew)} :: decompressing data");
            byte[] outFileArray;
            using (MemoryStream infile = new MemoryStream(!skip ? data : data.Skip(4).ToArray()))
            {
                Decode(infile, out outFileArray);
            }
            if (uncompressedSize > 0 && outFileArray.Length != uncompressedSize)
                throw new InvalidDataException($"{nameof(LZSS)}::{nameof(DecompressAllNew)} Expected size ({uncompressedSize}) != ({outFileArray.Length})");
            return outFileArray;
        }

        //Code borrowed from Java's implementation of LZSS by antiquechrono
        private static void Decode(Stream infile, out byte[] outFileArray)
        {
            List<byte> outfile = new List<byte>();

            int[] textBuf = new int[N + F - 1];    // ring buffer of size N, with extra F-1 bytes to facilitate string comparison

            int r = N - F; int flags = 0;
            for (; ; )
            {
                int c;
                if (((flags >>= 1) & 256) == 0)
                {
                    if ((c = infile.ReadByte()) == EOF) break;
                    flags = c | 0xff00;     // uses higher byte cleverly
                }                           // to Count eight
                if ((flags & 1) == 1)
                {
                    if ((c = infile.ReadByte()) == EOF) break;
                    outfile.Add((byte)c);
                    textBuf[r++] = c;
                    r &= (N - 1);
                }
                else
                {
                    int i;
                    if ((i = infile.ReadByte()) == EOF) break;
                    int j;
                    if ((j = infile.ReadByte()) == EOF) break;
                    i |= ((j & 0xf0) << 4); j = (j & 0x0f) + THRESHOLD;
                    int k;
                    for (k = 0; k <= j; k++)
                    {
                        c = textBuf[(i + k) & (N - 1)];
                        outfile.Add((byte)c);
                        textBuf[r++] = c;
                        r &= (N - 1);
                    }
                }
            }
            outFileArray = outfile.ToArray();
        }

        #endregion Methods
    }
}