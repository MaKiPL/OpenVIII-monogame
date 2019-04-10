using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal partial class Icons
    {
        #region Fields

        private static List<EntryGroup> entries = null;
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
                PalletCount = tex.TextureData.NumOfPalettes;
                icons = new Texture2D[PalletCount];
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
                    aw.GetListOfFiles().First(x => x.ToLower().Contains("icon.sp1")));
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "icons.sp1")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
                //read from icon.sp1
                using (MemoryStream ms = new MemoryStream(test))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        Count = br.ReadUInt32();
                        Loc[] locs = new Loc[Count];
                        for (int i = 0; i < Count; i++)
                        {
                            locs[i].pos = br.ReadUInt16();
                            locs[i].count = br.ReadUInt16();
                            //if (locs[i].count > 1) Count += (uint)(locs[i].count - 1);
                        }
                        entries = new List<EntryGroup>((int)Count+10);
                        for (int i = 0; i < Count; i++)
                        {
                            ms.Seek(locs[i].pos, SeekOrigin.Begin);
                            byte c = (byte)locs[i].count;
                            entries.Add(new EntryGroup(c));
                            for (int e = 0; e < c; e++)
                            {
                                Entry tmp = new Entry();
                                tmp.LoadfromStreamSP1(br);
                                tmp.Part = (byte)e;
                                tmp.SetLoc(locs[i]);
                                entries[i].Add(tmp);
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
                        Tile = Vector2.UnitY
                    };
                    Entry Border_TopLeft = new Entry
                    {
                        X = 16,
                        Y = 0,
                        Width = 8,
                        Height = 8,
                        CustomPallet = 0
                    };
                    Entry Border_Top = new Entry
                    {
                        X = 24,
                        Y = 0,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
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
                        Offset_Y = -8,
                        CustomPallet = 0
                    };
                    Entry Border_TopRight = new Entry
                    {
                        X = 32,
                        Y = 0,
                        Width = 8,
                        Height = 8,
                        Snap_Right = true,
                        Offset_X = -8,
                        CustomPallet = 0
                    };
                    Entry Border_Left = new Entry
                    {
                        X = 16,
                        Y = 8,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitY,
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
                        Offset_X = -8,
                        CustomPallet = 0
                    };
                    Entry Border_BottomLeft = new Entry
                    {
                        X = 16,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Snap_Bottom = true,
                        Offset_Y = -8,
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
                        Offset_X = -8,
                        Offset_Y = -8,
                        CustomPallet = 0
                    };


                    entries.Add(new EntryGroup(new Entry
                    {
                        X = 16,
                        Y = 24,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
                        CustomPallet = 0
                    }));
                    entries.Add(new EntryGroup(new Entry
                    {
                        X = 0,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
                        Offset_Y = 1,
                        CustomPallet = 5
                    }));
                    entries.Add(new EntryGroup(BG, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight));
                    entries.Add(new EntryGroup(BG, new Entry
                    {
                        X = 0,
                        Y = 64,
                        Offset_X = 256,
                        Width = 112,
                        Height = 16,
                        CustomPallet = 1,
                        Tile = Vector2.UnitY
                    }, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight));
                    Count = (uint)entries.Count;
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
            item0004,
            item0005,
            item0006,
            item0007,
            item0008,
            item0009,
            item0010,
            item0011,
            item0012,
            item0013,
            item0014,
            item0015,
            item0016,
            item0017,
            item0018,
            item0019,
            item0020,
            item0021,
            item0022,
            item0023,
            item0024,
            item0025,
            item0026,
            item0027,
            item0028,
            item0029,
            item0030,
            item0031,
            item0032,
            item0033,
            item0034,
            item0035,
            item0036,
            item0037,
            item0038,
            item0039,
            item0040,
            item0041,
            item0042,
            item0043,
            item0044,
            item0045,
            item0046,
            item0047,
            item0048,
            item0049,
            item0050,
            item0051,
            item0052,
            item0053,
            item0054,
            item0055,
            item0056,
            item0057,
            item0058,
            item0059,
            item0060,
            item0061,
            item0062,
            item0063,
            item0064,
            item0065,
            item0066,
            item0067,
            item0068,
            item0069,
            item0070,
            item0071,
            item0072,
            item0073,
            item0074,
            item0075,
            item0076,
            item0077,
            item0078,
            item0079,
            item0080,
            item0081,
            item0082,
            item0083,
            item0084,
            item0085,
            item0086,
            item0087,
            item0088,
            item0089,
            item0090,
            item0091,
            item0092,
            item0093,
            item0094,
            item0095,
            item0096,
            item0097,
            item0098,
            item0099,
            item0100,
            item0101,
            item0102,
            item0103,
            item0104,
            item0105,
            item0106,
            item0107,
            item0108,
            item0109,
            item0110,
            item0111,
            item0112,
            item0113,
            item0114,
            item0115,
            item0116,
            item0117,
            item0118,
            item0119,
            item0120,
            item0121,
            item0122,
            item0123,
            item0124,
            item0125,
            item0126,
            item0127,
            item0128,
            item0129,
            item0130,
            item0131,
            item0132,
            item0133,
            item0134,
            item0135,
            item0136,
            item0137,
            item0138,
            item0139,
            item0140,
            item0141,
            item0142,
            item0143,
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
            item0160,
            item0161,
            item0162,
            item0163,
            item0164,
            item0165,
            item0166,
            item0167,
            item0168,
            item0169,
            item0170,
            item0171,
            item0172,
            item0173,
            item0174,
            item0175,
            item0176,
            item0177,
            item0178,
            item0179,
            item0180,
            item0181,
            item0182,
            item0183,
            item0184,
            item0185,
            item0186,
            item0187,
            item0188,
            item0189,
            item0190,
            item0191,
            item0192,
            item0193,
            item0194,
            item0195,
            item0196,
            item0197,
            item0198,
            item0199,
            item0200,
            item0201,
            item0202,
            item0203,
            item0204,
            item0205,
            item0206,
            item0207,
            item0208,
            item0209,
            item0210,
            item0211,
            item0212,
            item0213,
            item0214,
            item0215,
            item0216,
            item0217,
            item0218,
            item0219,
            item0220,
            item0221,
            item0222,
            item0223,
            item0224,
            item0225,
            item0226,
            item0227,
            item0228,
            item0229,
            item0230,
            item0231,
            item0232,
            item0233,
            item0234,
            item0235,
            item0236,
            item0237,
            item0238,
            item0239,
            item0240,
            item0241,
            item0242,
            item0243,
            item0244,
            item0245,
            item0246,
            item0247,
            item0248,
            item0249,
            item0250,
            item0251,
            item0252,
            item0253,
            item0254,
            item0255,
            item0256,
            item0257,
            item0258,
            item0259,
            item0260,
            item0261,
            item0262,
            item0263,
            item0264,
            item0265,
            item0266,
            item0267,
            item0268,
            item0269,
            item0270,
            item0271,
            item0272,
            item0273,
            item0274,
            item0275,
            item0276,
            item0277,
            item0278,
            item0279,
            item0280,
            item0281,
            item0282,
            item0283,
            item0284,
            item0285,
            item0286,
            item0287,
            item0288,
            item0289,
            item0290,
            item0291,
            item0292,
            item0293,
            item0294,
            item0295,
            item0296,
            item0297,
            item0298,
            item0299,
            item0300,
            item0301,
            item0302,
            item0303,
            item0304,
            item0305,
            item0306,
            item0307,
            item0308,
            item0309,
            item0310,
            item0311,
            item0312,
            item0313,
            item0314,
            item0315,
            item0316,
            item0317,
            item0318,
            item0319,
            item0320,
            item0321,
            item0322,
            item0323,
            item0324,
            item0325,
            item0326,
            item0327,
            item0328,
            Bar_BG,
            Bar_Fill,
            Menu_BG_256,
            Menu_BG_368,
        }

        #endregion Enums

        #region Properties

        public UInt32 Count { get; private set; }
        public int PalletCount { get; private set; }

        #endregion Properties

        #region Methods

        public EntryGroup GetEntryGroup(ID id) => GetEntryGroup((int)id);

        public EntryGroup GetEntryGroup(int id) => entries[id];

        public Entry GetEntry(ID id, int index = 0) => GetEntry((int)id);

        public Entry GetEntry(int id, int index = 0) => entries[id][index] ?? null;

        internal void Draw(ID id, int pallet, Rectangle dst, float scale = 1f, float fade = 1f) => Draw((int)id, pallet, dst, scale, fade);

        internal void Draw(int id, int pallet, Rectangle dst, float scale = 1f, float fade = 1f)
        {
            Viewport vp = Memory.graphics.GraphicsDevice.Viewport;
            entries[id].Draw(icons, pallet, dst, scale, fade);
            Memory.SpriteBatchStartStencil();
            Memory.font.RenderBasicText(Font.CipherDirty(
                $"{((ID)(id)).ToString().Replace('_', ' ')}\nid: {id}"
                ), (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);
            Memory.SpriteBatchEnd();
        }

        internal void Draw(Rectangle dst, int pallet, float fade = 1f) => Memory.spriteBatch.Draw(icons[pallet], dst, Color.White * fade);

        #endregion Methods
    }
}