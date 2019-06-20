namespace FF8
{
    public partial class Kernel_bin
    {
        public enum CharacterAbilityFlags : uint
        {
            None = 0x0,
            Mug = 0x1,
            MedData = 0x2,
            Counter = 0x4,
            Return_Damage = 0x8,
            Cover = 0x10,
            Initiative = 0x10000,
            Move_HPUp = 0x20000,
            HPBonus = 0x80,
            StrBonus = 0x100,
            VitBonus = 0x200,
            MagBonus = 0x400,
            SprBonus = 0x800,
            Auto_Protect = 0x4000,
            Auto_Shell = 0x2000,
            Auto_Reflect = 0x1000,
            Auto_Haste = 0x8000,
            AutoPotion = 0x40000,
            Expendx2_1 = 0x20,
            Expendx3_1 = 0x40,
            Ribbon = 0x80000,
        }
    }
}