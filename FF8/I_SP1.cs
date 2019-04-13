using Microsoft.Xna.Framework;
using System;

namespace FF8
{
    internal interface I_SP1 : I_SP2
    {
        EntryGroup this[Enum id] { get; }
        EntryGroup this[int id] { get; }

        Entry GetEntry(Enum id, int index = 0);

        Entry GetEntry(int id, int index = 0);

        EntryGroup GetEntryGroup(Enum id);

        EntryGroup GetEntryGroup(int id);

        void Draw(Enum id, int pallet, Rectangle dst, float scale = 1f, float fade = 1f);
        void Draw(int id, int pallet, Rectangle dst, float scale = 1f, float fade = 1f);
    }
}