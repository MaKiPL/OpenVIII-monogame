using Microsoft.Xna.Framework;
using System.IO;

namespace OpenVIII.Battle
{
    public struct Coordinate
    {
        #region Fields

        public short X;
        public short Y;
        public short Z;

        #endregion Fields

        #region Methods

        public static implicit operator Vector3(Coordinate @in) => @in.GetVector();

        public static Coordinate Read(BinaryReader br) => new Coordinate()
        {
            X = br.ReadInt16(),
            Y = br.ReadInt16(),
            Z = br.ReadInt16()
        };

        public Vector3 GetVector() => new Vector3(
            X,
            Y,
            -Z) / Memory.EnemyCoordinateScale; /// 100f;

        public override string ToString() => $"{X} {Y} {Z}";

        #endregion Methods
    }
}