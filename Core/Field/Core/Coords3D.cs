using System;

namespace OpenVIII.Fields
{
    public struct Coords3D
    {
        #region Fields

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        #endregion Fields

        #region Constructors

        public Coords3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"(X: {X}, Y: {Y}, Z: {Z})";

        #endregion Methods
    }
}