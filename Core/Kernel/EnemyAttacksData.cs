using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Enemy Attacks Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Enemy-attacks"/>
        public class EnemyAttacksData
        {
            #region Fields

            public const int Count = 384;
            public const int ID = 3;

            #endregion Fields

            #region Constructors

            public EnemyAttacksData(BinaryReader br, int i, IReadOnlyList<BattleCommand> battleCommands)
            {
                EnemyAttackID = i;
                Name = i == 2 ? battleCommands[1].Name : Memory.Strings.Read(Strings.FileID.Kernel, ID, i);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                MagicID = (MagicID)br.ReadUInt16(); //0x02	2 bytes Magic ID
                CameraChange = br.ReadByte(); //0x04	1 byte Camera Change
                Unknown0 = br.ReadByte(); //0x05	1 byte Unknown Maybe something similar to Target.
                AttackType = (AttackType)br.ReadByte();//0x06	1 byte Attack type
                AttackPower = br.ReadByte();//0x07	1 byte Attack power
                AttackFlags = (AttackFlags)(br.ReadByte());//0x08	1 byte Attack flags
                Unknown1 = br.ReadByte();//0x09	1 byte Unknown
                Element = (Element)br.ReadByte();//0x0A	1 byte Element
                Unknown2 = br.ReadByte();//0x0B	1 byte Unknown
                StatusAttack = br.ReadByte();//0x0C	1 byte Status attack enabler
                AttackParameter = br.ReadByte();//0x0D	1 byte Attack Parameter
                //Statuses = new BitArray(br.ReadBytes(6));
                Statuses0 = (PersistentStatuses)br.ReadUInt16();//0x0E	2 bytes status_0; //statuses 0-7
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();//0x10	4 bytes status_1; //statuses 8-31
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// 0x08	1 byte Attack flags
            /// </summary>
            public AttackFlags AttackFlags { get; }

            /// <summary>
            /// 0x0D	1 byte Attack Parameter, HIT?
            /// </summary>
            public byte AttackParameter { get; }

            /// <summary>
            /// 0x07	1 byte Attack power
            /// </summary>
            public byte AttackPower { get; }

            /// <summary>
            /// 0x06	1 byte Attack type
            /// </summary>
            public AttackType AttackType { get; }

            /// <summary>
            /// 0x04	1 byte Camera Change
            /// </summary>
            public byte CameraChange { get; }

            /// <summary>
            /// 0x0A	1 byte Element
            /// </summary>
            public Element Element { get; }

            /// <summary>
            /// Enemy Attack ID
            /// </summary>
            public int EnemyAttackID { get; }

            /// <summary>
            /// 0x02	2 bytes Magic ID
            /// </summary>
            public MagicID MagicID { get; }

            /// <summary>
            /// 0x00	2 bytes Offset to attack name
            /// </summary>
            public FF8String Name { get; }

            /// <summary>
            /// 0x0C	1 byte Status attack enabler
            /// </summary>
            public byte StatusAttack { get; }

            /// <summary>
            /// 0x0E	2 bytes status_0; //statuses 0-7
            /// </summary>
            public PersistentStatuses Statuses0 { get; }

            /// <summary>
            /// 0x10	4 bytes status_1; //statuses 8-31
            /// </summary>
            public BattleOnlyStatuses Statuses1 { get; }

            /// <summary>
            /// 0x05	1 byte Unknown
            /// </summary>
            public byte Unknown0 { get; }

            /// <summary>
            /// 0x09	1 byte Unknown
            /// </summary>
            public byte Unknown1 { get; }

            /// <summary>
            /// 0x0B	1 byte Unknown
            /// </summary>
            public byte Unknown2 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<EnemyAttacksData> Read(BinaryReader br, IReadOnlyList<BattleCommand> battleCommands)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x, battleCommands)).ToList();

            public override string ToString() => Name;

            private static EnemyAttacksData CreateInstance(BinaryReader br, int i,
                IReadOnlyList<BattleCommand> battleCommands)
                => new EnemyAttacksData(br, i, battleCommands);

            #endregion Methods
        }
    }
}