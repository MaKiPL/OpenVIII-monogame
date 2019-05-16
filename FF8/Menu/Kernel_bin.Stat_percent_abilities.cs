using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Stat Percentage Increasing Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Stat-percentage-increasing-abilities"/>
        internal class Stat_percent_abilities
        {
            internal const int count = 19;
            internal const int id = 13;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public Stat Stat { get; private set; }
            public byte Value { get; private set; }
            public byte Unknown0 { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                AP = br.ReadByte();
                //0x0004  1 byte AP needed to learn the ability
                Stat = (Stat)br.ReadByte();
                //0x0005  1 byte Stat to increase
                Value = br.ReadByte();
                //0x0006  1 byte Increase value
                Unknown0 = br.ReadByte();
                //0x0007  1 byte Unknown/ Unused
            }
            internal static Stat_percent_abilities[] Read(BinaryReader br)
            {
                var ret = new Stat_percent_abilities[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Stat_percent_abilities();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}