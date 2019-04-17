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
            TextureCount = 1;
            EntriesPerTexture = 1;
            TextureFilename = "";
            TextureBigFilename = null;
            TextureBigSplit = null;
            Scale = null;
            TextureStartOffset = 0;
            IndexFilename = "";
            Textures = null;
            Entries = null;
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
        /// Number of Entries
        /// </summary>
        public uint Count { get; protected set; }

        /// <summary>
        /// Entries per texture,ID MOD EntriesPerTexture to get current entry to use on this texture
        /// </summary>
        protected virtual uint EntriesPerTexture { get; set; }

        /// <summary>
        /// Number of Pallets
        /// </summary>
        public uint PalletCount { get; protected set; }

        /// <summary>
        /// Number of Textures
        /// </summary>
        public int TextureCount { get; protected set; }

        /// <summary>
        /// Dictionary of Entries
        /// </summary>
        protected virtual Dictionary<uint, Entry> Entries { get; set; }

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
        protected string TextureFilename { get; set; }

        /// <summary>
        /// For big textures.
        /// </summary>
        protected string[] TextureBigFilename { get; set; }

        /// <summary>
        /// Big versions of textures take the file and split it into multiple. How many splits per BigFilename.
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
            TextureHandler tex = GetTexture(id, out Vector2 scale);
            src.Location = (src.Location.ToVector2() * scale).ToPoint();
            src.Size = (src.Size.ToVector2() * scale).ToPoint();
            if (tex.Count == 1)
                Memory.spriteBatch.Draw((Texture2D)tex, dst, src, Color.White * fade);
            else if (tex.Count >= 1)
                tex.Draw(dst, src, Color.White * fade);
        }

        public virtual Entry GetEntry(Enum id)
        {
            if (Entries.ContainsKey(Convert.ToUInt32(id)))
                return Entries[Convert.ToUInt32(id)];
            if (Entries.ContainsKey(Convert.ToUInt32(id) % EntriesPerTexture))
                return Entries[Convert.ToUInt32(id) % EntriesPerTexture];
            return null;
        }

        public virtual TextureHandler GetTexture(Enum id, out Vector2 scale)
        {
            uint pos = Convert.ToUInt32(id);
            uint File = GetEntry(id).File;
            //check if we set a custom file and we have a pos more then set entriespertexture
            if (File == 0 && pos > EntriesPerTexture)
                File = pos / EntriesPerTexture;
            if (File >= TextureCount)
                File %= (uint)TextureCount;
            scale = Scale[File];
            return Textures[(int)File];
        }

        public virtual void Init()
        {
            if (Entries == null)
            {
                Textures = new List<TextureHandler>(TextureCount);
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                using (MemoryStream ms = new MemoryStream(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.IndexOf(IndexFilename,StringComparison.OrdinalIgnoreCase)>=0))))
                {
                    ushort[] locs;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        //ms.Seek(4, SeekOrigin.Begin);
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
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "face.sp2")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
                TEX tex;
                Scale = new Dictionary<uint, Vector2>(TextureCount);
                for (uint i = 0; i < TextureCount; i++)
                {
                    string path = aw.GetListOfFiles().First(x => x.ToLower().Contains(string.Format(TextureFilename, i + TextureStartOffset)));
                    tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, path));
                    if (TextureBigFilename != null)
                    {
                        TextureHandler th = new TextureHandler(TextureBigFilename[i], tex, 2, TextureBigSplit[i] / 2);

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