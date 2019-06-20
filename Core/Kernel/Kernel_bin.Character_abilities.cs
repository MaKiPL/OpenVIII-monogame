using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        /// <summary>
        /// Characters Abilities Data
        /// </summary>
        /// <see cref="https://github.com/alexfilth/doomtrain/wiki/Character-abilities"/>
        public class Character_abilities : Equipable_Ability
        {
            public new const int count = 20;
            public new const int id = 14;
            
            public CharacterAbilityFlags Flags { get; private set; }
            //public Battle_Commands BattleCommand { get; private set; } = null;

            public override void Read(BinaryReader br, int i)
            {
                Icon = Icons.ID.Ability_Character2;
                Name = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2);
                //0x0000	2 bytes Offset to name
                Description = Memory.Strings.Read(Strings.FileID.KERNEL, id, i * 2 + 1);
                //0x0002	2 bytes Offset to description
                br.BaseStream.Seek(4, SeekOrigin.Current);
                AP = br.ReadByte();
                //0x0004  1 byte AP Required to learn ability
                byte[] tmp = br.ReadBytes(3);
                int shift =0;
                Flags = (CharacterAbilityFlags)(tmp[2] << (16+ shift) | tmp[1] << (8+ shift) | tmp[0]<<(shift));
                //Flags = new BitArray(br.ReadBytes(3));
                //0x0005  3 byte Flags
            }
            public static Dictionary<Abilities,Character_abilities> Read(BinaryReader br)
            {
                Dictionary<Abilities, Character_abilities> ret = new Dictionary<Abilities, Character_abilities>(count);

                for (int i = 0; i < count; i++)
                {
                    var tmp = new Character_abilities();
                    tmp.Read(br, i);
                    ret[(Abilities)(i+(int)Abilities.Mug)] = tmp;
                }
                return ret;
            }
        }
    }

}