using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Magic-data"/>
        public class MagicData
        {
            #region Fields

            public const int Count = 57;

            public const int ID = 1;

            #endregion Fields

            #region Constructors

            private MagicData(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.Kernel, ID, i * 2 + 1);
                MagicDataID = (byte)i;
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (MagicID)br.ReadUInt16();
                Unknown = br.ReadByte();
                AttackType = (AttackType)br.ReadByte();
                SpellPower = br.ReadByte();
                Unknown2 = br.ReadByte();
                Target = (Target)br.ReadByte();
                AttackFlags = (AttackFlags)br.ReadByte();
                DrawResist = br.ReadByte();
                HitCount = br.ReadByte();
                Element = (Element)br.ReadByte();
                Unknown3 = br.ReadByte();
                Statuses1 = (BattleOnlyStatuses)br.ReadUInt32();
                Statuses0 = (PersistentStatuses)br.ReadUInt16();
                StatusAttack = br.ReadByte();
                Dictionary<Stat, byte> jVal = new Dictionary<Stat, byte>()
                {
                    {Stat.HP, br.ReadByte()},
                    {Stat.STR, br.ReadByte()},
                    {Stat.VIT, br.ReadByte()},
                    {Stat.MAG, br.ReadByte()},
                    {Stat.SPR, br.ReadByte()},
                    {Stat.SPD, br.ReadByte()},
                    {Stat.EVA, br.ReadByte()},
                    {Stat.HIT, br.ReadByte()},
                    {Stat.Luck, br.ReadByte()}
                };
                ElAtk = (Element)br.ReadByte();
                jVal.Add(Stat.ElAtk, br.ReadByte());
                ElDef = (Element)br.ReadByte();
                jVal.Add(Stat.ElDef1, br.ReadByte());
                jVal.Add(Stat.ElDef2, jVal[Stat.ElDef1]);
                jVal.Add(Stat.ElDef3, jVal[Stat.ElDef1]);
                jVal.Add(Stat.ElDef4, jVal[Stat.ElDef1]);
                jVal.Add(Stat.StAtk, br.ReadByte());
                jVal.Add(Stat.StDef1, br.ReadByte());
                jVal.Add(Stat.StDef2, jVal[Stat.StDef1]);
                jVal.Add(Stat.StDef3, jVal[Stat.StDef1]);
                jVal.Add(Stat.StDef4, jVal[Stat.StDef1]);
                JVal = jVal;
                StAtk = (JunctionStatuses)br.ReadUInt16();
                StDef = (JunctionStatuses)br.ReadUInt16();
                GFCompatibility = br.ReadBytes(16);
                Unknown4 = br.ReadBytes(2);
            }

            #endregion Constructors

            #region Properties

            ///0x000B  1 byte  Attack Flags
            public AttackFlags AttackFlags { get; }

            ///0x0007  1 byte  Attack type
            public AttackType AttackType { get; }

            ///0x0002	2 bytes Offset to spell description
            public FF8String Description { get; }

            ///0x000C  1 byte  Draw resist(how hard is the magic to draw)
            public byte DrawResist { get; }

            ///0x0020  1 byte Characters J - Elem attack
            /// <para>//public byte J_Val[Kernel_bin.Stat.EL_Atk];//0x0021  1 byte  Characters J - Elem attack value</para>
            public Element ElAtk { get; }

            ///0x0022  1 byte Characters J - Elem defense
            /// <para>//public byte J_Val[Kernel_bin.Stat.EL_Def_1];//0x0023  1 byte  Characters J - Elem defense value</para>
            public Element ElDef { get; }

            ///0x000E  1 byte Element
            public Element Element { get; }

            /// <summary>
            ///<para>0x002A  1 byte  Quezacotl compatibility</para>
            ///<para>0x002B  1 byte  Shiva compatibility</para>
            ///<para>0x002C  1 byte  Ifrit compatibility</para>
            ///<para>0x002D  1 byte  Siren compatibility</para>
            ///<para>0x002E  1 byte  Brothers compatibility</para>
            ///<para>0x002F  1 byte  Diablos compatibility</para>
            ///<para>0x0030  1 byte  Carbuncle compatibility</para>
            ///<para>0x0031  1 byte  Leviathan compatibility</para>
            ///<para>0x0032  1 byte  Pandemona compatibility</para>
            ///<para>0x0033  1 byte  Cerberus compatibility</para>
            ///<para>0x0034  1 byte  Alexander compatibility</para>
            ///<para>0x0035  1 byte  Doomtrain compatibility</para>
            ///<para>0x0036  1 byte  Bahamut compatibility</para>
            ///<para>0x0037  1 byte  Cactuar compatibility</para>
            ///<para>0x0038  1 byte  Tonberry compatibility</para>
            ///<para>0x0039  1 byte  Eden compatibility</para>
            /// </summary>
            public byte[] GFCompatibility { get; }

            ///0x000D  1 byte  Hit Count(works with meteor animation, not sure about others)
            public byte HitCount { get; }

            /// <summary>
            ///<para>public byte HP_J;          0x0017  1 byte  Characters HP junction value</para>
            ///<para>public byte STR_J;         0x0018  1 byte  Characters STR junction value</para>
            ///<para>public byte VIT_J;         0x0019  1 byte  Characters VIT junction value</para>
            ///<para>public byte MAG_J;         0x001A  1 byte  Characters MAG junction value</para>
            ///<para>public byte SPR_J;         0x001B  1 byte  Characters SPR junction value</para>
            ///<para>public byte SPD_J;         0x001C  1 byte  Characters SPD junction value</para>
            ///<para>public byte EVA_J;         0x001D  1 byte  Characters EVA junction value</para>
            ///<para>public byte HIT_J;         0x001E  1 byte  Characters HIT junction value</para>
            ///<para>public byte LUCK_J;        0x001F  1 byte  Characters LUCK junction value</para>
            /// </summary>
            public IReadOnlyDictionary<Stat, byte> JVal { get; }

            public byte MagicDataID { get; }

            ///0x0004	2 bytes Magic ID
            public MagicID MagicID { get; }

            ///0x0000	2 bytes Offset to spell name
            public FF8String Name { get; }

            public bool PositiveMagic
            {
                get
                {
                    switch (AttackType)
                    {
                        case AttackType.CurativeItem:
                        case AttackType.CurativeMagic:
                        case AttackType.GivePercentageHP:
                        case AttackType.Revive:
                        case AttackType.ReviveAtFullHP:
                        case AttackType.WhiteWindQuistis:
                        case AttackType.Scan: //scan is kinda both.
                            return true;

                        case AttackType.None:
                        case AttackType.PhysicalAttack:
                        case AttackType.MagicAttack:
                        case AttackType.PhysicalDamage:
                        case AttackType.MagicDamage:
                        case AttackType.RenzokukenFinisher:
                        case AttackType.SquallGunbladeAttack:
                        case AttackType.GF:
                        case AttackType.LvDown:
                        case AttackType.SummonItem:
                        case AttackType.GFIgnoreTargetSPR:
                        case AttackType.LvUp:
                        case AttackType.Card:
                        case AttackType.Kamikaze:
                        case AttackType.Devour:
                        case AttackType.GFDamage:
                        case AttackType.Unknown1:
                        case AttackType.MagicAttackIgnoreTargetSPR:
                        case AttackType.AngeloSearch:
                        case AttackType.MoogleDance:
                        case AttackType.LvAttack:
                        case AttackType.FixedDamage:
                        case AttackType.TargetCurrentHP1:
                        case AttackType.FixedMagicDamageBasedOnGFLevel:
                        case AttackType.Unknown2:
                        case AttackType.Unknown3:
                        case AttackType.Unknown4:
                        case AttackType.EveryoneGrudge:
                        case AttackType._1_HP_Damage:
                        case AttackType.PhysicalAttackIgnoreTargetVIT:
                            return false;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            ///0x0008  1 byte  Spell power(used in damage formula)
            public byte SpellPower { get; }

            /// <summary>
            /// //0x0026  2 bytes Characters J - Statuses Attack
            /// <para>//public byte J_Val[Kernel_bin.Stat.ST_Atk];//0x0024  1 byte  Characters J - Status attack value</para>
            /// </summary>
            public JunctionStatuses StAtk { get; }

            ///0x0016  1 byte  Status attack enabler
            public byte StatusAttack { get; }

            ///0x0014  2 bytes Statuses 0
            public PersistentStatuses Statuses0 { get; }

            ///0x0010  4 bytes Statuses 1
            public BattleOnlyStatuses Statuses1 { get; }

            /// <summary>
            /// //0x0028  2 bytes Characters J - Statuses Defend
            /// <para>//public byte J_Val[Kernel_bin.Stat.ST_Def_1];//0x0025  1 byte  Characters J - Status defense value</para>
            /// </summary>
            public JunctionStatuses StDef { get; }

            ///0x000A  1 byte  Default_target
            public Target Target { get; }

            ///0x0006  1 byte  Unknown
            public byte Unknown { get; }

            ///0x0009  1 byte  Unknown
            public byte Unknown2 { get; }

            ///0x000F  1 byte  Unknown
            public byte Unknown3 { get; }

            ///0x003A  2 bytes Unknown
            public byte[] Unknown4 { get; }

            #endregion Properties

            #region Methods

            public static IReadOnlyList<MagicData> Read(BinaryReader br)
                => Enumerable.Range(0, Count).Select(x => CreateInstance(br, x)).ToList();

            public override string ToString() => Name;

            public uint TotalStatVal(Stat stat)
            {
                switch (stat)
                {
                    case Stat.HP:
                    case Stat.STR:
                    case Stat.VIT:
                    case Stat.MAG:
                    case Stat.SPR:
                    case Stat.SPD:
                    case Stat.EVA:
                    case Stat.HIT:
                    case Stat.Luck:
                        return JVal[stat];

                    case Stat.ElAtk:
                        return JVal[stat] * ElAtk.Count();

                    case Stat.StAtk:
                        return JVal[stat] * StAtk.Count();

                    case Stat.ElDef1:
                    case Stat.ElDef2:
                    case Stat.ElDef3:
                    case Stat.ElDef4:
                        return JVal[stat] * ElDef.Count();

                    case Stat.StDef1:
                    case Stat.StDef2:
                    case Stat.StDef3:
                    case Stat.StDef4:
                        return JVal[stat] * StDef.Count();

                    case Stat.None:
                        return 0;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
                }
            }

            private static MagicData CreateInstance(BinaryReader br, int i) => new MagicData(br, i);

            #endregion Methods
        }
    }
}