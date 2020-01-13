using System;

namespace OpenVIII.Fields
{
public struct Coords3D
    {
        public readonly Int32 X;
        public readonly Int32 Y;
        public readonly Int32 Z;

        public Coords3D(Int32 x, Int32 y, Int32 z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override String ToString()
        {
            return $"(X: {X}, Y: {Y}, Z: {Z})";
        }
    }
}