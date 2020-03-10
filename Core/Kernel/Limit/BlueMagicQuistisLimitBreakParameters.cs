using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Blue Magic Parameters - Four per spell for each crisis level. Higher Crisis Levels do more damage and/or have more effects.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Blue_Magic_(Final_Fantasy_VIII)"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Quistis-limit-break-parameters"/>
        [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 0)]
        public sealed class BlueMagicQuistisLimitBreakParameters
        {
            #region Fields

            /// <summary>
            /// <para>Section Count</para>
            /// 64 total but I want to add these to the Blue_Magic_Quistis_limit_break in an array
            /// </summary>
            public const int Count = 4;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 20;
            /// <summary>
            /// Fixed Size of Data
            /// </summary>
            public const int Size = 8;

            #endregion Fields

            #region Properties

            [field: FieldOffset(6)]
            public byte AttackParam { get; }

            [field: FieldOffset(5)]
            public byte AttackPower { get; }

            [field: FieldOffset(4)]
            public PersistentStatuses Statuses0 { get; }

            [field: FieldOffset(0)]
            public BattleOnlyStatuses Statuses1 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<BlueMagicQuistisLimitBreakParameters> Read(BinaryReader br)
                => Enumerable.Range(0, Count)
                    .Select(i => CreateInstance(br)).ToList();

            private static BlueMagicQuistisLimitBreakParameters CreateInstance(BinaryReader br) => new BlueMagicQuistisLimitBreakParameters(br);

            private BlueMagicQuistisLimitBreakParameters(BinaryReader br)
            {
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x0000  4 bytes Status 1
                Statuses0 = (PersistentStatuses)br.ReadUInt16();
                //0x0004  2 bytes Status 0
                AttackPower = br.ReadByte();
                //0x0006  1 bytes Attack Power
                AttackParam = br.ReadByte();
                //0x0007  1 byte Attack Param
            }

            #endregion Methods
        }
    }
}