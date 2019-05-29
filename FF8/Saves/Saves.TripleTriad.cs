using System;
using System.IO;

namespace FF8
{
    public static partial class Saves
    {
        /// <summary>
        /// TT vars.
        /// </summary>
        /// <see cref="https://github.com/myst6re/hyne/blob/master/SaveData.h"/>
        public class TripleTriad //128
        {
            byte[] cards;
            byte[] card_locations;
            byte[] cards_rare;
            byte u1; // padding ?
            ushort tt_victory_count;
            ushort tt_defeat_count;
            ushort tt_egality_count;
            ushort u2;
            ulong u3;

            public TripleTriad()
            {
            }

            public TripleTriad(BinaryReader br) => Read(br);
            public void Read(BinaryReader br)
            {
                cards = br.ReadBytes(77);
                card_locations = br.ReadBytes(33);
                cards_rare = br.ReadBytes(5);
                u1 = br.ReadByte(); // padding ?
                tt_victory_count = br.ReadUInt16();
                tt_defeat_count = br.ReadUInt16();
                tt_egality_count = br.ReadUInt16();
                u2 = br.ReadUInt16();
                u3 = br.ReadUInt32();
            }

            internal TripleTriad Clone() => (TripleTriad) MemberwiseClone();
        }
    }
}