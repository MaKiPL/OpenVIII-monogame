using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.Fields
{
    public readonly struct WalkMeshCoords
    {
        #region Fields

        private readonly bool _hasZ;
        private readonly Vector3 _pos;
        private readonly int _triangleId;

        #endregion Fields

        #region Constructors

        public WalkMeshCoords(int triangleIdId, Point pos) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, pos.X, pos.Y, 0, false);

        public WalkMeshCoords(int triangleIdId, Vector2 pos) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, pos.X, pos.Y, 0, false);

        public WalkMeshCoords(int triangleIdId, float x, float y) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, x, y, 0, false);

        public WalkMeshCoords(int triangleIdId, Vector3 pos) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, pos.X, pos.Y, pos.Z, true);

        public WalkMeshCoords(int triangleIdId, float x, float y, float z) => (_triangleId, _pos.X, _pos.Y, _pos.Z, _hasZ) = (triangleIdId, x, y, z, true);

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

        public override string ToString() => _hasZ
                ? $"(Triangle: {_triangleId}, X: {X}, Y: {Y}, Z: {Z})"
                : $"(Triangle: {_triangleId}, X: {X}, Y: {Y})";

        #endregion Methods
    }
}