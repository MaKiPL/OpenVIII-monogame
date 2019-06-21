using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    /// <summary>
    /// class to add offset to point
    /// </summary>
    public static class PointEx
    {
        #region Methods

        public static Point Offset(this ref Point source, Point offset)
        {
            source = (source.ToVector2() + offset.ToVector2()).ToPoint();
            return source;
        }

        public static Point Offset(this ref Point source, Vector2 offset)
        {
            source = (source.ToVector2() + offset).ToPoint();
            return source;
        }

        public static Point Transform(this Point point, Matrix matrix)
        {
            point = Vector2.Transform(point.ToVector2(), Matrix.Invert(matrix)).RoundedPoint();
            return point;
        }

        public static Rectangle Scale(this Rectangle source, Matrix matrix)
        {
            Vector2 scale = Memory.Scale();
            Vector2 loc = source.Location.ToVector2();
            source.Offset(matrix.Translation.X, matrix.Translation.Y);
            source.Location = Vector2.Transform(loc, Matrix.Invert(matrix)).RoundedPoint();
            source.Size = (source.Size.ToVector2() * scale).RoundedPoint();
            return source;
        }

        public static Rectangle Scale(this Rectangle source, Vector2 scale)
        {
            source.Location = (source.Location.ToVector2() * scale).RoundedPoint();
            source.Size = (source.Size.ToVector2() * scale).RoundedPoint();
            return source;
        }

        public static Rectangle Scale(this Rectangle source)
        {
            Vector2 scale = Memory.Scale();
            source.Location = (source.Location.ToVector2() * scale).RoundedPoint();
            source.Size = (source.Size.ToVector2() * scale).RoundedPoint();
            return source;
        }

        public static Point Scale(this Point source, Vector2 scale)
        {
            source = (source.ToVector2() * scale).RoundedPoint();
            return source;
        }

        public static Point Scale(this Point source)
        {
            Vector2 scale = Memory.Scale();
            source = (source.ToVector2() * scale).RoundedPoint();
            return source;
        }

        public static Point RoundedPoint(this Vector2 v) => new Point((int)Math.Round(v.X), (int)Math.Round(v.Y));

        public static Point CeilingPoint(this Vector2 v) => v.Ceiling().ToPoint();

        public static Point FloorPoint(this Vector2 v) => v.Floor().ToPoint();

        public static Vector2 Round(this Vector2 v) => new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));

        public static Vector2 Ceiling(this Vector2 v) => new Vector2((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y));

        public static Vector2 Floor(this Vector2 v) => new Vector2((float)Math.Floor(v.X), (float)Math.Floor(v.Y));

        public static Vector2 FloorOrCeiling(this Vector2 v, Vector2 target)
        {
            float X, Y;
            X = v.X < target.X ? (float)Math.Ceiling(v.X) : (float)Math.Floor(v.X);
            Y = v.Y < target.Y ? (float)Math.Ceiling(v.Y) : (float)Math.Floor(v.Y);
            return new Vector2(X, Y);
        }

        /// <summary>
        /// Count how many flags set in enum.
        /// </summary>
        /// <param name="statuses">varible you need to number of flags set.</param>
        /// <returns>count</returns>
        /// <see cref="https://stackoverflow.com/questions/677204/counting-the-number-of-flags-set-on-an-enumeration"/>
        public static uint Count(this Kernel_bin.J_Statuses statuses)
        {
            uint v = (uint)statuses;
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
        }

        /// <summary>
        /// Count how many flags set in enum.
        /// </summary>
        /// <param name="element">varible you need to number of flags set.</param>
        /// <returns>count</returns>
        /// <see cref="https://stackoverflow.com/questions/677204/counting-the-number-of-flags-set-on-an-enumeration"/>
        public static uint Count(this Kernel_bin.Element element)
        {
            uint v = (uint)element;
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
        }

        public static Characters ToCharacters(this Faces.ID id)
        {
            if ((byte)id > 10)
                return Characters.Blank;
            return (Characters)id;
        }

        public static GFs ToGFs(this Faces.ID id)
        {
            if ((byte)id < 16 || (byte)id > 31)
                return GFs.Blank;
            return (GFs)(id - 16);
        }

        public static Faces.ID ToFacesID(this Characters id) => (Faces.ID)id;

        public static Faces.ID ToFacesID(this GFs id)
        {
            if (GFs.All == id || GFs.Blank == id)
                return Faces.ID.Blank;
            return (Faces.ID)(id + 16);
        }

        #endregion Methods
    }
}