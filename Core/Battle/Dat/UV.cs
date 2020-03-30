using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2)]
    public struct UV
    {
        #region Fields

        [field: FieldOffset(0)]
        public readonly byte U;

        [field: FieldOffset(1)]
        public readonly byte V;

        #endregion Fields

        #region Constructors

        private UV(byte u, byte v) => (U, V) = (u, v);

        private UV(BinaryReader br)
            => (U, V) = (br.ReadByte(), br.ReadByte());

        #endregion Constructors

        #region Methods

        public static UV CreateInstance(BinaryReader br) => new UV(br);

        public static UV CreateInstance(byte u, byte v) => new UV(u, v);

        public override string ToString() => $"{U};{U1()};{V};{V1()}";

        public Vector2 ToVector2(float width, float height) => new Vector2(U1(width), V1(height));

        public float U1(float height = 128f) => U / height;

        public float V1(float width = 128f)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            return V > 128 ? //if bigger than 128, then multi texture index to odd
                (V - 128f) / width
                : Math.Abs(width - 32) < float.Epsilon ? //if equals 32, then it's weapon texture and should be in range of 96-128
                (V - 96) / width
                : V / width;
        }

        #endregion Methods
    }
}