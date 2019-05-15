using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Renzokuken Finishers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Renzokuken-finishers"/>
        internal struct Renzokuken_Finishers_Data
        {
            internal const int id = 5;
            internal const int count = 4;
            internal FF8String Name { get; private set; }
            internal FF8String Description { get; private set; }

            public override string ToString() => Name;

            internal Magic_ID MagicID;          //0x0004	2 bytes Magic ID
            internal Attack_Type Attack_Type;   //0x0006	1 byte Attack Type
            internal byte Unknown0;             //0x0007	1 byte Unknown
            internal byte Attack_Power;         //0x0008	1 byte Attack power
            internal byte Unknown1;             //0x0009	1 byte Unknown
            internal Target Target;             //0x000A	1 byte Target info
            internal Attack_Flags Attack_Flags; //0x000B	1 byte Attack Flags
            internal byte Hit_Count;            //0x000C	1 byte Hit count
            internal Element Element;           //0x000D	1 byte Element Attack
            internal byte Element_Percent;      //0x000E	1 byte Element Attack %
            internal byte Status_Attack;        //0x000F	1 byte Status Attack Enabler
            internal byte[] Unknown2;           //0x0010	2 bytes Unknown
            internal Statuses0 Statuses0;       //0x0012	2 bytes status_0; //statuses 0-7
            internal Statuses1 Statuses1;       //0x0014	4 bytes status_1; //statuses 8-39

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16();          //0x0004	2 bytes Magic ID
                Attack_Type = (Attack_Type)br.ReadByte();   //0x0006	1 byte Attack Type
                Unknown0 = br.ReadByte();             //0x0007	1 byte Unknown
                Attack_Power = br.ReadByte();         //0x0008	1 byte Attack power
                Unknown1 = br.ReadByte();             //0x0009	1 byte Unknown
                Target = (Target)br.ReadByte();             //0x000A	1 byte Target info
                Attack_Flags = (Attack_Flags)br.ReadByte(); //0x000B	1 byte Attack Flags
                Hit_Count = br.ReadByte();            //0x000C	1 byte Hit count
                Element = (Element)br.ReadByte();           //0x000D	1 byte Element Attack
                Element_Percent = br.ReadByte();      //0x000E	1 byte Element Attack %
                Status_Attack = br.ReadByte();        //0x000F	1 byte Status Attack Enabler
                Unknown2 = br.ReadBytes(2);             //0x0010	2 bytes Unknown
                Statuses0 = (Statuses0)br.ReadUInt16();       //0x0012	2 bytes status_0; //statuses 0-7
                Statuses1 = (Statuses1)br.ReadUInt32();       //0x0014	4 bytes status_1; //statuses 8-39
            }
        }
    }
}