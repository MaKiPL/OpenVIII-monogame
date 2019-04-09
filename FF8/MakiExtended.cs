using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    //Class that provides language extensions made by Maki
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

#if DEBUG || _WINDOWS
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
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
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

        //I don't know why I'm getting different results when using MonoGame's rotation matrix. Like it calculates the values or what?
        //Tried googling why, but no results. Every points to the method I'm implementing here. If you know the answer, please tell me- Maki
        public static Matrix GetRotationMatrixX(float angle)
        => new Matrix(
            1, 0, 0, 0,
            0, (float)Math.Cos(Radians(angle)), -(float)Math.Sin(Radians(angle)), 0,
            0, (float)Math.Sin(Radians(angle)), (float)Math.Cos(Radians(angle)), 0,
            0, 0, 0, 0);

        public static Matrix GetRotationMatrixY(double angle)
        => new Matrix(
            (float)Math.Cos(Radians(angle)), 0, (float)Math.Sin(Radians(angle)), 0,
            0, 1, 0, 0,
            -(float)Math.Sin(Radians(angle)), 0, (float)Math.Cos(Radians(angle)), 0,
            0, 0, 0, 0);

        public static Matrix GetRotationMatrixZ(float angle)
        => new Matrix(
            (float)Math.Cos(Radians(angle)), -(float)Math.Sin(Radians(angle)), 0, 0,
            (float)Math.Sin(Radians(angle)), (float)Math.Cos(Radians(angle)), 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 0);

        public static Matrix MatrixMultiply(Matrix a, Matrix b)
            => new Matrix(
                b.M11 * a.M11 + b.M21 * a.M12 + b.M31 * a.M13, b.M11 * a.M21 + b.M21 * a.M22 + b.M31 * a.M23, b.M11 * a.M31 + b.M21 * a.M32 + b.M31 * a.M33, 0,
                b.M12 * a.M11 + b.M22 * a.M12 + b.M32 * a.M13, b.M12 * a.M21 + b.M22 * a.M22 + b.M32 * a.M23, b.M12 * a.M31 + b.M22 * a.M32 + b.M32 * a.M33, 0,
                b.M13 * a.M11 + b.M23 * a.M12 + b.M33 * a.M13, b.M13 * a.M21 + b.M23 * a.M22 + b.M33 * a.M23, b.M13 * a.M31 + b.M23 * a.M32 + b.M33 * a.M33, 0,
                0, 0, 0, 0);

        //This is the first time I had issue with precision. Cosinus from 270o was different for float and double. MathHelper is broken...
        public static double Radians(double angle) => angle * Math.PI / 180;

        public static double Cos(double angle) => Math.Cos(Radians(angle));

        public static double Sin(double angle) => Math.Sin(Radians(angle));

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

        private static MakiDebugger DebuggerWindow;
        public static List<System.Reflection.FieldInfo> DebuggerFood;
        public static void Debugger_Spawn()
        {
            if (DebuggerWindow == null)
                DebuggerWindow = new MakiDebugger();
            else
            {
                DebuggerWindow.Close();
                DebuggerWindow.Dispose();
                DebuggerWindow = new MakiDebugger();
            }
            if (DebuggerFood == null)
                DebuggerFood = new List<System.Reflection.FieldInfo>();
            DebuggerFood.Clear();
            DebuggerWindow.Show();
        }

        public static void Debugger_Feed(Type b)
        {
            var a = b.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            DebuggerFood.AddRange(a);
            DebuggerWindow.UpdateWindow();
        }

        public static void Debugger_Update()
        {
            if(DebuggerWindow!=null)
                DebuggerWindow._Refresh();
        }
    }
}
