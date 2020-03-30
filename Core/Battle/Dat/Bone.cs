using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 1b: Bones
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_DAT#Bone_struct"/>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = ByteSize)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class Bone
    {
        #region Fields

        public const int ByteSize = 16 + UnkSize;
        public const int UnkSize = 32;

        /// <summary>
        /// Bone Size
        /// </summary>
        [field: FieldOffset(2)]
        public readonly short BoneSize;

        /// <summary>
        /// Parent ID
        /// </summary>
        [field: FieldOffset(0)]
        public readonly ushort ParentId;

        /// <summary>
        /// Unknown Often Empty Bytes
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = UnkSize)]
        [field: FieldOffset(16)]
        public readonly byte[] Unk;

        /// <summary>
        /// rotation X * 360f
        /// </summary>
        [field: FieldOffset(4)]
        private readonly short _rotX;

        /// <summary>
        /// rotation Y * 360f
        /// </summary>
        [field: FieldOffset(6)]
        private readonly short _rotY;

        /// <summary>
        /// rotation Z * 360f
        /// </summary>
        [field: FieldOffset(8)]
        private readonly short _rotZ;

        [field: FieldOffset(10)]
        private readonly short _unk4;

        [field: FieldOffset(12)]
        private readonly short _unk5;

        [field: FieldOffset(14)]
        private readonly short _unk6;

        #endregion Fields

        #region Constructors

        private Bone(BinaryReader br)
        {
            if (br.BaseStream.Position + ByteSize >= br.BaseStream.Length) return;
            (ParentId, BoneSize, _rotX, _rotY, _rotZ, _unk4, _unk5, _unk6, Unk) = (br.ReadUInt16(), br.ReadInt16(),
                br.ReadInt16(), br.ReadInt16(), br.ReadInt16(), br.ReadInt16(), br.ReadInt16(), br.ReadInt16(),
                br.ReadBytes(UnkSize));
        }

        private Bone()
        {
        }

        #endregion Constructors

        #region Properties

        public (float X, float Y, float Z) Rot =>
                    (_rotX / 4096.0f * 360.0f, _rotY / 4096.0f * 360.0f, _rotZ / 4096.0f * 360.0f);

        public float Size => BoneSize / DatFile.ScaleHelper;

        public (float A, float B, float C) UnkV => (_unk4 / 4096.0f, _unk5 / 4096.0f, _unk6 / 4096.0f);

        #endregion Properties

        #region Methods

        public static IReadOnlyList<Bone> CreateInstances(BinaryReader br, ushort cBones)
        {
            if (br.BaseStream.Position + ByteSize * cBones < br.BaseStream.Length)
                return Enumerable.Range(0, cBones)
                .Select(_ => Extended.ByteArrayToClass<Bone>(br.ReadBytes(ByteSize))).ToList()
                .AsReadOnly();
            throw new InvalidDataException($"{nameof(Bone)}::{nameof(CreateInstances)} Stream to short to read {cBones} bones");
        }

        #endregion Methods
    }
}