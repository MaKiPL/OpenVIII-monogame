using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        /// <summary>
        /// Junction Ability Flags
        /// </summary>
        /// <remarks>There are 115 abilities. Each type that uses flags seems to have their own set.
        /// Doomtrain seems to break it into a groups of 8 or 16, I might need to do that.</remarks>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        [Flags]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public enum JunctionAbilityFlags : uint // cannot contain all abilities because 
        {
            None = 0x_0,
            HP = 0x_1,
            Str = 0x_2,
            Vit = 0x_4,
            Mag = 0x_8,
            Spr = 0x_10,
            Spd = 0x_20,
            Eva = 0x_40,
            Hit = 0x_80,
            Luck = 0x_100,
            ElemAtk = 0x_200,
            StAtk = 0x_400,
            ElemDef = 0x_800,
            StDef = 0x_1000,
            ElemDef2 = 0x_2000,
            ElemDef4 = 0x_4000,
            StDef2 = 0x_8000,
            StDef4 = 0x_1_0000,
            Ability3 = 0x_2_0000,
            Ability4 = 0x_4_0000,
        }
    }
}