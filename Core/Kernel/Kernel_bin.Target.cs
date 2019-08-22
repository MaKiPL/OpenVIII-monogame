using System;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Target info
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/MainForm.cs"/>
        [Flags]
        public enum Target :byte
        {
            None = 0x0,
            Dead = 0x1,
            UNK02 = 0x2,
            Ally = 0x4,// was unk04
            Single_Side = 0x8,
            Single_Target = 0x10,
            UNK20 = 0x20,
            Enemy = 0x40,
            UNK80 = 0x80,
        }
    }
}