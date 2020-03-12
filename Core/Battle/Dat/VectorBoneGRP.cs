using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    public struct VectorBoneGRP
    {
        private Vector3 Vector { get; set; }
        public int BoneID { get; }
        public float X => Vector.X;
        public float Y => Vector.Y;
        public float Z => Vector.Z;

        public static implicit operator Vector3(VectorBoneGRP vbg) => vbg.Vector;

        public VectorBoneGRP(Vector3 vector, int boneID)
        {
            Vector = vector;
            BoneID = boneID;
        }

        public override string ToString() => $"Vector: {Vector}, Bone BattleID: {BoneID}";
    }
}