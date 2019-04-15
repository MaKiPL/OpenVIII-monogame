using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF8
{
    internal abstract class SP2 //: I_SP2
    {
        /// <summary>
        /// Number of Entries
        /// </summary>
        public uint Count { get; protected set; }

        /// <summary>
        /// Number of Pallets
        /// </summary>
        public uint PalletCount { get; protected set; }

        /// <summary>
        /// Number of Textures
        /// </summary>
        public int TextureCount { get; protected set; }

        /// <summary>
        /// Entries per texture,ID MOD EntriesPerTexture to get current entry to use on this texture
        /// </summary>
        public uint EntriesPerTexture { get; protected set; }

        /// <summary>
        /// Texture filename. To match more than one number use {0:00} or {00:00} for ones with
        /// leading zeros.
        /// </summary>
        /// TODO make array maybe to add support for highres versions. the array will need to come
        /// with a scale factor
        protected string TextureFilename { get; set; }

        /// <summary>
        /// Should be Vector2.One unless reading a high res version of textures.
        /// </summary>
        protected Vector2 Scale { get; set; }

        /// <summary>
        /// Some textures start with 1 and some start with 0. This is added to the current number in
        /// when reading the files in.
        /// </summary>
        protected int TextureStartOffset { get; set; }

        /// <summary>
        /// *.sp1 or *.sp2 that contains the entries or entrygroups. With Rectangle and offset information.
        /// </summary>
        protected string IndexFilename { get; set; }

        /// <summary>
        /// List of textures
        /// </summary>
        protected virtual List<Texture2D> Textures { get; set; }

        /// <summary>
        /// Dictionary of Entries
        /// </summary>
        protected virtual Dictionary<uint, Entry> Entries { get; set; }

        /// <summary>
        /// Draw Item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dst"></param>
        /// <param name="fade"></param>
        public virtual void Draw(Enum id, Rectangle dst, float fade = 1)
        {
            if (Entries.ContainsKey(Convert.ToUInt32(id) % EntriesPerTexture) && Entries[Convert.ToUInt32(id) % EntriesPerTexture].File < Textures.Count)
                Memory.spriteBatch.Draw(Textures[Entries[Convert.ToUInt32(id) % EntriesPerTexture].File], dst, Entries[Convert.ToUInt32(id) % EntriesPerTexture].GetRectangle, Color.White * fade);
        }
        public Entry this[Enum id] => GetEntry(id);
        public virtual Entry GetEntry(Enum id)
        {
            if (Entries.ContainsKey(Convert.ToUInt32(id) % EntriesPerTexture))
                return Entries[Convert.ToUInt32(id) % EntriesPerTexture];
            return null;
        }

        public virtual void Init()
        {
            if (Entries == null)
            {
                Textures = new List<Texture2D>(TextureCount);
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                byte[] test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains(IndexFilename)));
                using (MemoryStream ms = new MemoryStream(test))
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
                            if (t == 0 || t == 96)
                            {
                                Count = i + 1;
                                break;
                            }

                            Entries[i] = new Entry();
                            fid = Entries[i].LoadfromStreamSP2(br, locs[i], (byte)(Last != null ? Last.Y : 0), fid);

                            Last = Entries[i];
                        }
                    }
                }
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "face.sp2")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
                TEX tex;
                for (int i = 0; i < TextureCount; i++)
                {
                    tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                        aw.GetListOfFiles().First(x => x.ToLower().Contains(string.Format(TextureFilename, i + TextureStartOffset)))));
                    Textures[i] = tex.GetTexture();
                }
            }
        }
    }

    internal class Faces : SP2
    {
        #region Fields

        protected static Texture2D[] textures;

        protected static Dictionary<Enum, Entry> entries;

        #endregion Fields

        #region Constructors

        public Faces()
        {
            TextureFilename = "face{0:0}.tex";
            TextureStartOffset = 1;
            IndexFilename = "face.sp2";
            TextureCount = 2;
            EntriesPerTexture = 16;
            Init();
        }

        #endregion Constructors

        #region Enums

        /// <summary>
        /// First half in faces1.tex, second half in faces2.tex, 8 cols 2 rows per file.
        /// </summary>
        public enum ID
        {
            Squall_Leonhart = 0,
            Zell_Dincht,
            Irvine_Kinneas,
            Quistis_Trepe,
            Rinoa_Heartilly,
            Selphie_Tilmitt,
            Seifer_Almasy,
            Edea_Kramer,
            Laguna_Loire,
            Kiros_Seagill,
            Ward_Zabac,
            Lion = Ward_Zabac + 2, //skipped blank
            MiniMog,
            Boko,
            Angelo,
            Quezacotl,
            Shiva,
            Ifrit,
            Siren,
            Brothers,
            Diablos,
            Carbuncle,
            Leviathan,
            Pandemona,
            Cerberus,
            Alexander,
            Doomtrain,
            Bahamut,
            Cactuar,
            Tonberry,
            Eden
        }

        #endregion Enums

        #region Properties


        #endregion Properties

        #region Methods

        protected void Process(int tc, string fn, int tso, string ind, ref Dictionary<Enum, Entry> dict, ref Texture2D[] texs)
        {
            if (dict == null)
            {
                texs = new Texture2D[tc];
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                byte[] test = ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                    aw.GetListOfFiles().First(x => x.ToLower().Contains(ind)));
                using (MemoryStream ms = new MemoryStream(test))
                {
                    ushort[] locs;
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        //ms.Seek(4, SeekOrigin.Begin);
                        Count = br.ReadUInt32();
                        locs = new ushort[Count];//br.ReadUInt32(); 32 valid values in face.sp2 rest is invalid
                        dict = new Dictionary<Enum, Entry>((int)Count);
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
                            if (t == 0 || t == 96)
                            {
                                Count = i + 1;
                                break;
                            }

                            dict[(ID)i] = new Entry();
                            fid = dict[(ID)i].LoadfromStreamSP2(br, locs[i], (byte)(Last != null ? Last.Y : 0), fid);

                            Last = dict[(ID)i];
                        }
                    }
                }
                //using (FileStream fs = File.OpenWrite(Path.Combine("d:\\", "face.sp2")))
                //{
                //    fs.Write(test, 0, test.Length);
                //}
                TEX tex;
                for (int i = 0; i < tc; i++)
                {
                    tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU,
                        aw.GetListOfFiles().First(x => x.ToLower().Contains(string.Format(fn, i + tso)))));
                    texs[i] = tex.GetTexture();
                }
            }
        }


        public virtual Entry GetEntry(int id) => entries[(ID)id];

        protected Entry GetEntry(Enum id, ref Dictionary<Enum, Entry> e) => e[id] ?? null;

        protected Entry GetEntry(int id, ref Dictionary<Enum, Entry> e) => e[(ID)id] ?? null;

        protected void Draw(Enum id, Rectangle dst, float fade, ref Dictionary<Enum, Entry> e, ref Texture2D[] t) => Memory.spriteBatch.Draw(t[e[id].File], dst, e[id].GetRectangle, Color.White * fade);

        protected void Draw(int id, Rectangle dst, float fade, ref Dictionary<Enum, Entry> e, ref Texture2D[] t) => Draw((ID)id, dst, fade, ref e, ref t);
        
        public virtual void Draw(int id, Rectangle dst, float fade = 1f) => Draw((ID)id, dst, fade, ref entries, ref textures);

        #endregion Methods
    }
}