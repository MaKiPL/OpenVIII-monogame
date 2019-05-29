using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Renzokuken Finishers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Renzokuken-finishers"/>
        public class Battle_Items_Data
        {
            public const int id = 7;
            public const int count = 33;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }

            //0x0000	2 bytes Offset to item name
            public FF8String Description { get; private set; }

            //0x0002	2 bytes Offset to item description
            public Magic_ID MagicID;

            //0x0004	2 bytes Magic ID
            public Attack_Type Attack_Type;

            //0x0006	1 byte Attack type
            public byte Attack_Power;

            //0x0007	1 byte Attack power
            public byte Unknown0;

            //0x0008	1 byte Unknown
            public Target Target;

            //0x0009	1 byte Target info
            public byte Unknown1;

            //0x000A	1 byte Unknown
            public Attack_Flags Attack_Flags;

            //0x000B	1 byte Attack flags
            public byte Unknown2;

            //0x000C	1 bytes Unknown
            public byte Status_Attack;

            //0x000D	1 byte Status Attack Enabler
            public Statuses0 Statuses0;

            //0x000E	2 bytes status_0; //statuses 0-7
            public Statuses1 Statuses1;

            //0x0010	4 bytes status_1; //statuses 8-39
            public byte Attack_Param;

            //0x0014	1 byte Attack Param
            public byte Unknown3;

            //0x0015	1 byte Unknown
            public byte Hit_Count;

            //0x0016	1 bytes Hit Count
            public Element Element;

            //0x0017	1 bytes Element

            public void Read(BinaryReader br, int i)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current);

                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to item name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to item description
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0004	2 bytes Magic ID
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0006	1 byte Attack type
                Attack_Power = br.ReadByte();
                //0x0007	1 byte Attack power
                Unknown0 = br.ReadByte();
                //0x0008	1 byte Unknown
                Target = (Target)br.ReadByte();
                //0x0009	1 byte Target info
                Unknown1 = br.ReadByte();
                //0x000A	1 byte Unknown
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x000B	1 byte Attack flags
                Unknown2 = br.ReadByte();
                //0x000C	1 bytes Unknown
                Status_Attack = br.ReadByte();
                //0x000D	1 byte Status Attack Enabler
                Statuses0 = (Statuses0)br.ReadUInt16();
                //0x000E	2 bytes status_0; //statuses 0-7
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x0010	4 bytes status_1; //statuses 8-39
                Attack_Param = br.ReadByte();
                //0x0014	1 byte Attack Param
                Unknown3 = br.ReadByte();
                //0x0015	1 byte Unknown
                Hit_Count = br.ReadByte();
                //0x0016	1 bytes Hit Count
                Element = (Element)br.ReadByte();
                //0x0017	1 bytes Element
            }

            public static Battle_Items_Data[] Read(BinaryReader br)
            {
                Battle_Items_Data[] ret = new Battle_Items_Data[count];

                for (int i = 0; i < count; i++)
                {
                    Battle_Items_Data tmp = new Battle_Items_Data();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}