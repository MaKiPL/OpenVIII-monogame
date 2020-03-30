using System;
using System.IO;

namespace OpenVIII
{
    public static partial class Saves
    {
        /// <see cref="https://github.com/myst6re/hyne/blob/master/SaveData.h"/>
        /// <summary>
        /// ChocoboWorld Save Data
        /// </summary>
        public class ChocoboWorld //64
        {
            [Flags]
            public enum EnabledFlags : byte
            {
                Disabled = 0x0,
                Enabled = 0x1,
                In_world = 0x2,
                MiniMog_found = 0x4,
                Demon_King_defeated = 0x8,
                Koko_kidnapped = 0x10,
                Hurry = 0x20,
                Koko_met = 0x40,
                Event_Wait_off = 0x80,
            }
            [Flags]
            public enum BokuAttackFlags : byte {
                chocobraise = 0x1,
                chocoflammes = 0x2,
                chocométéore = 0x4,
                grochocobo = 0x8,
            }

            public EnabledFlags enabled;// Enabled|In world|MiniMog found|Demon King defeated|Koko kidnapped|Hurry!|Koko met|Event Wait off
            public byte level;
            public byte current_hp;
            public byte max_hp;
            public byte[] weapon;// 4 bit = 1 weapon
            public byte rank;
            public byte move;
            public ulong saveCount;
            public ushort id_related;
            public byte[] u1;
            public byte itemClassACount;
            public byte itemClassBCount;
            public byte itemClassCCount;
            public byte itemClassDCount;
            public byte[] u2;
            public ulong associatedSaveID;
            public byte u3;
            public BokuAttackFlags boko_attack;// star Count (chocobraise | chocoflammes | chocométéore | grochocobo)
            public byte u4;
            public byte home_walking;
            public byte[] u5;
            public ChocoboWorld(){}
            public ChocoboWorld(BinaryReader br) => Read(br);
            /// <summary>
            /// Shadow copy
            /// </summary>
            /// <returns></returns>
            public ChocoboWorld Clone() => (ChocoboWorld) MemberwiseClone();

            public void Read(BinaryReader br)
            {
                enabled = (EnabledFlags)br.ReadByte();// Enabled|In world|MiniMog found|Demon King defeated|Koko kidnapped|Hurry!|Koko met|Event Wait off
                level = br.ReadByte();
                current_hp = br.ReadByte();
                max_hp = br.ReadByte();
                var tmp = br.ReadUInt16();// 4 bit = 1 weapon
                weapon = new byte[4];
                weapon[0] = (byte)(tmp & 0x000F);
                weapon[1] = (byte)((tmp & 0x00F0)>>4);     
                weapon[2] = (byte)((tmp & 0x0F00)>>8);     
                weapon[3] = (byte)((tmp & 0xF000)>>12);     
                rank = br.ReadByte();
                move = br.ReadByte();
                saveCount = br.ReadUInt32();
                id_related = br.ReadUInt16();
                u1 = br.ReadBytes(6);
                itemClassACount = br.ReadByte();
                itemClassBCount = br.ReadByte();
                itemClassCCount = br.ReadByte();
                itemClassDCount = br.ReadByte();
                u2 = br.ReadBytes(16);
                associatedSaveID = br.ReadUInt32();
                u3 = br.ReadByte();
                boko_attack = (BokuAttackFlags)br.ReadByte();// star Count (chocobraise | chocoflammes | chocométéore | grochocobo)
                u4 = br.ReadByte();
                home_walking = br.ReadByte();
                u5 = br.ReadBytes(16);
            }
        }
    }
}