using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Temporary Characters Limit Breaks
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Temporary-character-limit-breaks"/>
        public sealed class TemporaryCharacterLimitBreaks
        {
            #region Fields

            public const int Count = 5;

            public const int ID = 18;

            #endregion Fields

            #region Constructors

            private TemporaryCharacterLimitBreaks(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (MagicID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                AttackType = (AttackType)br.ReadByte();
                //0x0006  1 byte Attack Type
                AttackPower = br.ReadByte();
                //0x0007  1 byte Attack Power
                Unknown0 = br.ReadBytes(2);
                //0x0008  2 bytes Unknown
                Target = (Target)br.ReadByte();
                //0x000A  1 byte Target Info
                AttackFlags = (AttackFlags)br.ReadByte();
                //0x000B  1 byte Attack Flags
                HitCount = br.ReadByte();
                //0x000C  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x000D  1 byte Element Attack
                ElementPercent = br.ReadByte();
                //0x000E  1 byte Element Attack %
                StatusAttack = br.ReadByte();
                //0x000F  1 byte Status Attack Enabler
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();
                //0x0010  2 bytes status_0; //statuses 0-7
                Unknown1 = br.ReadBytes(2);
                //0x0012  2 bytes Unknown
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x0014  4 bytes status_1; //statuses 8-39
            }

            #endregion Constructors

            #region Properties

            public AttackFlags AttackFlags { get; }

            public byte AttackPower { get; }

            public AttackType AttackType { get; }

            public FF8String Description { get; }

            public Element Element { get; }

            public byte ElementPercent { get; }

            public byte HitCount { get; }

            public MagicID MagicID { get; }

            public FF8String Name { get; }

            public byte StatusAttack { get; }

            public Persistent_Statuses Statuses0 { get; }

            public BattleOnlyStatuses Statuses1 { get; }

            public Target Target { get; }

            public byte[] Unknown0 { get; }

            public byte[] Unknown1 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<TemporaryCharacterLimitBreaks> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList().AsReadOnly();

            public override string ToString() => Name;

            private static TemporaryCharacterLimitBreaks CreateInstance(BinaryReader br, int i) => new TemporaryCharacterLimitBreaks(br, i);

            #endregion Methods
        }
    }
}