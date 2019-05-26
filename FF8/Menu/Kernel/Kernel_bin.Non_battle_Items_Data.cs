namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Non battle Items Mame and Description Offsets Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Non-battle-item-name-and-description-offsets"/>
        public struct Non_battle_Items_Data
        {
            public static readonly int count = 166;
            public static readonly int id = 8;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }

            //0x0000	2 bytes Offset to item name
            public FF8String Description { get; private set; }

            //0x0002	2 bytes Offset to item description

            public void Read(int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to item name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to item description
                //br.BaseStream.Seek(4,SeekOrigin.Current);
            }
        }
    }
}