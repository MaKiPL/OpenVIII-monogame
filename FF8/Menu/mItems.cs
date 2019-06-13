using System;
using System.Collections.Generic;
using System.IO;

namespace FF8
{
    public struct MItem
    {
        public enum Type : byte
        {
            Heal = 0x00,
            Revive= 0x01,
            HealGF = 0x03,
            ReviveGF = 0x04,
            SavePointHeal = 0x06,
            Battle = 0x07,
            Ammo = 0x08,
            Magazine = 0x09,
            None = 0x0A, //Refine or None
            GF = 0x0B, //Rename
            Angelo = 0x0C, //Rename
            Chocobo = 0x0D, //Rename
            Lamp = 0x0E, //start battle
            SolomonRing = 0x0F, // get Doomtrain
            GF_Compatability = 0x10,
            GF_Learn = 0x11,
            GF_Forget = 0x12,
            BlueMagic = 0x13, // learn
            Stat = 0x14,
            Cure_Abnormal_Status = 0x15,
        }
        [Flags]
        public enum Target :byte
        {
            None= 0x0,
            Useable = 0x1,//usable
            Character = 0x2,
            GF = 0x4,
            All = 0x8,
            All2 = 0x10,// all two?
            KO = 0x20,
            BlueMagic = 0x40,//blue magic?
            Compatability = 0x80,//compatability
        }
        public Type b0 { get; private set; }
        public Target b1 { get; private set; }
        public byte b2 { get; private set; }
        public byte b3 { get; private set; }
        public byte ID { get; private set; }

        public static MItem Read(BinaryReader br, byte i) => new MItem
        {
            b0 = (Type)br.ReadByte(),
            b1 = (Target)br.ReadByte(),
            b2 = br.ReadByte(),
            b3 = br.ReadByte(),
            ID = i
        };
    }
    /// <summary>
    /// Item data from mItems.bin
    /// </summary>
    /// <see cref="http://forums.qhimm.com/index.php?topic=17098.0"/>
    public class MItems
    {
        private List<MItem> _items;

        private MItems() => _items = new List<MItem>();

        public IReadOnlyList<MItem> Items => _items;

        public static MItems Read()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);

            using (BinaryReader br = new BinaryReader(new MemoryStream(aw.GetBinaryFile("mitem.bin"))))
            {
                return Read(br);
            }
        }

        private static MItems Read(BinaryReader br)
        {
            MItems ret = new MItems();
            for (byte i = 0; br.BaseStream.Position + 4 < br.BaseStream.Length; i++)
            {
                MItem tmp = MItem.Read(br, i);
                ret._items.Add(tmp);
            }
            return ret;
        }
    }
}