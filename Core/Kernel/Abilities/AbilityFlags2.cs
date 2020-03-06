using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        ///TODO remove any flag group that isn't used.
        ///TODO if are used correct the flag values.
        /// <summary>
        /// remaining abilities that doesn't fit in AbilityFlags.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum AbilityFlags2 : uint
        {
            Magic = 0x80000,
            GF = 0x100000,
            Draw = 0x200000,
            Item = 0x400000,
            Empty = 0x800000,
            Card = 0x1000000,
            Doom = 0x2000000,
            MadRush = 0x4000000,
            Treatment = 0x8000000,
            Defend = 0x10000000,
            Darkside = 0x20000000,
            Recover = 0x40000000,
            Absorb = 0x80000000,
            Revive = 0x1,
            LvDown = 0x2,
            LvUp = 0x4,
            Kamikaze = 0x8,
            Devour = 0x10,
            MiniMog = 0x20,
            HP20 = 0x40,
            HP40 = 0x80,
            HP80 = 0x100,
            STR20 = 0x200,
            STR40 = 0x400,
            STR60 = 0x800,
            VIT20 = 0x1000,
            VIT40 = 0x2000,
            VIT60 = 0x4000,
            MAG20 = 0x8000,
            MAG40 = 0x10000,
            MAG60 = 0x20000,
            SPR20 = 0x40000,
            SPR40 = 0x80000,
            SPR60 = 0x100000,
            SPD20 = 0x200000,
            SPD40 = 0x400000,
            Eva30 = 0x800000,
            Luck50 = 0x1000000,
            
        }
    }
}