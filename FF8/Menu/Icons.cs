using Microsoft.Xna.Framework;
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

        private new Dictionary<ID, EntryGroup> Entries = null;

        #endregion Fields

        #region Constructors

        public Icons()
        {
            //FORCE_ORIGINAL = true;
            TextureBigFilename = new string[] { "iconfl{0:00}.TEX" };
            TextureBigSplit = new uint[] { 4 };
            TextureFilename[0] = "icon.tex";
            IndexFilename = "icon.sp1";
            Init();
        }

        #endregion Constructors

        #region Properties

        public new uint Count => (uint)Entries.Count();
        public new uint PalletCount => (uint)Textures.Count();
        public new uint TextureCount => 1;
        private new uint TextureStartOffset => 0;// this really isn't improtant to icons.
        public new uint EntriesPerTexture => (uint)Enum.GetValues(typeof(Icons.ID)).Cast<Icons.ID>().Max(); // this really isn't improtant to icons.

        #endregion Properties

        #region Indexers

        public new EntryGroup this[Enum id] => GetEntryGroup(id);

        #endregion Indexers

        #region Methods

        protected override void InitTextures(ArchiveWorker aw = null)
        {
            TEX tex;
            tex = new TEX(ArchiveWorker.GetBinaryFile(ArchiveString,
                aw.GetListOfFiles().First(x => x.IndexOf(TextureFilename[0], StringComparison.OrdinalIgnoreCase) >= 0)));
            Textures = new List<TextureHandler>(tex.TextureData.NumOfPalettes);
            for (int i = 0; i < tex.TextureData.NumOfPalettes; i++)
            {
                if (FORCE_ORIGINAL == false && TextureBigFilename != null && TextureBigSplit != null)
                    Textures.Add(new TextureHandler(TextureBigFilename[0], tex, 2, TextureBigSplit[0] / 2, i));
                else
                    Textures.Add(new TextureHandler(TextureFilename[0], tex, 1, 1, i));
            }
        }
        protected override void InitEntries(ArchiveWorker aw = null)
        {
            if (Entries == null)
            {
                //read from icon.sp1
                using (MemoryStream ms = new MemoryStream(ArchiveWorker.GetBinaryFile(ArchiveString,
                    aw.GetListOfFiles().First(x => x.IndexOf(IndexFilename, StringComparison.OrdinalIgnoreCase) >= 0))))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        Loc[] locs = new Loc[br.ReadUInt32()];
                        for (int i = 0; i < locs.Length; i++)
                        {
                            locs[i].seek = br.ReadUInt16();
                            locs[i].length = br.ReadUInt16();
                        }
                        Entries = new Dictionary<ID, EntryGroup>(locs.Length + 10);
                        for (int i = 0; i < locs.Length; i++)
                        {
                            ms.Seek(locs[i].seek, SeekOrigin.Begin);
                            byte c = (byte)locs[i].length;
                            Entries[(ID)i] = new EntryGroup(c);
                            for (int e = 0; e < c; e++)
                            {
                                Entry tmp = new Entry();
                                tmp.LoadfromStreamSP1(br);
                                tmp.Part = (byte)e;
                                tmp.SetLoc(locs[i]);
                                Entries[(ID)i].Add(tmp);
                            }
                        }
                    }
                    //custom stuff not in sp1
                    InsertCustomEntries();
                }
            }
        }
        public void Draw(int number,byte type, int pallet, Vector2 location, Vector2 scale, float fade = 1f)
        {
            ID[] numberstarts = { ID.Size_08x08_0, ID.Size_08x08_ALT_0, ID.Size_08x16_ALT_0, ID.Size_08x16_0, ID.Size_16x16_0, ID.Size_08x08_ALT2_0 };
            List<ID>[] nums = new List<ID>[numberstarts.Length];
            int j = 0;
            foreach (ID id in numberstarts)
            {
                nums[j++] = new List<ID>(10);
                for (byte i = 0; i < 10; i++)
                {
                    nums[0].Add(id + i);
                }
            }
            IEnumerable<int> intList = $"{number}".Select(digit => int.Parse(digit.ToString()));
            var dst = new Rectangle { Location = location.ToPoint() };
            foreach (int i in intList)
            {
                Draw(nums[type][i], pallet,dst, scale, fade);
                dst.Offset(Entries[nums[type][i]].GetRectangle.Width, 0);
            }

        }

        public void Draw(Enum id, int pallet, Rectangle dst, Vector2 scale, float fade = 1f) => Entries[(ID)id].Draw(Textures, pallet, dst, scale, fade);

        public override void Draw(Enum id, Rectangle dst, float fade = 1) => Draw((ID)id, 2, dst, Vector2.One,fade);

        public Entry GetEntry(Enum id, int index) => Entries[(ID)id][index] ?? null;

        public override Entry GetEntry(Enum id) => Entries[(ID)id][0] ?? null;

        public EntryGroup GetEntryGroup(Enum id) => Entries[(ID)id] ?? null;

        #endregion Methods
    }
}