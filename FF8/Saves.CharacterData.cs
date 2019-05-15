using System.Collections.Generic;
using System.IO;

namespace FF8
{

    internal static partial class Saves
    {
        /// <summary>
        /// Data for each Character
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Characters"/>
        internal struct CharacterData
        {
            internal int Level => (int)((Experience / 1000) + 1);
            internal int ExperienceToNextLevel => (int)((Level) * 1000 - Experience);
            internal FF8String Name; //not saved to file.
            internal ushort CurrentHP; //0x00 -- forgot this one heh
            internal ushort MaxHPs; //0x02
            internal uint Experience; //0x02
            internal byte ModelID; //0x04
            internal byte WeaponID; //0x08
            internal byte STR; //0x09
            internal byte VIT; //0x0A
            internal byte MAG; //0x0B
            internal byte SPR; //0x0C
            internal byte SPD; //0x0D
            internal byte LCK; //0x0E
            internal ushort[] Magics; //0x0F
            internal byte[] Commands; //0x10
            internal byte Paddingorunusedcommand; //0x50
            internal uint Abilities; //0x53
            internal ushort JunctionnedGFs; //0x54
            internal byte Unknown1; //0x58
            internal byte Alternativemodel; //0x5A (Normal, SeeD, Soldier...)
            internal byte JunctionHP; //0x5B
            internal byte JunctionSTR; //0x5C
            internal byte JunctionVIT; //0x5D
            internal byte JunctionMAG; //0x5E
            internal byte JunctionSPR; //0x5F
            internal byte JunctionSPD; //0x60
            internal byte JunctionEVA; //0x61
            internal byte JunctionHIT; //0x62
            internal byte JunctionLCK; //0x63
            internal byte Junctionelementalattack; //0x64
            internal byte Junctionmentalattack; //0x65
            internal uint Junctionelementaldefense; //0x66
            internal uint Junctionmentaldefense; //0x67
            internal byte Unknown2; //0x6B (padding?)
            internal Dictionary<GFs, ushort> CompatibilitywithGFs; //0x6F
            internal ushort Numberofkills; //0x70
            internal ushort NumberofKOs; //0x90
            internal byte Exists; //0x92
            internal byte Unknown3; //0x94
            internal byte MentalStatus; //0x95
            internal byte Unknown4; //0x96

            internal void Read(BinaryReader br)
            {
                CurrentHP = br.ReadUInt16();//0x00
                MaxHPs = br.ReadUInt16();//0x02
                Experience = br.ReadUInt32();//0x04
                ModelID = br.ReadByte();//0x08
                WeaponID = br.ReadByte();//0x09
                STR = br.ReadByte();//0x0A
                VIT = br.ReadByte();//0x0B
                MAG = br.ReadByte();//0x0C
                SPR = br.ReadByte();//0x0D
                SPD = br.ReadByte();//0x0E
                LCK = br.ReadByte();//0x0F
                Magics = new ushort[32];
                for (int i = 0; i < 32; i++)
                    Magics[i] = br.ReadUInt16();//0x10
                Commands = br.ReadBytes(3);//0x50
                Paddingorunusedcommand = br.ReadByte();//0x53
                Abilities = br.ReadUInt32();//0x54
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

            public override string ToString() => Name.Length>0?Name.ToString():base.ToString();
        }
    }
}