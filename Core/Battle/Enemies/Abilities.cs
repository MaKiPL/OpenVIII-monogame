using System;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
    public struct Abilities
    {
        [Flags]
        public enum KernelFlag : byte
        {
            None = 0,
            unk0 = 0x1,
            magic = 0x2,
            item = 0x4,
            monster = 0x8,
            unk1 = 0x10,
            unk2 = 0x20,
            unk3 = 0x40,
            unk4 = 0x80,
        }

        [FieldOffset(0)]
        public KernelFlag kernelId; //0x2 magic, 0x4 item, 0x8 monsterAbility;

        [FieldOffset(1)]
        public byte animation; // ifrit states one of theses is an animation BattleID.

        [FieldOffset(2)]
        public ushort abilityId;

        private const string unk = "Unknown";

        public Kernel.MagicData MAGIC => (kernelId & KernelFlag.magic) != 0 && Memory.Kernel_Bin.MagicData.Count > abilityId ? Memory.Kernel_Bin.MagicData[abilityId] : null;
        public Item_In_Menu? ITEM => (kernelId & KernelFlag.item) != 0 && Memory.MItems != null && Memory.MItems.Items.Count > abilityId ? Memory.MItems?.Items[abilityId] : null;
        public Kernel.EnemyAttacksData MONSTER => (kernelId & KernelFlag.monster) != 0 && Memory.Kernel_Bin.EnemyAttacksData.Count > abilityId ? Memory.Kernel_Bin.EnemyAttacksData[abilityId] : null;


        public override string ToString()
        {
            if (MAGIC != null)
                return MAGIC.Name ?? unk;
            if (ITEM != null)
                return ITEM.Value.Name ?? unk;
            if (MONSTER != null)
                return MONSTER.Name ?? unk;

            return "";
        }
    }
}