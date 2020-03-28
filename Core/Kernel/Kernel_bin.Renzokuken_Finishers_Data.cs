using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Renzokuken Finishers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Renzokuken-finishers"/>
        public class Renzokuken_Finishers_Data
        {
            public const int id = 5;
            public const int count = 4;
            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }

            public override string ToString() => Name;

            public Magic_ID MagicID { get; private set; }          //0x0004	2 bytes Magic ID
            public Attack_Type Attack_Type { get; private set; }   //0x0006	1 byte Attack Type
            public byte Unknown0 { get; private set; }             //0x0007	1 byte Unknown
            public byte Attack_Power { get; private set; }         //0x0008	1 byte Attack power
            public byte Unknown1 { get; private set; }             //0x0009	1 byte Unknown
            public Target Target { get; private set; }             //0x000A	1 byte Target info
            public Attack_Flags Attack_Flags { get; private set; } //0x000B	1 byte Attack Flags
            public byte Hit_Count { get; private set; }            //0x000C	1 byte Hit count
            public Element Element { get; private set; }           //0x000D	1 byte Element Attack
            public byte Element_Percent { get; private set; }      //0x000E	1 byte Element Attack %
            public byte Status_Attack { get; private set; }        //0x000F	1 byte Status Attack Enabler
            public byte[] Unknown2 { get; private set; }           //0x0010	2 bytes Unknown
            public Persistent_Statuses Statuses0 { get; private set; }       //0x0012	2 bytes status_0; //statuses 0-7
            public Battle_Only_Statuses Statuses1 { get; private set; }       //0x0014	4 bytes status_1; //statuses 8-39

            public void Read(BinaryReader br, int i)
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
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();       //0x0012	2 bytes status_0; //statuses 0-7
                Statuses1 = (Battle_Only_Statuses)br.ReadUInt32();       //0x0014	4 bytes status_1; //statuses 8-39
            }

            public static Dictionary<Renzokuken_Finisher, Renzokuken_Finishers_Data> Read(BinaryReader br)
            {
                List<Kernel_bin.Renzokuken_Finisher> flags = Enum.GetValues(typeof(Kernel_bin.Renzokuken_Finisher))
                   .Cast<Kernel_bin.Renzokuken_Finisher>()
                   .ToList();
                var ret = new Dictionary<Renzokuken_Finisher, Renzokuken_Finishers_Data>(count);

                for (int i = 0; i < count; i++)
                {
                    Renzokuken_Finishers_Data tmp = new Renzokuken_Finishers_Data();
                    tmp.Read(br, i);
                    ret[flags[i]] = tmp;
                }
                return ret;
            }
        }
    }
}