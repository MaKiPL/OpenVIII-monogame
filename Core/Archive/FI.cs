using System.Runtime.InteropServices;

namespace OpenVIII
{
    [StructLayout(LayoutKind.Explicit, Size = 12, Pack = 1)]
    public class FI
    {
        //changed to int because libraries require casting to int anyway.
        [FieldOffset(0)]
        public int UncompressedSize;

        [FieldOffset(4)]
        public int Offset;

        [FieldOffset(8)]
        public CompressionType CompressionType;
        public FI()
        { }
        public FI(int offset, int uncompressedSize, CompressionType compressionType = 0)
        {
            UncompressedSize = uncompressedSize;
            Offset = offset;
            CompressionType = compressionType;
        }

        public override string ToString() => $"{{{UncompressedSize}, {Offset}, {CompressionType}}}";
        
    }
    public enum CompressionType
    {
        None = 0,
        LZSS = 1,
        LZ4 = 2,
    }
}