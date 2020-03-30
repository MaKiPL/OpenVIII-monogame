using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum ItemTarget : byte
    {
        None = 0x0,
        Useable = 0x1,//usable
        Character = 0x2,
        GF = 0x4,
        All = 0x8,
        All2 = 0x10,// all two?
        KO = 0x20,
        BlueMagic = 0x40,//blue magic?
        Compatibility = 0x80,//compatibility
    }

    public enum ItemType : byte
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

        GFCompatibility = 0x10,
        GFLearn = 0x11,
        GFForget = 0x12,

        /// <summary>
        /// blue magic only shows quistis in target list, should detect if she knows the spell to
        /// grey it out item.
        /// </summary>
        BlueMagic = 0x13, // learn

        Stat = 0x14,
        CureAbnormalStatus = 0x15,
    }

    /// <summary>
    /// Effects of Items inside the in game menu.
    /// </summary>
    public struct ItemInMenu
    {
        #region Fields

        private const int BulletOffset = 101;

        private byte _b2;
        private byte _b3;
        private ItemTarget _itemTarget;
        private ItemType _itemType;
        private Dictionary<ItemType, Func<Faces.ID, bool, bool>> _useActions;

        #endregion Fields

        #region Properties

        private bool All => (ItemTarget & (ItemTarget.All | ItemTarget.All2)) != 0;

        private Kernel.AttackType AttackType => Battle?.AttackType ?? Kernel.AttackType.None;

        public Kernel.BattleItemData Battle
        {
            get
            {
                if (Memory.KernelBin.BattleItemsData != null)
                    return (Memory.KernelBin.BattleItemsData.Count) > ID
                        ? Memory.KernelBin.BattleItemsData[ID]
                        : null;
                return null;
            }
        }

        /// <summary>
        /// Which persistant statuses are removed.
        /// </summary>
        public Kernel.PersistentStatuses CleansedStatuses
        {
            get
            {
                var ret = Kernel.PersistentStatuses.None;
                if (ItemType == ItemType.HealGF || ItemType == ItemType.Heal || ItemType == ItemType.Revive || ItemType == ItemType.ReviveGF ||
                    ItemType == ItemType.CureAbnormalStatus || ItemType == ItemType.SavePointHeal)
                    ret = (Kernel.PersistentStatuses)_b3;
                return ret;
            }
        }

        public FF8String Description => Battle?.Description ?? NonBattle?.Description;

        public byte FadedPalette => 7;

        /// <summary>
        /// How much healing is done
        /// </summary>
        public ushort Heals
        {
            get
            {
                if (ItemType != ItemType.Heal && ItemType != ItemType.HealGF &&
                    ItemType != ItemType.SavePointHeal) return 0;
                if (AttackType == Kernel.AttackType.GivePercentageHP)
                    return (byte)((_b2 * 100) / byte.MaxValue);
                return (ushort)(_b2 * 0x32);
                //else if (Type == _Type.Revive || Type == _Type.ReviveGF)
                //    return 0; // 12.5%
            }
        }

        public Icons.ID Icon { get; private set; }

        /// <summary>
        /// Item ID
        /// </summary>
        public byte ID { get; private set; }

        /// <summary>
        /// Who is targeted and 0x01 seems to be a useable item in menu item. Magazine values don't
        /// seem to corrispond.
        /// </summary>
        public ItemTarget ItemTarget => ItemType == ItemType.Magazine ? ItemTarget.None : _itemTarget;

        /// <summary>
        /// Type of item.
        /// </summary>
        public ItemType ItemType
        {
            get
            {
                if (ID == 0) _itemType = ItemType.None;
                return _itemType;
            }
            private set => _itemType = value;
        }

        /// <summary>
        /// Abilities a GF can learn
        /// </summary>
        public Kernel.Abilities Learn => ItemType == ItemType.GFLearn ? (Kernel.Abilities)_b2 : Kernel.Abilities.None;

        public Kernel.BlueMagic LearnedBlueMagic => ItemType == ItemType.BlueMagic ? (Kernel.BlueMagic)_b2 : Kernel.BlueMagic.None;

        public FF8String Name => Battle?.Name ?? NonBattle?.Name;

        public Kernel.NonBattleItemsData NonBattle => Battle == null ? Memory.KernelBin.NonBattleItemsData[ID - (Memory.KernelBin.BattleItemsData?.Count ?? 0)] : null;

        public byte Palette => 9;

        public Kernel.ShotIrvineLimitBreak Shot
        {
            get
            {
                if (Memory.KernelBin.ShotIrvineLimitBreak == null) return null;
                var id = ID - BulletOffset;
                return (Memory.KernelBin.ShotIrvineLimitBreak.Count) < id ||
                       id < 0
                    ? null
                    : Memory.KernelBin.ShotIrvineLimitBreak[id];
            }
        }

        public Kernel.Stat Stat => ItemType == ItemType.Stat ? (Kernel.Stat)_b3 : Kernel.Stat.None;
        public byte StatIncrease => (byte)(ItemType == ItemType.Stat ? _b2 : 0);
        /*
                /// <summary>
                /// Target in byte form
                /// </summary>
                private byte TargetByte => (byte)_target;
        */

        private IReadOnlyDictionary<ItemType, Func<Faces.ID, bool, bool>> UseActions
        {
            get
            {
                if (_useActions == null)
                {
                    _useActions = new Dictionary<ItemType, Func<Faces.ID, bool, bool>>
                    {
                        {ItemType.Heal, HealAction },
                        {ItemType.Revive, ReviveAction },
                        {ItemType.HealGF, HealGFAction },
                        {ItemType.ReviveGF, ReviveGFAction },
                        {ItemType.SavePointHeal, SavePointHealAction },
                        {ItemType.Battle, BattleAction },
                        {ItemType.Ammo, AmmoAction },
                        {ItemType.Magazine, MagazineAction },
                        {ItemType.None, NoneAction },
                        {ItemType.GF, GFAction },
                        {ItemType.Angelo, AngeloAction },
                        {ItemType.Chocobo, ChocoboAction },
                        {ItemType.Lamp, LampAction },
                        {ItemType.SolomonRing, SolomonRingAction },
                        {ItemType.GFCompatibility, GFCompatibilityAction },
                        {ItemType.GFLearn, GFLearnAction },
                        {ItemType.GFForget, GFForgetAction },
                        {ItemType.BlueMagic, Blue_MagicAction },
                        {ItemType.Stat, StatAction },
                        {ItemType.CureAbnormalStatus, Cure_Abnormal_StatusAction },
                    };
                }
                return _useActions;
            }
        }

        #endregion Properties

        #region Methods

        public static ItemInMenu Read(BinaryReader br, byte i)
        {
            Memory.Log.WriteLine($"{nameof(ItemInMenu)} :: {nameof(Read)} :: {i}");
            var tmp = new ItemInMenu
            {
                ItemType = (ItemType)br.ReadByte(),
                _itemTarget = (ItemTarget)br.ReadByte(),
                _b2 = br.ReadByte(),
                _b3 = br.ReadByte(),
                ID = i
            };
            switch (tmp.ItemType)
            {
                case ItemType.Heal:
                case ItemType.Revive:
                case ItemType.CureAbnormalStatus:
                    tmp.Icon = Icons.ID.Item_Recovery;
                    break;

                case ItemType.HealGF:
                case ItemType.ReviveGF:
                case ItemType.GF:
                case ItemType.GFForget:
                case ItemType.GFLearn:
                    tmp.Icon = Icons.ID.Item_GF;
                    break;

                case ItemType.Battle:
                case ItemType.Angelo:
                case ItemType.Chocobo: // i'm not sure about this one i have no chocobo items.
                case ItemType.SolomonRing:
                case ItemType.Lamp:
                    tmp.Icon = Icons.ID.Item_Battle;
                    break;

                case ItemType.SavePointHeal:
                    tmp.Icon = Icons.ID.Item_Tent;
                    break;

                case ItemType.Ammo:
                    tmp.Icon = Icons.ID.Item_Ammo;
                    break;

                case ItemType.Magazine:
                    tmp.Icon = Icons.ID.Item_Magazine;
                    break;

                case ItemType.None:
                case ItemType.BlueMagic:
                case ItemType.GFCompatibility:
                case ItemType.Stat:
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
            if (ItemType == ItemType.GFCompatibility)
            {
                gf = (GFs)_b2;
                switch (_b3)
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
                        neg = 0x00;
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
            if (LearnedBlueMagic != Kernel.BlueMagic.None)
                return !Memory.State.LimitBreakQuistisUnlockedBlueMagic[(int)LearnedBlueMagic];
            return false;
        }

        public bool TestCharacter(ref Faces.ID id, out Characters character)
        {
            character = id.ToCharacters();
            if (ItemType == ItemType.BlueMagic)
            {
                return character == Characters.Quistis_Trepe && TestBlueMagic();
            }

            var teamLaguna = character == Characters.Laguna_Loire ||
                    character == Characters.Kiros_Seagill ||
                    character == Characters.Ward_Zabac;
            if(Memory.State.TeamLaguna && !teamLaguna)
                return false;
            if (!Memory.State.TeamLaguna && teamLaguna)
                return false;

            return ItemTarget.HasFlag(ItemTarget.Character)&& (Memory.State[character]?.Available ?? false);
        }

        public bool TestGF(ref Faces.ID id, out GFs gf)
        {
            gf = id.ToGFs();
            if (ItemType == ItemType.Angelo && id == Faces.ID.Angelo)
            {
                return true;
            }
            if (ItemType == ItemType.Chocobo && id == Faces.ID.Boko)
            {
                return true;
            }
            if (gf == GFs.Blank || gf == GFs.All || (ItemTarget & ItemTarget.GF) == 0)
                return false;
            if (Memory.State?.GFs != null && Memory.State.GFs.ContainsKey(gf))// && Memory.State.GFs[gf].VisibleInMenu)
            {
                if (ItemType == ItemType.GFLearn && (!Memory.State.GFs[gf].TestGFCanLearn(Learn) || Memory.State.GFs[gf].MaxGFAbilities))
                    return false;
                return true;
            }
            return false;
        }

        public override string ToString() => Name ?? base.ToString();

        public bool Use(Faces.ID obj, bool battle = false)
        {
            if (UseActions.ContainsKey(ItemType))
            {
                var ret = GFAction(UseActions[ItemType], obj, battle);
                ret = CharAction(UseActions[ItemType], obj, battle) || ret;
                if (ret)
                {
                    for (var i = 0; i < Memory.State.Items.Count; i++)
                    {
                        if (Memory.State.Items[i].ID == ID)
                            Memory.State.Items[i].UsedOne();
                    }
                    //Memory.State.Items.Where(m => m.ID == ID).ForEach(x => x.UsedOne());
                    return true;
                }
            }
            return false;
        }

        public bool ValidTarget(bool b = false)
        {
            if (ItemType == ItemType.Magazine) return true; //TODO pressing okay display Magazine.
            else if (ItemType == ItemType.SolomonRing) return !Memory.State[GFs.Doomtrain]?.Exists ?? true; //TODO detect doomtrain and return false if unlocked
            else if (ItemType == ItemType.Lamp) return !Memory.State[GFs.Diablos]?.Exists ?? true;  //TODO detect diablo and return false if unlocked
            Faces.ID _face;
            foreach (var face in (Faces.ID[])Enum.GetValues(typeof(Faces.ID)))
            {
                _face = face;
                if (TestCharacter(ref _face, out _) || TestGF(ref _face, out _))
                {
                    return true;
                }
            }
            if (Battle != null && b) return true;
            return false;
        }

        private static bool ChocoboAction(Faces.ID obj, bool battle = false) => false;

        private static bool GFAction(Faces.ID obj, bool battle = false) => false;

        private static bool GFCompatibilityAction(Faces.ID obj, bool battle = false) => false;

        /// <summary>
        /// Spawn a menu of GF's unlocked abilities and you select one to remove.
        /// </summary>
        private static bool GFForgetAction(Faces.ID obj, bool battle = false) => false;

        private bool AmmoAction(Faces.ID obj, bool battle = false) => false;

        private bool AngeloAction(Faces.ID obj, bool battle = false) => false;

        private bool BattleAction(Faces.ID obj, bool battle = false) => false;

        private bool Blue_MagicAction(Faces.ID obj, bool battle = false) => false;

        private bool CharAction(Func<Faces.ID, bool, bool> func, Faces.ID obj, bool battle = false)
        {
            var ret = false;
            if (All)
            {
                foreach (var c in Memory.State.PartyData)
                {
                    obj = c.ToFacesID();
                    if (TestCharacter(ref obj, out _))
                        ret = func(obj, battle) || ret;
                }
                return ret;
            }
            if (TestCharacter(ref obj, out _))
                ret = func(obj, battle);

            return ret;
        }

        private bool Cure_Abnormal_StatusAction(Faces.ID obj, bool battle = false) => Cure_Abnormal_StatusAction(Memory.State[obj]);

        private bool Cure_Abnormal_StatusAction(IStatusEffects c)
        {
            var ret = false;
            if (c != null && !c.StatusImmune)
                ret = c.DealStatus(CleansedStatuses, Battle?.Statuses1, Battle?.AttackType ?? Kernel.AttackType.CurativeItem, Battle?.AttackFlags);
            return ret;
        }

        private bool GFAction(Func<Faces.ID, bool, bool> func, Faces.ID obj, bool battle = false)
        {
            var ret = false;
            if (All)
            {
                foreach (var c in Memory.State.GFs.Where(x => x.Value.Exists))
                {
                    obj = c.Key.ToFacesID();
                    if (TestGF(ref obj, out _))
                        ret = func(obj, battle) || ret;
                }
                return ret;
            }

            if (TestGF(ref obj, out _))
                ret = func(obj, battle);

            return ret;
        }

        /// <summary>
        /// GF learns new skill if below max abilities
        /// </summary>
        private bool GFLearnAction(Faces.ID obj, bool battle = false) => GFLearnAction(Memory.State[obj.ToGFs()]);

        private bool GFLearnAction(Saves.GFData gf) => gf.Learn(Learn);

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
            var ret = false;

            if (c != null && !ZombieCheck(c.Statuses0, battle))
            {
                if (!c.StatusImmune && CleansedStatuses != Kernel.PersistentStatuses.None)
                    ret = c.DealStatus(CleansedStatuses, Battle?.Statuses1, Battle?.AttackType ?? Kernel.AttackType.CurativeItem, Battle?.AttackFlags);
                ret = c.DealDamage(Heals, Battle?.AttackType ?? Kernel.AttackType.CurativeItem, Battle?.AttackFlags) || ret;
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
            var ret = false;
            if (c != null && !ZombieCheck(c.Statuses0, battle))
            {
                if (!c.StatusImmune && CleansedStatuses != Kernel.PersistentStatuses.None)
                    ret = c.DealStatus(CleansedStatuses, Battle?.Statuses1, Battle?.AttackType ?? Kernel.AttackType.CurativeItem, Battle?.AttackFlags);
                ret = c.DealDamage(0, Battle?.AttackType ?? Kernel.AttackType.Revive, Battle?.AttackFlags) || ret;
            }
            return ret;
        }

        private bool ReviveGFAction(Faces.ID obj, bool battle = false) => ReviveAction(Memory.State[obj], battle);

        private bool SavePointHealAction(Faces.ID obj, bool battle = false) => false;

        private bool SolomonRingAction(Faces.ID obj, bool battle = false) => false;

        private bool StatAction(Faces.ID obj, bool battle = false) => false;

        private bool ZombieCheck(Kernel.PersistentStatuses statuses0, bool battle = false) =>
                                                                                                    (statuses0 & Kernel.PersistentStatuses.Zombie) != 0 &&
            (CleansedStatuses & Kernel.PersistentStatuses.Zombie) == 0 &&
            !battle;

        #endregion Methods
    }

    /// <summary>
    /// Info on Items in menus. Items have a slightly limited effect in menues. See
    /// Kernel_bin.Battle_Items for effects in battle.
    /// </summary>
    /// <see cref="http://forums.qhimm.com/index.php?topic=17098.0"/>
    public class ItemsInMenu : IReadOnlyList<ItemInMenu>
    {
        #region Fields

        private readonly List<ItemInMenu> _items;

        #endregion Fields

        #region Constructors

        private ItemsInMenu()
        {
            Memory.Log.WriteLine($"{nameof(ItemsInMenu)} :: new ");
            _items = new List<ItemInMenu>();
        }

        #endregion Constructors

        #region Properties

        public int Count => _items.Count;
        public IReadOnlyList<ItemInMenu> Items => _items;

        #endregion Properties

        #region Indexers

        public ItemInMenu this[byte item] => Items[item];
        public ItemInMenu this[Saves.Item item] => Items[item.ID];

        public ItemInMenu this[int index] => _items[index];

        #endregion Indexers

        #region Methods

        public static ItemsInMenu Read()
        {
            var aw = ArchiveWorker.Load(Memory.Archives.A_MENU);
            var buffer = aw.GetBinaryFile("mitem.bin");
            if (buffer != null)
                using (var br = new BinaryReader(new MemoryStream(buffer)))
                {
                    return Read(br);
                }
            return default;
        }

        public IEnumerator<ItemInMenu> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

        private static ItemsInMenu Read(BinaryReader br)
        {
            Memory.Log.WriteLine($"{nameof(ItemsInMenu)} :: {nameof(Read)} ");
            var ret = new ItemsInMenu();
            for (byte i = 0; br.BaseStream.Position + 4 <= br.BaseStream.Length; i++)
            {
                var tmp = ItemInMenu.Read(br, i);
                ret._items.Add(tmp);
            }
            return ret;
        }

        #endregion Methods
    }
}