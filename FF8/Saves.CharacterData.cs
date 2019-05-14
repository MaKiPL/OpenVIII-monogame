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
            public int Level => (int)((Experience / 1000) + 1);
            public int ExperienceToNextLevel => (int)((Level) * 1000 - Experience);
            public FF8String Name; //not saved to file.
            public ushort CurrentHP; //0x00 -- forgot this one heh
            public ushort MaxHPs; //0x02
            public uint Experience; //0x02
            public byte ModelID; //0x04
            public byte WeaponID; //0x08
            public byte STR; //0x09
            public byte VIT; //0x0A
            public byte MAG; //0x0B
            public byte SPR; //0x0C
            public byte SPD; //0x0D
            public byte LCK; //0x0E
            public ushort[] Magics; //0x0F
            public byte[] Commands; //0x10
            public byte Paddingorunusedcommand; //0x50
            public uint Abilities; //0x53
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
            public byte Exists; //0x92
            public byte Unknown3; //0x94
            public byte MentalStatus; //0x95
            public byte Unknown4; //0x96

            public void Read(BinaryReader br)
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