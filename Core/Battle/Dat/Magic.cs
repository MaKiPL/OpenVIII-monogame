using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public struct Magic
    {
        [field: FieldOffset(0)]
        public readonly byte ID;

        [field: FieldOffset(1)]
        public readonly byte Unk;

        public Kernel.MagicData Data => Memory.Kernel_Bin.MagicData.Count > ID ? Memory.Kernel_Bin.MagicData[ID] : null;
        public Kernel.JunctionableGFsData JunctionableGFsData => Memory.Kernel_Bin.JunctionableGFsData.ContainsKey(GF) ? Memory.Kernel_Bin.JunctionableGFsData[GF] : null;

        ///<remarks>per IFRIT GFs BattleID is between 0x40 and 0x4F. And they seem to be in order of GFs enum.</remarks>
        public GFs GF => ID > 0x4F || ID < 0x40 ? GFs.Blank : (GFs)(ID - 0x40);

        public FF8String Name => GF != GFs.Blank ? Memory.Strings.GetName(GF) : Data?.Name;

        public override string ToString() => Name;
    }
}