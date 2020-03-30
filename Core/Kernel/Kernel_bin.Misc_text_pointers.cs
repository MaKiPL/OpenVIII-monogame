using System.Collections.Generic;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Misc Text Pointers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Misc-text-pointers"/>
        public class Misc_text_pointers
        {
            public const int count = 128;
            public const int id = 30;
            public const int size = 2;

            public override string ToString() => Value;

            public static explicit operator FF8String(Misc_text_pointers v) => v.Value;

            public FF8String Value { get; private set; }

            //0x0000	2 bytes Offset to item name

            public void Read(int i) => Value = Memory.Strings.Read(Strings.FileID.KERNEL, id, i);//0x0000	2 bytes Offset to item name

            public static List<Misc_text_pointers> Read()
            {
                var ret = new List<Misc_text_pointers>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Misc_text_pointers();
                    tmp.Read(i);
                    ret.Add(tmp);
                }
                return ret;
            }
        }
    }
}