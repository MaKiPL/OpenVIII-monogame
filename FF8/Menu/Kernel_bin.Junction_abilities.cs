using System;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Contains 64 abilities, there are 115 abilities. Max is 64 per flags enum. Rest in AbilityFlags2
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        [Flags]
        public enum AbilityFlags : ulong // cannot contain all abilities because 
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
            Revive = 0x100000000,
            LVDown = 0x200000000,
            LVUp = 0x400000000,
            Kamikaze = 0x800000000,
            Devour = 0x1000000000,
            MiniMog = 0x2000000000,
            HP_20 = 0x4000000000,
            HP_40 = 0x8000000000,
            HP_80 = 0x10000000000,
            STR_20 = 0x20000000000,
            STR_40 = 0x40000000000,
            STR_60 = 0x80000000000,
            VIT_20 = 0x100000000000,
            VIT_40 = 0x200000000000,
            VIT_60 = 0x400000000000,
            MAG_20 = 0x800000000000,
            MAG_40 = 0x1000000000000,
            MAG_60 = 0x2000000000000,
            SPR_20 = 0x4000000000000,
            SPR_40 = 0x8000000000000,
            SPR_60 = 0x10000000000000,
            SPD_20 = 0x20000000000000,
            SPD_40 = 0x40000000000000,
            EVA_30 = 0x80000000000000,
            LUCK_50 = 0x100000000000000,
            Mug = 0x200000000000000,
            MedData = 0x400000000000000,
            Counter = 0x800000000000000,
            Return_Damage = 0x1000000000000000,
            Cover = 0x2000000000000000,
            Initiative = 0x4000000000000000,
            Move_HPUp = 0x8000000000000000,
        }
        /// <summary>
        /// remaning abilities that doen't fit in AbilityFlags.
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        [Flags]
        public enum AbilityFlags2 : ulong
        {
            None = 0x0,
            HPBonus = 0x1,
            StrBonus = 0x2,
            VitBonus = 0x4,
            MagBonus = 0x8,
            SprBonus = 0x10,
            Auto_Protect = 0x20,
            Auto_Shell = 0x40,
            Auto_Reflect = 0x80,
            Auto_Haste = 0x100,
            AutoPotion = 0x200,
            Expendx2_1 = 0x400,
            Expendx3_1 = 0x800,
            Ribbon = 0x1000,
            Alert = 0x2000,
            Move_Find = 0x4000,
            Enc_Half = 0x8000,
            Enc_None = 0x10000,
            RareItem = 0x20000,
            SumMag_10 = 0x40000,
            SumMag_20 = 0x80000,
            SumMag_30 = 0x100000,
            SumMag_40 = 0x200000,
            GFHP_10 = 0x400000,
            GFHP_20 = 0x800000,
            GFHP_30 = 0x1000000,
            GFHP_40 = 0x2000000,
            Boost = 0x4000000,
            Haggle = 0x8000000,
            Sell_High = 0x10000000,
            Familiar = 0x20000000,
            CallShop = 0x40000000,
            JunkShop = 0x80000000,
            TMag_RF = 0x100000000,
            IMag_RF = 0x200000000,
            FMag_RF = 0x400000000,
            LMag_RF = 0x800000000,
            TimeMag_RF = 0x1000000000,
            STMag_RF = 0x2000000000,
            SuptMag_RF = 0x4000000000,
            ForbidMag_RF = 0x8000000000,
            RecovMed_RF = 0x10000000000,
            STMed_RF = 0x20000000000,
            Ammo_RF = 0x40000000000,
            Tool_RF = 0x80000000000,
            ForbidMed_RF = 0x100000000000,
            GFRecovMed_RF = 0x200000000000,
            GFAblMed_RF = 0x400000000000,
            MidMag_RF = 0x800000000000,
            HighMag_RF = 0x1000000000000,
            MedLVUp = 0x2000000000000,
            CardMod = 0x4000000000000,
        }

        /// <summary>
        /// 115 abilities. Flags version in AbilityFlags and AbilitiyFlags2
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junctionable-Abilities"/>
        public enum Abilities :byte
        {
            None,
            HP_J,
            Str_J,
            Vit_J,
            Mag_J,
            Spr_J,
            Spd_J,
            Eva_J,
            Hit_J,
            Luck_J,
            Elem_Atk_J,
            ST_Atk_J,
            Elem_Def_J,
            ST_Def_J,
            Elem_Defx2,
            Elem_Defx4,
            ST_Def_Jx2,
            ST_Def_Jx4,
            Abilityx3,
            Abilityx4,
            Magic,
            GF,
            Draw,
            Item,
            Empty,
            Card,
            Doom,
            MadRush,
            Treatment,
            Defend,
            Darkside,
            Recover,
            Absorb,
            Revive,
            LVDown,
            LVUp,
            Kamikaze,
            Devour,
            MiniMog,
            HP_20,
            HP_40,
            HP_80,
            STR_20,
            STR_40,
            STR_60,
            VIT_20,
            VIT_40,
            VIT_60,
            MAG_20,
            MAG_40,
            MAG_60,
            SPR_20,
            SPR_40,
            SPR_60,
            SPD_20,
            SPD_40,
            EVA_30,
            LUCK_50,
            Mug,
            MedData,
            Counter,
            Return_Damage,
            Cover,
            Initiative,
            Move_HPUp,
            HPBonus,
            StrBonus,
            VitBonus,
            MagBonus,
            SprBonus,
            Auto_Protect,
            Auto_Shell,
            Auto_Reflect,
            Auto_Haste,
            AutoPotion,
            Expendx2_1,
            Expendx3_1,
            Ribbon,
            Alert,
            Move_Find,
            Enc_Half,
            Enc_None,
            RareItem,
            SumMag_10,
            SumMag_20,
            SumMag_30,
            SumMag_40,
            GFHP_10,
            GFHP_20,
            GFHP_30,
            GFHP_40,
            Boost,
            Haggle,
            Sell_High,
            Familiar,
            CallShop,
            JunkShop,
            TMag_RF,
            IMag_RF,
            FMag_RF,
            LMag_RF,
            TimeMag_RF,
            STMag_RF,
            SuptMag_RF,
            ForbidMag_RF,
            RecovMed_RF,
            STMed_RF,
            Ammo_RF,
            Tool_RF,
            ForbidMed_RF,
            GFRecovMed_RF,
            GFAblMed_RF,
            MidMag_RF,
            HighMag_RF,
            MedLVUp,
            CardMod,
        }
        /// <summary>
        /// Junction Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Junction_abilities"/>
        public class Junction_abilities
        {
            public const int count = 20;
            public const int id = 11;

            public override string ToString() => Name;

            public FF8String Name { get; private set; }
            public FF8String Description { get; private set; }
            public byte AP { get; private set; }

            //public BitArray J_Flags { get; private set; }
            public AbilityFlags J_Flags { get; private set; }

            public void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                //J_Flags = new BitArray(br.ReadBytes(3));
                byte[] tmp = br.ReadBytes(3);
                J_Flags = (AbilityFlags)(tmp[2] << 16 | tmp[1] << 8 | tmp[0]);

                //0x0005  3 byte J_Flag
            }

            public static Junction_abilities[] Read(BinaryReader br)
            {
                Junction_abilities[] ret = new Junction_abilities[count];

                for (int i = 0; i < count; i++)
                {
                    Junction_abilities tmp = new Junction_abilities();
                    tmp.Read(br, i);
                    ret[i] = tmp;
                }
                return ret;
            }
        }
    }
}