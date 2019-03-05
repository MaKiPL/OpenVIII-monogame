using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class MakiExtended
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
            return System.IO.Path.GetFullPath(pt.Replace('/'. '\\');
#else
            return System.IO.Path.GetFullPath(pt.Replace("\\", "/"));
#endif
        }

        public static bool In(int _in, Vector2 range) => /*IsNumber(_in) ?*/
                _in >= range.X && _in <= range.Y;
                //: false;
        public static bool In(int _in, int min, int max) => In(_in, new Vector2(min, max));
    }
}
