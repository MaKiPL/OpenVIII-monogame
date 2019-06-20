using System.Collections;
using System.IO;

namespace OpenVIII
{
    public static partial class Saves
    {
        /// <summary>
        /// Data for each GF
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#Guardian_Forces"/>
        public class GFData
        {
            public GFs ID { get; private set; }

            public FF8String Name; //Offset (0x00 terminated)
            public uint Experience; //0x00
            public byte Unknown; //0x0C
            public byte Exists; //0x10
            public ushort HP; //0x11
            public BitArray Complete; //0x12 abilities (1 bit = 1 ability completed, 9 bits unused)
            public byte[] APs; //0x14 (1 byte = 1 ability of the GF, 2 bytes unused)
            public ushort NumberKills; //0x24 of kills
            public ushort NumberKOs; //0x3C of KOs
            public byte Learning; //0x3E ability
            public byte[] Forgotten; //0x41 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)

            public GFData()
            {
            }

            public GFData(BinaryReader br, GFs g) => Read(br, g);

            public void Read(BinaryReader br, GFs g)
            {
                ID = g;
                Name = br.ReadBytes(12);//0x00 (0x00 terminated)
                Experience = br.ReadUInt32();//0x0C
                Unknown = br.ReadByte();//0x10
                Exists = br.ReadByte();//0x11
                HP = br.ReadUInt16();//0x12
                Complete = new BitArray(br.ReadBytes(16));//0x14 abilities (1 bit = 1 ability completed, 9 bits unused)
                APs = br.ReadBytes(24);//0x24 (1 byte = 1 ability of the GF, 2 bytes unused)
                NumberKills = br.ReadUInt16();//0x3C of kills
                NumberKOs = br.ReadUInt16();//0x3E of KOs
                Learning = br.ReadByte();//0x41 ability
                Forgotten = br.ReadBytes(3);//0x42 abilities (1 bit = 1 ability of the GF forgotten, 2 bits unused)
            }

            public byte Level
            {
                get
                {
                    uint ret = (Experience / Kernel_bin.JunctionableGFsData[ID].EXPperLevel);
                    return ret > 100 ? (byte)100 : (byte)ret;
                }
            }

            public ushort EXPtoNextLevel => Level >= 100 ? (ushort)0 : (ushort)(Experience - (Level * Kernel_bin.JunctionableGFsData[ID].EXPperLevel));

            public override string ToString() => Name.ToString();
        }
    }
}