using System;

namespace OpenVIII
{
    public struct Degrees
    {
        public readonly Int32 Angle360;

        public static Degrees FromAngle360(Int32 angle360) => new Degrees(angle360);

        public static Degrees FromAngle256(Int32 angle256)
        {
            if (angle256 < 0 || angle256 > 256)
                throw new ArgumentOutOfRangeException($"Angle {angle256} is out of range." +
                                                      "Final Fantasy 8 uses 256 degree circles." +
                                                      "Degrees 0 and 256 are defined as down, 64 right, 128 up, 192 left.", nameof(angle256));

            return new Degrees(angle256 * 360 / 256);
        }

        private Degrees(Int32 angle360)
        {
            Angle360 = angle360;
        }

        public override String ToString() => $"Angle: {Angle360}°";
        public override Int32 GetHashCode() => Angle360.GetHashCode();

        public override Boolean Equals(Object obj)
        {
            if (obj is Degrees degrees)
                return Angle360 == degrees.Angle360;

            return false;
        }

        public static Degrees operator -(Degrees angle)
        {
            return Degrees.FromAngle360(-angle.Angle360);
        }
    }
}