using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
// ReSharper disable UnusedMember.Global

namespace OpenVIII
{
    public static partial class Saves
    {
        [Flags]
        public enum BattleMiscFlags : byte
        {
            Dream = 0x1,
            Odin = 0x2,
            Phoenix = 0x4,
            Gilgamesh = 0x8,
            AngeloDisabled = 0x10,
            AngelWingEnabled = 0x20,
            Unk40 = 0x40,
            Unk80 = 0x80
        }
        #region Classes

        /// <summary>
        /// Save Data
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat"/>
        public class Data
        {
            #region Fields

            private Dictionary<Characters, CharacterData> _characters;
            private Dictionary<GFs, GFData> _gfs;
            private List<Shop> _shops;

            #endregion Fields

            #region Constructors

            public Data()
            {
                //Define Containers
                _gfs = new Dictionary<GFs, GFData>(16);
                _characters = new Dictionary<Characters, CharacterData>(8);
                _shops = new List<Shop>(20);
                Items = new List<Item>(198);
                TimePlayed = new TimeSpan();
                CoordinateX = new short[3];
                CoordinateY = new short[3];
                TriangleID = new ushort[3];
                FieldVars = new FieldVars(); //0x0D70 http://wiki.ffrtt.ru/index.php/FF8/Variables
                WorldMap = new Worldmap();//br.ReadBytes(128);//0x1270
                TripleTriad = new TripleTriad(); //br.ReadBytes(128);//0x12F0
                ChocoboWorld = new ChocoboWorld(); //br.ReadBytes(64);//0x1370
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// 0x000C 4 bytes Preview: Amount of Gil
            /// </summary>
            public uint AmountOfGilPreview { get; set; }

            /// <summary>
            /// 0x0B20 4 bytes Amount of Gil (Laguna)
            /// </summary>
            public uint AmountOfGilLaguna { get; set; }

            /// <summary>
            /// 0x0B1C 4 bytes Amount of Gil
            /// </summary>
            public uint AmountOfGil { get; set; }

            /// <summary>
            /// 0x0040 12 bytes Preview: Angelo's name (0x00 terminated)
            /// </summary>
            public FF8String AngeloName { get; set; }

            public byte AveragePartyLevel
            {
                get
                {
                    int level = 0;
                    int cnt = 0;
                    for (int p = 0; p < 3; p++)
                    {
                        Characters c = PartyData?[p] ?? OpenVIII.Characters.Squall_Leonhart;
                        if (c != OpenVIII.Characters.Blank)
                        {
                            level += Characters?[c].Level ?? 0;
                            cnt++;
                        }
                    }
                    return (byte)MathHelper.Clamp(level / cnt, 0, 100);
                }
            }

            /// <summary>
            /// 0x0D08 4 bytes Battle: First "Bomb" battle (Elemental tip)
            /// </summary>
            public uint BattleElemental { get; set; }

            /// <summary>
            /// 0x0CF2 2 bytes Battle: battle escaped
            /// </summary>
            public ushort BattleEscapeCount { get; set; }

            /// <summary>
            /// 0x0D10 4 bytes Battle: First "Irvine" battle (Irvine's limit break tip)
            /// </summary>
            public uint BattleIrvine { get; set; }

            /// <summary>
            /// 0x0D14 8 bytes Battle: Magic drawn once
            /// </summary>
            public BitArray BattleMagic { get; set; }

            /// <summary>
            /// 0x0D0C 4 bytes Battle: First "T-Rex" battle (Mental tip)
            /// </summary>
            public uint BattleMental { get; set; }

            /// <summary>
            /// 0x0D04 4 bytes Battle: First "Bug" battle (R1 tip)
            /// </summary>
            public uint BattleR1 { get; set; }

            /// <summary>
            /// 0x0D30 1 byte Battle: Renzokuken auto
            /// </summary>
            public bool BattleRenzokukenAuto { get; set; }

            /// <summary>
            /// 0x0D31 1 byte Battle: Renzokuken indicator
            /// </summary>
            public bool BattleRenzokukenIndicator { get; set; }

            /// <summary>
            /// 0x0D1C 20 bytes Battle: Enemy scanned once
            /// </summary>
            public BitArray BattleScan { get; set; }

            /// <summary>
            /// 0x0CF4 4 bytes Unknown
            /// </summary>
            public uint BattleTonberryKilledCount { get; set; }

            /// <summary>
            /// 0x0CFC 4 bytes Battle: Tonberry King killed (yeah, this is a boolean)
            /// </summary>
            public bool BattleTonberryKingKilled { get; set; }

            /// <summary>
            /// Battle: dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
            /// </summary>
            public BattleMiscFlags BattleMiscFlags { get; set; }

            

            /// <summary>
            /// 0x0CEC 4 bytes Battle: victory count
            /// </summary>
            public uint BattleVictoryCount { get; set; }

            /// <summary>
            /// 0x004C 12 bytes Preview: Boko's name (0x00 terminated)
            /// </summary>
            public FF8String BokoName { get; set; }

            /// <summary>
            /// 0x04A0-0x08C7 152 bytes each 8 of them. Characters: Squall-Edea
            /// </summary>
            public IReadOnlyDictionary<Characters, CharacterData> Characters
            {
                get
                {
                    if (_characters.Values.Count > 0)
                        return _characters;
                    return null;
                }
            }

            /// <summary>
            /// 0x1370 64 bytes Chocobo World (TODO)
            /// </summary>
            public ChocoboWorld ChocoboWorld { get; set; }

            /// <summary>
            /// 0x0AF0 20 bytes Configuration
            /// </summary>
            public Configuration Configuration { get; set; }

            /// <summary>
            /// 0x0D56 3*2 bytes (signed) coordinate X (party1, party2, party3)
            /// </summary>
            public short[] CoordinateX { get; set; }

            /// <summary>
            /// 0x0D5C 3*2 bytes (signed) coordinate Y (party1, party2, party3)
            /// </summary>
            public short[] CoordinateY { get; set; }

            /// <summary>
            /// 0x0CE4 4 bytes Countdown
            /// </summary>
            public uint Countdown { get; set; }

            /// <summary>
            /// 0x0058 4 bytes Preview: Current Disk (0 based)
            /// </summary>
            public uint CurrentDisk { get; set; }

            /// <summary>
            /// 0x0D52 2 bytes Current field
            /// </summary>
            public ushort CurrentField { get; set; }

            /// <summary>
            /// 0x005C 4 bytes Preview: Current save (last saved game)
            /// </summary>
            public uint CurrentSave { get; set; }

            /// <summary>
            /// 0x0D68 3*1 bytes Direction (party1, party2, party3)
            /// </summary>
            public byte[] Direction { get; set; }

            /// <summary>
            /// Time since loaded
            /// </summary>
            public TimeSpan ElapsedTimeSinceLoad => (Memory.TotalGameTime - LoadTime);

            /// <summary>
            /// 0x0D70 256 + 1024 bytes Field vars
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/Variables"/>
            public FieldVars FieldVars { get; set; }

            /// <summary>
            /// 0x0006 2 bytes Preview: 1st character's current HP
            /// </summary>
            public ushort FirstCharactersCurrentHp { get; set; }

            /// <summary>
            /// 0x0024 1 byte Preview: 1st character's level
            /// </summary>
            public byte FirstCharactersLevel { get; set; }

            /// <summary>
            /// 0x0008 2 bytes Preview: 1st character's max HP
            /// </summary>
            public ushort FirstCharactersMaxHp { get; set; }

            /// <summary>
            /// <para>0x0CE0 4 bytes Game time</para>
            /// <para>unsure if this is a duplicate of Time played or something.</para>
            /// </summary>
            public TimeSpan GameTime { get; private set; }

            /// <summary>
            /// 0x0060 - 0x049F 68 bytes each 16 of them. Guardian Forces: Quetzalcoatl-Eden
            /// </summary>
            public IReadOnlyDictionary<GFs, GFData> GFs => _gfs;

            /// <summary>
            /// 0x0B0C 12 bytes Griever name (FF8 text format)
            /// </summary>
            public FF8String GrieverName { get; set; }

            /// <summary>
            /// <para>0x0B54 396 bytes Items 198 items (Item ID and Quantity)</para>
            /// <para>
            /// The order of items out of battle and Each item uses 2 bytes 1 for ID and 1 for Quantity
            /// </para>
            /// </summary>
            public List<Item> Items { get; private set; }

            /// <summary>
            /// <para>0x0B34 32 bytes Items battle order</para>
            /// <para>Only the items that can be used in battle</para>
            /// <para>The order they appear in the battle item menu.</para>
            /// </summary>
            public byte[] ItemsBattleOrder { get; set; }

            /// <summary>
            /// 0x0B08 4 bytes Known weapons
            /// </summary>
            public BitArray KnownWeapons { get; set; }

            /// <summary>
            /// <para>0x0B2A 1 byte Limit Break Angelo completed</para>
            /// <para>Each bit is a completely learned ability</para>
            /// </summary>
            public Angelo LimitBreakAngeloCompleted { get; set; }

            /// <summary>
            /// <para>0x0B2B 1 byte Limit Break Angelo known</para>
            /// <para>Each bit is an ability is known about/able to be learned</para>
            /// </summary>
            public Angelo LimitBreakAngeloKnown { get; set; }

            /// <summary>
            /// <para>0x0B2C 8 bytes Limit Break Angelo points</para>
            /// <para>Each byte is progress to learning an ability</para>
            /// </summary>
            public Dictionary<Angelo, byte> LimitBreakAngeloPoints { get; set; }

            public BitArray LimitBreakIrvineUnlockedShot { get; set; }

            /// <summary>
            /// <para>0x0B24 2 bytes Limit Break Quistis</para>
            /// <para>Each bit is an unlocked blue magic spell</para>
            /// </summary>
            public BitArray LimitBreakQuistisUnlockedBlueMagic { get; set; }

            /// <summary>
            /// <para>0x0B29 1 byte Limit Break Selphie</para>
            /// <para>I think this sets bits when a rare spell is used.</para>
            /// </summary>
            public BitArray LimitBreakSelphieUsedRareSpells { get; set; }

            /// <summary>
            /// <para>0x0B26 2 bytes Limit Break Zell</para>
            /// <para>Each bit is a combo/attack unlocked. So it shows on screen.</para>
            /// <para>You can use the combo if you know it.</para>
            /// </summary>
            public BitArray LimitBreakZellUnlockedDuel { get; set; }

            /// <summary>
            /// xna GameTime when loaded
            /// </summary>
            public TimeSpan LoadTime { get; set; }

            /// <summary>
            /// 0x0004 2 bytes Preview: Location ID
            /// </summary>
            public ushort LocationID { get; set; }

            /// <summary>
            /// 0x0D50 2 bytes Module (1= field, 2= world map, 3= battle)
            /// </summary>
            public ushort Module { get; set; }

            public IEnumerable<CharacterData> NonPartyMembers => Characters == null
                ? null
                : (from i in Characters where !Party.Contains(i.Key) && i.Value.Available select i.Value);

            /// <summary>
            /// 0x0D6B 1 byte Padding
            /// </summary>
            public byte Padding { get; set; }

            /// <summary>
            /// 0x0025-0x0027 1 byte Preview: 1st-3rd character's portrait; 0xFF = blank;
            /// </summary>
            public List<Characters> Party { get; set; }

            /// <summary>
            /// 0x0D48 4 bytes Party (last byte always = 255)
            /// </summary>
            public Characters[] Party2 { get; set; }

            /// <summary>
            /// 0x0B04 4 bytes Party (0xFF terminated and/or blank)
            /// </summary>
            public List<Characters> PartyData { get; set; }

            /// <summary>
            /// 0x0D54 2 bytes Previous field
            /// </summary>
            public ushort PreviousField { get; set; }

            /// <summary>
            /// 0x0034 12 bytes Preview: Rinoa's name (0x00 terminated)
            /// </summary>
            public FF8String RinoaName { get; set; }

            /// <summary>
            /// 0x000A 2 bytes Preview: save count
            /// </summary>
            public ushort SaveCount { get; set; }

            /// <summary>
            /// 0x0D43 1 byte SeeD test level
            /// </summary>
            public byte SeeDTestLevel { get; set; }

            /// <summary>
            /// 0x0960-0x0AEF 400 total bytes 20 bytes each 20 of them. Shops
            /// </summary>
            public IReadOnlyList<Shop> Shops => _shops;

            public bool SmallTeam
            {
                get
                {
                    if (Characters != null)
                    {
                        foreach (KeyValuePair<Characters, CharacterData> i in Characters)
                        {
                            if (!Party.Contains(i.Key) && i.Value.Available)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }

            /// <summary>
            /// 0x0028 12 bytes Preview: Squall's name (0x00 terminated)
            /// </summary>
            public FF8String SquallName { get; set; }

            public bool TeamLaguna => Party != null && (Party[0] == OpenVIII.Characters.Laguna_Loire || Party[1] == OpenVIII.Characters.Laguna_Loire || Party[2] == OpenVIII.Characters.Laguna_Loire);

            /// <summary>
            /// Stored playtime in seconds. Made into timespan for easy parsing.
            /// </summary>
            /// <remarks>0x0020 4 bytes Preview: Total number of seconds played</remarks>
            public TimeSpan TimePlayed { get; set; }

            /// <summary>
            /// 0x0D62 3*2 bytes Triangle ID (party1, party2, party3)
            /// </summary>
            public ushort[] TriangleID { get; set; }

            /// <summary>
            /// 0x12F0 128 bytes Triple Triad (TODO flush this out)
            /// </summary>
            public TripleTriad TripleTriad { get; set; }

            /// <summary>
            /// 0x0D33 16 bytes Tutorial infos
            /// </summary>
            public BitArray TutorialInfos { get; set; }

            /// <summary>
            /// 0x0B18 2 bytes Unknown (always 7966?)
            /// </summary>
            public ushort Unknown1 { get; set; }

            /// <summary>
            /// 0x0B1A 2 bytes Unknown
            /// </summary>
            public ushort Unknown2 { get; set; }

            /// <summary>
            /// 0x0CE8 4 bytes Unknown
            /// </summary>
            public uint Unknown3 { get; set; }

            /// <summary>
            /// 0x0CF0 2 bytes Unknown
            /// </summary>
            public ushort Unknown4 { get; set; }

            public uint Unknown5 { get; set; }

            /// <summary>
            /// 0x0D00 4 bytes Unknown
            /// </summary>
            public uint Unknown6 { get; set; }

            /// <summary>
            /// 0x0D44 4 bytes Unknown
            /// </summary>
            public uint Unknown7 { get; set; }

            /// <summary>
            /// 0x0D4C 4 bytes Unknown
            /// </summary>
            public uint Unknown8 { get; set; }

            /// <summary>
            /// 0x0D6C 4 bytes Unknown
            /// </summary>
            public uint Unknown9 { get; set; }

            /// <summary>
            /// 0x1270 128 bytes World map
            /// </summary>
            public Worldmap WorldMap { get; set; }

            #endregion Properties

            #region Indexers

            public GFData this[GFs id] => GetDamageable(id);

            public CharacterData this[Characters id] => GetDamageable(id);

            public Damageable this[Faces.ID id] => GetDamageable(id);

            #endregion Indexers

            #region Methods

            public static Data LoadInitOut()
            {
                Memory.Log.WriteLine($"{nameof(Saves)} :: {nameof(Data)} :: {nameof(LoadInitOut)} ");
                ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_MAIN, true);
                byte[] buffer = aw.GetBinaryFile("init.out");
                if (buffer != null && buffer.Length >0)
                {
                    using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
                    {
                        Data data = new Data();
                        data.ReadInitOut(br);
                        return data;
                    }
                }
                return null;
            }

            /// <summary>
            /// preforms a Shadow Copy. Then does deep copy on any required objects.
            /// </summary>
            /// <returns></returns>
            public Data Clone()
            {
                Memory.Log.WriteLine($"{nameof(Saves)} :: {nameof(Data)} :: {nameof(Clone)} ");
                //shadow copy
                Data d = (Data)MemberwiseClone();
                //deep copy anything that needs it here.

                d._characters = _characters.ToDictionary(x => x.Key, x => (CharacterData)x.Value.Clone());
                d._gfs = _gfs.ToDictionary(x => x.Key, x => (GFData)x.Value.Clone());
                d.ChocoboWorld = ChocoboWorld.Clone();
                d.FieldVars = FieldVars.Clone();
                d.WorldMap = WorldMap.Clone();
                d.TripleTriad = TripleTriad.Clone();
                d.LimitBreakAngeloPoints = LimitBreakAngeloPoints.ToDictionary(x => x.Key, x => x.Value);
                d._shops = _shops.Select(x => x.Clone()).ToList();
                d.Items = Items.Select(x => x.Clone()).ToList();
                return d;
            }

            //public Damageable this[Damageable damageable]
            /// <summary>
            /// return -1 on error
            /// </summary>
            /// <param name="id"></param>
            /// <param name="character"></param>
            /// <param name="gf"></param>
            /// <returns></returns>
            public int CurrentHP(Faces.ID id = Faces.ID.Blank, Characters character = OpenVIII.Characters.Blank, GFs gf = OpenVIII.GFs.Blank)
            {
                if (character == OpenVIII.Characters.Blank)
                    character = id.ToCharacters();
                if (gf == OpenVIII.GFs.Blank)
                    gf = id.ToGFs();
                int hp = (Characters.ContainsKey(character) ? Characters[character].CurrentHP() : -1);
                hp = (hp < 0 && GFs.ContainsKey(gf) ? GFs[gf].CurrentHP() : hp);
                return hp;
            }

            /// <summary>
            /// How many dead characters there are.
            /// </summary>
            /// <returns>&gt;=0</returns>
            public int DeadCharacters() =>
                Characters?.Count(m =>
                    m.Value.Available && m.Value.CurrentHP() == 0 ||
                    (m.Value.Statuses0 & Kernel_bin.Persistent_Statuses.Death) != 0) ?? 0;

            /// <summary>
            /// How many dead party members there are.
            /// </summary>
            /// <returns>&gt;=0</returns>
            public int DeadPartyMembers() =>
                PartyData?.Count(m => m != OpenVIII.Characters.Blank && Characters[m].IsDead) ?? 0;

            public ConcurrentQueue<GFs> EarnAP(uint ap, out ConcurrentQueue<KeyValuePair<GFs, Kernel_bin.Abilities>> abilities)
            {
                ConcurrentQueue<GFs> ret = new ConcurrentQueue<GFs>();
                abilities = null;
                if (GFs == null) return ret;
                abilities = new ConcurrentQueue<KeyValuePair<GFs, Kernel_bin.Abilities>>();
                foreach (KeyValuePair<GFs, GFData> g in GFs.Where(i => i.Value != null && i.Value.Exists))
                {
                    if (!g.Value.EarnExp(ap, out Kernel_bin.Abilities ability)) continue;
                    if (ability != Kernel_bin.Abilities.None)
                    {
                        abilities.Enqueue(new KeyValuePair<GFs, Kernel_bin.Abilities>(g.Key, ability));
                    }
                    ret.Enqueue(g.Key);
                }
                return ret;
            }

            public bool EarnItem(Cards.ID card, byte qty, byte location = 0)
            {
                TTCardInfo i = new TTCardInfo { Unlocked = true, Qty = qty, Location = location };
                if (!TripleTriad.cards.TryAdd(card, i))
                {
                    TripleTriad.cards[card].Unlocked = i.Unlocked;
                    TripleTriad.cards[card].Qty += i.Qty;
                    TripleTriad.cards[card].Location = i.Location;
                    return true;
                }
                return false;
            }

            public bool EarnItem(KeyValuePair<Cards.ID, byte> keyValuePair, byte location = 0) => EarnItem(keyValuePair.Key, keyValuePair.Value, location);

            public Item EarnItem(byte iD, int qty)
            {
                if (Items.Any(i => i.ID == iD) && qty != 0)
                {
                    int k = Items.FindIndex(item => item.ID == iD);
                    if (qty < 0)
                        return Items[k] = Items[k].Add(checked((sbyte)qty));
                    if (qty > 0)
                        return Items[k] = Items[k].Add(checked((byte)qty));
                }
                else if (Items.Any(i => i.ID == 0) && qty > 0)
                {
                    int k = Items.FindIndex(item => item.ID == 0);
                    return Items[k] = Items[k].Add(checked((byte)qty), iD);
                }
                return default;
            }

            public Item EarnItem(Item item) => EarnItem(item.ID, item.QTY);

            public Dictionary<GFs, Characters> JunctionedGFs()
            {
                Dictionary<GFs, Characters> r = new Dictionary<GFs, Characters>();
                if (Characters == null) return r;
                foreach (KeyValuePair<Characters, CharacterData> c in Characters)
                {
                    foreach (GFs gf in c.Value.JunctionedGFs)
                        if(!r.ContainsKey(gf) && gf >= OpenVIII.GFs.Quezacotl && gf <= OpenVIII.GFs.Eden )
                            r.Add(gf, c.Key);


                }
                return r;
            }

            public bool MaxGFAbilities(GFs gf) => GFs.ContainsKey(gf) && GFs[gf].MaxGFAbilities;

            public bool PartyHasAbility(Kernel_bin.Abilities a)
            {
                if (PartyData == null) return false;
                foreach (Characters c in PartyData)
                {
                    if (Characters.TryGetValue(c, out CharacterData cd) && cd.Abilities.Contains(a))
                        return true;
                }
                return false;
            }

            public void Read(BinaryReader br)
            {
                //Read Data
                LocationID = br.ReadUInt16();//0x0004
                FirstCharactersCurrentHp = br.ReadUInt16();//0x0006
                FirstCharactersMaxHp = br.ReadUInt16();//0x0008
                SaveCount = br.ReadUInt16();//0x000A
                AmountOfGilPreview = br.ReadUInt32();//0x000C
                TimePlayed = new TimeSpan(0, 0, checked((int)br.ReadUInt32()));//0x0020
                FirstCharactersLevel = br.ReadByte();//0x0024
                Party = Array.ConvertAll(br.ReadBytes(3), item => (Characters)item).ToList();//0x0025//0x0026//0x0027 0xFF = blank.
                SquallName = br.ReadBytes(12);//0x0028
                RinoaName = br.ReadBytes(12);//0x0034
                AngeloName = br.ReadBytes(12);//0x0040
                BokoName = br.ReadBytes(12);//0x004C

                CurrentDisk = br.ReadUInt32();//0x0058
                CurrentSave = br.ReadUInt32();//0x005C
                ReadInitOut(br);
                GameTime = new TimeSpan(0, 0, (int)br.ReadUInt32());//0x0CE0
                Countdown = br.ReadUInt32();//0x0CE4
                Unknown3 = br.ReadUInt32();//0x0CE8
                BattleVictoryCount = br.ReadUInt32();//0x0CEC
                Unknown4 = br.ReadUInt16();//0x0CF0
                BattleEscapeCount = br.ReadUInt16();//0x0CF2
                Unknown5 = br.ReadUInt32();//0x0CF4
                BattleTonberryKilledCount = br.ReadUInt32();//0x0CF8
                BattleTonberryKingKilled = br.ReadUInt32() > 0;//0x0CFC (yeah, this is a boolean)
                Unknown6 = br.ReadUInt32();//0x0D00
                BattleR1 = br.ReadUInt32();//0x0D04 First bug battle (R1 tip)
                BattleElemental = br.ReadUInt32();//0x0D08 First "Bomb" battle (Elemental tip)
                BattleMental = br.ReadUInt32();//0x0D0C  First "T-Rex" battle (Mental tip)
                BattleIrvine = br.ReadUInt32();//0x0D10 First "Irvine" battle (Irvine's limit break tip)
                BattleMagic = new BitArray(br.ReadBytes(8));//0x0D14 Magic drawn once
                BattleScan = new BitArray(br.ReadBytes(20));//0x0D1C Enemy scanned once
                BattleRenzokukenAuto = br.ReadByte() > 0;//0x0D30 Renzokuken auto
                BattleRenzokukenIndicator = br.ReadByte() > 0;//0x0D31 Renzokuken indicator
                BattleMiscFlags = (BattleMiscFlags)br.ReadByte();//0x0D32 dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
                TutorialInfos = new BitArray(br.ReadBytes(16));//0x0D33
                SeeDTestLevel = br.ReadByte();//0x0D43
                Unknown7 = br.ReadUInt32();//0x0D44
                Party2 = Array.ConvertAll(br.ReadBytes(4), item => (Characters)item); //0x0D48 (last byte always = 255) //duplicate?
                Unknown8 = br.ReadUInt32();//0x0D4C
                Module = br.ReadUInt16();//0x0D50 (1= field, 2= world map, 3= battle)
                CurrentField = br.ReadUInt16();//0x0D52
                PreviousField = br.ReadUInt16();//0x0D54
                for (int i = 0; i < 3; i++)
                    CoordinateX[i] = br.ReadInt16();//0x0D56 signed  (party1, party2, party3)
                for (int i = 0; i < 3; i++)
                    CoordinateY[i] = br.ReadInt16();//0x0D5C signed  (party1, party2, party3)
                for (int i = 0; i < 3; i++)
                    TriangleID[i] = br.ReadUInt16();//0x0D62  (party1, party2, party3)
                Direction = br.ReadBytes(3 * 1);//0x0D68  (party1, party2, party3)
                Padding = br.ReadByte();//0x0D6B
                Unknown9 = br.ReadUInt32();//0x0D6C
                FieldVars.Read(br); //0x0D70 http://wiki.ffrtt.ru/index.php/FF8/Variables
                WorldMap.Read(br);//br.ReadBytes(128);//0x1270
                TripleTriad.Read(br); //br.ReadBytes(128);//0x12F0
                ChocoboWorld.Read(br); //br.ReadBytes(64);//0x1370
            }

            /// <summary>
            /// Read Init.Out
            /// </summary>
            /// <see cref="https://github.com/alexfilth/quezacotl/blob/master/Quezacotl/InitWorker.cs"/>
            public void ReadInitOut(BinaryReader br)
            {
                GetNames();
                //init offset 0;
                for (byte i = 0; i <= (int)OpenVIII.GFs.Eden; i++)
                {
                    _gfs.Add((GFs)i, GFData.Load(br, (GFs)i, this));
                }
                //init offset 1088;
                for (byte i = 0; i <= (int)OpenVIII.Characters.Edea_Kramer; i++)
                {
                    CharacterData tmp = CharacterData.Load(br, (Characters)i, this);

                    tmp.Name = Memory.Strings.GetName((Characters)i, this);
                    _characters.Add((Characters)i, tmp);// 0x04A0 -> 0x08C8 //152 bytes per 8 total
                }
                //init offset 2304
                for (int i = 0; i < _shops.Capacity; i++)
                    _shops.Add(new Shop(br));//0x0960 //400 bytes
                //init offset 2704
                //Configuration = br.ReadBytes(20); //0x0AF0 //20 bytes TODO break this up into a structure or class.
                Configuration = new Configuration(br);
                PartyData = Array.ConvertAll(br.ReadBytes(4), item => (Characters)item).ToList(); //0x0B04 // 4 bytes 0xFF terminated.
                if (Party == null) Party = PartyData.Take(3).ToList();
                KnownWeapons = new BitArray(br.ReadBytes(4)); //0x0B08 // 4 bytes
                GrieverName = br.ReadBytes(12); //0x0B0C // 12 bytes
                if (string.IsNullOrWhiteSpace(GrieverName)) GrieverName = Memory.Strings.GetName(Faces.ID.Griever);
                Unknown1 = br.ReadUInt16();//0x0B18  (always 7966?)
                Unknown2 = br.ReadUInt16();//0x0B1A
                AmountOfGil = br.ReadUInt32();//0x0B1C //dupilicate
                if (AmountOfGilPreview != AmountOfGil) AmountOfGilPreview = AmountOfGil;
                AmountOfGilLaguna = br.ReadUInt32();//0x0B20
                LimitBreakQuistisUnlockedBlueMagic = new BitArray(br.ReadBytes(2));//0x0B24
                LimitBreakZellUnlockedDuel = new BitArray(br.ReadBytes(2));//0x0B26
                LimitBreakIrvineUnlockedShot = new BitArray(br.ReadBytes(1));//0x0B28
                LimitBreakSelphieUsedRareSpells = new BitArray(br.ReadBytes(1));//0x0B29
                LimitBreakAngeloCompleted = (Angelo)br.ReadByte();//0x0B2A
                LimitBreakAngeloKnown = (Angelo)br.ReadByte();//0x0B2B
                LimitBreakAngeloPoints = br.ReadBytes(8).Select((value, key) => new { key, value }).ToDictionary(x => (Angelo)(x.key + 1), x => x.value);//0x0B2C
                ItemsBattleOrder = br.ReadBytes(32);//0x0B34
                //Init offset 2804
                for (int i = 0; br.BaseStream.Position + 2 <= br.BaseStream.Length && i < Items.Capacity; i++)
                    Items.Add(new Item(br.ReadByte(), br.ReadByte())); //0x0B54 198 items (Item ID and Quantity)
            }

            /// <summary>
            /// List of all Unlocked GFs
            /// </summary>
            public IEnumerable<GFs> UnlockedGFs => GFs.Where(x => x.Value.Exists).Select(x => x.Key);

            private CharacterData GetDamageable(Characters id)
            {
                CharacterData c = null;
                if (Characters != null && !Characters.TryGetValue(id, out c) && Characters.Count > 0 && Party != null)
                {
                    int ind = Party.FindIndex(x => x.Equals(id));

                    if (ind == -1 || !Characters.TryGetValue(PartyData[ind], out c))
                        throw new ArgumentException($"{this}::Cannot find {id} in CharacterData or Party");
                    return c;
                }
                return c;
            }

            private GFData GetDamageable(GFs id) => GFs.ContainsKey(id) ? GFs[id] : null;

            private Damageable GetDamageable(Faces.ID id)
            {
                GFs gf = id.ToGFs();
                Characters c = id.ToCharacters();
                if (c == OpenVIII.Characters.Blank)
                    return GetDamageable(gf);
                return GetDamageable(c);
            }

            private void GetNames()
            {
                if (string.IsNullOrWhiteSpace(SquallName)) SquallName = Memory.Strings.GetName(Faces.ID.Squall_Leonhart);
                if (string.IsNullOrWhiteSpace(RinoaName)) RinoaName = Memory.Strings.GetName(Faces.ID.Rinoa_Heartilly);
                if (string.IsNullOrWhiteSpace(AngeloName)) AngeloName = Memory.Strings.GetName(Faces.ID.Angelo);
                if (string.IsNullOrWhiteSpace(BokoName)) BokoName = Memory.Strings.GetName(Faces.ID.Boko);
            }

            #endregion Methods
        }

        #endregion Classes
    }
}