using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Slot Array Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Slot-array"/>
        public class Slot_array
        {
            public const int count = 60;
            public const int id = 26;
            public const int size = 1;

            public byte SlotID { get; private set; }

            public void Read(BinaryReader br, int i) => SlotID = br.ReadByte();
            public static Slot_array[] Read(BinaryReader br)
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