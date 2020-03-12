using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public struct Triangle
    {
        private ushort A;
        private ushort B;
        private ushort C;
        public UV vta;
        public UV vtb;
        public ushort texUnk;
        public UV vtc;
        public ushort u;
        private VertexPositionTexture[] TempVPT;

        public ushort A1 { get => (ushort)(A & 0xFFF); set => A = value; }
        public ushort B1 { get => (ushort)(B & 0xFFF); set => B = value; }
        public ushort C1 { get => (ushort)(C & 0xFFF); set => C = value; }
        public byte TextureIndex => (byte)((texUnk >> 6) & 0b111);

        public static Triangle Create(BinaryReader br)
        {
            Triangle r = new Triangle()
            {
                A = br.ReadUInt16(),
                B = br.ReadUInt16(),
                C = br.ReadUInt16(),
                vta = UV.Create(br),
                vtb = UV.Create(br),
                texUnk = br.ReadUInt16(),
                vtc = UV.Create(br),
                u = br.ReadUInt16(),
                TempVPT = new VertexPositionTexture[Count]
            };
            return r;
        }

        public ushort GetIndex(int i)
        {
            switch (i)
            {
                case 0:
                    return C1; //for some reason C is first in triangle

                case 1:
                    return A1;

                case 2:
                    return B1;
            }
            throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
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
            }
            throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
        }

        public byte this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return 0;

                    case 1:
                        return 1;

                    case 2:
                        return 2;
                }
                throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
            }
        }

        public byte[] Indices => new[] { this[0], this[1], this[2] };
        public static byte Count => 3;

        public VertexPositionTexture[] GenerateVPT(List<VectorBoneGRP> vertices, Quaternion rotation, Vector3 translationPosition, Texture2D preVarTex)
        {
            if (TempVPT == null)
                TempVPT = new VertexPositionTexture[Count];
            VertexPositionTexture GetVPT(ref Triangle triangle, byte i)
            {
                Vector3 GetVertex(ref Triangle _triangle, byte _i)
                {
                    return TransformVertex(vertices[_triangle.GetIndex(_i)], translationPosition, rotation);
                }
                return new VertexPositionTexture(GetVertex(ref triangle, i), triangle.GetUV(i).ToVector2(preVarTex.Width, preVarTex.Height));
            }
            TempVPT[0] = GetVPT(ref this, this[0]);
            TempVPT[1] = GetVPT(ref this, this[1]);
            TempVPT[2] = GetVPT(ref this, this[2]);
            return TempVPT;
        }
    }
}