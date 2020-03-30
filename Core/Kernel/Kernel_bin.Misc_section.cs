using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Misc Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Misc-section"/>
        public class Misc_section
        {
            public const int count = 1;
            public const int id = 29;

            public byte[] Status_Timers { get; private set; }
            public byte ATB_Speed_Multiplier { get; private set; }
            public byte Dead_Timer { get; private set; }
            public byte[] Status_Limit_Effects { get; private set; }
            public byte[] Duel_Timers_and_Start_Moves { get; private set; }
            public byte[] Shot_Timers { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Status_Timers = br.ReadBytes(14);
                ATB_Speed_Multiplier = br.ReadByte();
                Dead_Timer = br.ReadByte();
                Status_Limit_Effects = br.ReadBytes(32);
                Duel_Timers_and_Start_Moves = br.ReadBytes(8);
                Shot_Timers = br.ReadBytes(4);
            }
            public static List<Misc_section> Read(BinaryReader br)
            {
                var ret = new List<Misc_section>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Misc_section();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}