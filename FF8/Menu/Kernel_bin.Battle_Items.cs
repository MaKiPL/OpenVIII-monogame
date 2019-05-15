using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Renzokuken Finishers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Renzokuken-finishers"/>
        internal struct Battle_Items_Data
        {
            internal const int id = 7;
            internal const int count = 33;

            public override string ToString() => Name;

            internal FF8String Name { get; private set; }

            //0x0000	2 bytes Offset to item name
            internal FF8String Description { get; private set; }

            //0x0002	2 bytes Offset to item description
            internal Magic_ID MagicID;

            //0x0004	2 bytes Magic ID
            internal Attack_Type Attack_Type;

            //0x0006	1 byte Attack type
            internal byte Attack_Power;

            //0x0007	1 byte Attack power
            internal byte Unknown0;

            //0x0008	1 byte Unknown
            internal Target Target;

            //0x0009	1 byte Target info
            internal byte Unknown1;

            //0x000A	1 byte Unknown
            internal Attack_Flags Attack_Flags;

            //0x000B	1 byte Attack flags
            internal byte Unknown2;

            //0x000C	1 bytes Unknown
            internal byte Status_Attack;

            //0x000D	1 byte Status Attack Enabler
            internal Statuses0 Statuses0;

            //0x000E	2 bytes status_0; //statuses 0-7
            internal Statuses1 Statuses1;

            //0x0010	4 bytes status_1; //statuses 8-39
            internal byte Attack_Param;

            //0x0014	1 byte Attack Param
            internal byte Unknown3;

            //0x0015	1 byte Unknown
            internal byte Hit_Count;

            //0x0016	1 bytes Hit Count
            internal Element Element;

            //0x0017	1 bytes Element

            internal void Read(BinaryReader br, int i)
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
        }
    }
}