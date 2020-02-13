using Microsoft.Xna.Framework;
using System.IO;

namespace OpenVIII.Battle
{
    public struct Coordinate
    {
        #region Fields

        public short x;
        public short y;
        public short z;

        #endregion Fields

        #region Methods

        public static Coordinate Read(BinaryReader br) => new Coordinate()
        {
            x = br.ReadInt16(),
            y = br.ReadInt16(),
            z = br.ReadInt16()
        };

        public Vector3 GetVector() => new Vector3(
            x,
            y,
            -z) /Memory.EnemyCoordinateScale; /// 100f;
        public static implicit operator Vector3(Coordinate @in) => @in.GetVector();
        public override string ToString()
        {
            return $"{x} {y} {z}";
        }
        #endregion Methods
    }
}