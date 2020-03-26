using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Rinoa limit breaks (part 2)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Rinoa-limit-breaks-%28part-2%29"/>
        public sealed class RinoaLimitBreaksPart2
        {
            #region Fields

            public const int Count = 5;

            public const int ID = 25;

            public const int Size = 20;

            #endregion Fields

            #region Constructors

            private RinoaLimitBreaksPart2(BinaryReader br, int i)
            {
                RinoaLimit2ID = i;
                switch (i)
                {
                    case 0:
                        Angelo = Angelo.Cannon;
                        break;

                    case 1:
                        Angelo = Angelo.Strike;
                        break;

                    case 2:
                        Angelo = Angelo.Invincible_Moon;
                        break;

                    case 3:
                        Angelo = Angelo.Wishing_Star;
                        break;

                    case 4:
                        Angelo = Angelo.Angel_Wing;
                        break;

                    default:
                        Angelo = Angelo.None;
                        break;
                }
                var offset = br.ReadUInt16();
                Name = new FF8StringReference(Memory.Strings[Strings.FileID.Kernel].GetArchive(), Memory.Strings[Strings.FileID.Kernel].GetFileNames()[0],
                    Memory.Strings[Strings.FileID.Kernel].GetFiles().SubPositions[(int)((Strings.Kernel)Memory.Strings[Strings.FileID.Kernel]).StringLocations[ID].Item1] + offset, settings: (FF8StringReference.Settings.Namedic | FF8StringReference.Settings.MultiCharByte));
                //0x0000	2 bytes Offset to name
                //br.BaseStream.Seek(2, SeekOrigin.Current);
                MagicID = (MagicID)br.ReadUInt16();
                //0x0002  2 bytes Magic ID
                AttackType = (AttackType)br.ReadByte();
                //0x0004  1 byte Attack type
                AttackPower = br.ReadByte();
                //0x0005  1 byte Attack power
                AttackFlags = (AttackFlags)br.ReadByte();
                //0x0006  1 byte Attack flags
                Unknown0 = br.ReadByte();
                //0x0007  1 byte Unknown
                Target = (Target)br.ReadByte();
                //0x0008  1 byte Target info
                Unknown1 = br.ReadByte();
                //0x0009  1 byte Unknown
                HitCount = br.ReadByte();
                //0x000A  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x000B  1 byte Element Attack
                ElementPercent = br.ReadByte();
                //0x000C  1 byte Element Attack %
                StatusAttack = br.ReadByte();
                //0x000D  1 byte Status Attack Enabler
                Statuses0 = (PersistentStatuses)br.ReadUInt16();
                //0x000E  2 bytes status_0; //statuses 0-7
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x0010  4 bytes status_1; //statuses 8-39
            }

            #endregion Constructors

            #region Properties

            public Angelo Angelo { get; }

            public AttackFlags AttackFlags { get; }

            public byte AttackPower { get; }

            public AttackType AttackType { get; }

            public Element Element { get; }

            public byte ElementPercent { get; }

            public byte HitCount { get; }

            public MagicID MagicID { get; }

            public FF8String Name { get; }

            public int RinoaLimit2ID { get; }

            public byte StatusAttack { get; }

            public PersistentStatuses Statuses0 { get; }

            public BattleOnlyStatuses Statuses1 { get; }

            public Target Target { get; }

            public byte Unknown0 { get; }

            public byte Unknown1 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<RinoaLimitBreaksPart2> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList().AsReadOnly();

            public override string ToString() => Name;

            private static RinoaLimitBreaksPart2 CreateInstance(BinaryReader br, int i) => new RinoaLimitBreaksPart2(br, i);

            #endregion Methods
        }
    }
}