using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 44)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public struct Bone
    {
        public readonly ushort parentId;
        public readonly short boneSize;
        private readonly short rotX; //360
        private readonly short rotY; //360
        private readonly short rotZ; //360
        private readonly short unk4;
        private readonly short unk5;
        private readonly short unk6;

        public Bone(ushort parentId, short boneSize, short rotX, short rotY, short rotZ, short unk4, short unk5, short unk6, byte[] unk)
            => (this.parentId, this.boneSize, this.rotX, this.rotY, this.rotZ, this.unk4, this.unk5, this.unk6, Unk) =
                (parentId, boneSize, rotX, rotY, rotZ, unk4, unk5, unk6, unk);

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public byte[] Unk;

        public float Size => boneSize / DebugBattleDat.ScaleHelper;
        public float RotX => rotX / 4096.0f * 360.0f;  //rotX
        public float RotY => rotY / 4096.0f * 360.0f;  //rotY
        public float RotZ => rotZ / 4096.0f * 360.0f;  //rotZ
        public float Unk4 => unk4 / 4096.0f;  //unk1v
        public float Unk5 => unk5 / 4096.0f;  //unk2v
        public float Unk6 => unk6 / 4096.0f;  //unk3v
    }
}