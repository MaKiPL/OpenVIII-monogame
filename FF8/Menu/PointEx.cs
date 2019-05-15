using Microsoft.Xna.Framework;
using System;

namespace FF8
{
    /// <summary>
    /// class to add offset to point
    /// </summary>
    internal static class PointEx
    {
        #region Methods

        internal static Point Offset(this ref Point source, Point offset)
        {
            source = (source.ToVector2() + offset.ToVector2()).ToPoint();
            return source;
        }

        internal static Point Offset(this ref Point source, Vector2 offset)
        {
            source = (source.ToVector2() + offset).ToPoint();
            return source;
        }
        internal static Point Transform(this Point point, Matrix matrix)
        {
            point = Vector2.Transform(point.ToVector2(), Matrix.Invert(matrix)).RoundedPoint();
            return point;
        }
        internal static Rectangle Scale(this Rectangle source, Matrix matrix)
        {
            Vector2 scale = Memory.Scale();
            Vector2 loc = source.Location.ToVector2();
            source.Offset(matrix.Translation.X, matrix.Translation.Y);
            source.Location = Vector2.Transform(loc, Matrix.Invert(matrix)).RoundedPoint();
            source.Size = (source.Size.ToVector2() * scale).RoundedPoint();
            return source;
        }
        internal static Rectangle Scale(this Rectangle source, Vector2 scale)
        {
            source.Location = (source.Location.ToVector2() * scale).RoundedPoint();
            source.Size = (source.Size.ToVector2() * scale).RoundedPoint();
            return source;
        }
        internal static Rectangle Scale(this Rectangle source)
        {
            Vector2 scale = Memory.Scale();
            source.Location = (source.Location.ToVector2() * scale).RoundedPoint();
            source.Size = (source.Size.ToVector2() * scale).RoundedPoint();
            return source;
        }

        internal static Point Scale(this Point source, Vector2 scale)
        {
            source = (source.ToVector2() * scale).RoundedPoint();
            return source;
        }
        internal static Point Scale(this Point source)
        {
            Vector2 scale = Memory.Scale();
            source = (source.ToVector2() * scale).RoundedPoint();
            return source;
        }

        internal static Point RoundedPoint(this Vector2 v) => new Point((int)Math.Round(v.X), (int)Math.Round(v.Y));

        internal static Point CeilingPoint(this Vector2 v) => v.Ceiling().ToPoint();
        internal static Point FloorPoint(this Vector2 v) => v.Floor().ToPoint();
        internal static Vector2 Round(this Vector2 v) => new Vector2 ((float)Math.Round(v.X), (float)Math.Round(v.Y));
        internal static Vector2 Ceiling(this Vector2 v) => new Vector2((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y));
        internal static Vector2 Floor(this Vector2 v) => new Vector2((float)Math.Floor(v.X), (float)Math.Floor(v.Y));
        internal static Vector2 FloorOrCeiling(this Vector2 v, Vector2 target)
        {
            float X, Y;
            X = v.X < target.X ? (float)Math.Ceiling(v.X) : (float)Math.Floor(v.X);
            Y = v.Y < target.Y ? (float)Math.Ceiling(v.Y) : (float)Math.Floor(v.Y);
            return new Vector2(X, Y);
        }

        #endregion Methods
    }
}