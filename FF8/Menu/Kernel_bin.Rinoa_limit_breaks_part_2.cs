using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Rinoa limit breaks (part 2)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Rinoa-limit-breaks-%28part-2%29"/>
        internal class Rinoa_limit_breaks_part_2
        {
            internal const int count = 33;
            internal const int id = 25;
            internal const int size = 20;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public Magic_ID MagicID { get; private set; }
            public Attack_Type Attack_Type { get; private set; }
            public byte Attack_Power { get; private set; }
            public Attack_Flags Attack_Flags { get; private set; }
            public byte Unknown0 { get; private set; }
            public Target Target { get; private set; }
            public byte Unknown1 { get; private set; }
            public byte Hit_Count { get; private set; }
            public Element Element { get; private set; }
            public byte Element_Percent { get; private set; }
            public byte Status_Attack { get; private set; }
            public Statuses0 Statuses0 { get; private set; }
            public Statuses1 Statuses1 { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i);
                //0x0000	2 bytes Offset to name
                br.BaseStream.Seek(2, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0002  2 bytes Magic ID
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0004  1 byte Attack type
                Attack_Power = br.ReadByte();
                //0x0005  1 byte Attack power
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x0006  1 byte Attack flags
                Unknown0 = br.ReadByte();
                //0x0007  1 byte Unknown
                Target = (Target)br.ReadByte();
                //0x0008  1 byte Target info
                Unknown1 = br.ReadByte();
                //0x0009  1 byte Unknown
                Hit_Count = br.ReadByte();
                //0x000A  1 byte Hit Count
                Element = (Element)br.ReadByte();
                //0x000B  1 byte Element Attack
                Element_Percent = br.ReadByte();
                //0x000C  1 byte Element Attack %
                Status_Attack = br.ReadByte();
                //0x000D  1 byte Status Attack Enabler
                Statuses0 = (Statuses0)br.ReadUInt16();
                //0x000E  2 bytes status_0; //statuses 0-7
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x0010  4 bytes status_1; //statuses 8-39
            }
            internal static Rinoa_limit_breaks_part_2[] Read(BinaryReader br)
            {
                var ret = new Rinoa_limit_breaks_part_2[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Rinoa_limit_breaks_part_2();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}