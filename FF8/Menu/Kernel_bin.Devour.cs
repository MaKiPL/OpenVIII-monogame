using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Devour Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Devour"/>
        internal class Devour
        {
            internal const int count = 16;
            internal const int id = 28;
            internal const int size = 12;

            public override string ToString() => Description;

            public FF8String Description { get; private set; }
            public float Amount { get; private set; }

            /// <summary>
            /// True for heal, False for damage
            /// </summary>
            public bool DMGorHEAL { get; private set; }

            public Statuses1 Statuses1 { get; private set; }
            public Statuses0 Statuses0 { get; private set; }
            private StatFlags StatFlags { get; set; }
            public byte HP { get; private set; }

            internal void Read(BinaryReader br, int i)
            {
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                //0x0000  2 bytes Offset to devour description
                DMGorHEAL = br.ReadByte() == 0x1E ? true : false;
                //0x0002  1 byte Damage or heal HP and Status

                //0x1E - Cure
                //0x1F - Damage
                Quanity val = (Quanity)br.ReadByte();
                Amount = 0f;
                if ((val & Quanity._0625f) != 0) Amount += .0625f;
                if ((val & Quanity._1250f) != 0) Amount += .1250f;
                if ((val & Quanity._1f) != 0) Amount += 1f;
                if ((val & Quanity._25f) != 0) Amount += .25f;
                if ((val & Quanity._50f) != 0) Amount += .50f;
                //0x0003  1 byte HP Heal / DMG Quantity Flag

                //0x00 - 0 %
                //0x01 - 6.25 %
                //0x02 - 12.50 %
                //0x04 - 25 %
                //0x08 - 50 %
                //0x10 - 100 %
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x0004  4 bytes status_1; //statuses 8-39
                Statuses0 = (Statuses0)br.ReadUInt16();
                //0x0008  2 bytes status_0; //statuses 0-7

                StatFlags = (StatFlags)br.ReadByte();
                //0x000A  1 byte Raised Stat Flag

                //0x00 - None
                //0x01 - STR
                //0x02 - VIT
                //0x04 - MAG
                //0x08 - SPR
                //0x10 - SPD
                HP = br.ReadByte();
                //0x000B  1 byte Raised Stat HP Quantity
            }
            internal static Devour[] Read(BinaryReader br)
            {
                var ret = new Devour[count];

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Devour();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}