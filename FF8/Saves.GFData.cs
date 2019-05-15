using System.IO;

namespace FF8
{

    internal static partial class Saves
    {
        /// <summary>
        /// Data for each GF
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Guardian_Forces"/>
        internal struct GFData
        {
            
            internal FF8String Name; //Offset (0x00 terminated)
            internal uint Experience; //0x00
            internal byte Unknown; //0x0C
            internal byte Exists; //0x10
            internal ushort HP; //0x11
            internal byte[] Complete; //0x12 abilities (1 bit = 1 ability completed, 9 bits unused)
            internal byte[] APs; //0x14 (1 byte = 1 ability of the GF, 2 bytes unused)
            internal ushort NumberKills; //0x24 of kills
            internal ushort NumberKOs; //0x3C of KOs
            internal byte Learning; //0x3E ability
            internal byte[] Forgotten; //0x41 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)

            internal void Read(BinaryReader br)
            {
                Name = br.ReadBytes(12);//0x00 (0x00 terminated)
                Experience = br.ReadUInt32();//0x0C
                Unknown = br.ReadByte();//0x10
                Exists = br.ReadByte();//0x11
                HP = br.ReadUInt16();//0x12
                Complete = br.ReadBytes(16);//0x14 abilities (1 bit = 1 ability completed, 9 bits unused)
                APs = br.ReadBytes(24);//0x24 (1 byte = 1 ability of the GF, 2 bytes unused)
                NumberKills = br.ReadUInt16();//0x3C of kills
                NumberKOs = br.ReadUInt16();//0x3E of KOs
                Learning = br.ReadByte();//0x41 ability
                Forgotten = br.ReadBytes(3);//0x42 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)
            }

            public override string ToString() => Name.ToString();
        }
    }
}