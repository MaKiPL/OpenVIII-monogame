using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Slot Array Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Slot-array"/>
        internal class Slot_array
        {
            internal const int count = 60;
            internal const int id = 26;
            internal const int size = 1;

            public byte SlotID { get; private set; }

            internal void Read(BinaryReader br, int i) => SlotID = br.ReadByte();
            internal static Slot_array[] Read(BinaryReader br)
            {
                var ret = new Slot_array[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Slot_array();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}