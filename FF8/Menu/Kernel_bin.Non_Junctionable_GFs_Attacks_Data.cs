using System.IO;

namespace FF8
{
    internal partial class Kernel_bin
    {
        /// <summary>
        /// Non-Junctionable GFs Attacks data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Non-junctionable-GF-attacks"/>
        internal struct Non_Junctionable_GFs_Attacks_Data
        {
            internal const int count = 16;
            internal const int id = 9;

            public FF8String Name { get; private set; }

            //0x0000	2 bytes Offset to GF attack name
            internal Magic_ID MagicID;

            //0x0002	2 bytes Magic ID(decides what animation to play)
            internal Attack_Type Attack_Type;

            //0x0004	1 byte Attack type
            internal byte GF_Power;

            //0x0005	1 byte GF power(used in damage formula)
            internal byte Status_Attack;

            //0x0006	1 byte Status attack enabler
            internal byte Unknown0;

            //0x0007	1 byte Unknown
            internal byte Status_flags;

            //0x0008	1 byte Status flags ?
            internal byte[] Unknown1;

            //0x0009	2 bytes Unknown
            internal Element Element;

            //0x000B	1 byte Element

            //0x00 - Non-Elemental
            //0x01 - Fire
            //0x02 - Ice
            //0x04 - Thunder
            //0x08 - Earth
            //0x10 - Poison
            //0x20 - Wind
            //0x40 - Water
            //0x80 - Holy
            internal Statuses1 Statuses1;

            //0x000C	1 byte Status 1

            //0x00 - None
            //0x01 - Sleep
            //0x02 - Haste
            //0x04 - Slow
            //0x08 - Stop
            //0x10 - Regen
            //0x20 - Protect
            //0x40 - Shell
            //0x80 - Reflect
            //0x000D	1 byte Status 2

            //0x00 - None
            //0x01 - Aura
            //0x02 - Curse
            //0x04 - Doom
            //0x08 - Invincible
            //0x10 - Petrifying
            //0x20 - Float
            //0x40 - Confusion
            //0x80 - Drain
            //0x000E	1 byte Status 3

            //0x00 - None
            //0x01 - Eject
            //0x02 - Double
            //0x04 - Triple
            //0x08 - Defend
            //0x10 - ???
            //0x20 - ???
            //0x40 - ???
            //0x80 - ???
            //0x000F	1 byte Status 4

            //0x00 - None
            //0x01 - Vit0
            //0x02 - ???
            //0x04 - ???
            //0x08 - ???
            //0x10 - ???
            //0x20 - ???
            //0x40 - ???
            //0x80 - ???
            internal Statuses0 Statuses0;

            //0x0010	1 byte Status 5

            //0x00 - None
            //0x01 - Death
            //0x02 - Poison
            //0x04 - Petrify
            //0x08 - Darkness
            //0x10 - Silence
            //0x20 - Berserk
            //0x40 - Zombie
            //0x80 - ???
            //internal byte Unknown2;
            //0x0011	1 byte Unknown
            internal byte Power_Mod;

            //0x0012	1 byte Power Mod(used in damage formula)
            internal byte Level_Mod;

            //0x0013	1 byte Level Mod(used in damage formula)

            internal void Read(BinaryReader br, int i)
            {
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i);
                br.BaseStream.Seek(2, SeekOrigin.Current);
                //0x0000	2 bytes Offset to GF attack name
                MagicID = (Magic_ID)br.ReadUInt16();
                //0x0002	2 bytes Magic ID(decides what animation to play)
                Attack_Type = (Attack_Type)br.ReadByte();
                //0x0004	1 byte Attack type
                GF_Power = br.ReadByte();
                //0x0005	1 byte GF power(used in damage formula)
                Status_Attack = br.ReadByte();
                //0x0006	1 byte Status attack enabler
                Unknown0 = br.ReadByte();
                //0x0007	1 byte Unknown
                Status_flags = br.ReadByte();
                //0x0008	1 byte Status flags ?
                Unknown1 = br.ReadBytes(2);
                //0x0009	2 bytes Unknown
                Element = (Element)br.ReadByte();
                //0x000B	1 byte Element
                Statuses1 = (Statuses1)br.ReadUInt32();
                //0x000C	1 byte Status 1
                //0x000D	1 byte Status 2
                //0x000E	1 byte Status 3
                //0x000F	1 byte Status 4
                Statuses0 = (Statuses0)br.ReadUInt16();
                //0x0010	1 byte Status 5
                //0x0011	1 byte Unknown
                Power_Mod = br.ReadByte();
                //0x0012	1 byte Power Mod(used in damage formula)
                Level_Mod = br.ReadByte();
                //0x0013	1 byte Level Mod(used in damage formula)
            }
        }
    }
}