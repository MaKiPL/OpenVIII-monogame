using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal abstract class SP2
    {
        #region Constructors

        protected SP2()
        {
            Count = 0;
            PalletCount = 1;
            TextureCount = new int[] { 1 };
            EntriesPerTexture = 1;
            TextureFilename = new string [] {""};
            TextureBigFilename = null;
            TextureBigSplit = null;
            Scale = null;
            TextureStartOffset = 0;
            IndexFilename = "";
            Textures = null;
            Entries = null;
            ArchiveString = Memory.Archives.A_MENU;
        }

        #endregion Constructors

        #region Enums

        /// <summary>
        /// enum to be added to class when implemented
        /// </summary>
        public enum ID { NotImplemented }

        #endregion Enums

        #region Properties

        /// <summary>
        /// If true disable mods and high res textures.
        /// </summary>
        protected bool FORCE_ORIGINAL { get; set; } = false;

        /// <summary>
        /// Number of Entries
        /// </summary>
        public uint Count { get; protected set; }

        /// <summary>
        /// Entries per texture,ID MOD EntriesPerTexture to get current entry to use on this texture
        /// </summary>
        protected virtual int EntriesPerTexture { get; set; }

        /// <summary>
        /// Number of Pallets
        /// </summary>
        public uint PalletCount { get; protected set; }

        /// <summary>
        /// Number of Textures
        /// </summary>
        public int[] TextureCount { get; protected set; }

        /// <summary>
        /// Dictionary of Entries
        /// </summary>
        protected virtual Dictionary<uint, Entry> Entries { get; set; }
        protected string ArchiveString { get; set; }

        /// <summary>
        /// *.sp1 or *.sp2 that contains the entries or entrygroups. With Rectangle and offset information.
        /// </summary>
        protected string IndexFilename { get; set; }

        /// <summary>
        /// Should be Vector2.One unless reading a high res version of textures.
        /// </summary>
        protected Dictionary<uint, Vector2> Scale { get; set; }

        /// <summary>
        /// Texture filename. To match more than one number use {0:00} or {00:00} for ones with
        /// leading zeros.
        /// </summary>
        /// TODO make array maybe to add support for highres versions. the array will need to come
        /// with a scale factor
        protected string[] TextureFilename { get; set; }

        /// <summary>
        /// For big textures.
        /// </summary>
        protected string[] TextureBigFilename { get; set; }

        /// <summary>
        /// Big versions of textures take the file and split it into multiple. How many splits per BigFilename. 
        /// Value to be interval of 2. As these files are all 2 cols wide. And must be >= 2
        /// </summary>
        protected uint[] TextureBigSplit { get; set; }

        /// <summary>
        /// List of textures
        /// </summary>
        protected virtual List<TextureHandler> Textures { get; set; }

        /// <summary>
        /// Some textures start with 1 and some start with 0. This is added to the current number in
        /// when reading the files in.
        /// </summary>
        protected int TextureStartOffset { get; set; }

        #endregion Properties

        #region Indexers

        public Entry this[Enum id] => GetEntry(id);

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Draw Item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dst"></param>
        /// <param name="fade"></param>
        public virtual void Draw(Enum id, Rectangle dst, float fade = 1)
        {
            Rectangle src = GetEntry(id).GetRectangle;
            TextureHandler tex = GetTexture(id);
            tex.Draw(dst, src, Color.White * fade);
        }
        //public virtual void Draw(Enum id, Rectangle dst, Vector2 scale, float fade = 1)
        //{
        //    Rectangle src = GetEntry(id).GetRectangle;
        //    TextureHandler tex = GetTexture(id);
        //    tex.Draw(dst, src, Color.White * fade);
        //}

        private TextureHandler GetTexture(Enum id) => GetTexture(id,out Vector2 scale);

        public virtual Entry GetEntry(Enum id)
        {
            if (Entries.ContainsKey(Convert.ToUInt32(id)))
                return Entries[Convert.ToUInt32(id)];
            if (Entries.ContainsKey((uint)(Convert.ToUInt32(id) % EntriesPerTexture)))
                return Entries[(uint)(Convert.ToUInt32(id) % EntriesPerTexture)];
            return null;
        }

        public virtual TextureHandler GetTexture(Enum id, out Vector2 scale)
        {
            uint pos = Convert.ToUInt32(id);
            uint File = GetEntry(id).File;
            //check if we set a custom file and we have a pos more then set entriespertexture
            if (File == 0 && EntriesPerTexture > 0 && pos > EntriesPerTexture)
                File = (uint)(pos / EntriesPerTexture);
            if (File > 0)
            {
                uint j = (uint)TextureCount.Sum();
                if (File >= j)
                {
                    File %= j;
                }
            }            
            scale = Scale[File];
            return Textures[(int)File];
        }

        protected virtual void Init()
        {


            ArchiveWorker aw = new ArchiveWorker(ArchiveString);
            InitEntries(aw);
            InitTextures(aw);
            InsertCustomEntries();
        }

        protected virtual void InsertCustomEntries() {}

        protected virtual void InitEntries(ArchiveWorker aw = null)
        {
            if (Entries == null)
            {
                if (aw == null)
                    aw = new ArchiveWorker(ArchiveString);
                using (MemoryStream ms = new MemoryStream(ArchiveWorker.GetBinaryFile(ArchiveString,
                    aw.GetListOfFiles().First(x => x.IndexOf(IndexFilename, StringComparison.OrdinalIgnoreCase) >= 0))))
                {
                    ushort[] locs;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        Count = br.ReadUInt32();
                        locs = new ushort[Count];//br.ReadUInt32(); 32 valid values in face.sp2 rest is invalid
                        Entries = new Dictionary<uint, Entry>((int)Count);
                        for (uint i = 0; i < Count; i++)
                        {
                            locs[i] = br.ReadUInt16();
                            ms.Seek(2, SeekOrigin.Current);
                        }
                        byte fid = 0;
                        Entry Last = null;
                        for (uint i = 0; i < Count; i++)
                        {
                            ms.Seek(locs[i] + 6, SeekOrigin.Begin);
                            byte t = br.ReadByte();
                            if (t == 0 || t == 96) // known invalid entries in sp2 files have this value. there might be more to it.
                            {
                                Count = i;
                                break;
                            }

                            Entries[i] = new Entry();
                            Entries[i].LoadfromStreamSP2(br, locs[i], Last, ref fid);

                            Last = Entries[i];
                        }
                    }
                }
            }
        }
        protected virtual void InitTextures(ArchiveWorker aw = null)
        {
            if (Textures == null)
                Textures = new List<TextureHandler>(TextureCount.Sum());
            if (Textures.Count <= 0)
            {
                if (aw == null)
                    aw = new ArchiveWorker(ArchiveString);
                TEX tex;
                Scale = new Dictionary<uint, Vector2>(TextureCount.Sum());
                int b = 0;
                for (int j =0; j< TextureCount.Length; j++)
                for (uint i = 0; i < TextureCount[j]; i++)
                {
                    string path = aw.GetListOfFiles().First(x => x.ToLower().Contains(string.Format(TextureFilename[j], i + TextureStartOffset)));
                    tex = new TEX(ArchiveWorker.GetBinaryFile(ArchiveString, path));
                    if (TextureBigFilename != null && FORCE_ORIGINAL == false && b< TextureBigFilename.Length && b< TextureBigSplit.Length)
                    {
                        TextureHandler th = new TextureHandler(TextureBigFilename[b], tex, 2, TextureBigSplit[b++] / 2);

                        Textures.Add(th);
                        Scale[i] = Vector2.One;//th.GetScale();
                    }
                    else
                    {
                        TextureHandler th = new TextureHandler(path, tex);
                        Textures.Add(th);
                        Scale[i] = th.GetScale();
                    }
                }
            }
        }

        #endregion Methods
    }
}