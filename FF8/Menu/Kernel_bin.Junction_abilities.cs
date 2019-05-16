using System.Collections;
using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Junction Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junction-abilities"/>
        internal class Junction_abilities
        {
            internal const int count = 20;
            internal const int id = 11;

            public override string ToString() => Name;

            internal FF8String Name { get; private set; }
            internal FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public BitArray J_Flags { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                J_Flags = new BitArray(br.ReadBytes(3));
                //0x0005  3 byte J-Flag
            }

            internal static Junction_abilities[] Read(BinaryReader br)
            {
                var ret = new Junction_abilities[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Junction_abilities();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}