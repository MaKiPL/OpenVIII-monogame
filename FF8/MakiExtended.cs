using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    static class MakiExtended
    {
        //https://stackoverflow.com/a/2887/4509036
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

#if DEBUG
        public static void DumpBuffer(byte[] buffer, string path)
            => System.IO.File.WriteAllBytes(path, buffer);

        public static void DumpBuffer(System.IO.MemoryStream ms, string path)
            => System.IO.File.WriteAllBytes(path, ms.GetBuffer());

        public static void DumpBuffer(System.IO.MemoryStream ms)
            => System.IO.File.WriteAllBytes(GetUnixFullPath(System.IO.Path.Combine(Memory.FF8DIR, "debugUnpack.debug")), ms.GetBuffer());
#endif

        //https://stackoverflow.com/questions/1130698/checking-if-an-object-is-a-number-in-c-sharp
        public static bool IsNumber(object value) => value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;

        public static double Distance3D(Vector3 xo, Vector3 xa) => Vector3.Distance(xo, xa);

        public static string GetUnixFullPath(string pt)
        {
#if _WINDOWS
            return System.IO.Path.GetFullPath(pt.Replace('/', '\\'));
#else
            return System.IO.Path.GetFullPath(pt.Replace("\\", "/"));
#endif
        }

        public static bool In(int _in, Vector2 range) => /*IsNumber(_in) ?*/
                _in >= range.X && _in <= range.Y;
        //: false;
        public static bool In(int _in, int min, int max) => In(_in, new Vector2(min, max));

        public static ushort UshortLittleEndian(ushort ushort_)
    => (ushort)((ushort_ << 8) | (ushort_ >> 8));

        public static short ShortLittleEndian(short ushort_)
            => (short)((ushort_ << 8) | (ushort_ >> 8));

        public static uint UintLittleEndian(uint uint_)
            => (uint_ << 24) | ((uint_ << 8) & 0x00FF0000) |
            ((uint_ >> 8) & 0x0000FF00) | (uint_ >> 24);

        public static int UintLittleEndian(int uint_)
            => (uint_ << 24) | ((uint_ << 8) & 0x00FF0000) |
            ((uint_ >> 8) & 0x0000FF00) | (uint_ >> 24);

        public static int ClampOverload(int a, int min, int max)
            => a < min ? max - Math.Abs(a) : a > max ? a - max : a;

        public static float ClampOverload(float a, float min, float max)
            => a < min ? max - Math.Abs(a) : a > max ? a - max : a;
    }
}
