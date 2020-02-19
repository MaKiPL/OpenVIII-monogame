using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.Fields
{
    public readonly struct WalkmeshCoords
    {
        #region Fields

        private readonly Boolean _hasZ;
        private readonly Vector3 _pos;
        private readonly Int32 _triangleId;

        #endregion Fields

        #region Constructors

        public WalkmeshCoords(Int32 triangleIdId, Point pos) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, pos.X, pos.Y, 0, false);

        public WalkmeshCoords(Int32 triangleIdId, Vector2 pos) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, pos.X, pos.Y, 0, false);

        public WalkmeshCoords(Int32 triangleIdId, float x, float y) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, x, y, 0, false);

        public WalkmeshCoords(Int32 triangleIdId, Vector3 pos) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, pos.X, pos.Y, pos.Z, true);

        public WalkmeshCoords(Int32 triangleIdId, float x, float y, float z) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, x, y, z, true);

        #endregion Constructors

        #region Properties

        public bool HasZ => _hasZ;
        public Vector3 Pos => _pos;
        public int TriangleId => _triangleId;
        public float X => _pos.X;
        public float Y => _pos.Y;
        public float Z => _pos.Z;

        #endregion Properties

        #region Methods

        public override String ToString() => _hasZ
                ? $"(Triangle: {_triangleId}, X: {X}, Y: {Y}, Z: {Z})"
                : $"(Triangle: {_triangleId}, X: {X}, Y: {Y})";

        #endregion Methods
    }
}