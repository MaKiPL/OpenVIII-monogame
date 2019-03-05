using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class LZSS
    {
        /// <summary>
        /// Decompiles LZSS. You have to know the OutputSize.
        /// </summary>
        /// <param name="data">buffer</param>
        /// <param name="fileSize">Original filesize of compressed file</param>
        /// <param name="size">Filesize of final file</param>
        /// <returns>Byte array</returns>
        public static byte[] DecompressAll(byte[] data, uint fileSize, int size = 0)
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
                    if(fileData +1 >= endFileData) return !bDynamic ? result : ReturnDynamic(result, curResult);
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