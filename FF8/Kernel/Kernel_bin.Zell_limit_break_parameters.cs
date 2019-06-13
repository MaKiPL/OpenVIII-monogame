using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Zell Limit Break Parameters
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Zell-limit-break-parameters"/>
        public class Zell_limit_break_parameters
        {
            public const int count = 24;
            public const int id = 23;
            public const int size = 4;

            public byte[] Moves { get; private set; }

            public void Read(BinaryReader br, int i) => Moves = br.ReadBytes(4);//0x0000  1 byte[[StartMove 0//0x0001  1 byte  Next Sequence 0_1//0x0002  1 byte  Next Sequence 0_2//0x0003  1 byte  Next Sequence 0_3

            public static List<Zell_limit_break_parameters> Read(BinaryReader br)
            {
                var ret = new List<Zell_limit_break_parameters>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Zell_limit_break_parameters();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}