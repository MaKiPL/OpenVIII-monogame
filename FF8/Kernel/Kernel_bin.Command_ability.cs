using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Command Ability
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Command-ability-data"/>
        [Obsolete("Enum to be replaced with Kernel_Bin.Abilities", true)]
        public enum Command_ability
        {
            Recover,
            Revive,
            Treatment,
            Mad_Rush,
            Doom,
            Absorb,
            LV_Down,
            LV_Up,
            Kamikaze,
            Devour,
            Card,
            Defend,
        }
    }
}