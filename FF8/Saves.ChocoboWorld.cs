using System;
using System.IO;

namespace FF8
{

    internal static partial class Saves
    {
        /// <see cref="https://github.com/myst6re/hyne/blob/master/SaveData.h"/>
        /// <summary>
        /// ChocoboWorld Save Data
        /// </summary>
        internal struct ChocoboWorld//64
        {
            byte enabled;// Enabled|In world|MiniMog found|Demon King defeated|Koko kidnapped|Hurry!|Koko met|Event Wait off
            byte level;
            byte current_hp;
            byte max_hp;
            ushort weapon;// 4 bit = 1 weapon
            byte rank;
            byte move;
            ulong saveCount;
            ushort id_related;
            byte[] u1;
            byte itemClassACount;
            byte itemClassBCount;
            byte itemClassCCount;
            byte itemClassDCount;
            byte[] u2;
            ulong associatedSaveID;
            byte u3;
            byte boko_attack;// star count (chocobraise | chocoflammes | chocométéore | grochocobo)
            byte u4;
            byte home_walking;
            byte[] u5;
            internal void Read(BinaryReader br)
            {
                enabled = br.ReadByte();// Enabled|In world|MiniMog found|Demon King defeated|Koko kidnapped|Hurry!|Koko met|Event Wait off
                level = br.ReadByte();
                current_hp = br.ReadByte();
                max_hp = br.ReadByte();
                weapon = br.ReadUInt16();// 4 bit = 1 weapon
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
                boko_attack = br.ReadByte();// star count (chocobraise | chocoflammes | chocométéore | grochocobo)
                u4 = br.ReadByte();
                home_walking = br.ReadByte();
                u5 = br.ReadBytes(16);
            }
        }
    }
}