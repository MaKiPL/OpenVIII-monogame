using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public static partial class Saves
    {
        #region Classes

        /// <summary>
        /// Save Data
        /// </summary>
        /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat"/>
        public class Data
        {
            #region Fields

            private TimeSpan _gametime;
            private TimeSpan _timeplayed;

            #endregion Fields

            #region Methods

            private CharacterData GetDamageable(Characters id)
            {
                if (Characters != null && !Characters.TryGetValue(id, out CharacterData c))
                {
                    var ind = Party.FindIndex(x=>x.Equals(id));

                    if (ind == -1 || !Characters.TryGetValue(PartyData[ind], out c))
                        throw new ArgumentException($"{this}::Cannot find {id} in CharacterData or Party");
                    return c;
                }
                return null;
            }

            private GFData GetDamageable(GFs id) => GFs.ContainsKey(id) ? GFs[id] : null;

            private Damageable GetDamageable(Faces.ID id)
            {
                GFs gf = id.ToGFs();
                Characters c = id.ToCharacters();
                if (c == OpenVIII.Characters.Blank)
                    return GetDamageable(gf);
                else
                    return GetDamageable(c);
            }

            #endregion Methods

            #region Properties

            /// <summary>
            /// 0x000C 4 bytes Preview: Amount of Gil
            /// </summary>
            public uint AmountofGil { get; set; }

            /// <summary>
            /// 0x0B20 4 bytes Amount of Gil (Laguna)
            /// </summary>
            public uint AmountofGil_Laguna { get; set; }

            /// <summary>
            /// 0x0B1C 4 bytes Amount of Gil
            /// </summary>
            public uint AmountofGil2 { get; set; }

            /// <summary>
            /// 0x0040 12 bytes Preview: Angelo's name (0x00 terminated)
            /// </summary>
            public FF8String Angelosname { get; set; }

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
            public uint BattleELEMENTAL { get; set; }

            /// <summary>
            /// 0x0CF2 2 bytes Battle: battle escaped
            /// </summary>
            public ushort BattleEscapeCount { get; set; }

            /// <summary>
            /// 0x0D10 4 bytes Battle: First "Irvine" battle (Irvine's limit break tip)
            /// </summary>
            public uint BattleIRVINE { get; set; }

            /// <summary>
            /// 0x0D14 8 bytes Battle: Magic drawn once
            /// </summary>
            public BitArray BattleMAGIC { get; set; }

            /// <summary>
            /// 0x0D0C 4 bytes Battle: First "T-Rex" battle (Mental tip)
            /// </summary>
            public uint BattleMENTAL { get; set; }

            /// <summary>
            /// 0x0D04 4 bytes Battle: First "Bug" battle (R1 tip)
            /// </summary>
            public uint BattleR1 { get; set; }

            /// <summary>
            /// 0x0D30 1 byte Battle: Renzokuken auto
            /// </summary>
            public bool BattleRAUTO { get; set; }

            /// <summary>
            /// 0x0D31 1 byte Battle: Renzokuken indicator
            /// </summary>
            public bool BattleRINDICATOR { get; set; }

            /// <summary>
            /// 0x0D1C 20 bytes Battle: Ennemy scanned once
            /// </summary>
            public BitArray BattleSCAN { get; set; }

            /// <summary>
            /// 0x0CF4 4 bytes Unknown
            /// </summary>
            public uint BattleTonberryKilledCount { get; set; }

            /// <summary>
            /// 0x0CFC 4 bytes Battle: Tonberry Sr killed (yeah, this is a boolean)
            /// </summary>
            public bool BattleTonberrySrKilled { get; set; }

            /// <summary>
            /// Battle: dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
            /// </summary>
            public BitArray BattleUNK { get; set; }

            /// <summary>
            /// 0x0CEC 4 bytes Battle: victory count
            /// </summary>
            public uint BattleVictoryCount { get; set; }

            /// <summary>
            /// 0x004C 12 bytes Preview: Boko's name (0x00 terminated)
            /// </summary>
            public FF8String Bokosname { get; set; }

            /// <summary>
            /// 0x04A0-0x08C7 152 bytes each 8 of them. Characters: Squall-Edea
            /// </summary>
            public Dictionary<Characters, CharacterData> Characters { get; set; }

            /// <summary>
            /// 0x1370 64 bytes Chocobo World (TODO)
            /// </summary>
            public ChocoboWorld ChocoboWorld { get; set; }

            /// <summary>
            /// 0x0AF0 20 bytes Configuration
            /// </summary>
            public byte[] Configuration { get; set; }

            /// <summary>
            /// 0x0D56 3*2 bytes (signed) Coord X (party1, party2, party3)
            /// </summary>
            public short[] CoordX { get; set; }

            /// <summary>
            /// 0x0D5C 3*2 bytes (signed) Coord Y (party1, party2, party3)
            /// </summary>
            public short[] CoordY { get; set; }

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
            public uint Currentsave { get; set; }

            /// <summary>
            /// 0x0D68 3*1 bytes Direction (party1, party2, party3)
            /// </summary>
            public byte[] Direction { get; set; }

            /// <summary>
            /// Time since loaded
            /// </summary>
            public TimeSpan ElapsedTimeSinceLoad => Memory.gameTime != null && Loadtime != null ? (Memory.gameTime.TotalGameTime - Loadtime) : new TimeSpan();

            /// <summary>
            /// 0x0D70 256 + 1024 bytes Field vars
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/Variables"/>
            public FieldVars Fieldvars { get; set; }

            /// <summary>
            /// 0x0006 2 bytes Preview: 1st character's current HP
            /// </summary>
            public ushort FirstCharactersCurrentHP { get; set; }

            /// <summary>
            /// 0x0024 1 byte Preview: 1st character's level
            /// </summary>
            public byte FirstCharactersLevel { get; set; }

            /// <summary>
            /// 0x0008 2 bytes Preview: 1st character's max HP
            /// </summary>
            public ushort FirstCharactersMaxHP { get; set; }

            /// <summary>
            /// <para>0x0CE0 4 bytes Game time</para>
            /// <para>unsure if this is a duplicate of Timeplayed or something.</para>
            /// </summary>
            public TimeSpan Gametime { get => _gametime; private set => _gametime = value; }

            /// <summary>
            /// 0x0060 - 0x049F 68 bytes each 16 of them. Guardian Forces: Quetzalcoatl-Eden
            /// </summary>
            public Dictionary<GFs, GFData> GFs { get; set; }

            /// <summary>
            /// 0x0B0C 12 bytes Griever name (FF8 text format)
            /// </summary>
            public FF8String Grieversname { get; set; }

            /// <summary>
            /// <para>0x0B54 396 bytes Items 198 items (Item ID and Quantity)</para>
            /// <para>
            /// The order of items out of battle and Each item uses 2 bytes 1 for ID and 1 for Quantity
            /// </para>
            /// </summary>
            public List<Item> Items { get; set; }

            /// <summary>
            /// <para>0x0B34 32 bytes Items battle order</para>
            /// <para>Only the items that can be used in battle</para>
            /// <para>The order they appear in the battle item menu.</para>
            /// </summary>
            public byte[] Itemsbattleorder { get; set; }

            /// <summary>
            /// 0x0B08 4 bytes Known weapons
            /// </summary>
            public BitArray KnownWeapons { get; set; }

            /// <summary>
            /// <para>0x0B2A 1 byte Limit Break Angelo completed</para>
            /// <para>Each bit is a completely learned ability</para>
            /// </summary>
            public BitArray LimitBreakAngelocompleted { get; set; }

            /// <summary>
            /// <para>0x0B2B 1 byte Limit Break Angelo known</para>
            /// <para>Each bit is an ability is known about/able to be learned</para>
            /// </summary>
            public BitArray LimitBreakAngeloknown { get; set; }

            /// <summary>
            /// <para>0x0B2C 8 bytes Limit Break Angelo points</para>
            /// <para>Each byte is progress to learing an ability</para>
            /// </summary>
            public byte[] LimitBreakAngelopoints { get; set; }

            public BitArray LimitBreakIrvine_Unlocked_Shot { get; set; }
            /// <summary>
            /// <para>0x0B24 2 bytes Limit Break Quistis</para>
            /// <para>Each bit is an unlocked blue magic spell</para>
            /// </summary>
            public BitArray LimitBreakQuistis_Unlocked_BlueMagic { get; set; }

            /// <summary>
            /// <para>0x0B29 1 byte Limit Break Selphie</para>
            /// <para>I think this sets bits when a rare spell is used.</para>
            /// </summary>
            public BitArray LimitBreakSelphie_Used_RareSpells { get; set; }

            /// <summary>
            /// <para>0x0B26 2 bytes Limit Break Zell</para>
            /// <para>Each bit is a combo/attack unlocked. So it shows on screen.</para>
            /// <para>You can use the combo if you know it.</para>
            /// </summary>
            public BitArray LimitBreakZell_Unlocked_Duel { get; set; }

            /// <summary>
            /// xna GameTime when loaded
            /// </summary>
            public TimeSpan Loadtime { get; set; }

            /// <summary>
            /// 0x0004 2 bytes Preview: Location ID
            /// </summary>
            public ushort LocationID { get; set; }

            /// <summary>
            /// 0x0D50 2 bytes Module (1= field, 2= worldmap, 3= battle)
            /// </summary>
            public ushort Module { get; set; }

            public List<CharacterData> NonPartyMembers
            {
                get
                {
                    List<CharacterData> c = null;

                    if (Characters != null)
                    {
                        c = new List<CharacterData>();
                        foreach (KeyValuePair<Characters, CharacterData> i in Characters)
                        {
                            if (!Party.Contains(i.Key) && i.Value.Available)
                            {
                                c.Add(i.Value);
                            }
                        }
                    }
                    return c;
                }
            }

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
            public FF8String Rinoasname { get; set; }

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
            public List<Shop> Shops { get; set; }

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
            public FF8String Squallsname { get; set; }

            public bool TeamLaguna => Party != null && (Party[0] == OpenVIII.Characters.Laguna_Loire || Party[1] == OpenVIII.Characters.Laguna_Loire || Party[2] == OpenVIII.Characters.Laguna_Loire);
            /// <summary>
            /// Stored playtime in seconds. Made into timespan for easy parsing.
            /// </summary>
            /// <remarks>0x0020 4 bytes Preview: Total number of seconds played</remarks>
            public TimeSpan Timeplayed { get => _timeplayed; set => _timeplayed = value; }

            /// <summary>
            /// 0x0D62 3*2 bytes Triangle ID (party1, party2, party3)
            /// </summary>
            public ushort[] Triangle_ID { get; set; }

            /// <summary>
            /// 0x12F0 128 bytes Triple Triad (TODO)
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
            /// 0x1270 128 bytes Worldmap
            /// </summary>
            public Worldmap Worldmap { get; set; }

            #endregion Properties

            #region Indexers

            public GFData this[GFs id] => GetDamageable(id);

            public CharacterData this[Characters id] => GetDamageable(id);

            public Damageable this[Faces.ID id] => GetDamageable(id);

            //public Damageable this[Damageable damageable]

            #endregion Indexers

            /// <summary>
            /// preforms a Shadow Copy. Then does deep copy on any required objects.
            /// </summary>
            /// <returns></returns>
            public Data Clone()
            {
                //shadowcopy
                Data d = (Data)MemberwiseClone();
                //deepcopy anything that needs it here.

                d.Characters = Characters.ToDictionary(entry => entry.Key,
                    entry => (CharacterData)entry.Value.Clone());
                d.GFs = GFs.ToDictionary(entry => entry.Key,
                    entry => (GFData)entry.Value.Clone());
                d.ChocoboWorld = ChocoboWorld.Clone();
                d.Fieldvars = Fieldvars.Clone();
                d.Worldmap = Worldmap.Clone();
                d.TripleTriad = TripleTriad.Clone();
                d.Shops = new List<Shop>(Shops.Count);
                foreach (Shop s in Shops)
                    d.Shops.Add(s.Clone());

                d.Items = new List<Item>(Items.Count);
                foreach (Item i in Items)
                    d.Items.Add(i.Clone());
                return d;
            }

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
            public int DeadCharacters() => Characters.Where(m => m.Value.Available && m.Value.CurrentHP() == 0 || (m.Value.Statuses0 & Kernel_bin.Persistant_Statuses.Death) != 0).Count();

            /// <summary>
            /// How many dead party members there are.
            /// </summary>
            /// <returns>&gt;=0</returns>
            public int DeadPartyMembers() => PartyData.Where(m => m != OpenVIII.Characters.Blank && Characters[m].IsDead).Count();

            public Queue<GFs> EarnAP(uint ap, out Queue<KeyValuePair<GFs, Kernel_bin.Abilities>> abilities)
            {
                Queue<GFs> ret = new Queue<GFs>();
                abilities = new Queue<KeyValuePair<GFs, Kernel_bin.Abilities>>();
                foreach (KeyValuePair<GFs, GFData> g in GFs.Where(i => i.Value.Exists))
                {
                    if (g.Value.EarnExp(ap, out Kernel_bin.Abilities ability))
                    {
                        if (ability != Kernel_bin.Abilities.None)
                        {
                            abilities.Enqueue(new KeyValuePair<GFs, Kernel_bin.Abilities>(g.Key, ability));
                        }
                        ret.Enqueue(g.Key);
                    }
                }
                return ret;
            }
            public bool EarnItem(Cards.ID card, byte qty, byte location = 0)
            {
                TTCardInfo i = new TTCardInfo() { Unlocked = true, Qty = qty, Location = location };
                if (!TripleTriad.cards.TryAdd(card, i))
                {
                    TripleTriad.cards[card].Unlocked = i.Unlocked;
                    TripleTriad.cards[card].Qty += i.Qty;
                    TripleTriad.cards[card].Location = i.Location;
                    return true;
                }
                return false;
            }
            public bool EarnItem(KeyValuePair<Cards.ID, byte> keyValuePair, byte location = 0)
            {
                return EarnItem(keyValuePair.Key, keyValuePair.Value, location);                
            }

            public bool EarnItem(Item item)
            {
                Item f;
                IEnumerable<Item> tmp = Items.Where(i => i.ID == item.ID);
                IEnumerable<Item> tmp2 = Items.Where(i => i.ID == 0);
                if (tmp.Count() > 0)
                {
                    f = tmp.First();
                    return f.Add(item.QTY);
                }
                else if (tmp2.Count() > 0)
                {
                    f = tmp2.First();
                    return f.Add(item.QTY, item.ID);
                }

                return false;
            }

            public Dictionary<GFs, Characters> JunctionedGFs()
            {
                Dictionary<GFs, Characters> r = new Dictionary<GFs, Characters>(16);
                if (Characters != null)
                {
                    foreach (KeyValuePair<Characters, CharacterData> c in Characters)
                    {
                        if (c.Value.JunctionnedGFs != GFflags.None)
                        {
                            IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(GFflags)).Cast<Enum>();
                            foreach (Enum flag in availableFlags.Where(c.Value.JunctionnedGFs.HasFlag))
                            {
                                if ((GFflags)flag == GFflags.None) continue;
                                r.Add(ConvertGFEnum[(GFflags)flag], c.Key);
                            }
                        }
                    }
                }
                return r;
            }

            public bool MaxGFAbilities(GFs gf) => GFs.ContainsKey(gf) ? GFs[gf].MaxGFAbilities : false;

            public bool PartyHasAbility(Kernel_bin.Abilities a)
            {
                foreach (Characters c in PartyData)
                {
                    if (Characters.TryGetValue(c, out CharacterData cd) && cd.Abilities.Contains(a))
                        return true;
                }
                return false;
            }

            public void Read(BinaryReader br)
            {
                //Define Containers
                Timeplayed = new TimeSpan();
                GFs = new Dictionary<GFs, GFData>(16);
                Characters = new Dictionary<Characters, CharacterData>(8);

                //Read Data
                LocationID = br.ReadUInt16();//0x0004
                FirstCharactersCurrentHP = br.ReadUInt16();//0x0006
                FirstCharactersMaxHP = br.ReadUInt16();//0x0008
                SaveCount = br.ReadUInt16();//0x000A
                AmountofGil = br.ReadUInt32();//0x000C
                Timeplayed = new TimeSpan(0, 0, checked((int)br.ReadUInt32()));//0x0020
                FirstCharactersLevel = br.ReadByte();//0x0024
                Party = Array.ConvertAll(br.ReadBytes(3), Item => (Characters)Item).ToList();//0x0025//0x0026//0x0027 0xFF = blank.
                Squallsname = br.ReadBytes(12);//0x0028
                Rinoasname = br.ReadBytes(12);//0x0034
                Angelosname = br.ReadBytes(12);//0x0040
                Bokosname = br.ReadBytes(12);//0x004C
                CurrentDisk = br.ReadUInt32();//0x0058
                Currentsave = br.ReadUInt32();//0x005C
                for (byte i = 0; i <= (int)OpenVIII.GFs.Eden; i++)
                {
                    GFs[(GFs)i] = new GFData(br, (GFs)i);
                }
                for (byte i = 0; i <= (int)OpenVIII.Characters.Edea_Kramer; i++)
                {

                    Characters[(Characters)i] = new CharacterData(br, (Characters)i)
                    {
                        Name = Memory.Strings.GetName((Characters)i, this)
                    }; // 0x04A0 -> 0x08C8 //152 bytes per 8 total
                }
                Shops = new List<Shop>(20); //0x0960 //400 bytes
                for (int i = 0; i < Shops.Capacity; i++)
                    Shops.Add(new Shop(br));
                Configuration = br.ReadBytes(20); //0x0AF0 //20 bytes TODO break this up into a structure or class.
                PartyData = Array.ConvertAll(br.ReadBytes(4), Item => (Characters)Item).ToList(); //0x0B04 // 4 bytes 0xFF terminated.
                KnownWeapons = new BitArray(br.ReadBytes(4)); //0x0B08 // 4 bytes
                Grieversname = br.ReadBytes(12); //0x0B0C // 12 bytes

                Unknown1 = br.ReadUInt16();//0x0B18  (always 7966?)
                Unknown2 = br.ReadUInt16();//0x0B1A
                AmountofGil2 = br.ReadUInt32();//0x0B1C //dupilicate
                AmountofGil_Laguna = br.ReadUInt32();//0x0B20
                LimitBreakQuistis_Unlocked_BlueMagic = new BitArray(br.ReadBytes(2));//0x0B24
                LimitBreakZell_Unlocked_Duel = new BitArray(br.ReadBytes(2));//0x0B26
                LimitBreakIrvine_Unlocked_Shot = new BitArray(br.ReadBytes(1));//0x0B28
                LimitBreakSelphie_Used_RareSpells = new BitArray(br.ReadBytes(1));//0x0B29
                LimitBreakAngelocompleted = new BitArray(br.ReadBytes(1));//0x0B2A
                LimitBreakAngeloknown = new BitArray(br.ReadBytes(1));//0x0B2B
                LimitBreakAngelopoints = br.ReadBytes(8);//0x0B2C
                Itemsbattleorder = br.ReadBytes(32);//0x0B34
                Items = new List<Item>(198);
                for (int i = 0; i < 198; i++)
                    Items.Add(new Item(br.ReadByte(), br.ReadByte())); //0x0B54 198 items (Item ID and Quantity)
                Gametime = new TimeSpan(0, 0, (int)br.ReadUInt32());//0x0CE0
                Countdown = br.ReadUInt32();//0x0CE4
                Unknown3 = br.ReadUInt32();//0x0CE8
                BattleVictoryCount = br.ReadUInt32();//0x0CEC
                Unknown4 = br.ReadUInt16();//0x0CF0
                BattleEscapeCount = br.ReadUInt16();//0x0CF2
                Unknown5 = br.ReadUInt32();//0x0CF4
                BattleTonberryKilledCount = br.ReadUInt32();//0x0CF8
                BattleTonberrySrKilled = br.ReadUInt32() > 0;//0x0CFC (yeah, this is a boolean)
                Unknown6 = br.ReadUInt32();//0x0D00
                BattleR1 = br.ReadUInt32();//0x0D04 First "Bug" battle (R1 tip)
                BattleELEMENTAL = br.ReadUInt32();//0x0D08 First "Bomb" battle (Elemental tip)
                BattleMENTAL = br.ReadUInt32();//0x0D0C  First "T-Rex" battle (Mental tip)
                BattleIRVINE = br.ReadUInt32();//0x0D10 First "Irvine" battle (Irvine's limit break tip)
                BattleMAGIC = new BitArray(br.ReadBytes(8));//0x0D14 Magic drawn once
                BattleSCAN = new BitArray(br.ReadBytes(20));//0x0D1C Ennemy scanned once
                BattleRAUTO = br.ReadByte() > 0;//0x0D30 Renzokuken auto
                BattleRINDICATOR = br.ReadByte() > 0;//0x0D31 Renzokuken indicator
                BattleUNK = new BitArray(br.ReadBytes(1));//0x0D32 dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
                TutorialInfos = new BitArray(br.ReadBytes(16));//0x0D33
                SeeDTestLevel = br.ReadByte();//0x0D43
                Unknown7 = br.ReadUInt32();//0x0D44
                Party2 = Array.ConvertAll(br.ReadBytes(4), Item => (Characters)Item); //0x0D48 (last byte always = 255) //dupicate?
                Unknown8 = br.ReadUInt32();//0x0D4C
                Module = br.ReadUInt16();//0x0D50 (1= field, 2= worldmap, 3= battle)
                CurrentField = br.ReadUInt16();//0x0D52
                PreviousField = br.ReadUInt16();//0x0D54
                CoordX = new short[3];
                for (int i = 0; i < 3; i++)
                    CoordX[i] = br.ReadInt16();//0x0D56 signed  (party1, party2, party3)
                CoordY = new short[3];
                for (int i = 0; i < 3; i++)
                    CoordY[i] = br.ReadInt16();//0x0D5C signed  (party1, party2, party3)
                Triangle_ID = new ushort[3];
                for (int i = 0; i < 3; i++)
                    Triangle_ID[i] = br.ReadUInt16();//0x0D62  (party1, party2, party3)
                Direction = br.ReadBytes(3 * 1);//0x0D68  (party1, party2, party3)
                Padding = br.ReadByte();//0x0D6B
                Unknown9 = br.ReadUInt32();//0x0D6C
                Fieldvars = new FieldVars(br); //0x0D70 http://wiki.ffrtt.ru/index.php/FF8/Variables
                Worldmap = new Worldmap(br);//br.ReadBytes(128);//0x1270
                TripleTriad = new TripleTriad(br); //br.ReadBytes(128);//0x12F0
                ChocoboWorld = new ChocoboWorld(br); //br.ReadBytes(64);//0x1370
            }

            /// <summary>
            /// List of all Unlocked GFs
            /// </summary>
            public List<GFs> UnlockedGFs()
            {
                List<GFs> r = new List<GFs>(16);
                if (GFs != null)
                {
                    foreach (KeyValuePair<GFs, GFData> g in GFs)
                    {
                        if (g.Value.Exists) //needs testing could be wrong.
                        {
                            r.Add(g.Key);
                        }
                    }
                }
                return r;
            }
        }

        #endregion Classes
    }
}