using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Shot (Irvine limit break)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Shot-%28Irvine-limit-break%29"/>
        public sealed class ShotIrvineLimitBreak
        {
            #region Fields

            public const int Count = 8;

            public const int ID = 21;

            public const int Size = 24;

            #endregion Fields

            #region Constructors

            private ShotIrvineLimitBreak(BinaryReader br, int i)
            {
                //0x0000	2 bytes Offset to name
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0002	2 bytes Offset to description
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  2 bytes Magic ID
                MagicID = (MagicID)br.ReadUInt16();
                //0x0006  1 byte Attack Type
                AttackType = (AttackType)br.ReadByte();
                //0x0007  1 byte Attack Power
                AttackPower = br.ReadByte();
                //0x0008  2 bytes Unknown
                Unknown0 = br.ReadBytes(2);
                //0x000A  1 byte Target Info
                Target = (Target)br.ReadByte();
                //0x000B  1 byte Attack Flags
                AttackFlags = (AttackFlags)br.ReadByte();
                //0x000C  1 byte Hit Count
                HitCount = br.ReadByte();
                //0x000D  1 byte Element Attack
                Element = (Element)br.ReadByte();
                //0x000E  1 byte Element Attack %
                ElementPercent = br.ReadByte();
                //0x000F  1 byte Status Attack Enabler
                StatusAttack = br.ReadByte();
                //0x0010  2 bytes status_0; //statuses 0-7
                Statuses0 = (PersistentStatuses)br.ReadUInt16();
                //0x0012  1 byte Used item index
                ItemIndex = br.ReadByte();
                //0x0013  1 byte Critical increase
                Critical = br.ReadByte();
                //0x0014  4 bytes status_1; //statuses 8-39
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
            }

            #endregion Constructors

            #region Properties

            public AttackFlags AttackFlags { get; }

            public byte AttackPower { get; }

            public AttackType AttackType { get; }

            public byte Critical { get; }

            public FF8String Description { get; }

            public Element Element { get; }

            public byte ElementPercent { get; }

            public byte HitCount { get; }

            public byte ItemIndex { get; }

            public MagicID MagicID { get; }

            public FF8String Name { get; }

            public byte StatusAttack { get; }

            public PersistentStatuses Statuses0 { get; }

            public BattleOnlyStatuses Statuses1 { get; }

            public Target Target { get; }

            public byte[] Unknown0 { get; }

            #endregion Properties

            #region Methods

            public static ShotIrvineLimitBreak CreateInstance(BinaryReader br, int i) => new ShotIrvineLimitBreak(br, i);

            public static IReadOnlyList<ShotIrvineLimitBreak> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList().AsReadOnly();

            public override string ToString() => Name;

            #endregion Methods
        }
    }
}