using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Blue magic (Quistis limit break)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Blue-magic-%28Quistis-limit-break%29"/>
        public sealed class BlueMagicQuistisLimitBreak
        {
            #region Fields

            /// <summary>
            /// Section Count
            /// </summary>
            public const int Count = 16;

            /// <summary>
            /// Section ID
            /// </summary>
            public const int ID = 19;

            #endregion Fields

            #region Constructors

            private BlueMagicQuistisLimitBreak(BinaryReader br, BlueMagic i)
            {
                BlueMagic = i;
                //0x0000	2 bytes Offset to name
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, (byte)i * 2);
                //0x0002	2 bytes Offset to description
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, (byte)i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                //0x0004  2 bytes Magic ID
                MagicID = (MagicID)br.ReadUInt16();
                //0x0006  1 byte Unknown
                Unknown0 = br.ReadByte();
                //0x0007  1 byte Attack Type
                AttackType = (AttackType)br.ReadByte();
                //0x0008  1 byte Unknown
                Unknown1 = br.ReadByte();
                //0x0009  1 byte Target
                // noticed we were missing a target
                // this byte made sense when tested.
                Target = (Target)br.ReadByte();
                //0x000A  1 byte Attack Flags
                AttackFlags = (AttackFlags)br.ReadByte();
                //0x000B  1 byte Unknown
                Unknown2 = br.ReadByte();
                //0x000C  1 byte Element
                Element = (Element)br.ReadByte();
                //0x000D  1 byte Status Attack
                StatusAttack = br.ReadByte();
                //0x000E  1 byte Critical Bonus
                Critical = br.ReadByte();
                //0x000F  1 byte Unknown
                Unknown3 = br.ReadByte();
                //Related Crisis Level Parameters
                CrisisLevels = GetCrisisLevels(br, (byte)i);
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Attack flags
            /// </summary>
            public AttackFlags AttackFlags { get; }

            /// <summary>
            /// Attack type
            /// </summary>
            public AttackType AttackType { get; }

            /// <summary>
            /// Blue Magic
            /// </summary>
            public BlueMagic BlueMagic { get; }

            /// <summary>
            /// Crisis Level Parameters
            /// </summary>
            public IReadOnlyList<BlueMagicQuistisLimitBreakParameters> CrisisLevels { get; }

            /// <summary>
            /// Critical Bonus
            /// </summary>
            public byte Critical { get; }

            /// <summary>
            /// Description of ability
            /// </summary>
            public FF8String Description { get; }

            /// <summary>
            /// Elements used
            /// </summary>
            public Element Element { get; }

            /// <summary>
            /// Magic ID
            /// </summary>
            public MagicID MagicID { get; }

            /// <summary>
            /// Name of ability
            /// </summary>
            public FF8String Name { get; }

            /// <summary>
            /// Status attack
            /// </summary>
            public byte StatusAttack { get; }

            /// <summary>
            /// Target
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

            public static IReadOnlyDictionary<BlueMagic, BlueMagicQuistisLimitBreak> Read(BinaryReader br)
                => Enumerable.Range(0, Count).ToDictionary(i => (BlueMagic)i, i => CreateInstance(br, (BlueMagic)i));

            public override string ToString() => Name;

            private static BlueMagicQuistisLimitBreak CreateInstance(BinaryReader br, BlueMagic i)
                            => new BlueMagicQuistisLimitBreak(br, i);

            private static IReadOnlyList<BlueMagicQuistisLimitBreakParameters> GetCrisisLevels(BinaryReader br, byte i)
            {
                long current = br.BaseStream.Position;
                try
                {
                    br.BaseStream.Seek(
                        Memory.Strings[Strings.FileID.Kernel].GetFiles()
                            .SubPositions[BlueMagicQuistisLimitBreakParameters.ID] +
                        BlueMagicQuistisLimitBreakParameters.Size * i, SeekOrigin.Begin);
                    return BlueMagicQuistisLimitBreakParameters.Read(br);
                }
                finally
                {
                    br.BaseStream.Seek(current, SeekOrigin.Begin);
                }
            }

            #endregion Methods
        }
    }
}