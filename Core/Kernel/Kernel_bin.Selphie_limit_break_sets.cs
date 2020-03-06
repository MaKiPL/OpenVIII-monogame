using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    namespace Kernel
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
            private List<Slot> _slots;

            public IReadOnlyList<Slot> Slots { get => _slots; }

            public void Read(BinaryReader br, int i)
            {
                _slots = new List<Slot>(8);
                for (int s = 0; s < 8; s++)
                {
                    var tmp = new Slot();
                    tmp.Read(br);
                    _slots.Add(tmp);
                }
            }
            public static List<Selphie_limit_break_sets> Read(BinaryReader br)
            {
                var ret = new List<Selphie_limit_break_sets>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Selphie_limit_break_sets();
                    tmp.Read(br, i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}