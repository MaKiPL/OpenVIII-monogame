using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// Effects of Items inside the in game menu.
    /// </summary>
    public struct Item_In_Menu
    {

        #region Fields

        private _Target b1;

        private byte b2;

        private byte b3;
        private _Type _type;

        #endregion Fields

        #region Enums

        [Flags]
        public enum _Target : byte
        {
            None = 0x0,
            Useable = 0x1,//usable
            Character = 0x2,
            GF = 0x4,
            All = 0x8,
            All2 = 0x10,// all two?
            KO = 0x20,
            BlueMagic = 0x40,//blue magic?
            Compatability = 0x80,//compatability
        }

        public enum _Type : byte
        {
            Heal = 0x00,
            Revive = 0x01,
            HealGF = 0x03,
            ReviveGF = 0x04,
            SavePointHeal = 0x06,
            Battle = 0x07,
            Ammo = 0x08,
            Magazine = 0x09,
            /// <summary>
            /// Refine or None
            /// </summary>
            None = 0x0A,
            /// <summary>
            /// Rename
            /// </summary>
            GF = 0x0B,
            /// <summary>
            /// Rename
            /// </summary>
            Angelo = 0x0C,
            /// <summary>
            /// Rename
            /// </summary>
            Chocobo = 0x0D,
            /// <summary>
            /// start battle
            /// </summary>
            Lamp = 0x0E,
            /// <summary>
            /// get Doomtrain
            /// </summary>
            SolomonRing = 0x0F,
            GF_Compatability = 0x10,
            GF_Learn = 0x11,
            GF_Forget = 0x12,
            /// <summary>
            /// blue magic only shows quistis in target list, should detect if she knows the spell to grey it out item.
            /// </summary>
            Blue_Magic = 0x13, // learn
            Stat = 0x14,
            Cure_Abnormal_Status = 0x15,
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Abilities a GF can learn
        /// </summary>
        public Kernel_bin.Abilities Learn => Type == _Type.GF_Learn ? (Kernel_bin.Abilities)b2 : Kernel_bin.Abilities.None;

        /// <summary>
        /// Which persistant statuses are removed.
        /// </summary>
        public Kernel_bin.Persistant_Statuses Cleansed_Statuses
        {
            get
            {
                Kernel_bin.Persistant_Statuses ret = Kernel_bin.Persistant_Statuses.None;
                if (Type == _Type.HealGF || Type == _Type.Heal || Type == _Type.Revive || Type == _Type.ReviveGF ||
                    Type == _Type.Cure_Abnormal_Status || Type == _Type.SavePointHeal)
                    ret = (Kernel_bin.Persistant_Statuses)b3;
                return ret;
            }
        }

        /// <summary>
        /// How much healing is done
        /// </summary>
        public ushort Heals
        {
            get
            {
                if (Type == _Type.Heal || Type == _Type.HealGF || Type == _Type.SavePointHeal)
                {
                    return (ushort)(b2 * 0x32);
                }
                return 0;
            }
        }
        public Kernel_bin.Stat Stat => Type == _Type.Stat ? (Kernel_bin.Stat)b3 : Kernel_bin.Stat.None;
        public byte Stat_Increase => (byte)(Type == _Type.Stat ? b2 : 0);
        /// <summary>
        /// Item ID
        /// </summary>
        public byte ID { get; private set; }
        /// <summary>
        /// Who is targeted and 0x01 seems to be a useable item in menu item. Magazine values don't seem to corrispond.
        /// </summary>
        public _Target Target => Type == _Type.Magazine ? _Target.None : b1;
        /// <summary>
        /// Type of item.
        /// </summary>
        public _Type Type { get {
                if (ID == 0) _type = _Type.None;
                return _type; } private set => _type = value; }
        /// <summary>
        /// Target in byte form
        /// </summary>
        private byte b1_byte => (byte)b1;
        public byte Palette => 9;
        public byte Faded_Palette => 7;
        public Kernel_bin.Battle_Items_Data Battle => (Kernel_bin.BattleItemsData?.Count ?? 0) > ID ? Kernel_bin.BattleItemsData[ID] : null;
        public Kernel_bin.Non_battle_Items_Data Non_Battle => Battle == null ? Kernel_bin.NonbattleItemsData[ID - (Kernel_bin.BattleItemsData?.Count ?? 0)] : null;

        public FF8String Name => Battle?.Name ?? Non_Battle?.Name;
        public FF8String Description => Battle?.Description ?? Non_Battle?.Description;
        #endregion Properties

        #region Methods

        public static Item_In_Menu Read(BinaryReader br, byte i)
        {
            var tmp = new Item_In_Menu
            {
                Type = (_Type)br.ReadByte(),
                b1 = (_Target)br.ReadByte(),
                b2 = br.ReadByte(),
                b3 = br.ReadByte(),
                ID = i
            };
            switch (tmp.Type)
            {
                case _Type.Heal:
                case _Type.Revive:
                case _Type.Cure_Abnormal_Status:
                    tmp.Icon = Icons.ID.Item_Recovery;
                    break;
                case _Type.HealGF:
                case _Type.ReviveGF:
                case _Type.GF:
                case _Type.GF_Forget:
                case _Type.GF_Learn:
                    tmp.Icon = Icons.ID.Item_GF;
                    break;
                case _Type.Battle:
                case _Type.Angelo:
                case _Type.Chocobo: // i'm not sure about this one i have no chocobo items.
                case _Type.SolomonRing:
                case _Type.Lamp:
                    tmp.Icon = Icons.ID.Item_Battle;
                    break;
                case _Type.SavePointHeal:
                    tmp.Icon = Icons.ID.Item_Tent;
                    break;
                case _Type.Ammo:
                    tmp.Icon = Icons.ID.Item_Ammo;
                    break;
                case _Type.Magazine:
                    tmp.Icon = Icons.ID.Item_Magazine;
                    break;
                case _Type.None:
                case _Type.Blue_Magic:
                case _Type.GF_Compatability:
                case _Type.Stat:
                    tmp.Icon = Icons.ID.Item_Misc;
                    break;
                default:
                    tmp.Icon = Icons.ID.None;
                    break;
            }
            return tmp;
        }

        public Kernel_bin.Blue_Magic Learned_Blue_Magic => Type == _Type.Blue_Magic ? (Kernel_bin.Blue_Magic)b2 : Kernel_bin.Blue_Magic.None;

        public Icons.ID Icon { get; private set; }

        /// <summary>
        /// How much Compatability is added to gf. neg is how much is removed from other gfs.
        /// </summary>
        /// <param name="gf">Which GF is effected</param>
        /// <param name="neg">How much Comptability is lost by all other GFs</param>
        /// <returns>Total compatability gained by selected character for this GF.</returns>
        /// <see cref="https://gamefaqs.gamespot.com/ps/197343-final-fantasy-viii/faqs/6110"/>
        public byte Compatibility(out GFs gf, out sbyte neg)
        {
            gf = 0; neg = 0;
            byte ret = 0;
            if (Type == _Type.GF_Compatability)
            {
                gf = (GFs)b2;
                switch (b3)
                {
                    case 0x08://Weak C Item
                        ret = 0x01;
                        neg = (sbyte)(gf == GFs.All ? 0x00 : -0x01);
                        break;
                    case 0x10://Strong C Item
                        ret = 0x03;
                        neg = (sbyte)(gf == GFs.All ? 0x00 : -0x02);
                        break;
                    case 0x65: //LuvLuvG
                        ret = 0x14;
                        neg = (sbyte)(gf == GFs.All ? 0x00 : 0x00);
                        break;

                }
            }
            return ret;
        }

        #endregion Methods
    }
    /// <summary>
    /// Info on Items in menus. Items have a slightly limited effect in menues. See Kernel_bin.Battle_Items for effects in battle.
    /// </summary>
    /// <see cref="http://forums.qhimm.com/index.php?topic=17098.0"/>
    public class Items_In_Menu
    {
        #region Fields

        private List<Item_In_Menu> _items;

        #endregion Fields

        #region Constructors

        private Items_In_Menu() => _items = new List<Item_In_Menu>();

        #endregion Constructors

        #region Properties

        public IReadOnlyList<Item_In_Menu> Items => _items;

        #endregion Properties

        #region Methods
        public Item_In_Menu this[byte item] => Items[item];
        public Item_In_Menu this[Saves.Item item] => Items[item.ID];

        public static Items_In_Menu Read()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);

            using (BinaryReader br = new BinaryReader(new MemoryStream(aw.GetBinaryFile("mitem.bin"))))
            {
                return Read(br);
            }
        }

        private static Items_In_Menu Read(BinaryReader br)
        {
            Items_In_Menu ret = new Items_In_Menu();
            for (byte i = 0; br.BaseStream.Position + 4 <= br.BaseStream.Length; i++)
            {
                Item_In_Menu tmp = Item_In_Menu.Read(br, i);
                ret._items.Add(tmp);
            }
            return ret;
        }

        #endregion Methods
    }
}