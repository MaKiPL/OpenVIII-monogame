using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Renzokuken Finishers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Renzokuken-finishers"/>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Renzokuken#Finishing_moves"/>
        public sealed class RenzokukenFinishersData
        {
            #region Fields

            public const int Count = 4;

            public const int ID = 5;

            #endregion Fields

            #region Constructors

            private RenzokukenFinishersData(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (MagicID)br.ReadUInt16();          //0x0004	2 bytes Magic ID
                AttackType = (AttackType)br.ReadByte();   //0x0006	1 byte Attack Type
                Unknown0 = br.ReadByte();             //0x0007	1 byte Unknown
                AttackPower = br.ReadByte();         //0x0008	1 byte Attack power
                Unknown1 = br.ReadByte();             //0x0009	1 byte Unknown
                Target = (Target)br.ReadByte();             //0x000A	1 byte Target info
                AttackFlags = (AttackFlags)br.ReadByte(); //0x000B	1 byte Attack Flags
                HitCount = br.ReadByte();            //0x000C	1 byte Hit Count
                Element = (Element)br.ReadByte();           //0x000D	1 byte Element Attack
                ElementPercent = br.ReadByte();      //0x000E	1 byte Element Attack %
                StatusAttack = br.ReadByte();        //0x000F	1 byte Status Attack Enabler
                Unknown2 = br.ReadBytes(2);             //0x0010	2 bytes Unknown
                Statuses0 = (PersistentStatuses)br.ReadUInt16();       //0x0012	2 bytes status_0; //statuses 0-7
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();       //0x0014	4 bytes status_1; //statuses 8-39
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            ///0x000B	1 byte Attack Flags
            /// </summary>
            public AttackFlags AttackFlags { get; }

            /// <summary>
            ///0x0008	1 byte Attack power
            /// </summary>
            public byte AttackPower { get; }

            /// <summary>
            ///0x0006	1 byte Attack Type
            /// </summary>
            public AttackType AttackType { get; }

            public FF8String Description { get; }

            /// <summary>
            ///0x000D	1 byte Element Attack
            /// </summary>
            public Element Element { get; }

            /// <summary>
            ///0x000E	1 byte Element Attack %
            /// </summary>
            public byte ElementPercent { get; }

            /// <summary>
            ///0x000C	1 byte Hit Count
            /// </summary>
            public byte HitCount { get; }

            /// <summary>
            /// ///0x0004	2 bytes Magic ID
            /// </summary>
            public MagicID MagicID { get; }

            public FF8String Name { get; }

            /// <summary>
            ///0x000F	1 byte Status Attack Enabler
            /// </summary>
            public byte StatusAttack { get; }

            /// <summary>
            ///0x0012	2 bytes status_0; //statuses 0-7
            /// </summary>
            public PersistentStatuses Statuses0 { get; }

            /// <summary>
            ///0x0014	4 bytes status_1; //statuses 8-39
            /// </summary>
            public BattleOnlyStatuses Statuses1 { get; }

            /// <summary>
            ///0x000A	1 byte Target info
            /// </summary>
            public Target Target { get; }

            /// <summary>
            ///0x0007	1 byte Unknown
            /// </summary>
            public byte Unknown0 { get; }

            /// <summary>
            ///0x0009	1 byte Unknown
            /// </summary>
            public byte Unknown1 { get; }

            /// <summary>
            ///0x0010	2 bytes Unknown
            /// </summary>
            public byte[] Unknown2 { get; }

            private static IEnumerable<RenzokukenFinisher> Flags { get; } =
            Enum.GetValues(typeof(RenzokukenFinisher))
                .Cast<RenzokukenFinisher>()
                .ToList().AsReadOnly();

            #endregion Properties

            #region Methods

            public static IReadOnlyDictionary<RenzokukenFinisher, RenzokukenFinishersData> Read(BinaryReader br)
                => Flags.OrderBy(x => x).Select(((finisher, i) => new { i, finisher }))
                    .ToDictionary(x => x.finisher, x => CreateInstance(br, x.i));

            public override string ToString() => Name;

            private static RenzokukenFinishersData CreateInstance(BinaryReader br, int i) => new RenzokukenFinishersData(br, i);

            #endregion Methods
        }
    }
}