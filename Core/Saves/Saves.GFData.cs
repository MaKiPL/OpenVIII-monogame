using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static partial class Saves
    {
        #region Classes

        /// <summary>
        /// Data for each GF
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Guardian_Forces"/>
        public class GFData : Damageable
        {
            #region Fields

            /// <summary>
            /// <para>0x14 (1 byte = 1 ability of the GF, 2 bytes unused)</para>
            /// <para>AP of the abilties the GF can learn. See Kernel_Bin for list and prerequirements.</para>
            /// </summary>
            public byte[] APs;

            /// <summary>
            /// <para>0x12 abilities (1 bit = 1 ability completed, 9 bits unused)</para>
            /// </summary>
            public BitArray Complete;

            /// <summary>
            /// 0x10 Seems to be 0 for false and 1 for true.
            /// </summary>
            public bool Exists;

            /// <summary>
            /// <para>0x00 Total exp for GF</para>
            /// <para>Exp per level is in Kernel_bin</para>
            /// </summary>
            public uint Experience;

            /// <summary>
            /// <para>0x41 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)</para>
            /// <para>I think: Corrisponds to APs. Basically disables learnable abilities.</para>
            /// </summary>
            public BitArray Forgotten;


            /// <summary>
            /// <para>0x24 Number of kills</para>
            /// </summary>
            public ushort NumberKills;

            /// <summary>
            /// <para>0x3C Number of KOs</para>
            /// </summary>
            public ushort NumberKOs;

            /// <summary>
            /// 0x10 unknown value
            /// </summary>
            public byte Unknown;

            #endregion Fields

            #region Constructors

            public GFData()
            {
            }

            //public GFData(BinaryReader br, GFs g) => Read(br, g);

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Total exp to the next level up
            /// </summary>
            public ushort EXPtoNextLevel => Level >= 100 ? (ushort)0 :
                (ushort)Math.Abs((Level * JunctionableGFsData.EXPperLevel) - Experience);

            /// <summary>
            /// Enum ID for this GF
            /// </summary>
            public GFs ID { get; private set; }

            /// <summary>
            /// This is the ability that will gain AP from battles.
            /// </summary>
            public Kernel_bin.Abilities Learning { get; private set; }

            /// <summary>
            /// Current Level for this GF
            /// </summary>
            public override byte Level
            {
                get
                {
                    uint ret = (Experience / JunctionableGFsData.EXPperLevel)+1;
                    return ret > 100 ? (byte)100 : (byte)ret;
                }
            }


            /// <summary>
            /// True if at max.
            /// </summary>
            public bool MaxGFAbilities => (from bool m in Complete
                                           where m
                                           select m).Count() >= 22;

            /// <summary>
            /// Total amount of percent based hp buff
            /// </summary>
            public int Percent
            {
                get
                {
                    int p = 0;
                    List<Kernel_bin.Abilities> unlocked = UnlockedAbilities;
                    if (unlocked.Contains(Kernel_bin.Abilities.GFHP_10))
                        p += 10;
                    if (unlocked.Contains(Kernel_bin.Abilities.GFHP_20))
                        p += 20;
                    if (unlocked.Contains(Kernel_bin.Abilities.GFHP_30))
                        p += 30;
                    if (unlocked.Contains(Kernel_bin.Abilities.GFHP_40))
                        p += 40;
                    return p;
                }
            }

            /// <summary>
            /// Unlocked abilities
            /// </summary>
            public List<Kernel_bin.Abilities> UnlockedAbilities
            {
                get
                {
                    List<Kernel_bin.Abilities> abilities = new List<Kernel_bin.Abilities>();
                    for (int i = 1; i < Complete.Length; i++)//0 is none so skipping it.
                    {
                        if (Complete[i])
                            abilities.Add((Kernel_bin.Abilities)i);
                    }

                    return abilities;
                }
            }

            public override byte SPD => 0;

            public override byte SPR => 0;

            public override byte STR => 0;

            public override byte VIT => 0;

            public override byte MAG => 0;

            public override byte EVA => 0;

            public override int EXP => checked((int)Experience);

            public override byte HIT => 0;

            public override byte LUCK => 0;

            /// <summary>
            /// Gf ability data
            /// </summary>
            private IReadOnlyDictionary<Kernel_bin.Abilities, Kernel_bin.GF_abilities> GFabilities => Kernel_bin.GFabilities;

            /// <summary>
            /// Kernel bin data on this GF
            /// </summary>
            private Kernel_bin.Junctionable_GFs_Data JunctionableGFsData => Kernel_bin.JunctionableGFsData[ID];

            #endregion Properties

            #region Methods

            /// <summary>
            /// Create a copy of this gfdata object
            /// </summary>
            public override Damageable Clone()
            {
                //Shadowcopy
                GFData c = (GFData)MemberwiseClone();
                //Deepcopy
                c.Name = Name.Clone();
                c.Complete = (BitArray)(Complete.Clone());
                c.Forgotten = (BitArray)(Forgotten.Clone());
                c.APs = (byte[])(APs.Clone());
                return c;
            }

            public bool EarnExp(uint ap, out Kernel_bin.Abilities ability)
            {
                bool ret = false;
                ability = Kernel_bin.Abilities.None;

                if (!IsGameOver)
                {
                    if (EXPtoNextLevel <= ap && Level < 100)
                        ret = true;
                    Experience += ap;
                    if (!Learning.Equals(Kernel_bin.Abilities.None) && Kernel_bin.AllAbilities.ContainsKey(Learning))
                    {
                        byte ap_tolearn = Kernel_bin.AllAbilities[Learning].AP;
                        if (JunctionableGFsData.Ability.TryGetIndexByKey(Learning, out int ind) && TestGFCanLearn(Learning, false))
                        {
                            if (ap_tolearn < APs[ind] + ap)
                            {
                                APs[ind] = ap_tolearn;
                                ability = Learning;
                                Learn(Learning);
                            }
                            else
                            {
                                APs[ind] += (byte)ap;
                            }
                        }
                    }
                }
                return ret;
            }

            public override short ElementalResistance(Kernel_bin.Element @in) => throw new System.NotImplementedException();

            /// <summary>
            /// Learn this ability
            /// </summary>
            /// <param name="ability"></param>
            /// <returns></returns>
            public bool Learn(Kernel_bin.Abilities ability)
            {
                if (!MaxGFAbilities)
                {
                    if (!Complete[(int)ability])
                    {
                        Complete[(int)ability] = true;
                        if (Learning.Equals(ability))
                        {
                            SetLearning();
                        }
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Max HP for GF
            /// </summary>
            public override ushort MaxHP()
            {
                int max = ((Level * Level / 25) + 250 + JunctionableGFsData.HP_MOD * Level) * (Percent + 100) / 100;
                return (ushort)(max > Kernel_bin.MAX_HP_VALUE ? Kernel_bin.MAX_HP_VALUE : max);
            }
            public static GFData Load(BinaryReader br, GFs @enum) => Load<GFData>(br,@enum);
            /// <summary>
            /// Read in values from safe data.
            /// </summary>
            /// <param name="br">Binary Reader to raw save data</param>
            /// <param name="g">Which GF we are reading in</param>
            protected override void ReadData(BinaryReader br, Enum @enum)
            {
                if (!@enum.GetType().Equals(typeof(GFs))) throw new ArgumentException($"Enum {@enum} is not GFs");
                StatusImmune = true;
                ID = (GFs)@enum;
                Name = br.ReadBytes(12);//0x00 (0x00 terminated)
                if (string.IsNullOrWhiteSpace(Name)) Name = Memory.Strings.GetName((GFs)@enum);
                Experience = br.ReadUInt32();//0x0C
                Unknown = br.ReadByte();//0x10
                Exists = br.ReadByte() == 1 ? true : false;//0x11 //1 unlocked //0 locked
                _CurrentHP = br.ReadUInt16();//0x12
                Complete = new BitArray(br.ReadBytes(16));//0x14 abilities (1 bit = 1 ability completed, 9 bits unused)
                APs = br.ReadBytes(24);//0x24 (1 byte = 1 ability of the GF, 2 bytes unused)
                NumberKills = br.ReadUInt16();//0x3C of kills
                NumberKOs = br.ReadUInt16();//0x3E of KOs
                Learning = (Kernel_bin.Abilities)br.ReadByte();//0x41 ability
                Forgotten = new BitArray(br.ReadBytes(3));//0x42 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)
            }

            /// <summary>
            /// <para>Set which Ability the GF is learning.</para>
            /// <para>Could actually be a byte to corrispond to APs and Forgotten.</para>
            /// </summary>
            /// <param name="ability">If null sets to first in learnable list</param>
            public void SetLearning(Kernel_bin.Abilities? _ability = null)
            {
                Kernel_bin.Abilities a = _ability ?? Kernel_bin.Abilities.None;
                bool _set = false;
                if (a == Kernel_bin.Abilities.None)
                {
                    foreach (KeyValuePair<Kernel_bin.Abilities, Kernel_bin.Unlocker> kvp in JunctionableGFsData.Ability)
                    {
                        if (TestGFCanLearn(kvp.Key, false))
                        {
                            Learning = kvp.Key;
                            _set = true;
                            break;
                        }
                    }
                }
                else if (TestGFCanLearn(a, false))
                {
                    Learning = a;
                    _set = true;
                }

                if (!_set)
                    Learning = Kernel_bin.Abilities.None;
            }

            public override sbyte StatusResistance(Kernel_bin.Battle_Only_Statuses s) => sbyte.MaxValue;
            public override sbyte StatusResistance(Kernel_bin.Persistant_Statuses s) => sbyte.MaxValue;

            /// <summary>
            /// False if gf knows ability, True if can learn it.
            /// </summary>
            /// <param name="ability">Ability you want to learn.</param>
            /// <param name="item">
            /// If using an Item you don't need the prereq, set to false if need prereq
            /// </param>
            public bool TestGFCanLearn(Kernel_bin.Abilities ability, bool item = true) => !Complete[(int)ability] && ((item) || UnlockerTest(ability));

            /// <summary>
            /// If converting to string display GF's Name.
            /// </summary>
            public override string ToString() => Name.ToString();
            public override ushort TotalStat(Kernel_bin.Stat s) => 0;

            public bool UnlockerTest(Kernel_bin.Abilities a)
            {
                if (JunctionableGFsData.Ability.TryGetByKey(a, out Kernel_bin.Unlocker u))
                {
                    return UnlockerTest(u);
                }
                //return true if there is no prereq.
                return true;
            }

            public bool UnlockerTest(Kernel_bin.Unlocker u)
            {
                if (u == Kernel_bin.Unlocker.None)
                {
                    return true;
                }
                else if (u < Kernel_bin.Unlocker.GFLevel100)
                {
                    return ((byte)u <= Level);
                }
                else
                {
                    int ind = (u - Kernel_bin.Unlocker.GFLevel100);
                    if (JunctionableGFsData.Ability.TryGetKeyByIndex(ind, out Kernel_bin.Abilities key))
                        return Complete[(int)key];
                    else
                        return false;
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}