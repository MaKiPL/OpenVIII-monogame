using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    public struct AnimationYOffset
    {
        #region Constructors

        public AnimationYOffset(int iD, int frame, Vector4 lowHigh)
            => (ID, Frame, LowY, HighY, MidX, MidZ) = (iD, frame, lowHigh.X, lowHigh.Y, lowHigh.Z, lowHigh.W);

        #endregion Constructors

        #region Properties

        public int Frame { get; }
        public float HighY { get; }
        public int ID { get; }
        public float LowY { get; }
        public float MidX { get; }
        public float MidZ { get; }

        #endregion Properties

        #region Methods

        public override string ToString() => $"[{ID}, {Frame}, {LowY}, {HighY}, {MidX}, {MidZ}]";

        #endregion Methods
    }
}