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
            //public byte[] OffsetName;    //0x0000	2 bytes Offset to ability name

            //public byte[] OffsetDesc;    //0x0002	2 bytes Offset to ability description
            /// <summary>
            /// Ability data ID
            /// </summary>
            public byte Ability;           //0x0004	1 byte Ability data ID
            /// <summary>
            /// Unknown Flags
            /// </summary>
            public byte Flags;             //0x0005	1 byte Unknown Flags
            /// <summary>
            /// Target
            /// </summary>
            public byte Target;            //0x0006	1 byte Target
            /// <summary>
            /// Unknown / Unused
            /// </summary>
            public byte Unknown;           //0x0007	1 byte Unknown / Unused

            public void Read(BinaryReader br)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current);
                Ability = br.ReadByte();
                Flags = br.ReadByte();
                Target = br.ReadByte();
                Unknown = br.ReadByte();
            }
        }
    }
}

