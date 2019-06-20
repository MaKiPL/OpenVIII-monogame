using System;

namespace OpenVIII.Encoding
{
    public sealed class FF8TextEncoding : System.Text.Encoding
    {
        private readonly FF8TextEncoder _encoder;
        private readonly FF8TextDecoder _decoder;

        private static readonly Object _lock = new Object();
        private static volatile FF8TextEncoding _instance;

        public new static FF8TextEncoding Default
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_lock)
                {
                    if (_instance != null)
                        return _instance;

                    _instance = new FF8TextEncoding(FF8TextEncodingCodepage.Create());
                }

                return _instance;
            }
        }

        public FF8TextEncoding(FF8TextEncodingCodepage codepage)
        {
            _encoder = new FF8TextEncoder(codepage);
            _decoder = new FF8TextDecoder(codepage);
        }

        public override Int32 GetByteCount(Char[] chars, Int32 index, Int32 count)
        {
            return _encoder.GetByteCount(chars, index, count);
        }

        public override Int32 GetBytes(Char[] chars, Int32 charIndex, Int32 charCount, Byte[] bytes, Int32 byteIndex)
        {
            return _encoder.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        public override Int32 GetCharCount(Byte[] bytes, Int32 index, Int32 count)
        {
            return _decoder.GetCharCount(bytes, index, count);
        }

        public override Int32 GetChars(Byte[] bytes, Int32 byteIndex, Int32 byteCount, Char[] chars, Int32 charIndex)
        {
            return _decoder.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override Int32 GetMaxByteCount(Int32 charCount)
        {
            return _encoder.GetMaxByteCount(charCount);
        }

        public override Int32 GetMaxCharCount(Int32 byteCount)
        {
            return _decoder.GetMaxCharCount(byteCount);
        }
    }
}