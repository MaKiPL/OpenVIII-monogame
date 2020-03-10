using OpenVIII.Encoding.Tags;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Duel (Zell limit break)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Duel-%28Zell-limit-break%29"/>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Duel"/>
        public class DuelZellLimitBreak
        {
            #region Fields

            public const int Count = 10;
            public const int ID = 22;
            public const int Size = 32;

            #endregion Fields

            #region Constructors

            public DuelZellLimitBreak(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (MagicID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                AttackType = (AttackType)br.ReadByte();
                //0x0006  1 byte Attack type
                AttackPower = br.ReadByte();
                //0x0007  1 byte Attack power
                AttackFlags = (AttackFlags)br.ReadByte();
                //0x0008  1 byte Attack flags
                Unknown0 = br.ReadByte();
                //0x0009  1 byte Unknown
                Target = (Target)br.ReadByte();
                //0x000A  1 byte Target Info
                Unknown1 = br.ReadByte();
                //0x000B  1 bytes Unknown
                HitCount = br.ReadByte();
                //0x000C  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x000D  1 byte Element Attack
                ElementPercent = br.ReadByte();
                //0x000E  1 byte Element Attack %
                StatusAttack = br.ReadByte();
                //0x000F  1 byte Status Attack Enabler
                ButtonCombo = new List<IReadOnlyList<FF8TextTagKey>>(5);
                for (int b = 0; b < 5; b++)
                {
                    ButtonCombo.Add(Input2.Convert_Flags((Button_Flags)br.ReadUInt16()));
                }
                //0x0010  2 bytes Sequence Button 1
                //0x0012  2 bytes Sequence Button 2
                //0x0014  2 bytes Sequence Button 3
                //0x0016  2 bytes Sequence Button 4
                //0x0018  2 bytes Sequence Button 5
                Statuses0 = (PersistentStatuses)br.ReadUInt16();
                //0x001A  2 bytes status_0; //statuses 0-7
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                //0x001C  4 bytes status_1; //statuses 8-39
            }

            #endregion Constructors

            #region Properties

            public AttackFlags AttackFlags { get; }

            public byte AttackPower { get; }

            public AttackType AttackType { get; }

            public List<IReadOnlyList<FF8TextTagKey>> ButtonCombo { get; }

            public FF8String Description { get; }

            public Element Element { get; }

            public byte ElementPercent { get; }

            public byte HitCount { get; }

            public MagicID MagicID { get; }

            public FF8String Name { get; }

            public byte StatusAttack { get; }

            public PersistentStatuses Statuses0 { get; }

            public BattleOnlyStatuses Statuses1 { get; }

            public Target Target { get; }

            public byte Unknown0 { get; }

            public byte Unknown1 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<DuelZellLimitBreak> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList();

            public override string ToString() => Name;

            private static DuelZellLimitBreak CreateInstance(BinaryReader br, int i) => new DuelZellLimitBreak(br, i);

            #endregion Methods
        }
    }
}