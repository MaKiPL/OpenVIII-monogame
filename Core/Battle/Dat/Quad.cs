using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 2f: Quad
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_DAT#Useful_structures"/>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = ByteSize)]
    public class Quad
    {
        #region Fields

        public const int ByteSize = 20;

        [field: FieldOffset(10)]
        public readonly ushort TexUnk;

        [field: FieldOffset(14)]
        public readonly ushort U;

        [field: FieldOffset(8)]
        public readonly UV Vta;

        [field: FieldOffset(12)]
        public readonly UV Vtb;

        [field: FieldOffset(16)]
        public readonly UV Vtc;

        [field: FieldOffset(18)]
        public readonly UV Vtd;

        [field: FieldOffset(0)]
        private readonly ushort _a;

        [field: FieldOffset(2)]
        private readonly ushort _b;

        [field: FieldOffset(4)]
        private readonly ushort _c;

        [field: FieldOffset(6)]
        private readonly ushort _d;

        #endregion Fields

        #region Constructors

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private Quad(BinaryReader br)
        {
            _a = br.ReadUInt16();
            _b = br.ReadUInt16();
            _c = br.ReadUInt16();
            _d = br.ReadUInt16();
            Vta = UV.CreateInstance(br);
            TexUnk = br.ReadUInt16();
            Vtb = UV.CreateInstance(br);
            U = br.ReadUInt16();
            Vtc = UV.CreateInstance(br);
            Vtd = UV.CreateInstance(br);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private Quad()
        {
        }

        #endregion Constructors

        #region Properties

        public static byte Count => 6;
        public ushort A => (ushort)(_a & 0xFFF);
        public ushort B => (ushort)(_b & 0xFFF);
        public ushort C => (ushort)(_c & 0xFFF);
        public ushort D => (ushort)(_d & 0xFFF);
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

        #endregion Indexers

        #region Methods

        public static Quad CreateInstance(BinaryReader br) => Extended.ByteArrayToClass<Quad>(br.ReadBytes(ByteSize));

        public static IReadOnlyList<Quad> CreateInstances(BinaryReader br, ushort count) =>
            Enumerable.Range(0, count).Select(_ => CreateInstance(br)).ToList().AsReadOnly();

        public VertexPositionTexture[] GenerateVPT(List<VectorBoneGRP> vertices, Quaternion rotation,
            Vector3 translationPosition, TextureHandler preVarTex)
        {
            var tempVPT = new VertexPositionTexture[Count];
            VertexPositionTexture GetVPT(Quad quad, byte i)
            {
                Vector3 GetVertex(Quad refQuad, byte j)
                {
                    return DatFile.TransformVertex(vertices[refQuad.GetIndex(j)], translationPosition, rotation);
                }

                var (x, y) = ((float)preVarTex.Width / preVarTex.Height, (float)preVarTex.ClassicWidth / preVarTex.ClassicHeight);

                //Debug.Assert(Math.Abs(x - y) < float.Epsilon);
                var classicWidth = Math.Abs(x - y) < float.Epsilon ? preVarTex.ClassicWidth : preVarTex.ClassicHeight * x;
                return new VertexPositionTexture(GetVertex(quad, i),
                    quad.GetUV(i).ToVector2(classicWidth, preVarTex.ClassicHeight));
            }
            tempVPT[0] = tempVPT[3] = GetVPT(this, this[0]);
            tempVPT[1] = GetVPT(this, this[1]);
            tempVPT[4] = GetVPT(this, this[4]);
            tempVPT[2] = tempVPT[5] = GetVPT(this, this[2]);
            return tempVPT;
        }

        public ushort GetIndex(int i)
        {
            switch (i)
            {
                case 0:
                    return A;

                case 1:
                    return B;

                case 2:
                    return C;

                case 3:
                    return D;
            }
            throw new IndexOutOfRangeException($"{this} :: 0-3 are only valid values");
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

                case 3:
                    return Vtd;
            }
            throw new IndexOutOfRangeException($"{this} :: 0-3 are only valid values");
        }

        #endregion Methods
    }
}