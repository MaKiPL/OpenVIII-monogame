using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public static partial class Saves
    {
        #region Classes

        /// <summary>
        /// TT vars.
        /// </summary>
        /// <see cref="https://github.com/myst6re/hyne/blob/master/SaveData.h"/>
        /// <seealso cref="https://github.com/myst6re/hyne/blob/master/PageWidgets/TTriadEditor.cpp"/>
        public class TripleTriad //128
        {
            #region Fields

            private const int NumberOfCards = 77;
            private const int NumberOfUniqueCards = 33;

            public OrderedDictionary<Cards.ID, TTCardInfo> cards;

            /// <summary>
            /// I'm not sure what this is for.
            /// </summary>
            public byte[] cards_rare;

            /// <summary>
            /// How often you lost a game
            /// </summary>
            public ushort tt_defeat_count;

            /// <summary>
            /// How often you had a draw
            /// </summary>
            public ushort tt_egality_count;

            /// <summary>
            /// How often you won a game
            /// </summary>
            public ushort tt_victory_count;

            public byte u1;

            public ushort u2;

            public ulong u3;

            #endregion Fields

            #region Constructors

            public TripleTriad()
            {
            }

            public TripleTriad(BinaryReader br) => Read(br);

            #endregion Constructors

            #region Methods

            public TripleTriad Clone()
            {
                TripleTriad tt = (TripleTriad)MemberwiseClone();
                tt.cards = new OrderedDictionary<Cards.ID, TTCardInfo>(cards.Count);
                foreach (KeyValuePair<Cards.ID, TTCardInfo> i in cards)
                    tt.cards.TryAdd(i.Key, i.Value.Clone());
                tt.cards_rare = (byte[])cards_rare.Clone();
                return tt;
            }

            public void Read(BinaryReader br)
            {
                //cards = br.ReadBytes(77);
                cards = new OrderedDictionary<Cards.ID, TTCardInfo>(NumberOfCards);
                for (byte i = 0; i < NumberOfCards; i++)
                {
                    cards.TryAdd((Cards.ID)i, new TTCardInfo(br.ReadByte()));
                }
                for (byte i = NumberOfCards - NumberOfUniqueCards; i < NumberOfCards; i++)
                    cards[(Cards.ID)i].Location = br.ReadByte();

                //card_locations = br.ReadBytes(33);
                cards_rare = br.ReadBytes(5);
                u1 = br.ReadByte(); // padding ?
                tt_victory_count = br.ReadUInt16();
                tt_defeat_count = br.ReadUInt16();
                tt_egality_count = br.ReadUInt16();
                u2 = br.ReadUInt16();
                u3 = br.ReadUInt32();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}