using System.Collections.Generic;
using System.IO;

namespace FF8
{

    public static partial class Saves
    {
        /// <summary>
        /// Data for each Character
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Characters"/>
        public struct CharacterData
        {
           
            public FF8String Name; //not saved to file.
            public ushort CurrentHP; //0x00 -- forgot this one heh
            public ushort _HP; //0x02
            public uint Experience; //0x02
            public byte ModelID; //0x04
            public byte WeaponID; //0x08
            public byte _STR; //0x09
            public byte _VIT; //0x0A
            public byte _MAG; //0x0B
            public byte _SPR; //0x0C
            public byte _SPD; //0x0D
            public byte _LCK; //0x0E
            public Dictionary<byte,byte> Magics; //0x0F
            public byte[] Commands; //0x10
            public byte Paddingorunusedcommand; //0x50
            public byte[] Abilities; //0x53
            public ushort JunctionnedGFs; //0x54
            public byte Unknown1; //0x58
            public byte Alternativemodel; //0x5A (Normal, SeeD, Soldier...)
            public byte JunctionHP; //0x5B
            public byte JunctionSTR; //0x5C
            public byte JunctionVIT; //0x5D
            public byte JunctionMAG; //0x5E
            public byte JunctionSPR; //0x5F
            public byte JunctionSPD; //0x60
            public byte JunctionEVA; //0x61
            public byte JunctionHIT; //0x62
            public byte JunctionLCK; //0x63
            public byte Junctionelementalattack; //0x64
            public byte Junctionmentalattack; //0x65
            public uint Junctionelementaldefense; //0x66
            public uint Junctionmentaldefense; //0x67
            public byte Unknown2; //0x6B (padding?)
            public Dictionary<GFs, ushort> CompatibilitywithGFs; //0x6F
            public ushort Numberofkills; //0x70
            public ushort NumberofKOs; //0x90
            public byte Exists; //0x92 //15,9,7,4,1 shows on menu, 0 locked, 6 hidden // I think
            public bool VisibleInMenu => Exists != 0 && Exists != 6;
            public bool CanAddToParty => true; // I'm sure one of the Exists values determines this but I donno yet.
            public byte Unknown3; //0x94
            public byte MentalStatus; //0x95
            public byte Unknown4; //0x96
            public Characters ID { get; private set; }
            public void Read(BinaryReader br,Characters c)
            {
                ID = c;
                CurrentHP = br.ReadUInt16();//0x00
                _HP = br.ReadUInt16();//0x02
                Experience = br.ReadUInt32();//0x04
                ModelID = br.ReadByte();//0x08
                WeaponID = br.ReadByte();//0x09
                _STR = br.ReadByte();//0x0A
                _VIT = br.ReadByte();//0x0B
                _MAG = br.ReadByte();//0x0C
                _SPR = br.ReadByte();//0x0D
                _SPD = br.ReadByte();//0x0E
                _LCK = br.ReadByte();//0x0F
                Magics = new Dictionary<byte, byte>(32);
                for (int i = 0; i < 32; i++)
                {
                    var key = br.ReadByte();
                    var val = br.ReadByte();
                    if(key >= 0 && !Magics.ContainsKey(key))
                    Magics.Add(key, val);//0x10                    
                }
                Commands = br.ReadBytes(3);//0x50
                Paddingorunusedcommand = br.ReadByte();//0x53
                Abilities = br.ReadBytes(4);//0x54
                JunctionnedGFs = br.ReadUInt16();//0x58
                Unknown1 = br.ReadByte();//0x5A
                Alternativemodel = br.ReadByte();//0x5B (Normal, SeeD, Soldier...)
                JunctionHP = br.ReadByte();//0x5C
                JunctionSTR = br.ReadByte();//0x5D
                JunctionVIT = br.ReadByte();//0x5E
                JunctionMAG = br.ReadByte();//0x5F
                JunctionSPR = br.ReadByte();//0x60
                JunctionSPD = br.ReadByte();//0x61
                JunctionEVA = br.ReadByte();//0x62
                JunctionHIT = br.ReadByte();//0x63
                JunctionLCK = br.ReadByte();//0x64
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
                if (c == Characters.Blank)
                    c = ID;
                int total = 0;
                foreach (var i in Abilities)
                {
                    int key = i - 0x27;
                    if (key >= 0 && key < Kernel_bin.Statpercentabilities.Length && Kernel_bin.Statpercentabilities[key].Stat == Kernel_bin.Stat.HP)
                        total += Kernel_bin.Statpercentabilities[key].Value;
                }

                return (ushort)Kernel_bin.CharacterStats[c].HP((sbyte)Level, JunctionHP, Magics[JunctionHP], _HP, total);
            }
            public float PercentFullHP => (float)CurrentHP / MaxHP();
            public override string ToString() => Name.Length>0?Name.ToString():base.ToString();
        }
    }
}