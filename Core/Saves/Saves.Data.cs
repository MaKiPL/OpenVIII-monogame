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
        public class Data
        {
            public ushort LocationID;//0x0004
            public ushort firstcharacterscurrentHP;//0x0006
            public ushort firstcharactersmaxHP;//0x0008
            public ushort savecount;//0x000A
            public uint AmountofGil;//0x000C

            private TimeSpan _timeplayed;//0x0020

            public byte firstcharacterslevel;//0x0024

            /// <summary>
            /// 0xFF = blank; The value should cast to Faces.ID
            /// </summary>
            public List<Characters> Party;//0x0025//0x0026//0x0027

            /// <summary>
            /// 12 characters 0x00 terminated
            /// </summary>
            public FF8String Squallsname;//0x0028 //12 characters 0x00 terminated

            public FF8String Rinoasname;//0x0034 //12 characters 0x00 terminated
            public FF8String Angelosname;//0x0040 //12 characters 0x00 terminated
            public FF8String Bokosname;//0x004C //12 characters 0x00 terminated

            // 0 = Disc 1
            public uint CurrentDisk;//0x0058

            public uint Currentsave;//0x005C

            public Dictionary<GFs, GFData> GFs; // 0x0060 -> 0x045C //68 bytes per 16 total
            public Dictionary<Characters, CharacterData> Characters; // 0x04A0 -> 0x08C8 //152 bytes per 8 total
            public List<Shop> Shops; //0x0960 //400 bytes
            public byte[] Configuration; //0x0AF0 //20 bytes
            public List<Characters> PartyData; //0x0B04 // 4 bytes 0xFF terminated.
            public byte[] KnownWeapons; //0x0B08 // 4 bytes
            public FF8String Grieversname; //0x0B0C // 12 bytes

            public ushort Unknown1; //0x0B18  (always 7966?)

            public Queue<GFs> EarnAP(uint ap)
            {
                Queue<GFs> ret = new Queue<GFs>();
                foreach (var g in GFs.Where(i => i.Value.Exists))
                {
                    if (g.Value.EarnExp(ap))
                        ret.Enqueue(g.Key);
                }
                return ret;
            }

            public ushort Unknown2; //0x0B1A
            public uint AmountofGil2; //0x0B1C
            public uint AmountofGil_Laguna; //0x0B20
            public BitArray LimitBreakQuistis; //0x0B24
            public BitArray LimitBreakZell; //0x0B26
            public BitArray LimitBreakIrvine; //0x0B28
            public BitArray LimitBreakSelphie; //0x0B29
            public BitArray LimitBreakAngelocompleted; //0x0B2A
            public BitArray LimitBreakAngeloknown; //0x0B2B
            public byte[] LimitBreakAngelopoints; //0x0B2C
            public byte[] Itemsbattleorder; //0x0B34
            public List<Item> Items; //0x0B54 198 items (Item ID and Quantity)
            private TimeSpan _gametime; //0x0CE0
            public uint Countdown; //0x0CE4
            public uint Unknown3; //0x0CE8
            public uint Battlevictorycount; //0x0CEC
            public ushort Unknown4; //0x0CF0
            public ushort Battlebattleescaped; //0x0CF2
            public uint Unknown5; //0x0CF4
            public uint BattleTonberrykilledcount; //0x0CF8
            public bool BattleTonberrySrkilled; //0x0CFC (yeah, this is a boolean)
            public uint Unknown6; //0x0D00
            public uint BattleR1; //0x0D04 First "Bug" battle (R1 tip)
            public uint BattleELEMENTAL; //0x0D08 First "Bomb" battle (Elemental tip)
            public uint BattleMENTAL; //0x0D0C  First "T-Rex" battle (Mental tip)
            public uint BattleIRVINE; //0x0D10 First "Irvine" battle (Irvine's limit break tip)
            public byte[] BattleMAGIC; //0x0D14 Magic drawn once
            public byte[] BattleSCAN; //0x0D1C Ennemy scanned once
            public byte BattleRAUTO; //0x0D30 Renzokuken auto
            public byte BattleRINDICATOR; //0x0D31 Renzokuken indicator
            public byte BattleUNK; //0x0D32 dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
            public byte[] Tutorialinfos; //0x0D33
            public byte SeeDtestlevel; //0x0D43
            public uint Unknown7; //0x0D44
            public Characters[] Party2; //0x0D48 (last byte always = 255)
            public uint Unknown8; //0x0D4C
            public ushort Module; //0x0D50 (1= field, 2= worldmap, 3= battle)
            public ushort Currentfield; //0x0D52
            public ushort Previousfield; //0x0D54
            public short[] CoordX; //0x0D56 signed  (party1, party2, party3)
            public short[] CoordY; //0x0D5C signed  (party1, party2, party3)
            public ushort[] Triangle_ID; //0x0D62  (party1, party2, party3)
            public byte[] Direction; //0x0D68  (party1, party2, party3)
            public byte Padding; //0x0D6B
            public uint Unknown9; //0x0D6C

            /// <summary>
            /// </summary>
            /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/Variables"/>
            public FieldVars Fieldvars; //0x0D70

            public Worldmap Worldmap; //0x1270
            public TripleTriad TripleTriad; //0x12F0
            public ChocoboWorld ChocoboWorld; //0x1370

            public bool TeamLaguna => Party != null && (Party[0] == OpenVIII.Characters.Laguna_Loire || Party[1] == OpenVIII.Characters.Laguna_Loire || Party[2] == OpenVIII.Characters.Laguna_Loire);

            public bool EarnItem(Item item)
            {
                Item f;
                if ((f = Items.FirstOrDefault(i=>i.ID == item.ID))!=null)
                {
                    return f.Add(item.QTY);
                }
                else if ((f = Items.FirstOrDefault(i => i.ID == 0)) != null)
                {
                    return f.Add(item.QTY,item.ID);
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
            public GFData this[GFs id] => GetDamagable(id);
            public CharacterData this[Characters id] => GetDamagable(id);
            public Damageable this[Faces.ID id] => GetDamagable(id);
            private CharacterData GetDamagable(Characters id)
            {
                return Characters.ContainsKey(id) ? Characters[id] : null;
            }
            private GFData GetDamagable(GFs id)
            {
                return GFs.ContainsKey(id) ? GFs[id] : null;
            }
            private Damageable GetDamagable(Faces.ID id)
            {
                GFs gf = id.ToGFs();
                Characters c = id.ToCharacters();
                if (c == OpenVIII.Characters.Blank)
                    return GetDamagable(gf);
                else
                    return GetDamagable(c);
            }

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
            /// unsure if this is a duplicate of Timeplayed or something.
            /// </summary>
            public TimeSpan Gametime { get => _gametime; private set => _gametime = value; }

            /// <summary>
            /// xna GameTime when loaded
            /// </summary>
            public TimeSpan Loadtime { get; set; }

            /// <summary>
            /// Stored playtime in seconds. Made into timespan for easy parsing.
            /// </summary>
            public TimeSpan Timeplayed { get => _timeplayed; set => _timeplayed = value; }

            /// <summary>
            /// Time since loaded
            /// </summary>
            public TimeSpan ElapsedTimeSinceLoad => Memory.gameTime != null && Loadtime != null ? (Memory.gameTime.TotalGameTime - Loadtime) : new TimeSpan();

            public void Read(BinaryReader br)
            {
                Timeplayed = new TimeSpan();
                GFs = new Dictionary<GFs, GFData>(16);
                Characters = new Dictionary<Characters, CharacterData>(8);
                LocationID = br.ReadUInt16();//0x0004
                firstcharacterscurrentHP = br.ReadUInt16();//0x0006
                firstcharactersmaxHP = br.ReadUInt16();//0x0008
                savecount = br.ReadUInt16();//0x000A
                AmountofGil = br.ReadUInt32();//0x000C
                Timeplayed = new TimeSpan(0, 0, (int)br.ReadUInt32());//0x0020
                firstcharacterslevel = br.ReadByte();//0x0024
                Party = Array.ConvertAll(br.ReadBytes(3), Item => (Characters)Item).ToList();//0x0025//0x0026//0x0027 0xFF = blank.
                Squallsname = br.ReadBytes(12);//0x0028
                Rinoasname = br.ReadBytes(12);//0x0034
                Angelosname = br.ReadBytes(12);//0x0040
                Bokosname = br.ReadBytes(12);//0x004C
                CurrentDisk = br.ReadUInt32();//0x0058
                Currentsave = br.ReadUInt32();//0x005C
                for (int i = 0; i <= (int)OpenVIII.GFs.Eden; i++)
                {
                    GFs[(GFs)i] = new GFData(br, (GFs)i);
                }
                for (int i = 0; i <= (int)OpenVIII.Characters.Edea_Kramer; i++)
                {
                    Characters[(Characters)i] = new CharacterData(br, (Characters)i)
                    {
                        Name = Memory.Strings.GetName((Characters)i, this)
                    }; // 0x04A0 -> 0x08C8 //152 bytes per 8 total
                }
                int ShopCount = 400 / (16 + 1 + 3);
                Shops = new List<Shop>(ShopCount); //0x0960 //400 bytes
                for (int i = 0; i < ShopCount; i++)
                    Shops.Add(new Shop(br));
                Configuration = br.ReadBytes(20); //0x0AF0 //20 bytes

                PartyData = Array.ConvertAll(br.ReadBytes(4), Item => (Characters)Item).ToList(); //0x0B04 // 4 bytes 0xFF terminated.
                KnownWeapons = br.ReadBytes(4); //0x0B08 // 4 bytes
                Grieversname = br.ReadBytes(12); //0x0B0C // 12 bytes

                Unknown1 = br.ReadUInt16();//0x0B18  (always 7966?)
                Unknown2 = br.ReadUInt16();//0x0B1A
                AmountofGil2 = br.ReadUInt32();//0x0B1C //dupilicate
                AmountofGil_Laguna = br.ReadUInt32();//0x0B20
                LimitBreakQuistis = new BitArray(br.ReadBytes(2));//0x0B24
                LimitBreakZell = new BitArray(br.ReadBytes(2));//0x0B26
                LimitBreakIrvine = new BitArray(br.ReadBytes(1));//0x0B28
                LimitBreakSelphie = new BitArray(br.ReadBytes(1));//0x0B29
                LimitBreakAngelocompleted = new BitArray(br.ReadBytes(1));//0x0B2A
                LimitBreakAngeloknown = new BitArray(br.ReadBytes(1));//0x0B2B
                LimitBreakAngelopoints = br.ReadBytes(8);//0x0B2C
                Itemsbattleorder = br.ReadBytes(32);//0x0B34
                Items = new List<Item>(198);
                for (int i = 0; i < 198; i++)
                    Items.Add(new Item (br.ReadByte(), br.ReadByte()) ); //0x0B54 198 items (Item ID and Quantity)
                Gametime = new TimeSpan(0, 0, (int)br.ReadUInt32());//0x0CE0
                Countdown = br.ReadUInt32();//0x0CE4
                Unknown3 = br.ReadUInt32();//0x0CE8
                Battlevictorycount = br.ReadUInt32();//0x0CEC
                Unknown4 = br.ReadUInt16();//0x0CF0
                Battlebattleescaped = br.ReadUInt16();//0x0CF2
                Unknown5 = br.ReadUInt32();//0x0CF4
                BattleTonberrykilledcount = br.ReadUInt32();//0x0CF8
                BattleTonberrySrkilled = br.ReadUInt32() > 0;//0x0CFC (yeah, this is a boolean)
                Unknown6 = br.ReadUInt32();//0x0D00
                BattleR1 = br.ReadUInt32();//0x0D04 First "Bug" battle (R1 tip)
                BattleELEMENTAL = br.ReadUInt32();//0x0D08 First "Bomb" battle (Elemental tip)
                BattleMENTAL = br.ReadUInt32();//0x0D0C  First "T-Rex" battle (Mental tip)
                BattleIRVINE = br.ReadUInt32();//0x0D10 First "Irvine" battle (Irvine's limit break tip)
                BattleMAGIC = br.ReadBytes(8);//0x0D14 Magic drawn once
                BattleSCAN = br.ReadBytes(20);//0x0D1C Ennemy scanned once
                BattleRAUTO = br.ReadByte();//0x0D30 Renzokuken auto
                BattleRINDICATOR = br.ReadByte();//0x0D31 Renzokuken indicator
                BattleUNK = br.ReadByte();//0x0D32 dream/Odin/Phoenix/Gilgamesh/Angelo disabled/Angel Wing enabled/???/???
                Tutorialinfos = br.ReadBytes(16);//0x0D33
                SeeDtestlevel = br.ReadByte();//0x0D43
                Unknown7 = br.ReadUInt32();//0x0D44
                Party2 = Array.ConvertAll(br.ReadBytes(4), Item => (Characters)Item); //0x0D48 (last byte always = 255) //dupicate?
                Unknown8 = br.ReadUInt32();//0x0D4C
                Module = br.ReadUInt16();//0x0D50 (1= field, 2= worldmap, 3= battle)
                Currentfield = br.ReadUInt16();//0x0D52
                Previousfield = br.ReadUInt16();//0x0D54
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

            public bool MaxGFAbilities(GFs gf) => GFs.ContainsKey(gf) ? GFs[gf].MaxGFAbilities : false;

            /// <summary>
            /// How many dead party members there are.
            /// </summary>
            /// <returns>>=0</returns>
            public int DeadPartyMembers() => PartyData.Where(m => m != OpenVIII.Characters.Blank && Characters[m].IsDead).Count();

            /// <summary>
            /// How many dead characters there are.
            /// </summary>
            /// <returns>>=0</returns>
            public int DeadCharacters() => Characters.Where(m=>m.Value.Available && m.Value.CurrentHP() == 0 || (m.Value.Statuses0 & Kernel_bin.Persistant_Statuses.Death) != 0).Count();

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
                    entry => entry.Value.Clone());
                d.GFs = GFs.ToDictionary(entry => entry.Key,
                    entry => entry.Value.Clone());
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
        }
    }
}