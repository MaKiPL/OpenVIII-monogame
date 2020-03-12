using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public struct Skeleton
    {
        public ushort cBones;
        public ushort scale;
        public ushort unk2;
        public ushort unk3;
        public ushort unk4;
        public ushort unk5;
        public ushort unk6;
        public ushort unk7;
        public Bone[] bones;

        public Vector3 GetScale => new Vector3(scale / DebugBattleDat.ScaleHelper * 12, scale / DebugBattleDat.ScaleHelper * 12, scale / DebugBattleDat.ScaleHelper * 12);
    }
}