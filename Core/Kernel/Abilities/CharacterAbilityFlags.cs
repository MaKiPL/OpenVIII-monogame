using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    namespace Kernel
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Flags]
        public enum CharacterAbilityFlags : uint
        {
            None = 0x0,
            Mug = 0x1,
            MedData = 0x2,
            Counter = 0x4,
            ReturnDamage = 0x8,
            Cover = 0x10,
            Initiative = 0x10000,
            MoveHPUp = 0x20000,
            HPBonus = 0x80,
            StrBonus = 0x100,
            VitBonus = 0x200,
            MagBonus = 0x400,
            SprBonus = 0x800,
            AutoProtect = 0x4000,
            AutoShell = 0x2000,
            AutoReflect = 0x1000,
            AutoHaste = 0x8000,
            AutoPotion = 0x40000,
            Expend2 = 0x20,
            Expend3 = 0x40,
            Ribbon = 0x80000,
        }
    }
}