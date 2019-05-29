using System;

namespace FF8
{
    public struct WalkmeshCoords
    {
        public readonly Int32 TriangleId;
        public readonly Int32 X;
        public readonly Int32 Y;
        public readonly Int32 Z;
        public readonly Boolean HasZ;

        public WalkmeshCoords(Int32 triangleIdId, Int32 x, Int32 y)
        {
            TriangleId = triangleIdId;
            X = x;
            Y = y;
            Z = 0;
            HasZ = false;
        }

        public WalkmeshCoords(Int32 triangleIdId, Int32 x, Int32 y, Int32 z)
        {
            TriangleId = triangleIdId;
            X = x;
            Y = y;
            Z = z;
            HasZ = true;
        }

        public override String ToString()
        {
            return HasZ
                ? $"(Triangle: {TriangleId}, X: {X}, Y: {Y}, Z: {Z})"
                : $"(Triangle: {TriangleId}, X: {X}, Y: {Y})";
        }
    }
}