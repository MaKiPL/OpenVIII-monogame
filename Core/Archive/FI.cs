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
        public uint CompressionType;
        public FI()
        { }
        public FI(int offset, int uncompressedSize, uint compressionType = 0)
        {
            UncompressedSize = uncompressedSize;
            Offset = offset;
            CompressionType = compressionType;
        }

        public override string ToString() => $"{{{UncompressedSize}, {Offset}, {CompressionType}}}";
    }
}