using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{

    public static partial class Saves
    {
        /// <summary>
        /// Data for each Character
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Characters"/>
        public class CharacterData 
        {
            
            public FF8String Name; //not saved to file.
            private ushort _CurrentHP; //0x00 -- forgot this one heh
            /// <summary>
            /// Raw HP buff from items.
            /// </summary>
            public ushort _HP; //0x02
            public uint Experience; //0x02
            public byte ModelID; //0x04
            public byte WeaponID; //0x08
            /// <summary>
            /// Stats that can be incrased via items. Except for HP because it's a ushort not a byte.
            /// </summary>
            public Dictionary<Kernel_bin.Stat, byte> RawStats;
            //public byte _STR; //0x09
            //public byte _VIT; //0x0A
            //public byte _MAG; //0x0B
            //public byte _SPR; //0x0C
            //public byte _SPD; //0x0D
            //public byte _LCK; //0x0E
            public Dictionary<byte,byte> Magics; //0x0F
            public List<Kernel_bin.Abilities> Commands; //0x10
            public byte Paddingorunusedcommand; //0x50
            public List<Kernel_bin.Abilities> Abilities; //0x53
            public GFflags JunctionnedGFs; //0x54
            public byte Unknown1; //0x58
            public byte Alternativemodel; //0x5A (Normal, SeeD, Soldier...)
            public Dictionary<Kernel_bin.Stat,byte> JunctionStat;
            //public byte JunctionHP; //0x5B
            //public byte JunctionSTR; //0x5C
            //public byte JunctionVIT; //0x5D
            //public byte JunctionMAG; //0x5E
            //public byte JunctionSPR; //0x5F
            //public byte JunctionSPD; //0x60
            //public byte JunctionEVA; //0x61
            //public byte JunctionHIT; //0x62
            //public byte JunctionLCK; //0x63
            public byte Junctionelementalattack; //0x64
            public byte Junctionmentalattack; //0x65
            public uint Junctionelementaldefense; //0x66
            public uint Junctionmentaldefense; //0x67
            public byte Unknown2; //0x6B (padding?)
            public Dictionary<GFs, ushort> CompatibilitywithGFs; //0x6F
            public ushort Numberofkills; //0x70
            public ushort NumberofKOs; //0x90
            public byte Exists; //0x92 //15,9,7,4,1 shows on menu, 0 locked, 6 hidden // I think I wonder if this is a flags value.
            public bool VisibleInMenu => Exists != 0 && Exists != 6;
            public bool CanAddToParty => true; // I'm sure one of the Exists values determines this but I donno yet.
            public byte Unknown3; //0x94
            public byte MentalStatus; //0x95
            public byte Unknown4; //0x96

            public CharacterData()
            {
            }

            public CharacterData(BinaryReader br, Characters c) => Read(br, c);
            public Characters ID { get; private set; }
            public List<Kernel_bin.Abilities> UnlockedGFAbilities
            {
                get
                {
                    BitArray total = new BitArray(16*8);
                    List<Kernel_bin.Abilities> abilities = new List<Kernel_bin.Abilities>();
                    var availableFlags = Enum.GetValues(typeof(GFflags)).Cast<Enum>();
                    foreach (var flag in availableFlags.Where(JunctionnedGFs.HasFlag))
                    {
                        if ((GFflags)flag == GFflags.None) continue;
                        total.Or(Memory.State.GFs[ConvertGFEnum[(GFflags)flag]].Complete);
                    }
                    for(var i=1; i< total.Length; i++)//0 is none so skipping it.
                    {
                        if(total[i])
                            abilities.Add((Kernel_bin.Abilities)i);
                    }

                    return abilities;
                }
            }
            public void Read(BinaryReader br,Characters c)
            {
                ID = c;
                _CurrentHP = br.ReadUInt16();//0x00
                _HP = br.ReadUInt16();//0x02
                Experience = br.ReadUInt32();//0x04
                ModelID = br.ReadByte();//0x08
                WeaponID = br.ReadByte();//0x09
                RawStats = new Dictionary<Kernel_bin.Stat, byte>(6);
                RawStats[Kernel_bin.Stat.STR] = br.ReadByte();//0x0A
                RawStats[Kernel_bin.Stat.VIT] = br.ReadByte();//0x0B
                RawStats[Kernel_bin.Stat.MAG] = br.ReadByte();//0x0C
                RawStats[Kernel_bin.Stat.SPR] = br.ReadByte();//0x0D
                RawStats[Kernel_bin.Stat.SPD] = br.ReadByte();//0x0E
                RawStats[Kernel_bin.Stat.LUCK] = br.ReadByte();//0x0F
                Magics = new Dictionary<byte, byte>(33);
                for (int i = 0; i < 32; i++)
                {
                    var key = br.ReadByte();
                    var val = br.ReadByte();
                    if(key >= 0 && !Magics.ContainsKey(key))
                    Magics.Add(key, val);//0x10                    
                }
                Commands = Array.ConvertAll(br.ReadBytes(3), Item => (Kernel_bin.Abilities)Item).ToList();//0x50
                Paddingorunusedcommand = br.ReadByte();//0x53
                Abilities = Array.ConvertAll(br.ReadBytes(4), Item => (Kernel_bin.Abilities)Item).ToList();//0x54
                JunctionnedGFs = (GFflags)br.ReadUInt16();//0x58
                Unknown1 = br.ReadByte();//0x5A
                Alternativemodel = br.ReadByte();//0x5B (Normal, SeeD, Soldier...)
                JunctionStat = new Dictionary<Kernel_bin.Stat, byte>(9);
                for (int i = 0; i < 9; i++)
                {
                    var key = (Kernel_bin.Stat)i;
                    var val = br.ReadByte();
                    if (!JunctionStat.ContainsKey(key))
                        JunctionStat.Add(key, val);                   
                }
                //JunctionHP = br.ReadByte();//0x5C
                //JunctionSTR = br.ReadByte();//0x5D
                //JunctionVIT = br.ReadByte();//0x5E
                //JunctionMAG = br.ReadByte();//0x5F
                //JunctionSPR = br.ReadByte();//0x60
                //JunctionSPD = br.ReadByte();//0x61
                //JunctionEVA = br.ReadByte();//0x62
                //JunctionHIT = br.ReadByte();//0x63
                //JunctionLCK = br.ReadByte();//0x64
                Junctionelementalattack = br.ReadByte();//0x65
                Junctionmentalattack = br.ReadByte();//0x66
                Junctionelementaldefense = br.ReadUInt32();//0x67
                Junctionmentaldefense = br.ReadUInt32();//0x6B
                Unknown2 = br.ReadByte();//0x6F (padding?)
                CompatibilitywithGFs = new Dictionary<GFs,ushort>(16);
                for (int i = 0; i < 16; i++)
                    CompatibilitywithGFs.Add((GFs)i,br.ReadUInt16());//0x70
                Numberofkills = br.ReadUInt16();//0x90
                NumberofKOs = br.ReadUInt16();//0x92
                Exists = br.ReadByte();//0x94
                Unknown3 = br.ReadByte();//0x95
                MentalStatus = br.ReadByte();//0x96
                Unknown4 = br.ReadByte();//0x97
            }
            public int Level => (int)((Experience / 1000) + 1);
            public int ExperienceToNextLevel => (int)((Level) * 1000 - Experience);

            /// <summary>
            /// Max HP
            /// </summary>
            /// <param name="c">Force another character's HP calculation</param>
            /// <returns></returns>
            public ushort MaxHP(Characters c = Characters.Blank)
            {
                return TotalStat(Kernel_bin.Stat.HP, c);
            }
            public ushort TotalStat(Kernel_bin.Stat s, Characters c = Characters.Blank)
            {
                if (c == Characters.Blank)
                    c = ID;
                int total = 0;
                foreach (var i in Abilities)
                {
                    if (Kernel_bin.Statpercentabilities.ContainsKey(i) && Kernel_bin.Statpercentabilities[i].Stat == s)
                        total += Kernel_bin.Statpercentabilities[i].Value;
                }
                switch (s)
                {
                    case Kernel_bin.Stat.HP:
                        return Kernel_bin.CharacterStats[c].HP((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]], _HP, total);
                    case Kernel_bin.Stat.EVA:
                        //TODO confirm if there is no flat stat buff for eva. If there isn't then remove from function.
                        return Kernel_bin.CharacterStats[c].EVA((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]],0, TotalStat(Kernel_bin.Stat.SPD,c), total);
                    case Kernel_bin.Stat.SPD:
                        return Kernel_bin.CharacterStats[c].SPD((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]], RawStats[s], total);
                    case Kernel_bin.Stat.HIT:
                        return Kernel_bin.CharacterStats[c].HIT(JunctionStat[s], JunctionStat[s] == 0 ? 0:Magics[JunctionStat[s]], WeaponID);
                    case Kernel_bin.Stat.LUCK:
                        return Kernel_bin.CharacterStats[c].LUCK((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]], RawStats[s], total);
                    case Kernel_bin.Stat.MAG:
                        return Kernel_bin.CharacterStats[c].MAG((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]], RawStats[s], total);
                    case Kernel_bin.Stat.SPR:
                        return Kernel_bin.CharacterStats[c].SPR((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]], RawStats[s], total);
                    case Kernel_bin.Stat.STR:
                        return Kernel_bin.CharacterStats[c].STR((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]], RawStats[s], total,WeaponID);
                    case Kernel_bin.Stat.VIT:
                        return Kernel_bin.CharacterStats[c].VIT((sbyte)Level, JunctionStat[s], JunctionStat[s] == 0 ? 0 : Magics[JunctionStat[s]], RawStats[s], total);

                }
                return 0;
            }
            public ushort CurrentHP(Characters c = Characters.Blank)
            {
                ushort max = MaxHP(c);
                if (max < _CurrentHP) _CurrentHP = max;
                return _CurrentHP;
            }

            public float PercentFullHP(Characters c = Characters.Blank) => (float)_CurrentHP / MaxHP(c);
            public override string ToString() => Name.Length>0?Name.ToString():base.ToString();
            public CharacterData Clone()
            {
                //Shadowcopy
                CharacterData c = (CharacterData)MemberwiseClone();
                //Deepcopy
                c.CompatibilitywithGFs = CompatibilitywithGFs.ToDictionary(e => e.Key, e => e.Value);
                c.JunctionStat = JunctionStat.ToDictionary(e => e.Key, e => e.Value);
                c.Magics = Magics.ToDictionary(e => e.Key, e => e.Value);
                c.JunctionStat = JunctionStat.ToDictionary(e => e.Key, e => e.Value);
                c.Magics = Magics.ToDictionary(e => e.Key, e => e.Value);
                c.RawStats = RawStats.ToDictionary(e => e.Key, e => e.Value);
                c.Commands = Commands.ConvertAll(Item => Item);
                c.Abilities = Abilities.ConvertAll(Item => Item);
                return c;
            }
        }
    }
}