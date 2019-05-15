using System;
using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {

        [Flags]
        internal enum Renzokeken_Level : byte
        {
            Rough_Divide = 0x01,
            Fated_Circle = 0x02,
            Blasting_Zone = 0x04,
            Lion_Heart = 0x08,
        }
        /// <summary>
        /// Attack Type
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/blob/master/Doomtrain/Resources/Attack_Type_List.txt"/>
        internal enum Attack_Type
        {
            None,
            Physical_Attack,
            Magic_Attack,
            Curative_Magic,
            Curative_Item,
            Revive,
            Revive_At_Full_HP,
            Physical_Damage,
            Magic_Damage,
            Renzokuken_Finisher,
            Squall_Gunblade_Attack,
            GF,
            Scan,
            LV_Down,
            Summon_Item,
            GF_Ignore_Target_SPR,
            LV_Up,
            Card,
            Kamikaze,
            Devour,
            GF_Damage,
            Unknown_1,
            Magic_Attack_Ignore_Target_SPR,
            Angelo_Search,
            Moogle_Dance,
            White_WindQuistis,
            LV_Attack,
            Fixed_Damage,
            Target_Current_HP_1,
            Fixed_Magic_Damage_Based_on_GF_Level,
            Unknown_2,
            Unknown_3,
            Give_Percentage_HP,
            Unknown_4,
            Everyones_Grudge,
            _1_HP_Damage,
            Physical_AttackIgnore_Target_VIT
        }
        internal struct Weapons_Data
        {
            internal const int count =33;
            internal const int id=4;
            internal FF8String Name { get; private set; }
            public override string ToString() => Name;

            //0x0000	2 bytes Offset to weapon name
            internal Renzokeken_Level Renzokuken; //0x0002	1 byte Renzokuken finishers
            internal byte Unknown0; //0x0003	1 byte Unknown
            internal Saves.Characters Character;//0x0004	1 byte Character ID
            internal Attack_Type Attack_type;//0x0005	1 bytes Attack Type
            internal byte Attack_power;//0x0006	1 byte Attack Power
            internal byte HIT;//0x0007	1 byte Attack Parameter
            internal byte STR;//0x0008	1 byte STR Bonus
            internal byte Tier;//0x0009	1 byte Weapon Tier
            internal byte CRIT;//0x000A	1 byte Crit Bonus
            internal bool Melee;//0x000B	1 byte Melee Weapon?

            internal void Read(BinaryReader br,int string_id = 0)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, string_id);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                Renzokuken = (Renzokeken_Level)br.ReadByte(); //0x0002	1 byte Renzokuken finishers
                Unknown0 = br.ReadByte(); //0x0003	1 byte Unknown
                Character = (Saves.Characters)br.ReadByte();//0x0004	1 byte Character ID
                Attack_type = (Attack_Type)br.ReadByte();//0x0005	1 bytes Attack Type
                Attack_power = br.ReadByte();//0x0006	1 byte Attack Power
                HIT = br.ReadByte();//0x0007	1 byte Attack Parameter
                STR = br.ReadByte();//0x0008	1 byte STR Bonus
                Tier = br.ReadByte();//0x0009	1 byte Weapon Tier
                CRIT = br.ReadByte();//0x000A	1 byte Crit Bonus
                Melee = br.ReadByte()==0?false:true;//0x000B	1 byte Melee Weapon?
        }
        }
    }
}