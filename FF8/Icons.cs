using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

namespace FF8
{
    internal partial class Icons
    {
        #region Fields

        private static EntryGroup[] entries = null;
        private Texture2D[] icons;

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
                using (MemoryStream ms = new MemoryStream(test))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        Count = br.ReadUInt32();
                        Loc[] locs = new Loc[Count];
                        for (int i = 0; i < locs.Length; i++)
                        {
                            locs[i].pos = br.ReadUInt16();
                            locs[i].count = br.ReadUInt16();
                            //if (locs[i].count > 1) Count += (uint)(locs[i].count - 1);
                        }

                        entries = new EntryGroup[Count += 4];

                        for (int i = 0; i < locs.Length; i++)
                        {
                            ms.Seek(locs[i].pos, SeekOrigin.Begin);
                            byte c = (byte)locs[i].count;
                            entries[i] = new EntryGroup(c);
                            for (int e = 0; e < c; e++)
                            {
                                Entry tmp = new Entry();
                                //tmp.CurrentPos = (ushort)ms.Position;
                                tmp.LoadfromStreamSP1(br);
                                tmp.Part = (byte)e;
                                tmp.SetLoc(locs[i]);
                                entries[i].Add(tmp);
                            }
                        }
                    }
                    //custom
                    Entry BG = new Entry
                    {
                        X = 0,
                        Y = 48,
                        Width = 256,
                        Height = 16,
                        CustomPallet = 1,
                        Tile = Vector2.UnitY
                    };
                    Entry BG2 = new Entry
                    {
                        X = 0,
                        Y = 64,
                        Offset_X = 256,
                        Width = 112,
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
                    Entry LoadBar_Empty = new Entry
                    {
                        X = 16,
                        Y = 24,
                        Width = 8,
                        Height = 8,
                        Tile=Vector2.UnitX,
                        CustomPallet = 0
                    };
                    Entry LoadBar_Piece = new Entry
                    {
                        X = 0,
                        Y = 16,
                        Width = 8,
                        Height = 8,
                        Tile = Vector2.UnitX,
                        Offset_Y = 1,
                        CustomPallet = 5
                    };

                    //Border_TopRight.Offset_X = (short)(BG2.Offset_X + BG2.Width + -8);
                    //Border_Right.Offset_X = (short)(BG2.Offset_X + BG2.Width + -8);
                    entries[Count - 1] = new EntryGroup(BG, BG2, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
                    //Border_TopRight.Offset_X = (short)(BG2.Offset_X + -8);
                    //Border_Right.Offset_X = (short)(BG2.Offset_X + BG2.Width + -8);
                    entries[Count - 2] = new EntryGroup(BG, Border_Top, Border_Left, Border_Right, Border_Bottom, Border_TopLeft, Border_TopRight, Border_BottomLeft, Border_BottomRight);
                    entries[Count - 3] = new EntryGroup(LoadBar_Piece);
                    entries[Count - 4] = new EntryGroup(LoadBar_Empty);
                }
            }
        }

        #endregion Constructors

        #region Enums

        public enum ID
        {
            One,
            Two
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

        internal void Draw(int id, int pallet, Rectangle dst, float scale = 1f, float fade = 1f) => entries[id].Draw(icons, pallet, dst, scale, fade);//Memory.font.RenderBasicText(Font.CipherDirty($"pos: {entries[id].GetLoc().pos}\ncount: {entries[id].GetLoc().count}\n\nid: {id}\n\nUNKS: {string.Join(", ", entries[id].UNK)}\nALTS: {string.Join(", ", Array.ConvertAll(entries[id].UNK, item => (sbyte)item))}\n\npallet: {pallet}\nx: {entries[id].X}\ny: {entries[id].Y}\nwidth: {entries[id].Width}\nheight: {entries[id].Height} \n\nOffset X: {entries[id].Offset_X}\nOffset Y: {entries[id].Offset_Y}"), (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);

        internal void Draw(Rectangle dst, int pallet, float fade = 1f) => Memory.spriteBatch.Draw(icons[pallet], dst, Color.White * fade);

        #endregion Methods
    }
}