using Microsoft.Xna.Framework;
using System;

namespace FF8
{
    internal interface I_SP2
    {
        uint Count { get; }
        uint PalletCount { get; }

        Entry GetEntry(Enum id);
        Entry GetEntry(int id);

        void Init();

        void Draw(Enum id, Rectangle dst, float fade = 1f);
        void Draw(int id, Rectangle dst, float fade = 1f);

    }
}