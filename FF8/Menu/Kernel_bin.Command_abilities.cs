using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Command Abilities
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-abilities"/>
        public class Command_abilities
        {
            public const int count = 19;
            public const int id = 12;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }
            public byte Index { get; private set; }
            public byte[] Unknown0 { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description

                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                Index = br.ReadByte();
                //0x0005  1 byte Index to Battle commands
                Unknown0 = br.ReadBytes(2);
                //0x0006  2 bytes Unknown/ Unused
            }
            public static Command_abilities[] Read(BinaryReader br)
            {
                var ret = new Command_abilities[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Command_abilities();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}