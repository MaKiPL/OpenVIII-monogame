using System;

namespace OpenVIII.Encoding
{
    public sealed class FF8TextEncoding : System.Text.Encoding
    {
        private readonly FF8TextEncoder _encoder;
        private readonly FF8TextDecoder _decoder;

        private static readonly object _lock = new object();
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

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return _encoder.GetByteCount(chars, index, count);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return _encoder.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return _decoder.GetCharCount(bytes, index, count);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return _decoder.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount)
        {
            return _encoder.GetMaxByteCount(charCount);
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return _decoder.GetMaxCharCount(byteCount);
        }
    }
}