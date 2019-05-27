using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Target info
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/MainForm.cs"/>
        [Flags]
        public enum Target
        {
            Dead = 0x01,
            UNK02 = 02,
            UNK04 = 04,
            Single_Side = 0x08,
            Single_Target = 0x10,
            UNK20 = 20,
            Enemy = 0x40,
            UNK80 = 80,
        }
    }
}