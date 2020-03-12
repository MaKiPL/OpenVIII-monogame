using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 20)]
    public struct Quad
    {
        private ushort A;
        private ushort B;
        private ushort C;
        private ushort D;
        public UV vta;
        public ushort texUnk;
        public UV vtb;
        public ushort u;
        public UV vtc;
        public UV vtd;

        private VertexPositionTexture[] TempVPT;

        public ushort A1 { get => (ushort)(A & 0xFFF); set => A = value; }
        public ushort B1 { get => (ushort)(B & 0xFFF); set => B = value; }
        public ushort C1 { get => (ushort)(C & 0xFFF); set => C = value; }
        public ushort D1 { get => (ushort)(D & 0xFFF); set => D = value; }
        public byte TextureIndex => (byte)((texUnk >> 6) & 0b111);

        public static Quad Create(BinaryReader br) => new Quad()
        {
            A = br.ReadUInt16(),
            B = br.ReadUInt16(),
            C = br.ReadUInt16(),
            D = br.ReadUInt16(),
            vta = UV.Create(br),
            texUnk = br.ReadUInt16(),
            vtb = UV.Create(br),
            u = br.ReadUInt16(),
            vtc = UV.Create(br),
            vtd = UV.Create(br),
            TempVPT = new VertexPositionTexture[Count]
        };

        public ushort GetIndex(int i)
        {
            switch (i)
            {
                case 0:
                    return A1;

                case 1:
                    return B1;

                case 2:
                    return C1;

                case 3:
                    return D1;
            }
            throw new IndexOutOfRangeException($"{this} :: 0-3 are only valid values");
        }

        public UV GetUV(int i)
        {
            switch (i)
            {
                case 0:
                    return vta;

                case 1:
                    return vtb;

                case 2:
                    return vtc;

                case 3:
                    return vtd;
            }
            throw new IndexOutOfRangeException($"{this} :: 0-3 are only valid values");
        }

        public byte this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                    case 3:
                        return 0;

                    case 1:
                        return 1;

                    case 2:
                    case 5:
                        return 3;

                    case 4:
                        return 2;
                }
                throw new IndexOutOfRangeException($"{this} :: 0-5 are only valid values");
            }
        }

        public byte[] Indices => new[] { this[0], this[1], this[2], this[3], this[4], this[5] };
        public static byte Count => 6;

        public VertexPositionTexture[] GenerateVPT(List<VectorBoneGRP> vertices, Quaternion rotation, Vector3 translationPosition, Texture2D preVarTex)
        {
            if (TempVPT == null)
                TempVPT = new VertexPositionTexture[Count];
            VertexPositionTexture GetVPT(ref Quad quad, byte i)
            {
                Vector3 GetVertex(ref Quad refQuad, byte j)
                {
                    return DebugBattleDat.TransformVertex(vertices[refQuad.GetIndex(j)], translationPosition, rotation);
                }
                return new VertexPositionTexture(GetVertex(ref quad, i), quad.GetUV(i).ToVector2(preVarTex.Width, preVarTex.Height));
            }
            TempVPT[0] = TempVPT[3] = GetVPT(ref this, this[0]);
            TempVPT[1] = GetVPT(ref this, this[1]);
            TempVPT[4] = GetVPT(ref this, this[4]);
            TempVPT[2] = TempVPT[5] = GetVPT(ref this, this[2]);
            return TempVPT;
        }
    }
}