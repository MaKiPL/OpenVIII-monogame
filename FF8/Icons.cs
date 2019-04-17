using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    /// <summary>
    /// Images of parts of most of the menus and ui.
    /// </summary>
    internal partial class Icons : SP2
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
                //test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                //    aw.GetListOfFiles().First(x => x.ToLower().Contains("cardanm.sp2")));
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "cardanm.sp2")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
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
                        entries = new Dictionary<ID, EntryGroup>(locs.Length + 10);
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
                        Offset = new Vector2(8, 0),
                        End = new Vector2(-8, 0),
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
                        Offset = new Vector2(8, -8),
                        End = new Vector2(-8, 0),
                        CustomPallet = 0
                    };
                    Entry Border_TopRight = new Entry
                    {
                        X = 32,
                        Y = 0,
                        Width = 8,
                        Height = 8,
                        Snap_Right = true,
                        Offset = new Vector2(-8, 0),
                        CustomPallet = 0
                    };
                    Entry Border_Left = new Entry
                    {
                        X = 16,
                        Y = 8,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitY,
                        Offset = new Vector2(0, 8),
                        End = new Vector2(0, -8),
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
                        Offset = new Vector2(-8, 8),
                        End = new Vector2(0, -8),
                        CustomPallet = 0
                    };
                    Entry Border_BottomLeft = new Entry
                    {
                        X = 16,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Snap_Bottom = true,
                        Offset = new Vector2(0, -8),
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
                        Offset = new Vector2(-8, -8),
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
                        Offset = new Vector2(0, 1),
                        CustomPallet = 5
                    });
                    entries[ID.Menu_BG_256] = new EntryGroup(BG, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
                    entries[ID.Menu_BG_368] = new EntryGroup(BG, new Entry
                    {
                        X = 0,
                        Y = 64,
                        Offset = new Vector2(256, 0),
                        Width = 112,
                        Height = 16,
                        CustomPallet = 1,
                        Tile = Vector2.UnitY
                    }, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);

                    entries[ID.DEBUG] = new EntryGroup(
                        new Entry { X = 128, Y = 24, Width = 7, Height = 8 },
                        new Entry { X = 65, Y = 8, Width = 6, Height = 8, Offset = new Vector2(7, 0) },
                        new Entry { X = 147, Y = 24, Width = 6, Height = 8, Offset = new Vector2(13, 0) },
                        new Entry { X = 141, Y = 24, Width = 6, Height = 8, Offset = new Vector2(19, 0) },
                        new Entry { X = 104, Y = 16, Width = 6, Height = 8, Offset = new Vector2(25, 0) }
                        );
                }
            }
        }

        #endregion Constructors

        #region Properties

        public new uint Count => (uint)entries.Count();
        public new uint PalletCount => (uint)icons.Length;

        #endregion Properties

        #region Indexers

        public new EntryGroup this[Enum id] => GetEntryGroup(id);

        #endregion Indexers

        #region Methods

        public void Draw(Enum id, int pallet, Rectangle dst, float scale, float fade = 1f) => entries[(ID)id].Draw(icons, pallet, dst, scale, fade);

        public override void Draw(Enum id, Rectangle dst, float fade = 1) => Draw((ID)id, 2, dst, 1f);

        public Entry GetEntry(Enum id, int index) => entries[(ID)id][index] ?? null;

        public override Entry GetEntry(Enum id) => entries[(ID)id][0] ?? null;

        public EntryGroup GetEntryGroup(Enum id) => entries[(ID)id] ?? null;

        #endregion Methods
    }
}