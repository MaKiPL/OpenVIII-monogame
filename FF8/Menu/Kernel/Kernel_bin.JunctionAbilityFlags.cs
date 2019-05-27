using System;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Junction Ability Flags
        /// </summary>
        /// <remarks>There are 115 abilities. Each type that uses flags seems to have their own set.
        /// Doomtrain seems to break it into a groups of 8 or 16, I might need to do that.</remarks>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        [Flags]
        public enum JunctionAbilityFlags : uint // cannot contain all abilities because 
        {
            None = 0x0,
            HP_J = 0x1,
            Str_J = 0x2,
            Vit_J = 0x4,
            Mag_J = 0x8,
            Spr_J = 0x10,
            Spd_J = 0x20,
            Eva_J = 0x40,
            Hit_J = 0x80,
            Luck_J = 0x100,
            Elem_Atk_J = 0x200,
            ST_Atk_J = 0x400,
            Elem_Def_J = 0x800,
            ST_Def_J = 0x1000,
            Elem_Defx2 = 0x2000,
            Elem_Defx4 = 0x4000,
            ST_Def_Jx2 = 0x8000,
            ST_Def_Jx4 = 0x10000,
            Abilityx3 = 0x20000,
            Abilityx4 = 0x40000,
        }
    }
}