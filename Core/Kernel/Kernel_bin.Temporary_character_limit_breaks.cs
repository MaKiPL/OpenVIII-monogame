using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Temporary Characters Limit Breaks
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Temporary-character-limit-breaks"/>
        public class Temporary_character_limit_breaks
        {
            public const int count = 5;
            public const int id = 18;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public Magic_ID MagicID { get; private set; }
            public AttackType Attack_Type { get; private set; }
            public byte Attack_Power { get; private set; }
            public byte[] Unknown0 { get; private set; }
            public Target Target { get; private set; }
            public AttackFlags Attack_Flags { get; private set; }
            public byte Hit_Count { get; private set; }
            public Element Element { get; private set; }
            public byte Element_Percent { get; private set; }
            public byte Status_Attack { get; private set; }
            public Persistent_Statuses Statuses0 { get; private set; }
            public byte[] Unknown1 { get; private set; }
            public Battle_Only_Statuses Statuses1 { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                Attack_Type = (AttackType)br.ReadByte();
                //0x0006  1 byte Attack Type
                Attack_Power = br.ReadByte();
                //0x0007  1 byte Attack Power
                Unknown0 = br.ReadBytes(2);
                //0x0008  2 bytes Unknown
                Target = (Target)br.ReadByte();
                //0x000A  1 byte Target Info
                Attack_Flags = (AttackFlags)br.ReadByte();
                //0x000B  1 byte Attack Flags
                Hit_Count = br.ReadByte();
                //0x000C  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x000D  1 byte Element Attack
                Element_Percent = br.ReadByte();
                //0x000E  1 byte Element Attack %
                Status_Attack = br.ReadByte();
                //0x000F  1 byte Status Attack Enabler
                Statuses0 = (Persistent_Statuses)br.ReadUInt16();
                //0x0010  2 bytes status_0; //statuses 0-7
                Unknown1 = br.ReadBytes(2);
                //0x0012  2 bytes Unknown
                Statuses1 = (Battle_Only_Statuses)br.ReadUInt32();
                //0x0014  4 bytes status_1; //statuses 8-39
            }
            public static List<Temporary_character_limit_breaks> Read(BinaryReader br)
            {
                var ret = new List<Temporary_character_limit_breaks>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Temporary_character_limit_breaks();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}