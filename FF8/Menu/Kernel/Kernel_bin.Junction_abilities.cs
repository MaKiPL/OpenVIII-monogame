using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Junction Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junction-abilities"/>
        public class Junction_abilities
        {
            public const int count = 20;
            public const int id = 11;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }

            //public BitArray J_Flags { get; private set; }
            public JunctionAbilityFlags J_Flags { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                //J_Flags = new BitArray(br.ReadBytes(3));
                byte[] tmp = br.ReadBytes(3);
                J_Flags = (JunctionAbilityFlags)(tmp[2] << 16 | tmp[1] << 8 | tmp[0]);

                //0x0005  3 byte J_Flag
            }

            public static Junction_abilities[] Read(BinaryReader br)
            {
                Junction_abilities[] ret = new Junction_abilities[count];

                for (int i = 0; i < count; i++)
                {
                    Junction_abilities tmp = new Junction_abilities();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}