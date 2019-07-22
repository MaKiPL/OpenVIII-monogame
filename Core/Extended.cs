using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    //Class that provides language extensions made by Maki
    public static class Extended
    {
        public enum languages
        {
            en,
            fr,
            de,
            es,
            it
        }

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

        public static void DumpTexture(Texture2D tex, string s)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(s, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                tex.SaveAsPng(fs, tex.Width, tex.Height);
        }
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

        public struct Barycentric
        {
            public float u;
            public float v;
            public float w;

            public Barycentric(Vector3 aV1, Vector3 aV2, Vector3 aV3, Vector3 aP)
            {
                Vector3 a = aV2 - aV3, b = aV1 - aV3, c = aP - aV3;
                float aLen = a.X * a.X + a.Y * a.Y + a.Z * a.Z;
                float bLen = b.X * b.X + b.Y * b.Y + b.Z * b.Z;
                float ab = a.X * b.X + a.Y * b.Y + a.Z * b.Z;
                float ac = a.X * c.X + a.Y * c.Y + a.Z * c.Z;
                float bc = b.X * c.X + b.Y * c.Y + b.Z * c.Z;
                float d = aLen * bLen - ab * ab;
                u = (aLen * bc - ab * ac) / d;
                v = (bLen * ac - ab * bc) / d;
                w = 1.0f - u - v;
            }
            public bool IsInside
            {
                get
                {
                    return (u >= 0.0f) && (u <= 1.0f) && (v >= 0.0f) && (v <= 1.0f) && (w >= 0.0f); //(w <= 1.0f)
                }
            }
            public Vector3 Interpolate(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                return v1 * u + v2 * v + v3 * w;
            }
        }

        /// <summary>
        /// Some debug text is crashing due to brackets not appearing in chartable. This function removes brackets inside string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveBrackets(string s) => s.Replace('{', ' ').Replace('}', ' ');
        public static bool GetBit(byte @object, int positionFromRight) => ((@object >> positionFromRight) & 1) > 0;
        public static bool GetBit(int @object, int positionFromRight) => ((@object >> positionFromRight) & 1) > 0;

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

        public static bool In(int _in, Vector2 range) =>
                _in >= range.X && _in <= range.Y;

        public static bool In(float _in, Vector2 range) => _in >= range.X && _in <= range.Y;
        //: false;
        public static bool In(int _in, int min, int max) => In(_in, new Vector2(min, max));
        public static bool In(float _in, float min, float max) => In(_in, new Vector2(min, max));

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

        /// <summary>
        /// This Matrix operation performs Matrix multiplication and transposing in-place
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix MatrixMultiply_transpose(Matrix a, Matrix b)
            => new Matrix(
                b.M11 * a.M11 + b.M21 * a.M12 + b.M31 * a.M13, b.M11 * a.M21 + b.M21 * a.M22 + b.M31 * a.M23, b.M11 * a.M31 + b.M21 * a.M32 + b.M31 * a.M33, 0,
                b.M12 * a.M11 + b.M22 * a.M12 + b.M32 * a.M13, b.M12 * a.M21 + b.M22 * a.M22 + b.M32 * a.M23, b.M12 * a.M31 + b.M22 * a.M32 + b.M32 * a.M33, 0,
                b.M13 * a.M11 + b.M23 * a.M12 + b.M33 * a.M13, b.M13 * a.M21 + b.M23 * a.M22 + b.M33 * a.M23, b.M13 * a.M31 + b.M23 * a.M32 + b.M33 * a.M33, 0,
                0, 0, 0, 0);

        //This is the first time I had issue with precision. Cosinus from 270o was different for float and double. MathHelper is broken...
        public static double Radians(double angle) => angle * Math.PI / 180;

        public static double Cos(double angle) => Math.Cos(Radians(angle));

        public static double Sin(double angle) => Math.Sin(Radians(angle));

        /// <summary>
        /// Converts short to float via x/4096f
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float S16ToFloat(short x) => x / 4096f;

        public static string GetLanguageShort(bool bUseAlternative = false)
        {
            string languageIndicator = Memory.languages.ToString();
            return bUseAlternative ? languageIndicator == "en" ? "us" : languageIndicator : languageIndicator;
        }

        /// <summary>
        /// Converts short to float via x/4096f
        /// </summary>
        /// <param name="x">mockup of short. I.e. short(50) -> float(50) -> 50.0f / 4096f</param>
        /// <returns></returns>
        public static float S16ToFloat(float x) => x / 4096f;

        /// <summary>
        /// Converts Vector3 containing direct short>float to Vector3 that XYZ are treated by S16ToFloat
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 S16VectorToFloat(Vector3 vec) => new Vector3(
            S16ToFloat(vec.X),
            S16ToFloat(vec.Y),
            S16ToFloat(vec.Z));

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
