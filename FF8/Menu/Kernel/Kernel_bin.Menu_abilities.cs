using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Menu Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Menu-abilities"/>
        public class Menu_abilities :Ability
        {
            public new const int count = 24;
            public new const int id = 17;

            public override string ToString() => Name;

            public byte Index { get; private set; }
            public byte Start { get; private set; }
            public byte End { get; private set; }

            public override void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                Index = br.ReadByte();
                //0x0005  1 byte Index to m00X files in menu.fs
                //(first 3 sections are treated as special cases)
                Start = br.ReadByte();
                //0x0006  1 byte Start offset
                End = br.ReadByte();
                //0x0007  1 byte End offset
            }
            public static Dictionary<Abilities,Menu_abilities> Read(BinaryReader br)
            {
                Dictionary<Abilities, Menu_abilities> ret = new Dictionary<Abilities, Menu_abilities>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Menu_abilities();
                    tmp.Read(br, i);
                    ret[(Abilities)(i+(int)Abilities.Haggle)] = tmp;
                }
                return ret;
            }
        }
    }
}