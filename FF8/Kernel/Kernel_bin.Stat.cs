using System.Collections.Generic;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Convert stat to stat junction
        /// </summary>
        public static Dictionary<Stat, Abilities> Stat2Ability = new Dictionary<Stat, Abilities>
                {
                    { Stat.HP, Abilities.HP_J },
                    { Stat.STR, Abilities.Str_J },
                    { Stat.VIT, Abilities.Vit_J},
                    { Stat.MAG, Abilities.Mag_J},
                    { Stat.SPR, Abilities.Spr_J },
                    { Stat.SPD, Abilities.Spd_J },
                    { Stat.EVA, Abilities.Eva_J },
                    { Stat.LUCK, Abilities.Luck_J },
                    { Stat.HIT, Abilities.Hit_J },
                };
        public enum Stat
        {
            HP,
            STR,
            VIT,
            MAG,
            SPR,
            SPD,
            EVA,
            HIT,
            LUCK
        }
    }
}