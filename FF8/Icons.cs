using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal partial class Icons : I_SP1
    {
        #region Fields

        private static Dictionary<ID, EntryGroup> entries = null;
        private static Texture2D[] icons;

        #endregion Fields

        #region Constructors

        public Icons()
        {
            if (entries == null)
            {
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                TEX tex;
                byte[] test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("icon.tex")));

                tex = new TEX(test);
                icons = new Texture2D[tex.TextureData.NumOfPalettes];
                for (int i = 0; i < PalletCount; i++)
                {
                    icons[i] = tex.GetTexture(i);
                    //using (FileStream fs = File.OpenWrite($"d:\\icons.{i}.png"))
                    //{
                    //    //fs.Write(test, 0, test.Length);

                    //    icons[i].SaveAsPng(fs, 256, 256);
                    //}
                }
                test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("cardanm.sp2")));
                using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "cardanm.sp2")))
                {
                    fs.Write(test, 0, test.Length);
                }
                test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("icon.sp1")));
                //read from icon.sp1
                using (MemoryStream ms = new MemoryStream(test))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        Loc[] locs = new Loc[br.ReadUInt32()];
                        for (int i = 0; i < locs.Length; i++)
                        {
                            locs[i].pos = br.ReadUInt16();
                            locs[i].count = br.ReadUInt16();
                            //if (locs[i].count > 1) Count += (uint)(locs[i].count - 1);
                        }
                        entries = new Dictionary<ID, EntryGroup>(locs.Length+ 10);
                        for (int i = 0; i < locs.Length; i++)
                        {
                            ms.Seek(locs[i].pos, SeekOrigin.Begin);
                            byte c = (byte)locs[i].count;
                            entries[(ID)i] = new EntryGroup(c);
                            for (int e = 0; e < c; e++)
                            {
                                Entry tmp = new Entry();
                                tmp.LoadfromStreamSP1(br);
                                tmp.Part = (byte)e;
                                tmp.SetLoc(locs[i]);
                                entries[(ID)i].Add(tmp);
                            }
                        }
                    }
                    //custom stuff not in sp1
                    Entry BG = new Entry
                    {
                        X = 0,
                        Y = 48,
                        Width = 256,
                        Height = 16,
                        CustomPallet = 1,
                        Tile = Vector2.UnitY,
                    };
                    Entry Border_TopLeft = new Entry
                    {
                        X = 16,
                        Y = 0,
                        Width = 8,
                        Height = 8,
                        CustomPallet = 0,
                    };
                    Entry Border_Top = new Entry
                    {
                        X = 24,
                        Y = 0,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
                        Offset = new Vector2(8,0),
                        End = new Vector2(-8,0),
                        CustomPallet = 0
                    };
                    Entry Border_Bottom = new Entry
                    {
                        X = 24,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
                        Snap_Bottom = true,
                        Offset = new Vector2(8,-8),
                        End = new Vector2(-8,0),
                        CustomPallet = 0
                    };
                    Entry Border_TopRight = new Entry
                    {
                        X = 32,
                        Y = 0,
                        Width = 8,
                        Height = 8,
                        Snap_Right = true,
                        Offset = new Vector2(-8,0),
                        CustomPallet = 0
                    };
                    Entry Border_Left = new Entry
                    {
                        X = 16,
                        Y = 8,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitY,
                        Offset = new Vector2(0,8),
                        End = new Vector2(0,-8),
                        CustomPallet = 0
                    };
                    Entry Border_Right = new Entry
                    {
                        X = 32,
                        Y = 8,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitY,
                        Snap_Right = true,
                        Offset = new Vector2(-8,8),
                        End = new Vector2(0,-8),
                        CustomPallet = 0
                    };
                    Entry Border_BottomLeft = new Entry
                    {
                        X = 16,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Snap_Bottom = true,
                        Offset = new Vector2(0,-8),
                        CustomPallet = 0
                    };
                    Entry Border_BottomRight = new Entry
                    {
                        X = 32,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Snap_Bottom = true,
                        Snap_Right = true,
                        Offset = new Vector2(-8,-8),
                        CustomPallet = 0
                    };

                    entries[ID.Bar_BG] = new EntryGroup(new Entry
                    {
                        X = 16,
                        Y = 24,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
                        CustomPallet = 0
                    });
                    entries[ID.Bar_Fill] = new EntryGroup(new Entry
                    {
                        X = 0,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
                        Offset = new Vector2(0,1),
                        CustomPallet = 5
                    });
                    entries[ID.Menu_BG_256] = new EntryGroup(BG, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
                    entries[ID.Menu_BG_368] = new EntryGroup(BG, new Entry
                    {
                        X = 0,
                        Y = 64,
                        Offset = new Vector2(256,0),
                        Width = 112,
                        Height = 16,
                        CustomPallet = 1,
                        Tile = Vector2.UnitY
                    }, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);

                    entries[ID.DEBUG] = new EntryGroup(
                        new Entry { X = 128, Y = 24, Width = 7, Height = 8 },
                        new Entry { X = 65, Y = 8, Width = 6, Height = 8, Offset = new Vector2(7,0) },
                        new Entry { X = 147, Y = 24, Width = 6, Height = 8, Offset = new Vector2(13,0) },
                        new Entry { X = 141, Y = 24, Width = 6, Height = 8, Offset = new Vector2(19, 0) },
                        new Entry { X = 104, Y = 16, Width = 6, Height = 8, Offset = new Vector2(25, 0) }
                        );
                }
            }
        }

        #endregion Constructors

        #region Enums

        public enum ID : ushort
        {
            Finger_Right,
            Finger_Left,

            item0002,
            item0003,
            Cross_Hair1,
            Cross_Hair2,
            Arrow_Right,
            item0007,
            JAPANESE01,
            JAPANESE02,
            JAPANESE03,
            G,
            P,
            Underline,
            Size_16x16_Lv_,
            Target,
            MISS,
            item0017,
            item0018,
            item0019,
            BULLET,
            Size_16x16_Lv,
            Complete_,
            Size_16x08_Lv_,
            Size_08x08_0,
            Size_08x08_1,
            Size_08x08_2,
            Size_08x08_3,
            Size_08x08_4,
            Size_08x08_5,
            Size_08x08_6,
            Size_08x08_7,
            Size_08x08_8,
            Size_08x08_9,
            Size_08x08_Forward_Slash,
            AC,
            item0036,
            Text_Cursor,
            Size_160x16_Bar_Left_Side,
            Size_160x16_Bar_Right_Side,
            Size_08x08_ALT_0,
            Size_08x08_ALT_1,
            Size_08x08_ALT_2,
            Size_08x08_ALT_3,
            Size_08x08_ALT_4,
            Size_08x08_ALT_5,
            Size_08x08_ALT_6,
            Size_08x08_ALT_7,
            Size_08x08_ALT_8,
            Size_08x08_ALT_9,
            Size_08x08_P_,
            item0051,
            item0052,
            item0053,
            item0054,
            Size_08x08_Period,
            Size_08x16_ALT_0,
            Size_08x16_ALT_1,
            Size_08x16_ALT_2,
            Size_08x16_ALT_3,
            Size_08x16_ALT_4,
            Size_08x16_ALT_5,
            Size_08x16_ALT_6,
            Size_08x16_ALT_7,
            Size_08x16_ALT_8,
            Size_08x16_ALT_9,
            item0066,
            Arrow_Left_2_Line,
            Size_160x16_Bar,
            DOUBLE_,
            TRIPLE__,
            PRICE,
            COMMAND,
            NAME,
            MAGIC,
            PAUSE,
            ITEM,
            NUM_,
            SPECIAL,
            CHOICE,
            GF,
            TARGET,
            STATUS,
            DOUBLE,
            TRIPLE,
            HELP,
            NOTICE,
            INFO,
            NAME2,
            CARDS,
            Size_08x64_Bar,
            HP,
            Arrow_Left,
            Arrow_Right2,
            ABILITY,
            CHOICE2,
            Size_08x16_0,
            Size_08x16_1,
            Size_08x16_2,
            Size_08x16_3,
            Size_08x16_4,
            Size_08x16_5,
            Size_08x16_6,
            Size_08x16_7,
            Size_08x16_8,
            Size_08x16_9,
            item0106,
            Size_08x16_Forward_Slash,
            item0108,
            Arrow_Down,
            Arrow_Up,
            TIME,
            Size_16x16_0,
            Size_16x16_1,
            Size_16x16_2,
            Size_16x16_3,
            Size_16x16_4,
            Size_16x16_5,
            Size_16x16_6,
            Size_16x16_7,
            Size_16x16_8,
            Size_16x16_9,
            Size_16x16_Colon,
            Size_16x16_A,
            Rewind,
            Rewind_Fast,
            Forward,
            Forward_Fast,
            Size_16x16_PSX_L2,
            Size_16x16_PSX_R2,
            Size_16x16_PSX_L1,
            Size_16x16_PSX_R1,
            Size_16x16_PSX_Triangle,
            Size_16x16_PSX_Circle,
            Size_16x16_PSX_Cross,
            Size_16x16_PSX_Square,
            SELECT,
            L_Stick,
            R_Stick,
            START,
            D_Pad_Up,
            D_Pad_Right,
            D_Pad_Down,
            D_Pad_Left,
            item0144,
            item0145,
            item0146,
            item0147,
            item0148,
            item0149,
            item0150,
            item0151,
            item0152,
            item0153,
            item0154,
            item0155,
            item0156,
            item0157,
            item0158,
            item0159,
            Vibration,
            ON,
            OFF,
            item0163,
            Perfect__,
            Trigger_,
            _0_Hit_,
            _1_Hit_,
            _2_Hit_,
            _3_Hit_,
            _4_Hit_,
            _5_Hit_,
            _6_Hit_,
            _7_Hit_,
            item0174,
            Star,
            S_Lv_,
            G_,
            A,
            item0179,
            item0180,
            item0181,
            item0182,
            item0183,
            Finger_Point_Down_Right_Shift_Up,
            Finger_Point_Down_Right_Shift_Down,
            Cross,
            item0187,
            item0188,
            item0189,
            item0190,
            item0191,
            GF_Junction,
            item0193,
            item0194,
            item0195,
            item0196,
            item0197,
            item0198,
            item0199,
            item0200,
            Bar_Left,
            Bar_Right,
            item0203,
            item0204,
            item0205,
            item0206,
            Arrow_Right_Line,
            Status_Attack,
            Status_Defense,
            Elemental_Attack,
            Elemental_Defense,
            Size_08_Alt_Num_0,
            Star2,
            Rank,
            Item_Card,
            Ability_Junction,
            Ability_Command,
            Ability_Character,
            Ability_Character2,
            Ability_Party,
            Ability_GF,
            Ability_Menu,
            Item_Recovery,
            Item_GF,
            Item_Tent,
            Item_Battle,
            Item_Ammo,
            Item_Magazine,
            Item_Misc,

            Stats_Hit_Points2,
            Stats_Strength2,
            Stats_Vitality2,
            Stats_Magic2,
            Stats_Spirit2,
            Stats_Speed2,
            Stats_Evade2,
            Stats_Hit_Percent2,
            Stats_Luck2,

            Icon_Elemental_Defense2,
            Size_16x08_PSX_L2,
            Size_16x08_PSX_R2,
            Size_16x08_PSX_L1,
            Size_16x08_PSX_R1,
            Size_08x08_PSX_Triangle,
            Size_08x08_PSX_Circle,
            Size_08x08_PSX_Cross,
            Size_08x08_PSX_Square,
            item0248,
            item0249,
            item0250,
            item0251,
            Attack_De_Estado,
            Defense_De_Estado,
            Attack_Elemental,
            Defense_Elemental,
            item0256,
            Size_08x08_ALT2_0,
            Size_08x08_ALT2_1,
            Size_08x08_ALT2_2,
            Size_08x08_ALT2_3,
            Size_08x08_ALT2_4,
            Size_08x08_ALT2_5,
            Size_08x08_ALT2_6,
            Size_08x08_ALT2_7,
            Size_08x08_ALT2_8,
            Size_08x08_ALT2_9,
            Percent,
            Slash_Forward,
            Colon,
            item0270,
            item0271,
            Status_Death,
            Status_Poison,
            Status_Petrify,
            Status_Darkness,
            Status_Silence,
            Status_Berserk,
            Status_Zombie,
            Status_Sleep,
            Status_Slow,
            Status_Stop,
            Status_Curse,
            Status_Confuse,
            Status_Drain,
            item0285,
            item0286,
            item0287,
            Element_Fire,
            Element_Ice,
            Element_Thunder,
            Element_Earth,
            Element_Poison,
            Element_Wind,
            Element_Water,
            Element_Holy,
            Icon_Status_Attack,
            Icon_Status_Defense,
            Icon_Elemental_Attack,
            Icon_Elemental_Defense,
            item0300,
            item0301,
            item0302,
            item0303,
            Stats_Hit_Points,
            Stats_Strength,
            Stats_Vitality,
            Stats_Magic,
            Stats_Spirit,
            Stats_Speed,
            Stats_Evade,
            Stats_Hit_Percent,
            Stats_Luck,

            item0313,
            item0314,
            item0315,
            item0316,
            item0317,
            item0318,
            item0319,
            Slow,
            Fast,
            PLAY,
            SeeD,
            HP2,
            Lv,
            TIME2,
            A2,
            DISC,
            Bar_BG,
            Bar_Fill,
            Menu_BG_256,
            Menu_BG_368,
            DEBUG,
        }

        #endregion Enums

        #region Properties

        public uint Count => (uint)entries.Count();
        public uint PalletCount => (uint)icons.Length;

        #endregion Properties

        #region Methods

        public EntryGroup GetEntryGroup(Enum id) => entries[(ID)id] ?? null;

        public EntryGroup GetEntryGroup(int id) => entries[(ID)id] ?? null;

        public Entry GetEntry(Enum id, int index) => entries[(ID)id][index] ?? null;

        public Entry GetEntry(int id, int index) => entries[(ID)id][index] ?? null;

        public Entry GetEntry(Enum id) => entries[(ID)id][0] ?? null;

        public Entry GetEntry(int id) => entries[(ID)id][0] ?? null;

        public EntryGroup this[Enum id] => GetEntryGroup(id);

        public EntryGroup this[int id] => GetEntryGroup(id);

        public void Draw(int id, int pallet, Rectangle dst, float scale = 1f, float fade = 1f) => Draw((ID)id, pallet, dst, scale, fade);

        public void Draw(Enum id, int pallet, Rectangle dst, float scale = 1f, float fade = 1f) => entries[(ID)id].Draw(icons, pallet, dst, scale, fade);

        public void Draw(int id, Rectangle dst, float fade = 1) => Draw((ID)id, 2, dst);

        public void Draw(Enum id, Rectangle dst, float fade = 1) => Draw((ID)id, 2, dst);

        #endregion Methods
    }
}