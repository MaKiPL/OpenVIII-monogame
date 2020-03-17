using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = ByteSize)]
    public class Triangle
    {
        #region Fields

        [field: FieldOffset(10)]
        public readonly ushort TexUnk;

        [field: FieldOffset(14)]
        public readonly ushort U;

        [field: FieldOffset(6)]
        public readonly UV Vta;

        [field: FieldOffset(8)]
        public readonly UV Vtb;

        [field: FieldOffset(12)]
        public readonly UV Vtc;

        private const int ByteSize = 16;

        [field: FieldOffset(0)]
        private readonly ushort _a;

        [field: FieldOffset(2)]
        private readonly ushort _b;

        [field: FieldOffset(4)]
        private readonly ushort _c;

        #endregion Fields

        #region Constructors

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private Triangle()
        { }

        private Triangle(BinaryReader br)

        {
            _a = br.ReadUInt16();//0
            _b = br.ReadUInt16();//2
            _c = br.ReadUInt16();//4
            Vta = UV.CreateInstance(br);//6
            Vtb = UV.CreateInstance(br);//8
            TexUnk = br.ReadUInt16();//10
            Vtc = UV.CreateInstance(br);//12
            U = br.ReadUInt16();//14
        }

        #endregion Constructors

        #region Properties

        public static byte Count => 3;
        public ushort A => (ushort)(_a & 0xFFF);
        public ushort B => (ushort)(_b & 0xFFF);
        public ushort C => (ushort)(_c & 0xFFF);
        public byte TextureIndex => (byte)((TexUnk >> 6) & 0b111);

        #endregion Properties

        #region Indexers

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

        #endregion Indexers

        #region Methods

        public static Triangle CreateInstance(BinaryReader br) => Extended.ByteArrayToClass<Triangle>(br.ReadBytes(ByteSize));

        public static IReadOnlyList<Triangle> CreateInstances(BinaryReader br, ushort count) => Enumerable.Range(0, count).Select(_ => CreateInstance(br)).ToList().AsReadOnly();

        public VertexPositionTexture[] GenerateVPT(List<VectorBoneGRP> vertices, Quaternion rotation, Vector3 translationPosition, Texture2D preVarTex)
        {
            VertexPositionTexture[] tempVPT = new VertexPositionTexture[Count];
            VertexPositionTexture GetVPT(Triangle triangle, byte i)
            {
                Vector3 GetVertex(ref Triangle refTriangle, byte j)
                {
                    return DebugBattleDat.TransformVertex(vertices[refTriangle.GetIndex(j)], translationPosition, rotation);
                }
                return new VertexPositionTexture(GetVertex(ref triangle, i), triangle.GetUV(i).ToVector2(preVarTex.Width, preVarTex.Height));
            }
            tempVPT[0] = GetVPT(this, this[0]);
            tempVPT[1] = GetVPT(this, this[1]);
            tempVPT[2] = GetVPT(this, this[2]);
            return tempVPT;
        }

        public ushort GetIndex(int i)
        {
            switch (i)
            {
                case 0:
                    return C; //for some reason C is first in triangle

                case 1:
                    return A;

                case 2:
                    return B;
            }
            throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
        }

        public UV GetUV(int i)
        {
            switch (i)
            {
                case 0:
                    return Vta;

                case 1:
                    return Vtb;

                case 2:
                    return Vtc;
            }
            throw new IndexOutOfRangeException($"{this} :: 0-2 are only valid values");
        }

        #endregion Methods
    }
}