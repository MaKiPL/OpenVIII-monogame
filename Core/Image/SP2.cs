using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// SP2 is a handler for .sp1 and .sp2 files. They are texture atlas coordinates
    /// <para>This stores the entries in Entry objects or EntryGroup objects</para>
    /// </summary>
    public abstract class SP2
    {
        #region Constructors

        protected SP2()
        {
            Count = 0;
            PaletteCount = 1;
            EntriesPerTexture = 1;
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
        /// Number of Entries
        /// </summary>
        public uint Count { get; protected set; }

        /// <summary>
        /// Number of Palettes
        /// </summary>
        public uint PaletteCount { get; protected set; }

        protected Memory.Archive ArchiveString { get; set; }

        /// <summary>
        /// Dictionary of Entries
        /// </summary>
        protected virtual Dictionary<uint, Entry> Entries { get; set; }

        /// <summary>
        /// Entries per texture,ID MOD EntriesPerTexture to get current entry to use on this texture
        /// </summary>
        protected virtual int EntriesPerTexture { get; set; }

        /// <summary>
        /// If true disable mods and high res textures.
        /// </summary>
        protected bool FORCE_ORIGINAL { get; set; } = false;

        /// <summary>
        /// *.sp1 or *.sp2 that contains the entries or entrygroups. With Rectangle and offset information.
        /// </summary>
        protected string IndexFilename { get; set; }

        protected List<TexProps> Props { get; set; }

        /// <summary>
        /// Should be Vector2.One unless reading a high res version of textures.
        /// </summary>
        protected Dictionary<uint, Vector2> Scale { get; set; }

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

        public virtual void Draw(Enum id, Rectangle dst, Vector2 fill, float fade = 1)
        {
            Rectangle src = GetEntry(id).GetRectangle;
            if (fill == Vector2.UnitX)
            {
                float r = (float)dst.Height / dst.Width;
                src.Height = (int)Math.Round(src.Height * r);
            }
            else if (fill == Vector2.UnitY)
            {
                float r = (float)dst.Width / dst.Height;
                src.Width = (int)Math.Round(src.Width * r);
            }
            TextureHandler tex = GetTexture(id);
            tex.Draw(dst, src, Color.White * fade);
        }

        public virtual Entry GetEntry(Enum id)
        {
            if (Entries.ContainsKey(Convert.ToUInt32(id)))
                return Entries[Convert.ToUInt32(id)];
            if (Entries.ContainsKey((uint)(Convert.ToUInt32(id) % EntriesPerTexture)))
                return Entries[(uint)(Convert.ToUInt32(id) % EntriesPerTexture)];
            return null;
        }

        public virtual TextureHandler GetTexture(Enum id)
        {
            uint pos = Convert.ToUInt32(id);
            uint File = GetEntry(id).File;
            //check if we set a custom file and we have a pos more then set entriespertexture
            if (File == 0 && EntriesPerTexture > 0 && pos > EntriesPerTexture)
                File = (uint)(pos / EntriesPerTexture);
            if (File > 0)
            {
                uint j = (uint)Props.Sum(x => x.Count);
                if (File >= j)
                {
                    File %= j;
                }
            }
            return Textures[(int)File];
        }

        public virtual TextureHandler GetTexture(Enum id, out Vector2 scale)
        {
            scale = Scale[GetEntry(id).File];
            return GetTexture(id);
        }

        public void Trim(Enum ic, byte pal)
        {
            Entry eg = this[ic];
            eg.SetTrimNonGroup(Textures[pal]);
        }

        protected virtual void Init()
        {
            if (Entries == null)
            {
                ArchiveWorker aw = new ArchiveWorker(ArchiveString);
                InitEntries(aw);
                InsertCustomEntries();
                InitTextures(aw);
            }
        }

        protected virtual void InitEntries(ArchiveWorker aw = null)
        {
            if (Entries == null)
            {
                if (aw == null)
                    aw = new ArchiveWorker(ArchiveString);
                MemoryStream ms = null;

                ushort[] locs;
                using (BinaryReader br = new BinaryReader(
                    ms = new MemoryStream(ArchiveWorker.GetBinaryFile(ArchiveString,
                aw.GetListOfFiles().First(x => x.IndexOf(IndexFilename, StringComparison.OrdinalIgnoreCase) >= 0)))))
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
                    ms = null;
                }
            }
        }

        protected virtual void InitTextures(ArchiveWorker aw = null)
        {
            int count = (int)Props.Sum(x => x.Count);
            if (Textures == null)
                Textures = new List<TextureHandler>(count);
            if (Textures.Count <= 0)
            {
                if (aw == null)
                    aw = new ArchiveWorker(ArchiveString);
                TEX tex;
                Scale = new Dictionary<uint, Vector2>(count);
                int b = 0;
                for (int j = 0; j < Props.Count; j++)
                    for (uint i = 0; i < Props[j].Count; i++)
                    {
                        string path = aw.GetListOfFiles().First(x => x.ToLower().Contains(string.Format(Props[j].Filename, i + TextureStartOffset)));
                        tex = new TEX(aw.GetBinaryFile(path));
                        if (Props[j].Big != null && FORCE_ORIGINAL == false && b < Props[j].Big.Count)
                        {
                            TextureHandler th = TextureHandler.Create(Props[j].Big[b].Filename, tex, 2, Props[j].Big[b++].Split / 2);

                            Textures.Add(th);
                            Scale[i] = Vector2.One;
                        }
                        else
                        {
                            TextureHandler th = TextureHandler.Create(path, tex);
                            Textures.Add(th);
                            Scale[i] = th.GetScale(); //scale might not be used outside of texturehandler.
                        }
                    }
            }
        }

        protected virtual void InsertCustomEntries()
        {
        }

        #endregion Methods

        #region Classes

        /// <summary>
        /// For big textures.
        /// </summary>
        public class BigTexProps
        {
            #region Fields

            /// <summary>
            /// leave null unless big version has a different custom palette than normal.
            /// </summary>
            public Color[] Colors;

            /// <summary>
            /// Filename; To match more than one number use {0:00} or {00:00} for ones with leading zeros.
            /// </summary>
            public string Filename;

            /// <summary>
            /// Big versions of textures take the file and split it into multiple. How many splits
            /// per BigFilename. Value to be interval of 2. As these files are all 2 cols wide. And
            /// must be &gt;= 2
            /// </summary>
            public uint Split;

            #endregion Fields

            #region Constructors

            public BigTexProps(string filename, uint split, Color[] colors = null)
            {
                Filename = filename;
                Split = split;
                Colors = colors;
            }

            #endregion Constructors
        }

        public class TexProps
        {  /// <summary>
            #region Fields

            /// <summary>
            /// For big textures.
            /// </summary>
            public List<BigTexProps> Big;

            /// <summary>
            /// Override palette of texture to this and don't load other palettes. If null ignore.
            /// </summary>
            public Color[] Colors;

            /// <summary>
            /// Number of Textures
            /// </summary>
            public uint Count;

            /// Filename. To match more than one number use {0:00} or {00:00} for ones with leading
            /// zeros. </summary>
            public string Filename;

            #endregion Fields

            #region Constructors

            public TexProps(string filename, uint count, params BigTexProps[] big)
            {
                Filename = filename;
                Count = count;
                if (big != null && Count != big.Length && big.Length > 0)
                    throw new Exception($"Count of big textures should match small ones {Count} != {big.Length}");
                Big = big.ToList();
                Colors = null;
            }

            public TexProps(string filename, uint count, Color[] colors, params BigTexProps[] big)
            {
                Filename = filename;
                Count = count;
                if (big != null && Count != big.Length && big.Length > 0)
                    throw new Exception($"Count of big textures should match small ones {Count} != {big.Length}");
                Big = big.ToList();
                Colors = colors;
            }

            #endregion Constructors
        }

        #endregion Classes
    }
}