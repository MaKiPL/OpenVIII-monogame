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

                        entries = new EntryGroup[Count];

                        for (int i = 0; i < locs.Length; i++)
                        {
                            ms.Seek(locs[i].pos, SeekOrigin.Begin);
                            byte c = (byte)locs[i].count;
                            entries[i] = new EntryGroup(c);
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
        public Entry GetEntry(ID id,int index = 0 ) => GetEntry((int)id);

        public Entry GetEntry(int id,int index = 0 ) => entries[id][index] ?? null;

        internal void Draw(ID id, int pallet, Rectangle dst, float fade = 1f) => Draw((int)id, pallet, dst, fade);

        internal void Draw(int id, int pallet, Rectangle dst, float fade = 1f)
        {
            entries[id].Draw(icons[pallet], dst, fade);
        
            //Memory.font.RenderBasicText(Font.CipherDirty($"pos: {entries[id].GetLoc().pos}\ncount: {entries[id].GetLoc().count}\n\nid: {id}\n\nUNKS: {string.Join(", ", entries[id].UNK)}\nALTS: {string.Join(", ", Array.ConvertAll(entries[id].UNK, item => (sbyte)item))}\n\npallet: {pallet}\nx: {entries[id].X}\ny: {entries[id].Y}\nwidth: {entries[id].Width}\nheight: {entries[id].Height} \n\nOffset X: {entries[id].Offset_X}\nOffset Y: {entries[id].Offset_Y}"), (int)(vp.Width * 0.10f), (int)(vp.Height * 0.05f), 1f, 2f, 0, 1);

        }

        internal void Draw(Rectangle dst, int pallet, float fade = 1f) => Memory.spriteBatch.Draw(icons[pallet], dst, Color.White * fade);

        #endregion Methods
    }
}