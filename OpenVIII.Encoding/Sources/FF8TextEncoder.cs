using System;
using FF8.Encoding.Tags;

namespace FF8.Encoding
{
    public sealed class FF8TextEncoder
    {
        private readonly FF8TextEncodingCodepage _codepage;

        public FF8TextEncoder(FF8TextEncodingCodepage codepage)
        {
            _codepage = codepage ?? throw new ArgumentNullException(nameof(codepage));
        }

        public Int32 GetMaxByteCount(Int32 charCount)
        {
            return charCount;
        }

        public Int32 GetByteCount(Char[] chars, Int32 index, Int32 count)
        {
            Int32 result = 0;

            Byte[] buff = new Byte[2];
            while (count > 0)
            {
                FF8TextTag tag = FF8TextTag.TryRead(chars, ref index, ref count);
                if (tag != null)
                {
                    Int32 offset = 0;
                    result += tag.Write(buff, ref offset);
                }
                else if (FF8TextComment.TryRead(chars, ref index, ref count) == null)
                {
                    count--;
                    result++;
                    index++;
                }
            }

            return result;
        }

        public Int32 GetBytes(Char[] chars, Int32 charIndex, Int32 charCount, Byte[] bytes, Int32 byteIndex)
        {
            Int32 result = 0;

            while (charCount > 0)
            {
                FF8TextTag tag = FF8TextTag.TryRead(chars, ref charIndex, ref charCount);
                if (tag != null)
                {
                    result += tag.Write(bytes, ref byteIndex);
                }
                else if (FF8TextComment.TryRead(chars, ref charIndex, ref charCount) == null)
                {
                    bytes[byteIndex++] = _codepage[chars[charIndex++]];
                    charCount--;
                    result++;
                }
            }

            return result;
        }
    }
}