using System;
using FF8.Encoding.Tags;

namespace FF8.Encoding
{
    public sealed class FF8TextDecoder
    {
        private readonly FF8TextEncodingCodepage _codepage;

        public FF8TextDecoder(FF8TextEncodingCodepage codepage)
        {
            _codepage = codepage ?? throw new ArgumentNullException(nameof(codepage));
        }

        public Int32 GetMaxCharCount(Int32 byteCount)
        {
            return byteCount * FF8TextTag.MaxTagLength;
        }

        public Int32 GetCharCount(Byte[] bytes, Int32 index, Int32 count)
        {
            Int32 result = 0;

            Char[] buff = new Char[FF8TextTag.MaxTagLength];
            while (count > 0)
            {
                FF8TextTag tag = FF8TextTag.TryRead(bytes, ref index, ref count);
                if (tag != null)
                {
                    Int32 offset = 0;
                    result += tag.Write(buff, ref offset);
                }
                else
                {
                    count--;
                    result++;
                    index++;
                }
            }

            return result;
        }

        public Int32 GetChars(Byte[] bytes, Int32 byteIndex, Int32 byteCount, Char[] chars, Int32 charIndex)
        {
            Int32 result = 0;

            while (byteCount > 0)
            {
                FF8TextTag tag = FF8TextTag.TryRead(bytes, ref byteIndex, ref byteCount);
                if (tag != null)
                {
                    result += tag.Write(chars, ref charIndex);
                }
                else
                {
                    chars[charIndex++] = _codepage[bytes[byteIndex++]];
                    byteCount--;
                    result++;
                }
            }

            return result;
        }
    }
}