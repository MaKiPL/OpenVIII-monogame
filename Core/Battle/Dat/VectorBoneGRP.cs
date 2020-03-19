using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    public struct VectorBoneGRP
    {
        #region Constructors

        public VectorBoneGRP(Vector3 vector, int boneID)
        {
            Vector = vector;
            BoneID = boneID;
        }

        #endregion Constructors

        #region Properties

        public int BoneID { get; }
        public float X => Vector.X;
        public float Y => Vector.Y;
        public float Z => Vector.Z;
        private Vector3 Vector { get; set; }

        #endregion Properties

        #region Methods

        public static implicit operator Vector3(VectorBoneGRP vbg) => vbg.Vector;

        public override string ToString() => $"Vector: {Vector}, Bone ID: {BoneID}";

        #endregion Methods
    }
}