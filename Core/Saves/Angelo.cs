using System;

namespace OpenVIII
{
    [Flags]
    public enum Angelo : ushort
    {
        None = 0x0,
        Rush = 0x1,
        Recover = 0x2,
        Reverse = 0x4,
        Search = 0x8,
        Cannon = 0x10,
        Strike = 0x20,
        Invincible_Moon = 0x40,
        Wishing_Star = 0x80,

        /// <summary>
        /// this value is tracked in Saves.Data.BattleMISCIndicator I just put this here as a place holder.
        /// </summary>
        Angel_Wing = 0x100,
    }
}