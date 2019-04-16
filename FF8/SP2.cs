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
        public uint EntriesPerTexture { get; protected set; }

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
            Texture2D tex = GetTexture(id, out Vector2 scale);
            src.Location = (src.Location.ToVector2() * scale).ToPoint();
            src.Size = (src.Size.ToVector2() * scale).ToPoint();
            Memory.spriteBatch.Draw(tex, dst, src, Color.White * fade);
        }

        public virtual Entry GetEntry(Enum id)
        {
            if (Entries.ContainsKey(Convert.ToUInt32(id)))
                return Entries[Convert.ToUInt32(id)];
            if (Entries.ContainsKey(Convert.ToUInt32(id) % EntriesPerTexture))
                return Entries[Convert.ToUInt32(id) % EntriesPerTexture];
            return null;
        }

        public virtual Texture2D GetTexture(Enum id, out Vector2 scale)
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
                    aw.GetListOfFiles().First(x => x.ToLower().Contains(IndexFilename)))))
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
                        for (int h = 0; h < TextureBigFilename.Length; h++)
                        {
                            TextureHandler th = new TextureHandler(TextureBigFilename[h], 2, TextureBigSplit[h] / 2)
                            {
                                oldSize = new Vector2(tex.TextureData.Width, tex.TextureData.Height),
                            };

                            Textures.Add(th);
                            Scale[i] = th.GetScale();
                        }
                    }
                    else
                    {
                        TextureHandler th = new TextureHandler(path);
                        Textures.Add(th);
                        Scale[i] = th.GetScale();
                    }
                }
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// This contains functions to Load Highres mod versions of textures and get scale vector.
    /// </summary>
    internal class TextureHandler
    {
        protected uint Cols { get; set; }
        protected uint Rows { get; set; }

        /// <summary>
        /// X = width and Y = height. The Size of original texture. Will be used in scaling
        /// </summary>
        public Vector2 oldSize { get; set; }
        /// <summary>
        /// X = width and Y = height. The Size of big version texture. Will be used in scaling
        /// </summary>
        public Vector2 Size { get; private set; }

        /// <summary>
        /// Scale vector from original to big
        /// </summary>
        public Vector2 PreScale { get => Size == Vector2.Zero || oldSize == Vector2.Zero? Vector2.One:Size / oldSize; }
        protected uint TextureCount { get; private set; }
        protected uint TextureStartOffset { get; private set; }
        protected string TextureFilename { get; set; }
        protected Texture2D[,] Textures { get; private set; }

        /// <summary>
        /// Scale vector big to modded or orignal to modded.
        /// </summary>
        protected Vector2[,] Scales { get; private set; }

        public TextureHandler(string filename, uint cols = 1, uint rows = 1)
        {
            Size = Vector2.Zero;
            TextureCount = cols * rows;
            Textures = new Texture2D[cols, rows];
            Scales = new Vector2[cols, rows];
            TextureStartOffset = 0;
            Rows = rows;
            Cols = cols;
            TextureFilename = filename;

            //load textures;
            Init();
        }

        protected void Init()
        {
            Vector2 size = Vector2.Zero;
            TEX tex = null;
            uint c2 = 0;
            uint r2 = 0;
            for (uint r = 0; r < Rows; r++)
            {
                for (uint c = 0; c < Cols; c++)
                {
                    ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                    string path = aw.GetListOfFiles().First(x => (x.IndexOf(string.Format(TextureFilename, c + r * Cols + TextureStartOffset),StringComparison.OrdinalIgnoreCase)>=0));
                    tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, path));
                    Texture2D pngTex = LoadPNG(path);

                    Textures[c, r] = (UseBest(tex, pngTex, out Vector2 scale));
                    Scales[c, r] = scale;

                    if (c2++ < Cols) size.X += tex.TextureData.Width;
                }
                if (r2++ < Rows) size.Y += tex.TextureData.Height;
            }
            Size = size;
        }

        public Texture2D this[int c, int r] => Textures[c, r];

        public static implicit operator Texture2D(TextureHandler t)
        {
            if (t.TextureCount == 1)
                return t[0, 0];
            throw new Exception("TextureHandler can only be cast to Texture2D if there is only one texture in the array use [cols,rows] instead");
            //return null;
        }

        /// <summary>
        /// Load Texture from a mod
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D LoadPNG(string path)
        {
            string bn = Path.GetFileNameWithoutExtension(path);
            string prefix = bn.Substring(0, 2);
            string pngpath = Path.Combine(Memory.FF8DIR, "..", "..", "textures", prefix, bn);
            if (Directory.Exists(pngpath))
            {//TODO: add support for multiple 
                pngpath = Directory.GetFiles(pngpath).Last(x => (x.IndexOf(bn,StringComparison.OrdinalIgnoreCase)>=0));//.ToLower().Contains(bn.ToLower())
                using (FileStream fs = File.OpenRead(pngpath))
                {
                    return Texture2D.FromStream(Memory.graphics.GraphicsDevice, fs);
                }
            }
            return null;
        }

        public Vector2 GetScale(int cols=0,int rows=0)
        {
            return Scales[cols, rows];
        }
        public static Vector2 GetScale(Vector2 _old, Vector2 _new) => _new / _old;

        public static Vector2 GetScale(Vector2 _old, Texture2D _new) => new Vector2(_new.Width / _old.X, _new.Height / _old.Y);

        public static Vector2 GetScale(TEX _old, Texture2D _new) => new Vector2((float)_new.Width / _old.TextureData.Width, (float)_new.Height / _old.TextureData.Height);

        public static Vector2 GetScale(Texture2D _old, Texture2D _new) => new Vector2((float)_new.Width / _old.Width, (float)_new.Height / _old.Height);

        public static Texture2D UseBest(Texture2D _old, Texture2D _new) => UseBest(_old, _new, out Vector2 scale);

        public static Texture2D UseBest(Texture2D _old, Texture2D _new, out Vector2 scale)
        {
            if (_new == null)
            {
                scale = Vector2.One;
                return _old;
            }
            else
            {
                scale = GetScale(_old, _new);
                _old.Dispose();
                return _new;
            }
        }

        public static Texture2D UseBest(TEX _old, Texture2D _new, int pallet = 0) => UseBest(_old, _new, out Vector2 scale, pallet);

        public static Texture2D UseBest(TEX _old, Texture2D _new, out Vector2 scale, int pallet = 0)
        {
            if (_new == null)
            {
                scale = Vector2.One;
                return _old.GetTexture(pallet);
            }
            else
            {
                scale = GetScale(_old, _new);
                return _new;
            }
        }
    }
}