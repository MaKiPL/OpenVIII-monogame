using System;

namespace FF8
{
    public struct RGBColor
    {
        public readonly Int32 R;
        public readonly Int32 G;
        public readonly Int32 B;

        public RGBColor(Int32 r, Int32 g, Int32 b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override String ToString()
        {
            return $"(R: {R}, G: {G}, B: {B})";
        }
    }
}