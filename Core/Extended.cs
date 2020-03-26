using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    //Class that provides language extensions made by Maki
    public static class Extended
    {
        public enum Languages
        {
            en,
            fr,
            de,
            es,
            it,
            jp
        }

        //WORLD MAP COORDINATES HELPER
        /// <summary>
        /// Indicates vanilla game X axis coordinate for minimum possible X - visually the nearest to left on worldmap
        /// </summary>
        public const int WORLD_COORDS_MINLEFT = unchecked((int)0xFFFE0000);
        /// <summary>
        /// Indicates vanilla game X axis coordinate for maximum possible X - visually the furthest to right on worldmap
        /// </summary>
        public const int WORLD_COORDS_MAXRIGHT = unchecked((int)0x0001FFFF);

        /// <summary>
        /// zero based range of maximum left-right for worldmap coordinates
        /// </summary>
        public static readonly int WORLD_COORDS_XRANGE = Math.Abs(WORLD_COORDS_MINLEFT) + WORLD_COORDS_MAXRIGHT;

        /// <summary>
        /// Indicates vanilla game Y axis coordinate for minimum possible Y - visually at the semi top of worldmap
        /// </summary>
        public const int WORLD_COORDS_MINTOP = unchecked((int)0xFFFE8000);
        /// <summary>
        /// Indicates vanilla game Y axis coordinate for maximum possible Y - visually at the very bottom of worldmap
        /// </summary>
        public const int WORLD_COORDS_MAXBOTTOM = unchecked((int)0x00017FFF);

        /// <summary>
        /// zero based range of maximum left-right for worldmap coordinates
        /// </summary>
        public static readonly int WORLD_COORDS_ZRANGE = Math.Abs(WORLD_COORDS_MINTOP) + WORLD_COORDS_MAXBOTTOM;

        /// <summary>
        /// Indicates the OpenVIII coordinate system which shows maximum X axis. Minimum is always zero (it's the nearest to right side of worldmap)
        /// </summary>
        public const int WORLD_OPENVIII_MAXRIGHT = -(32 * 512);
        /// <summary>
        /// Indicates the OpenVIII coordinate system which shows maximum Y axis. Minimum is always zero (it's the furthest to bottom of worldmap)
        /// </summary>
        public const int WORLD_OPENVIII_MAXBOTTOM = -(24 * 512);

        /// <summary>
        /// Result of Vanilla.Y * x = openviii.Y equation
        /// </summary>
        public const float WORLD_COORD_YHELPER = -0.06f;

        /// <summary>
        /// This method converts vanilla world map coordinates to openVIII equivalent. 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float ConvertVanillaWorldXAxisToOpenVIII(float x)
        {
            if(x>WORLD_COORDS_MINLEFT)
            {
                var leftSide = x - WORLD_COORDS_MINLEFT; //this is the distance from left to middle of map
                //0x1FFFF is one part of map
                var percentUsage = leftSide / 0x1FFFF; //we now know the left side map percentage use
                return (float)(percentUsage * (WORLD_OPENVIII_MAXRIGHT / 2.0));
            }
            else
            {
                var percentUsage = x / WORLD_COORDS_MAXRIGHT;
                var rightSide = WORLD_OPENVIII_MAXRIGHT / 2.0;
                return (float)(percentUsage * rightSide + rightSide);
            }
        }

        /// <summary>
        /// This method converts vanilla world map coordinates to openVIII equivalent. 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float ConvertVanillaWorldZAxisToOpenVIII(float z)
        {
            if (z > WORLD_COORDS_MINTOP)
            {
                var topSide = z - WORLD_COORDS_MINTOP; //this is the distance from left to middle of map
                //0x1FFFF is one part of map
                var percentUsage = topSide / 0x17FFF; //we now know the left side map percentage use
                return (float)(percentUsage * (WORLD_OPENVIII_MAXBOTTOM / 2.0));
            }
            else
            {
                var percentUsage = z / WORLD_COORDS_MAXBOTTOM;
                var rightSide = WORLD_OPENVIII_MAXBOTTOM / 2.0;
                return (float)(percentUsage * rightSide + rightSide);
            }
        }

        /// <summary>
        /// This method converts vanilla world map coordinates to openVIII equivalent 
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float ConvertVanillaWorldYAxisToOpenVIII(float y)
        => y * WORLD_COORD_YHELPER;



        //https://stackoverflow.com/a/2887/4509036
        public static T ByteArrayToClass<T>(byte[] bytes) where T : class
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
            if (Directory.Exists(Path.GetDirectoryName(s)))
            using (var fs = new System.IO.FileStream(s, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                tex.SaveAsPng(fs, tex.Width, tex.Height);
        }
#endif
        ///<summary>Detect if an object is a number</summary>
        ///<see cref="https://stackoverflow.com/questions/1130698/checking-if-an-object-is-a-number-in-c-sharp"/>
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

        /// <summary>
        /// Real-time collision 3D Raycast on plane + barycentric calculation
        /// </summary>
        /// <param name="R">ray with origin + given direction</param>
        /// <param name="a">vertex A</param>
        /// <param name="b">vertex B</param>
        /// <param name="c">vertex C</param>
        /// <param name="barycentric">out param for barycentric vector</param>
        /// <returns>0 - not applicable, 1- intersects;</returns>
        public static int RayIntersection3D(Ray R, Vector3 a, Vector3 b, Vector3 c, out Vector3 barycentric)
        {
            barycentric = Vector3.Zero;
            var p1 = b - a;
            var p2 = c - a;
            var lhs = Vector3.Cross(p1, p2);
            if (lhs == Vector3.Zero)
                return -1;
            var direction = R.Direction;
            var rhs = R.Position - a;
            var dot00 = -Vector3.Dot(lhs, rhs);
            var dot01 = Vector3.Dot(lhs, direction);
            if (Math.Abs(dot01) < 1E-08f)
                if (dot00 == 0f)
                    return 2;
                else return 0;
            else
            {
                var dot02 = dot00 / dot01;
                if (dot02 < 0.0)
                    return 0;
                barycentric = R.Position + dot02 * direction;
                var dot10 = Vector3.Dot(p1, p1);
                var dot11 = Vector3.Dot(p1, p2);
                var dot12 = Vector3.Dot(p2, p2);
                var lhs2 = barycentric - a;
                var dot21 = Vector3.Dot(lhs2, p1);
                var dot22 = Vector3.Dot(lhs2, p2);
                var dot30 = dot11 * dot11 - dot10 * dot12;
                var dot31 = (dot11 * dot22 - dot12 * dot21) / dot30;
                if (dot31 < 0.0 || dot31 > 1.0)
                    return 0;
                var dot32 = (dot11 * dot21 - dot10 * dot22) / dot30;
                if (dot32 < 0.0 || (dot31 + dot32) > 1.0)
                    return 0;
                return 1;
            }
        }

        public static BoundingBox GetBoundingBox(Vector3 a, Vector3 b, Vector3 c)
        {
            var Minx = (new float[] { a.X, b.X, c.X }).Min();
            var Maxx = (new float[] { a.X, b.X, c.X }).Max();
            var Miny = (new float[] { a.Y, b.Y, c.Y }).Min();
            var Maxy = (new float[] { a.Y, b.Y, c.Y }).Max();
            var Minz = (new float[] { a.Z, b.Z, c.Z }).Min();
            var Maxz = (new float[] { a.Z, b.Z, c.Z }).Max();
            var min = new Vector3(Minx, Miny, Minz);
            var max = new Vector3(Maxx, Maxy, Maxz);
            return new BoundingBox(min, max);
        }

        /// <summary>
        /// Provides VertexPositionTexture[] based on translatePosition and the scale. Result is plane geometry - 4 verts, 2 tris with UV of 0.0-1.0f
        /// </summary>
        /// <param name="translatePosition"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static VertexPositionTexture[] GetShadowPlane(Vector3 translatePosition, float scale=1f)
        {
            /*
             * THREE----ZERO
             * |         |
             * |         |
             * |         |
             * |         |
             * TWO------ONE*/

            var zero = new Vector3(1f * scale +translatePosition.X , translatePosition.Y, 1f * scale + translatePosition.Z);
            var one = new Vector3(1f * scale + translatePosition.X, translatePosition.Y, translatePosition.Z);
            var two = translatePosition;
            var three = new Vector3(translatePosition.X, translatePosition.Y, 1f * scale + translatePosition.Z);

            var vpt = new VertexPositionTexture[]
            {
                new VertexPositionTexture(zero, new Vector2(1f,1f)),
                new VertexPositionTexture(one, new Vector2(1f,0f)),
                new VertexPositionTexture(two, Vector2.Zero),
                new VertexPositionTexture(two, Vector2.Zero),
                new VertexPositionTexture(three, new Vector2(0f,1f)),
                new VertexPositionTexture(zero, new Vector2(1f,1f)),
            };
            return vpt;
        }

        /// <summary>
        /// Some debug text is crashing due to brackets not appearing in chartable. This function removes brackets inside string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveBrackets(string s) => s.Replace('{', ' ').Replace('}', ' ');
        public static bool GetBit(byte @object, int positionFromRight) => ((@object >> positionFromRight) & 1) > 0;
        public static bool GetBit(int @object, int positionFromRight) => ((@object >> positionFromRight) & 1) > 0;

        /// <summary>
        /// Reads given char[] until null terminator, but returns as byte[]- to be used with FF8String
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static byte[] GetBinaryString(BinaryReader br)
        {
            var bb = new List<byte>();
            byte b;
            while ((b = br.ReadByte()) != 0x00)
                bb.Add(b);
            return bb.ToArray();
        }

        public static bool IsLinux
        {
            get
            {
                var p = (int)Environment.OSVersion.Platform;
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
            var languageIndicator = Memory.languages.ToString();
            return bUseAlternative ? languageIndicator == "en" ? "us" : languageIndicator : languageIndicator;
        }

        public static Vector3 ShrinkVector4ToVector3(Vector4 reference, bool bMirrorY = false)
        {
            float x, y, z;
            reference.Deconstruct(out x, out y, out z, out _);
            if (bMirrorY)
                y = -y;
            return new Vector3(x, y, z);
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

        public static bool bRequestedBackBuffer = false;
        public static bool bBackBufferAvailable = false;

        private static Texture2D backBufferTexture;

        public static Texture2D BackBufferTexture
        {
            get
            {
                if (bBackBufferAvailable)
                {
                    bBackBufferAvailable = false;
                    return backBufferTexture;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                backBufferTexture = value;
            }
        }

        /// <summary>
        /// Makes a request AFTER base.draw() to dump the backBuffer- after that the callback happens to 
        /// postBackBufferDelegate, so make sure to set this parameter to method you want
        /// </summary>
        /// <param name="callbackMethod"></param>
        public static void RequestBackBuffer()
        {
            bBackBufferAvailable = false;
            bRequestedBackBuffer = true;
        }

        public delegate void PostBackBufferDelegate();
        public static PostBackBufferDelegate postBackBufferDelegate = EmptyPostBackBufferDelegate;

        public static void EmptyPostBackBufferDelegate()
        {
            ;
        }
    }
}
