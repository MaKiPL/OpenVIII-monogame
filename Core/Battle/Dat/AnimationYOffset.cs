using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    public struct AnimationYOffset
    {
        public int ID { get; }
        public int Frame { get; }
        public float LowY { get; }
        public float HighY { get; }
        public float MidX { get; }
        public float MidZ { get; }

        public AnimationYOffset(int iD, int frame, Vector4 lowHigh)
            => (ID, Frame, LowY, HighY, MidX, MidZ) = (iD, frame, lowHigh.X, lowHigh.Y, lowHigh.Z, lowHigh.W);

        public override string ToString() => $"[{ID}, {Frame}, {LowY}, {HighY}, {MidX}, {MidZ}]";
    }
}