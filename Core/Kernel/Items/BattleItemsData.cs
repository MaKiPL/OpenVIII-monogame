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
        public class BattleItemData
        {
            #region Fields

            public const int Count = 33;
            public const int ID = 7;

            #endregion Fields

            #region Constructors

            private BattleItemData(BinaryReader br, int i)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current);

                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0000	2 bytes Offset to item name
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                //0x0002	2 bytes Offset to item description
                MagicID = (MagicID)br.ReadUInt16();
                //0x0004	2 bytes Magic ID
                AttackType = (AttackType)br.ReadByte();
                //0x0006	1 byte Attack type
                AttackPower = br.ReadByte();
                //0x0007	1 byte Attack power
                Unknown0 = br.ReadByte();
                //0x0008	1 byte Unknown
                Target = (Target)br.ReadByte();
                //0x0009	1 byte Target info
                Unknown1 = br.ReadByte();
                //0x000A	1 byte Unknown
                AttackFlags = (AttackFlags)br.ReadByte();
                //0x000B	1 byte Attack flags
                Unknown2 = br.ReadByte();
                //0x000C	1 bytes Unknown
                StatusAttack = br.ReadByte();
                //0x000D	1 byte Status Attack Enabler
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();
                //0x000E	2 bytes status_0; //statuses 0-7
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x0010	4 bytes status_1; //statuses 8-39
                AttackParam = br.ReadByte();
                //0x0014	1 byte Attack Param
                Unknown3 = br.ReadByte();
                //0x0015	1 byte Unknown
                HitCount = br.ReadByte();
                //0x0016	1 bytes Hit Count
                Element = (Element)br.ReadByte();
                //0x0017	1 bytes Element
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Attack Flags
            /// </summary>
            public AttackFlags AttackFlags { get; }

            /// <summary>
            /// Attack Param
            /// </summary>
            public byte AttackParam { get; }

            /// <summary>
            /// Attack power
            /// </summary>
            public byte AttackPower { get; }

            /// <summary>
            /// Attack type
            /// </summary>
            public AttackType AttackType { get; }

            /// <summary>
            /// item description
            /// </summary>
            public FF8String Description { get; }

            /// <summary>
            /// Element
            /// </summary>
            public Element Element { get; }

            /// <summary>
            /// Hit Count
            /// </summary>
            public byte HitCount { get; }

            /// <summary>
            /// Magic ID
            /// </summary>
            public MagicID MagicID { get; }

            /// <summary>
            /// item name
            /// </summary>
            public FF8String Name { get; }

            /// <summary>
            /// Status Attack Enabler
            /// </summary>
            public byte StatusAttack { get; }

            /// <summary>
            /// statuses 0-7
            /// </summary>
            public Persistent_Statuses Statuses0 { get; }

            /// <summary>
            /// statuses 8-39
            /// </summary>
            public BattleOnlyStatuses Statuses1 { get; }

            /// <summary>
            /// Target info
            /// </summary>
            public Target Target { get; }

            /// <summary>
            /// Unknown byte
            /// </summary>
            public byte Unknown0 { get; }

            /// <summary>
            /// Unknown byte
            /// </summary>
            public byte Unknown1 { get; }

            /// <summary>
            /// Unknown byte
            /// </summary>
            public byte Unknown2 { get; }

            /// <summary>
            /// Unknown byte
            /// </summary>
            public byte Unknown3 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<BattleItemData> Read(BinaryReader br) =>
                Enumerable.Range(0, Count).Select(i => CreateInstance(br, i)).ToList();

            public override string ToString() => Name;

            private static BattleItemData CreateInstance(BinaryReader br, int i) => new BattleItemData(br, i);

            #endregion Methods
        }
    }
}