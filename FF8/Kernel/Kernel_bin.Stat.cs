using System.Collections.Generic;

namespace FF8
{
    public partial class Kernel_bin
    {
        #region Fields

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
                    { Stat.EL_Atk, Abilities.Elem_Atk_J },
                    { Stat.ST_Atk, Abilities.ST_Atk_J },
                    { Stat.EL_Def_1, Abilities.Elem_Def_Jx1 },//or Elem_Def_Jx2 or Elem_Def_Jx4
                    { Stat.EL_Def_2, Abilities.Elem_Def_Jx2 },//or Elem_Def_Jx4
                    { Stat.EL_Def_3, Abilities.Elem_Def_Jx4 },
                    { Stat.EL_Def_4, Abilities.Elem_Def_Jx4 },
                    { Stat.ST_Def_1, Abilities.ST_Def_Jx1 },//or ST_Def_Jx2 or ST_Def_Jx4
                    { Stat.ST_Def_2, Abilities.ST_Def_Jx2 },//or ST_Def_Jx4
                    { Stat.ST_Def_3, Abilities.ST_Def_Jx4 },
                    { Stat.ST_Def_4, Abilities.ST_Def_Jx4 },
                };

        #endregion Fields

        #region Enums

        public enum Stat :byte
        {
            HP,
            STR,
            VIT,
            MAG,
            SPR,
            SPD,
            EVA,
            HIT,
            LUCK,
            EL_Atk,
            ST_Atk,
            EL_Def_1,
            EL_Def_2,
            EL_Def_3,
            EL_Def_4,
            ST_Def_1,
            ST_Def_2,
            ST_Def_3,
            ST_Def_4,
            None = 0xFF,
        }

        #endregion Enums
    }
}