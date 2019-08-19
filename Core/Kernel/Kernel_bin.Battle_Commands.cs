using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Battle Commands
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Battle-commands"/>
        public class Battle_Commands
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
            public byte Ability { get; private set; }           //0x0004	1 byte Ability data ID
            /// <summary>
            /// Unknown Flags
            /// </summary>
            public BitArray Flags { get; private set; }             //0x0005	1 byte Unknown Flags
            /// <summary>
            /// Target
            /// </summary>
            public Target Target { get; private set; }            //0x0006	1 byte Target
            /// <summary>
            /// Unknown / Unused
            /// </summary>
            public byte Unknown { get; private set; }           //0x0007	1 byte Unknown / Unused

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2+1);
                br.BaseStream.Seek(4, SeekOrigin.Current);
                Ability = br.ReadByte();
                Flags = new BitArray(br.ReadBytes(1));
                Target = (Target)br.ReadByte();
                Unknown = br.ReadByte();
            }

            public static List<Battle_Commands> Read(BinaryReader br)
            {
                var ret = new List<Battle_Commands>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Battle_Commands();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}

