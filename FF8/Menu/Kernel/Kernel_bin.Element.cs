using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Element types
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Elements"/>
        [Flags]
        public enum Element : byte
        {
            Non_Elemental = 0x00, Fire = 0x01, Ice = 0x02, Thunder = 0x04,
            Earth = 0x08, Poison = 0x10, Wind = 0x20, Water = 0x40, Holy = 0x80,
        }
    }
}