using System;
using System.IO;

namespace OpenVIII
{
    public static class BinReaderExt
    {

        public static bool Read(this BinaryReader br, out uint val)
        {
            return br.Read(br.ReadUInt32, out val);
        }
        public static bool Read(this BinaryReader br, out int val)
        {
            return br.Read(br.ReadInt32, out val);
        }
        public static bool Read(this BinaryReader br, out ushort val)
        {
            return br.Read(br.ReadUInt16, out val);
        }
        public static bool Read(this BinaryReader br, out short val)
        {
            return br.Read(br.ReadInt16, out val);
        }
        public static bool Read(this BinaryReader br, out byte val)
        {
            return br.Read(br.ReadByte, out val);
        }
        public static bool Read(this BinaryReader br, out sbyte val)
        {
            return br.Read(br.ReadSByte, out val);
        }
        public static unsafe bool Read<T>(this BinaryReader br, Func<T> f, out T val) where T : unmanaged
        {
            if (br.BaseStream.Length - sizeof(T) > br.BaseStream.Position)
            {
                val = f.Invoke();
                return true;
            }

            val = default;
            return false;
        }
    }
}
