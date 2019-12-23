using System.IO;

namespace OpenVIII
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Slot Magic Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Selphie-limit-break-sets"/>
        public struct Slot
        {
            private byte _count;
            private byte _magicID;
            public Magic_Data Magic_Data => _magicID<MagicData.Count ? MagicData[_magicID]:null;
            public byte Casts { get => checked((byte)(Memory.Random.Next(_count)+1));}

            public void Read(BinaryReader br, int i = 0)
            {
                _magicID = br.ReadByte();
                _count = br.ReadByte();
            }
        }
    }
}