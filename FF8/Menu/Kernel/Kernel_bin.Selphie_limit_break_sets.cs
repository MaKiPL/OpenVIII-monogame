using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Slot Sets Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Selphie-limit-break-sets"/>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Slots_(ability_type)#Final_Fantasy_VIII"/>
        public class Selphie_limit_break_sets
        {
            public const int count = 16;
            public const int id = 27;
            public const int size = 16;
            public Slot[] Slots { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Slots = new Slot[8];
                for (int s = 0; s < 8; s++)
                {
                    Slots[s].Read(br, s);
                }
            }
            public static Selphie_limit_break_sets[] Read(BinaryReader br)
            {
                var ret = new Selphie_limit_break_sets[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Selphie_limit_break_sets();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}