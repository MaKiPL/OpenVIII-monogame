using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Misc Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Misc-section"/>
        internal class Misc_section
        {
            internal const int count = 1;
            internal const int id = 29;

            public byte[] Status_Timers { get; private set; }
            public byte ATB_Speed_Multiplier { get; private set; }
            public byte Dead_Timer { get; private set; }
            public byte[] Status_Limit_Effects { get; private set; }
            public byte[] Duel_Timers_and_Start_Moves { get; private set; }
            public byte[] Shot_Timers { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Status_Timers = br.ReadBytes(14);
                ATB_Speed_Multiplier = br.ReadByte();
                Dead_Timer = br.ReadByte();
                Status_Limit_Effects = br.ReadBytes(32);
                Duel_Timers_and_Start_Moves = br.ReadBytes(8);
                Shot_Timers = br.ReadBytes(4);
            }
            internal static Misc_section[] Read(BinaryReader br)
            {
                var ret = new Misc_section[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Misc_section();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}