using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Blue Magic Parameters - 4 for each spell for crisis level.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Blue_Magic_(Final_Fantasy_VIII)"/>
        /// <seealso cref="https://github.com/alexfilth/doomtrain/wiki/Quistis-limit-break-parameters"/>
        internal struct Quistis_limit_break_parameters
        {
            internal const int count = 4;//64 total but I want to add these to the Blue_magic_Quistis_limit_break in an array
            internal const int id = 20;
            internal const int size = 8;

            public Statuses1 Statuses1 { get; private set; }
            public Statuses0 Statuses0 { get; private set; }
            public byte Attack_Power { get; private set; }
            public byte Attack_Param { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x0000  4 bytes Status 1
                Statuses0 = (Statuses0)br.ReadUInt16();
                //0x0004  2 bytes Status 0
                Attack_Power = br.ReadByte();
                //0x0006  1 bytes Attack Power
                Attack_Param = br.ReadByte();
                //0x0007  1 byte Attack Param
            }
            internal static Quistis_limit_break_parameters[] Read(BinaryReader br)
            {
                var ret = new Quistis_limit_break_parameters[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Quistis_limit_break_parameters();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}