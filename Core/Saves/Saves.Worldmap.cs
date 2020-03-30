using System;
using System.IO;

namespace OpenVIII
{
    public static partial class Saves
    {
        /// <summary>
        /// WorldMap vars for savegame
        /// </summary>
        /// <see cref="https://github.com/myst6re/hyne/blob/master/SaveData.h"/>
        public class Worldmap
        {
            private short[] char_pos;// x z y ? ? rot(0->4095)
            private short[] uknown_pos1;
            private short[] ragnarok_pos;
            private short[] bgu_pos;
            private short[] car_pos;
            private short[] uknown_pos2;
            private short[] uknown_pos3;
            private short[] uknown_pos4;
            private ushort steps_related;
            private byte car_rent;// 0x00:forbidden|0xFF:none|0x51:balamb1|0x52:balamb2|0x57:esthar
            private byte[] u1;//u1[6] = deep sea?
            private ushort u2;// always 0xFFFF?
            private ushort u3;//u3(1] = deep sea?
                              // Next bytes don't work with french PC version

            private byte disp_map_config;// 0:none|1:minisphere|2:minimap
            private byte u4;
            private ushort car_steps_related;
            private ushort car_steps_related2;
            private byte vehicles_instructions_worldmap;//voiture|Unused|BGU|Chocobo|Hydre|???|???|Unused
            private byte koyok_quest;//04 : Mandy Beach|Winhill|Trabia|Kashkabald Desert|UFO? Battu|80 : Koyo K Battu/Soigné/Mangé
            private byte[] obel_quest;

            /* [0] => having hummed twice | ??? | Unused | Unused | n ricochets | infinite ricochets | Seen ryo | Seen ryo² ("100x more ricochets than you!")
             * [1] => Ryo gave tablet | Unused | Shadow clues to find the idiot | Unused | Unused | Unused | Shadow clue for Eldbeak | Eldbeak found
             * [2] => Minde Island Treasure | Mordor's Plain Treasure | Unused | Unused | Unused | Unused | Unused | Unused
             * [3] => ??? | Pierre Balamb | Pierre Ryo | Pierre Timber | Pierre Galbadia | All stones | Shade index for Balamb | ???
             * [4] => ??? (mordor var?)
             * [5] => ??? | ??? | ??? | ??? | Block access Lunatic Pandora | ??? | Block access Lunatic Pandora | ???
             * [6] => having spoken in the shade | Accepting to look for the idiot | Having seen the idiot | ...
             * [7] => ??? (temp var)
             */
            private byte[] u6;

            public Worldmap()
            {
            }
            public Worldmap(BinaryReader br) => Read(br);

            public void Read(BinaryReader br)
            {
                char_pos = new short[6]; for (var i = 0; i < char_pos.Length; i++) char_pos[i] = br.ReadInt16();// x z y ? ? rot(0->4095)
                uknown_pos1 = new short[6]; for (var i = 0; i < uknown_pos1.Length; i++) uknown_pos1[i] = br.ReadInt16();
                ragnarok_pos = new short[6]; for (var i = 0; i < ragnarok_pos.Length; i++) ragnarok_pos[i] = br.ReadInt16();
                bgu_pos = new short[6]; for (var i = 0; i < bgu_pos.Length; i++) bgu_pos[i] = br.ReadInt16();
                car_pos = new short[6]; for (var i = 0; i < car_pos.Length; i++) car_pos[i] = br.ReadInt16();
                uknown_pos2 = new short[6]; for (var i = 0; i < uknown_pos2.Length; i++) uknown_pos2[i] = br.ReadInt16();
                uknown_pos3 = new short[6]; for (var i = 0; i < uknown_pos3.Length; i++) uknown_pos3[i] = br.ReadInt16();
                uknown_pos4 = new short[6]; for (var i = 0; i < uknown_pos4.Length; i++) uknown_pos4[i] = br.ReadInt16();
                steps_related = br.ReadUInt16();
                car_rent = br.ReadByte();// 0x00:forbidden|0xFF:none|0x51:balamb1|0x52:balamb2|0x57:esthar
                u1 = br.ReadBytes(7);//u1[6] = deep sea?
                u2 = br.ReadUInt16();// always 0xFFFF?
                u3 = br.ReadUInt16();//u3(1] = deep sea?
                                     // Next bytes don't work with french PC version
                disp_map_config = br.ReadByte();// 0:none|1:minisphere|2:minimap
                u4 = br.ReadByte();
                car_steps_related = br.ReadUInt16();
                car_steps_related2 = br.ReadUInt16();
                vehicles_instructions_worldmap = br.ReadByte();//voiture|Unused|BGU|Chocobo|Hydre|???|???|Unused
                koyok_quest = br.ReadByte();//04 : Mandy Beach|Winhill|Trabia|Kashkabald Desert|UFO? Battu|80 : Koyo K Battu/Soigné/Mangé
                obel_quest = br.ReadBytes(8);
                /* [0] => having hummed twice | ??? | Unused | Unused | n ricochets | infinite ricochets | Seen ryo | Seen ryo² ("100x more ricochets than you!")
                 * [1] => Ryo gave tablet | Unused | Shadow clues to find the idiot | Unused | Unused | Unused | Shadow clue for Eldbeak | Eldbeak found
                 * [2] => Minde Island Treasure | Mordor's Plain Treasure | Unused | Unused | Unused | Unused | Unused | Unused
                 * [3] => ??? | Pierre Balamb | Pierre Ryo | Pierre Timber | Pierre Galbadia | All stones | Shade index for Balamb | ???
                 * [4] => ??? (mordor var?)
                 * [5] => ??? | ??? | ??? | ??? | Block access Lunatic Pandora | ??? | Block access Lunatic Pandora | ???
                 * [6] => having spoken in the shade | Accepting to look for the idiot | Having seen the idiot | ...
                 * [7] => ??? (temp var)
                 */
                u6 = br.ReadBytes(2);
            }

            internal Worldmap Clone() => (Worldmap) MemberwiseClone();
        }
    }
}