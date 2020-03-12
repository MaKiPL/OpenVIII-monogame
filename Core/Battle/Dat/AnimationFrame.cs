using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    public struct AnimationFrame
    {
        #region Fields

        public Matrix[] boneMatrix;
        public Vector3[] bonesVectorRotations;
        private Vector3 position;

        #endregion Fields

        #region Properties

        public Vector3 Position { get => position; set => position = value; }

        #endregion Properties
    }
}