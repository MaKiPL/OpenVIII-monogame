using System;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    /// <summary>
    /// </summary>
    /// <see cref="http://forums.qhimm.com/index.php?topic=8741.0"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php/FF8/FileFormat_DAT#Section_7:_Informations_.26_stats"/>
    /// <seealso cref="http://www.gjoerulv.com/"/>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = DebugBattleDat.Section7Size)]
    public struct Information
    {
        [Flags]
        public enum Flag1 : byte
        {
            None = 0,
            Zombie = 0x1,
            Fly = 0x2,
            zz1 = 0x4,
            zz2 = 0x8,
            zz3 = 0x10,
            Auto_Reflect = 0x20,
            Auto_Shell = 0x40,
            Auto_Protect = 0x80,
        }

        [Flags]
        public enum Flag2 : byte
        {
            None = 0,
            zz1 = 0x1,
            zz2 = 0x2,
            unused1 = 0x4,
            unused2 = 0x8,
            unused3 = 0x10,
            unused4 = 0x20,
            DiablosMissed = 0x40,
            AlwaysCard = 0x80,
        }

        [Flags]
        public enum UnkFlag : byte
        {
            None = 0,
            unk0 = 0x1,
            unk1 = 0x2,
            unk2 = 0x4,
            unk3 = 0x8,
            unk4 = 0x10,
            unk5 = 0x20,
            unk6 = 0x40,
            unk7 = 0x80,
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        private byte[] _name;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] hp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] str;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] vit;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] mag;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] spr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] spd;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] eva;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Abilities[] abilitiesLow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Abilities[] abilitiesMed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Abilities[] abilitiesHigh;

        /// <summary>
        /// Level med stats start
        /// </summary>

        public byte medLevelStart;

        /// <summary>
        /// Level high stats start
        /// </summary>

        public byte highLevelStart;

        public UnkFlag unkflag;

        public Flag1 bitSwitch;

        /// <summary>
        /// Cards per ifrit this is more of a drop, mod and rare mod
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Cards.ID[] card;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] devour;

        public Flag2 bitSwitch2;

        public UnkFlag unkflag2;

        public ushort expExtra;

        public ushort exp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Magic[] drawlow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Magic[] drawmed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Magic[] drawhigh;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Saves.Item[] muglow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Saves.Item[] mugmed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Saves.Item[] mughigh;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Saves.Item[] droplow;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Saves.Item[] dropmed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Saves.Item[] drophigh;

        public byte mugRate;

        public byte dropRate;

        public byte padding;

        public byte ap;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] unk3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] resistance;

        public byte deathResistanceMental;

        public byte poisonResistanceMental;

        public byte petrifyResistanceMental;

        public byte darknessResistanceMental;

        public byte silenceResistanceMental;

        public byte berserkResistanceMental;

        public byte zombieResistanceMental;

        public byte sleepResistanceMental;

        public byte hasteResistanceMental;

        public byte slowResistanceMental;

        public byte stopResistanceMental;

        public byte regenResistanceMental;

        public byte reflectResistanceMental;

        public byte doomResistanceMental;

        public byte slowPetrifyResistanceMental;

        public byte floatResistanceMental;

        public byte confuseResistanceMental;

        public byte drainResistanceMental;

        public byte explusionResistanceMental;

        public byte percentResistanceMental;

        public FF8String name => _name;
    }
}