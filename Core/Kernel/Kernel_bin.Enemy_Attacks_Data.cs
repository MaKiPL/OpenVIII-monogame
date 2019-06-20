using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Enemy Attacks Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Enemy-attacks"/>
        public class Enemy_Attacks_Data
        {
            public const int id = 3;
            public const int count = 384;

            public FF8String Name { get; private set; }
            public override string ToString() => Name;
            //0x00	2 bytes Offset to attack name
            public Magic_ID MagicID { get; private set; } //0x02	2 bytes Magic ID
            public byte CameraChange { get; private set; } //0x04	1 byte Camera Change
            public byte Unknown0 { get; private set; } //0x05	1 byte Unknown
            public Attack_Type Attack_type { get; private set; }//0x06	1 byte Attack type
            public byte Attack_power { get; private set; }//0x07	1 byte Attack power
            public Attack_Flags Attack_flags { get; private set; }//0x08	1 byte Attack flags
            public byte Unknown1 { get; private set; }//0x09	1 byte Unknown
            public Element Element { get; private set; }//0x0A	1 byte Element
            public byte Unknown2 { get; private set; }//0x0B	1 byte Unknown
            public byte StatusAttack { get; private set; }//0x0C	1 byte Status attack enabler
            public byte Attack_parameter { get; private set; }//0x0D	1 byte Attack Parameter, HIT?
            public Persistant_Statuses Statuses0 { get; private set; }//0x0E	2 bytes status_0; //statuses 0-7
            public Battle_Only_Statuses Statuses1 { get; private set; }//0x10	4 bytes status_1; //statuses 8-31
            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16(); //0x02	2 bytes Magic ID
                CameraChange = br.ReadByte(); //0x04	1 byte Camera Change
                Unknown0 = br.ReadByte(); //0x05	1 byte Unknown
                Attack_type = (Attack_Type) br.ReadByte();//0x06	1 byte Attack type
                Attack_power = br.ReadByte();//0x07	1 byte Attack power
                Attack_flags = (Attack_Flags)(br.ReadByte());//0x08	1 byte Attack flags
                Unknown1 = br.ReadByte();//0x09	1 byte Unknown
                Element = (Element)br.ReadByte();//0x0A	1 byte Element
                Unknown2 = br.ReadByte();//0x0B	1 byte Unknown
                StatusAttack = br.ReadByte();//0x0C	1 byte Status attack enabler
                Attack_parameter = br.ReadByte();//0x0D	1 byte Attack Parameter
                //Statuses = new BitArray(br.ReadBytes(6));
                Statuses0 = (Persistant_Statuses)br.ReadUInt16();//0x0E	2 bytes status_0; //statuses 0-7
                Statuses1 = (Battle_Only_Statuses)br.ReadUInt32();//0x10	4 bytes status_1; //statuses 8-31
            }
            public static List<Enemy_Attacks_Data> Read(BinaryReader br)
            {
                var ret = new List<Enemy_Attacks_Data>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Enemy_Attacks_Data();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}