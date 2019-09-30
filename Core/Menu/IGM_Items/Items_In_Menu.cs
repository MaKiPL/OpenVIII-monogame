using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Effects of Items inside the in game menu.
    /// </summary>
    public struct Item_In_Menu
    {
        #region Fields

        private _Type _type;
        private Dictionary<_Type, Func<Faces.ID, bool, bool>> _useActions;
        private _Target b1;

        private byte b2;

        private byte b3;

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
            /// blue magic only shows quistis in target list, should detect if she knows the spell to
            /// grey it out item.
            /// </summary>
            Blue_Magic = 0x13, // learn

            Stat = 0x14,
            Cure_Abnormal_Status = 0x15,
        }

        #endregion Enums

        #region Properties

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

        public byte Faded_Palette => 7;
        /// <summary>
        /// How much healing is done
        /// </summary>
        public ushort Heals
        {
            get
            {
                if (Type == _Type.Heal || Type == _Type.HealGF || Type == _Type.SavePointHeal)
                {
                    if (Attack_Type == Kernel_bin.Attack_Type.Give_Percentage_HP)
                        return (byte)((b2 * 100) / byte.MaxValue);
                    else
                        return (ushort)(b2 * 0x32);
                }
                //else if (Type == _Type.Revive || Type == _Type.ReviveGF)
                //    return 0; // 12.5%
                return 0;
            }
        }

        public Icons.ID Icon { get; private set; }
        /// <summary>
        /// Item ID
        /// </summary>
        public byte ID { get; private set; }

        /// <summary>
        /// Abilities a GF can learn
        /// </summary>
        public Kernel_bin.Abilities Learn => Type == _Type.GF_Learn ? (Kernel_bin.Abilities)b2 : Kernel_bin.Abilities.None;

        public Kernel_bin.Blue_Magic Learned_Blue_Magic => Type == _Type.Blue_Magic ? (Kernel_bin.Blue_Magic)b2 : Kernel_bin.Blue_Magic.None;
        public Kernel_bin.Non_battle_Items_Data Non_Battle => Battle == null ? Kernel_bin.NonbattleItemsData[ID - (Kernel_bin.BattleItemsData?.Count ?? 0)] : null;
        public byte Palette => 9;
        public byte Stat_Increase => (byte)(Type == _Type.Stat ? b2 : 0);
        /// <summary>
        /// Who is targeted and 0x01 seems to be a useable item in menu item. Magazine values don't
        /// seem to corrispond.
        /// </summary>
        public _Target Target => Type == _Type.Magazine ? _Target.None : b1;

        /// <summary>
        /// Type of item.
        /// </summary>
        public _Type Type
        {
            get
            {
                if (ID == 0) _type = _Type.None;
                return _type;
            }
            private set => _type = value;
        }

        private bool All => (Target & (_Target.All | _Target.All2)) != 0;
        private Kernel_bin.Attack_Type Attack_Type => Battle?.Attack_Type ?? Kernel_bin.Attack_Type.None;
        public Kernel_bin.Battle_Items_Data Battle => (Kernel_bin.BattleItemsData?.Count ?? 0) > ID ? Kernel_bin.BattleItemsData[ID] : null;
        public FF8String Description => Battle?.Description ?? Non_Battle?.Description;
        public FF8String Name => Battle?.Name ?? Non_Battle?.Name;
        public Kernel_bin.Stat Stat => Type == _Type.Stat ? (Kernel_bin.Stat)b3 : Kernel_bin.Stat.None;
        /// <summary>
        /// Target in byte form
        /// </summary>
        private byte b1_byte => (byte)b1;

        private IReadOnlyDictionary<_Type, Func<Faces.ID, bool, bool>> UseActions
        {
            get
            {
                if (_useActions == null)
                {
                    _useActions = new Dictionary<_Type, Func<Faces.ID, bool, bool>>
                    {
                        {_Type.Heal, HealAction },
                        {_Type.Revive, ReviveAction },
                        {_Type.HealGF, HealGFAction },
                        {_Type.ReviveGF, ReviveGFAction },
                        {_Type.SavePointHeal, SavePointHealAction },
                        {_Type.Battle, BattleAction },
                        {_Type.Ammo, AmmoAction },
                        {_Type.Magazine, MagazineAction },
                        {_Type.None, NoneAction },
                        {_Type.GF, GFAction },
                        {_Type.Angelo, AngeloAction },
                        {_Type.Chocobo, ChocoboAction },
                        {_Type.Lamp, LampAction },
                        {_Type.SolomonRing, SolomonRingAction },
                        {_Type.GF_Compatability, GF_CompatabilityAction },
                        {_Type.GF_Learn, GF_LearnAction },
                        {_Type.GF_Forget, GF_ForgetAction },
                        {_Type.Blue_Magic, Blue_MagicAction },
                        {_Type.Stat, StatAction },
                        {_Type.Cure_Abnormal_Status, Cure_Abnormal_StatusAction },
                    };
                }
                return _useActions;
            }
        }

        #endregion Properties

        #region Methods

        public static Item_In_Menu Read(BinaryReader br, byte i)
        {
            Item_In_Menu tmp = new Item_In_Menu
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

        /// <summary>
        /// If True Quistis can learn the ability. Else Quistis already knows the ability.
        /// </summary>
        /// <returns></returns>
        public bool TestBlueMagic()
        {
            if (Learned_Blue_Magic != Kernel_bin.Blue_Magic.None)
                return !Memory.State.LimitBreakQuistis_Unlocked_BlueMagic[(int)Learned_Blue_Magic];
            return false;
        }

        public bool TestCharacter(ref Faces.ID id, out Characters character)
        {
            character = id.ToCharacters();
            if (Type == Item_In_Menu._Type.Blue_Magic)
            {
                if (character != Characters.Quistis_Trepe)
                    return false;
                else return TestBlueMagic();
            }
            if (character == Characters.Blank || (Target & _Target.Character) == 0)
                return false;
            if (Memory.State.Characters.ContainsKey(character) && Memory.State.Characters[character].Available)
                return true;
            return false;
        }

        public bool TestGF(ref Faces.ID id, out GFs gf)
        {
            gf = id.ToGFs();
            if (Type == _Type.Angelo && id == Faces.ID.Angelo)
            {
                return true;
            }
            if (Type == _Type.Chocobo && id == Faces.ID.Boko)
            {
                return true;
            }
            if (gf == GFs.Blank || gf == GFs.All || (Target & _Target.GF) == 0)
                return false;
            if (Memory.State?.GFs != null && Memory.State.GFs.ContainsKey(gf))// && Memory.State.GFs[gf].VisibleInMenu)
            {
                if (Type == _Type.GF_Learn && (!Memory.State.GFs[gf].TestGFCanLearn(Learn) || Memory.State.GFs[gf].MaxGFAbilities))
                    return false;
                return true;
            }
            return false;
        }

        public override string ToString() => Name ?? base.ToString();

        public bool Use(Faces.ID obj, bool battle = false)
        {
            if (UseActions.ContainsKey(Type))
            {
                bool ret = GFAction(UseActions[Type], obj, battle);
                ret = CharAction(UseActions[Type], obj, battle) || ret;
                if (ret)
                {
                    byte id = ID;
                    for (int i = 0; i < Memory.State.Items.Count; i++)
                    {
                        if (Memory.State.Items[i].ID == ID)
                            Memory.State.Items[i].UsedOne();
                    }
                    //Memory.State.Items.Where(m => m.ID == id).ForEach(x => x.UsedOne());
                    return true;
                }
            }
            return false;
        }

        public bool ValidTarget(bool b = false)
        {
            if (Type == _Type.Magazine) return true; //TODO pressing okay display Magazine.
            else if (Type == _Type.SolomonRing) return !Memory.State[GFs.Doomtrain]?.Exists ?? true; //TODO detect doomtrain and return false if unlocked
            else if (Type == _Type.Lamp) return !Memory.State[GFs.Diablos]?.Exists ?? true;  //TODO detect diablo and return false if unlocked
            Faces.ID _face;
            foreach (Faces.ID face in (Faces.ID[])Enum.GetValues(typeof(Faces.ID)))
            {
                _face = face;
                if (TestCharacter(ref _face, out Characters character) || TestGF(ref _face, out GFs gf))
                {
                    return true;
                }
            }
            if (Battle != null && b) return true;
            return false;
        }

        private bool AmmoAction(Faces.ID obj, bool battle = false) => false;

        private bool AngeloAction(Faces.ID obj, bool battle = false) => false;

        private bool BattleAction(Faces.ID obj, bool battle = false) => false;

        private bool Blue_MagicAction(Faces.ID obj, bool battle = false) => false;

        private bool CharAction(Func<Faces.ID, bool, bool> func, Faces.ID obj, bool battle = false)
        {
            bool ret = false;
            if (All)
            {
                foreach (KeyValuePair<Characters, Saves.CharacterData> c in Memory.State.Characters.Where(x => (battle && Memory.State.PartyData.Contains(x.Key)) || x.Value.Available))
                {
                    obj = c.Key.ToFacesID();
                    if (TestCharacter(ref obj, out Characters character))
                        ret = func(obj, battle) || ret;
                }
                return ret;
            }
            else if (TestCharacter(ref obj, out Characters character))
                ret = func(obj, battle);

            return ret;
        }

        private bool ChocoboAction(Faces.ID obj, bool battle = false) => false;

        private bool Cure_Abnormal_StatusAction(Faces.ID obj, bool battle = false) => Cure_Abnormal_StatusAction(Memory.State[obj], battle);

        private bool Cure_Abnormal_StatusAction(Damageable c, bool battle)
        {
            bool ret = false;
            if (c != null && !c.StatusImmune)
                ret = c.DealStatus(Cleansed_Statuses, Battle?.Statuses1, Battle?.Attack_Type ?? Kernel_bin.Attack_Type.Curative_Item, Battle?.Attack_Flags);
            return ret;
        }

        private bool GF_CompatabilityAction(Faces.ID obj, bool battle = false) => false;

        /// <summary>
        /// Spawn a menu of GF's unlocked abilities and you select one to remove.
        /// </summary>
        private bool GF_ForgetAction(Faces.ID obj, bool battle = false) => false;

        /// <summary>
        /// GF learns new skill if below max abilities
        /// </summary>
        private bool GF_LearnAction(Faces.ID obj, bool battle = false) => GF_LearnAction(Memory.State[obj.ToGFs()]);

        private bool GF_LearnAction(Saves.GFData gf) => gf.Learn(Learn);

        private bool GFAction(Func<Faces.ID, bool, bool> func, Faces.ID obj, bool battle = false)
        {
            bool ret = false;
            if (All)
            {
                foreach (KeyValuePair<GFs, Saves.GFData> c in Memory.State.GFs.Where(x => x.Value.Exists))
                {
                    obj = c.Key.ToFacesID();
                    if (TestGF(ref obj, out GFs gf))
                        ret = func(obj, battle) || ret;
                }
                return ret;
            }
            else if (TestGF(ref obj, out GFs gf))
                ret = func(obj, battle);

            return ret;
        }

        private bool GFAction(Faces.ID obj, bool battle = false) => false;

        private bool HealAction(Faces.ID obj, bool battle = false) =>
            //Memory.State[obj].ChangeHP(-Heals);
            //return true;
            //bool ret = false;
            //if (!Memory.State[obj.ToCharacters()]?.StatusImmune ?? true && Cleansed_Statuses != Kernel_bin.Persistant_Statuses.None)
            //    ret = Memory.State[obj.ToCharacters()]?.DealStatus(Cleansed_Statuses, Battle?.Statuses1, Battle?.Attack_Type ?? Kernel_bin.Attack_Type.Curative_Item, Battle?.Attack_Flags) ?? false;
            //ret = Memory.State[obj.ToCharacters()]?.DealDamage(Heals, Battle?.Attack_Type ?? Kernel_bin.Attack_Type.Curative_Item, Battle?.Attack_Flags) ?? false || ret;
            //return ret;
            HealAction(Memory.State[obj], battle);

        private bool HealAction(Damageable c, bool battle)
        {
            //c.ChangeHP(-Heals);
            bool ret = false;

            if (c != null && !ZombieCheck(c.Statuses0, battle))
            {
                if (!c.StatusImmune && Cleansed_Statuses != Kernel_bin.Persistant_Statuses.None)
                    ret = c.DealStatus(Cleansed_Statuses, Battle?.Statuses1, Battle?.Attack_Type ?? Kernel_bin.Attack_Type.Curative_Item, Battle?.Attack_Flags);
                ret = c.DealDamage(Heals, Battle?.Attack_Type ?? Kernel_bin.Attack_Type.Curative_Item, Battle?.Attack_Flags) || ret;
            }
            return ret;
        }

        private bool HealGFAction(Faces.ID obj, bool battle = false) => HealAction(Memory.State[obj.ToGFs()], battle);

        private bool LampAction(Faces.ID obj, bool battle = false) => false;

        private bool MagazineAction(Faces.ID obj, bool battle = false) =>
            //TODO display magazine
            false;

        private bool NoneAction(Faces.ID obj, bool battle = false) => false;

        private bool ReviveAction(Faces.ID obj, bool battle = false) => ReviveAction(Memory.State[obj], battle);

        private bool ReviveAction(Damageable c, bool battle)
        {
            bool ret = false;
            if (c != null && !ZombieCheck(c.Statuses0, battle))
            {
                if (!c.StatusImmune && Cleansed_Statuses != Kernel_bin.Persistant_Statuses.None)
                    ret = c.DealStatus(Cleansed_Statuses, Battle?.Statuses1, Battle?.Attack_Type ?? Kernel_bin.Attack_Type.Curative_Item, Battle?.Attack_Flags);
                ret = c.DealDamage(0, Battle?.Attack_Type ?? Kernel_bin.Attack_Type.Revive, Battle?.Attack_Flags) || ret;
            }
            return ret;
        }

        private bool ReviveGFAction(Faces.ID obj, bool battle = false) => ReviveAction(Memory.State[obj], battle);

        private bool SavePointHealAction(Faces.ID obj, bool battle = false) => false;

        private bool SolomonRingAction(Faces.ID obj, bool battle = false) => false;

        private bool StatAction(Faces.ID obj, bool battle = false) => false;

        private bool ZombieCheck(Kernel_bin.Persistant_Statuses statuses0, bool battle = false) =>
                                                                                                    (statuses0 & Kernel_bin.Persistant_Statuses.Zombie) != 0 &&
            (Cleansed_Statuses & Kernel_bin.Persistant_Statuses.Zombie) == 0 &&
            !battle;

        #endregion Methods
    }

    /// <summary>
    /// Info on Items in menus. Items have a slightly limited effect in menues. See
    /// Kernel_bin.Battle_Items for effects in battle.
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

        #region Indexers

        public Item_In_Menu this[byte item] => Items[item];
        public Item_In_Menu this[Saves.Item item] => Items[item.ID];

        #endregion Indexers

        #region Methods

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