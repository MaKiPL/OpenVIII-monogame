using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 1: Skeleton
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_DAT#Section_1:_Skeleton"/>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Skeleton
    {
        #region Fields

        [field: FieldOffset(16)]
        public readonly IReadOnlyList<Bone> Bones;

        /// <summary>
        /// Number of bones
        /// </summary>
        [field: FieldOffset(0)]
        public readonly ushort CBones;

        [field: FieldOffset(2)]
        public readonly ushort Scale;

        [field: FieldOffset(4)]
        public readonly ushort Unk2;

        [field: FieldOffset(6)]
        public readonly ushort Unk3;

        [field: FieldOffset(8)]
        public readonly ushort Unk4;

        [field: FieldOffset(10)]
        public readonly ushort Unk5;

        [field: FieldOffset(12)]
        public readonly ushort Unk6;

        [field: FieldOffset(14)]
        public readonly ushort Unk7;

        #endregion Fields

        #region Constructors

        private Skeleton()
        {
        }

        private Skeleton(BinaryReader br)
        {
            if (br.BaseStream.Position + 16 >= br.BaseStream.Length) throw new InvalidDataException($"{nameof(Skeleton)} Stream to short to read");
            CBones = br.ReadUInt16();
            Scale = br.ReadUInt16();
            Unk2 = br.ReadUInt16();
            Unk3 = br.ReadUInt16();
            Unk4 = br.ReadUInt16();
            Unk5 = br.ReadUInt16();
            Unk6 = br.ReadUInt16();
            Unk7 = br.ReadUInt16();
            Bones = Bone.CreateInstances(br, CBones);
        }
        #endregion Constructors

        #region Properties

        public Vector3 GetScale => new Vector3(Scale / DatFile.ScaleHelper * 12, Scale / DatFile.ScaleHelper * 12, Scale / DatFile.ScaleHelper * 12);

        #endregion Properties

        #region Methods

        public static Skeleton CreateInstance(BinaryReader br, long startOffset)
        {
            br.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            return new Skeleton(br);
        }

        public static Skeleton CreateInstance(BinaryReader br) => new Skeleton(br);

        #endregion Methods
    }
}