using System.Collections;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Battle Commands
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Battle-commands"/>
        public struct Battle_Commands
        {

            public const int id = 0;
            public const int count = 39;
            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public override string ToString() => Name;

            //public byte[] OffsetName;    //0x0000	2 bytes Offset to ability name

            //public byte[] OffsetDesc;    //0x0002	2 bytes Offset to ability description
            /// <summary>
            /// Ability data ID
            /// </summary>
            public byte Ability;           //0x0004	1 byte Ability data ID
            /// <summary>
            /// Unknown Flags
            /// </summary>
            public BitArray Flags;             //0x0005	1 byte Unknown Flags
            /// <summary>
            /// Target
            /// </summary>
            public byte Target;            //0x0006	1 byte Target
            /// <summary>
            /// Unknown / Unused
            /// </summary>
            public byte Unknown;           //0x0007	1 byte Unknown / Unused

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2+1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                Ability = br.ReadByte();
                Flags = new BitArray(br.ReadBytes(1));
                Target = br.ReadByte();
                Unknown = br.ReadByte();
            }
        }
    }
}

