namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Misc Text Pointers Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Misc-text-pointers"/>
        internal class Misc_text_pointers
        {
            internal const int count = 128;
            internal const int id = 30;
            internal const int size = 2;

            public override string ToString() => Value;

            public static explicit operator FF8String(Misc_text_pointers v) => v.Value;

            internal FF8String Value { get; private set; }

            //0x0000	2 bytes Offset to item name

            internal void Read(int i) => Value = Memory.Strings.Read(Strings.FileID.KERNEL, id, i);//0x0000	2 bytes Offset to item name

            internal static Misc_text_pointers[] Read()
            {
                var ret = new Misc_text_pointers[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Misc_text_pointers();
                    tmp.Read(i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}