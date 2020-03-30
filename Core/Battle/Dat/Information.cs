using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// Section 7: Information & stats
    /// </summary>
    /// <see cref="http://forums.qhimm.com/index.php?topic=8741.0"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Section_7:_Informations_.26_stats"/>
    /// <seealso cref="http://www.gjoerulv.com/"/>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = ByteSize)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public struct Information
    {
        #region Fields

        public const int ByteSize = 380;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        private readonly byte[] _name;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] HP;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] STR;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] VIT;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] MAG;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] SPR;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] SPD;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] EVA;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly Abilities[] AbilitiesLow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly Abilities[] AbilitiesMed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly Abilities[] AbilitiesHigh;

        /// <summary>
        /// Level med stats start
        /// </summary>
        public readonly byte MedLevelStart;

        /// <summary>
        /// Level high stats start
        /// </summary>
        public readonly byte HighLevelStart;

        public readonly UnkFlag UnkFlag;

        public readonly Flag1 BitSwitch;

        /// <summary>
        /// Cards per ifrit this is more of a drop, mod and rare mod
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly Cards.ID[] Card;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] Devour;

        public readonly Flag2 BitSwitch2;

        public readonly UnkFlag UnkFlag2;

        public readonly ushort ExpExtra;

        public readonly ushort Exp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Magic[] DrawLow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Magic[] DrawMed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Magic[] DrawHigh;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Saves.Item[] MugLow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Saves.Item[] MugMed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Saves.Item[] MugHigh;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Saves.Item[] DropLow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Saves.Item[] DropMed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly Saves.Item[] DropHigh;

        public readonly byte MugRate;

        public readonly byte DropRate;

        public readonly byte Padding;

        public readonly byte AP;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] Unk3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] Resistance;

        public readonly byte DeathResistanceMental;

        public readonly byte PoisonResistanceMental;

        public readonly byte PetrifyResistanceMental;

        public readonly byte DarknessResistanceMental;

        public readonly byte SilenceResistanceMental;

        public readonly byte BerserkResistanceMental;

        public readonly byte ZombieResistanceMental;

        public readonly byte SleepResistanceMental;

        public readonly byte HasteResistanceMental;

        public readonly byte SlowResistanceMental;

        public readonly byte StopResistanceMental;

        public readonly byte RegenResistanceMental;

        public readonly byte ReflectResistanceMental;

        public readonly byte DoomResistanceMental;

        public readonly byte SlowPetrifyResistanceMental;

        public readonly byte FloatResistanceMental;

        public readonly byte ConfuseResistanceMental;

        public readonly byte DrainResistanceMental;

        public readonly byte ExpulsionResistanceMental;

        public readonly byte PercentResistanceMental;

        #endregion Fields

        #region Properties

        public FF8String Name => _name;

        #endregion Properties

        #region Methods

        public Information CreateInstance(BinaryReader br, long byteOffset)
        {
            br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
            return Extended.ByteArrayToStructure<Information>(br.ReadBytes(ByteSize));
        }

        public override string ToString() => Name;

        #endregion Methods
    }
}