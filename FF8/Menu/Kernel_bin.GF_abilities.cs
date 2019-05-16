using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// GF Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/GF-abilities"/>
        internal class GF_abilities
        {
            internal const int count = 9;
            internal const int id = 16;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public byte Boost { get; private set; }
            public Stat Stat { get; private set; }
            public byte Value { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004 1 byte AP needed to learn the ability
                Boost = br.ReadByte();
                //0x0005 Enable Boost
                Stat = (Stat)br.ReadByte();
                //0x0006  1 byte Stat to increase
                Value = br.ReadByte();
                //0x0007  1 byte Increase value
            }
            internal static GF_abilities[] Read(BinaryReader br)
            {
                var ret = new GF_abilities[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new GF_abilities();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}