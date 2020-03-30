using System;

namespace OpenVIII.Fields
{
    public struct Degrees
    {
        #region Fields

        public readonly int Angle360;

        #endregion Fields

        #region Constructors

        private Degrees(int angle360) => Angle360 = angle360;

        #endregion Constructors

        #region Methods

        public static Degrees FromAngle256(int angle256)
        {
            if (angle256 < 0 || angle256 > 256)
                throw new ArgumentOutOfRangeException($"Angle {angle256} is out of range." +
                                                      "Final Fantasy 8 uses 256 degree circles." +
                                                      "Degrees 0 and 256 are defined as down, 64 right, 128 up, 192 left.", nameof(angle256));

            return new Degrees(angle256 * 360 / 256);
        }

        public static Degrees FromAngle360(int angle360) => new Degrees(angle360);

        public static Degrees operator -(Degrees angle) => Degrees.FromAngle360(-angle.Angle360);

        public static bool operator !=(Degrees left, Degrees right) => !left.Equals(right);

        public static bool operator ==(Degrees left, Degrees right) => left.Equals(right);

        public override bool Equals(object obj)
        {
            if (obj is Degrees degrees)
                return Angle360 == degrees.Angle360;

            return false;
        }

        public override int GetHashCode() => Angle360.GetHashCode();

        public override string ToString() => $"Angle: {Angle360}°";

        #endregion Methods
    }
}