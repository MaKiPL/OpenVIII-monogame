using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
    public struct UV
    {
        public byte U;
        public byte V;

        public float U1(float h = 128f) => U / h;

        public float V1(float w = 128f)
        {
            if (w <= 0) throw new ArgumentOutOfRangeException(nameof(w));
            return V > 128 ? //if bigger than 128, then multi texture index to odd
                (V - 128f) / w
                : Math.Abs(w - 32) < float.Epsilon ? //if equals 32, then it's weapon texture and should be in range of 96-128
                (V - 96) / w
                : V / w;
        }

        public static UV Create(BinaryReader br) => new UV()
        {
            U = br.ReadByte(),
            V = br.ReadByte()
        };

        public Vector2 ToVector2(float w, float h) => new Vector2(U1(w), V1(h));

        public override string ToString() => $"{U};{U1()};{V};{V1()}";
    }
}