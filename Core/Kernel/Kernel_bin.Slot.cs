using System.IO;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Slot Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Selphie-limit-break-sets"/>
        public struct Slot
        {
            private byte _count;
            private byte _magicID;
            public MagicData Magic_Data => Memory.Kernel_Bin.MagicData != null && _magicID < Memory.Kernel_Bin.MagicData.Count ? Memory.Kernel_Bin.MagicData[_magicID] : null;

            public byte Casts => checked((byte)(Memory.Random.Next(_count) + 1));

            public void Read(BinaryReader br, int i = 0)
            {
                _magicID = br.ReadByte();
                _count = br.ReadByte();
            }
        }
    }
}
