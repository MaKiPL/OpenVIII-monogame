using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    public struct AnimationFrame
    {
        private Vector3 position;
        public Vector3[] bonesVectorRotations;
        public Matrix[] boneMatrix;

        public Vector3 Position { get => position; set => position = value; }
    }
}