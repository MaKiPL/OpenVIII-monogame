using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
    public struct Magic
    {
        public byte ID;
        public byte unk;

        public Kernel.MagicData DATA => Memory.Kernel_Bin.MagicData.Count > ID ? Memory.Kernel_Bin.MagicData[ID] : null;
        public Kernel.JunctionableGFsData JGFDATA => Memory.Kernel_Bin.JunctionableGFsData.ContainsKey(GF) ? Memory.Kernel_Bin.JunctionableGFsData[GF] : null;


        // per IFRIT gf's BattleID is between 0x40 and 0x4F. And they seem to be in order of GFs enum.
        public GFs GF => ID > 0x4F || ID < 0x40 ? GFs.Blank : (GFs)(ID - 0x40);

        public FF8String Name => GF != GFs.Blank ? Memory.Strings.GetName(GF) : DATA?.Name;

        public override string ToString() => Name;
    }
}