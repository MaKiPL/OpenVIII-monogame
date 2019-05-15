using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class LZSS
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
        #endregion

        

        private static readonly int N = 4096;
        private static readonly int F = 18;
        private static readonly int THRESHOLD = 2;
        private static readonly int EOF = -1;
        private static int[] text_buf = new int[N + F - 1];    /* ring buffer of size N, with extra F-1 bytes to facilitate string comparison */

        static MemoryStream infile;
        static List<byte> outfile;
        internal static byte[] DecompressAllNew(byte[] data)
        {
            infile = new MemoryStream(data);
            outfile = new List<byte>();
            Decode();
            infile.Close();
            return outfile.ToArray();
        }

        //Code borrowed from Java's implementation of LZSS by antiquechrono
        private static void Decode() 
        {
        int i, j, k, r, c;

        //unsigned int  flags;
        int flags;

        for (i = 0; i<N - F; i++) text_buf[i] = 0;
        r = N - F;  flags = 0;
        for ( ; ; ) {
            if (((flags >>= 1) & 256) == 0) {
                if ((c = infile.ReadByte()) == EOF) break;
                flags = c | 0xff00;		/* uses higher byte cleverly */
            }							/* to count eight */
//            if (flags & 1) {
            if ((flags & 1) == 1) {
                if ((c = infile.ReadByte()) == EOF) break;
                    outfile.Add((byte)c);
                text_buf[r++] = c;
                r &= (N - 1);
            } else {
                if ((i = infile.ReadByte()) == EOF) break;
                if ((j = infile.ReadByte()) == EOF) break;
                i |= ((j & 0xf0) << 4);  j = (j & 0x0f) + THRESHOLD;
                for (k = 0; k <= j; k++) {
                    c = text_buf[(i + k) & (N - 1)];
                        outfile.Add((byte)c);
                        text_buf[r++] = c;
                    r &= (N - 1);
                }
            }
        }
    }
        /// <summary>
        /// Decompiles LZSS. You have to know the OutputSize. [DEPRECATED]
        /// </summary>
        /// <param name="data">buffer</param>
        /// <param name="fileSize">Original filesize of compressed file</param>
        /// <param name="size">Filesize of final file</param>
        /// <returns>Byte array</returns>
        [Obsolete("This method proved to be broken. Please use DecompressAllNew")]
        internal static byte[] DecompressAll(byte[] data, uint fileSize, int size = 0)
        {
            try
            {
                bool bDynamic = false;
                if (size == 0)
                {
                    size = 0x4000000; //64MB
                    bDynamic = true;
                }

                var result = new byte[size];

                int curResult = 0;
                int curBuff = 4078, flagByte = 0;
                int fileData = 4,
                    endFileData = (int)fileSize;


                var textBuf = new byte[4113];

                while (true)
                {
                    if (fileData + 1 >= endFileData) return !bDynamic ? result : ReturnDynamic(result, curResult);
                    if (((flagByte >>= 1) & 256) == 0)
                        flagByte = data[fileData++] | 0xff00;

                    if (fileData >= endFileData)
                        return !bDynamic ? result : ReturnDynamic(result, curResult);

                    if ((flagByte & 1) > 0)
                    {
                        result[curResult] = textBuf[curBuff] = data[fileData++];

                        curBuff = (curBuff + 1) & 4095;
                        ++curResult;
                    }
                    else
                    {
                        if (fileData + 1 >= endFileData)
                            return !bDynamic ? result : ReturnDynamic(result, curResult);
                        int offset = (byte)BitConverter.ToChar(data, fileData++);
                        if (fileData + 1 >= endFileData)
                            return !bDynamic ? result : ReturnDynamic(result, curResult);
                        int length = (byte)BitConverter.ToChar(data, fileData++);
                        offset |= (length & 0xF0) << 4;
                        length = (length & 0xF) + 2 + offset;

                        int e;
                        for (e = offset; e <= length; e++)
                        {
                            textBuf[curBuff] = result[curResult] = textBuf[e & 4095];
                            curBuff = (curBuff + 1) & 4095;
                            ++curResult;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static byte[] ReturnDynamic(byte[] result, int curResult)
        {
            byte[] buffer = new byte[curResult];
            Array.Copy(result, buffer, buffer.Length);
            return buffer;
        }

    }
}