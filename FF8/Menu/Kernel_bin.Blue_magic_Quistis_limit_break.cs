using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Blue magic (Quistis limit break)
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Blue-magic-%28Quistis-limit-break%29"/>
        internal class Blue_magic_Quistis_limit_break
        {
            internal const int count = 16;
            internal const int id = 19;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public Magic_ID MagicID { get; private set; }
            public byte Unknown0 { get; private set; }
            public Attack_Type Attack_Type { get; private set; }
            public byte[] Unknown1 { get; private set; }
            public Attack_Flags Attack_Flags { get; private set; }
            public byte Unknown2 { get; private set; }
            public Element Element { get; private set; }
            public byte Status_Attack { get; private set; }
            public byte Crit { get; private set; }
            public byte Unknown3 { get; private set; }
            public Quistis_limit_break_parameters[] Crisis_Levels { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0004  2 bytes Magic ID
                Unknown0 = br.ReadByte();
                //0x0006  1 byte Unknown
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0007  1 byte Attack Type
                Unknown1 = br.ReadBytes(2);
                //0x0008  2 bytes Unknown
                Attack_Flags = (Attack_Flags)br.ReadByte();
                //0x000A  1 byte Attack Flags
                Unknown2 = br.ReadByte();
                //0x000B  1 byte Unknown
                Element = (Element)br.ReadByte();
                //0x000C  1 byte Element
                Status_Attack = br.ReadByte();
                //0x000D  1 byte Status Attack
                Crit = br.ReadByte();
                //0x000E  1 byte Crit Bonus
                Unknown3 = br.ReadByte();
                //0x000F  1 byte Unknown
                long current = br.BaseStream.Position;

                br.BaseStream.Seek(Memory.Strings.Files[Strings.FileID.KERNEL].subPositions[Quistis_limit_break_parameters.id + Quistis_limit_break_parameters.size * i], SeekOrigin.Begin);
                Crisis_Levels = new Quistis_limit_break_parameters[Quistis_limit_break_parameters.size];
                for (i = 0; i < Quistis_limit_break_parameters.count; i++)
                    Crisis_Levels[i].Read(br, i);
                br.BaseStream.Seek(current, SeekOrigin.Begin);
            }
            internal static Blue_magic_Quistis_limit_break[] Read(BinaryReader br)
            {
                var ret = new Blue_magic_Quistis_limit_break[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Blue_magic_Quistis_limit_break();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}