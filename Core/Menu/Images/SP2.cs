using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    // ReSharper disable once InconsistentNaming
    public abstract partial class SP2
    {
        #region Enums

        /// <summary>
        /// enum to be added to class when implemented
        /// </summary>
        // ReSharper disable once InconsistentNaming
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
        public virtual int EntriesPerTexture { get; protected set; }

        /// <summary>
        /// If true disable mods and high res textures.
        /// </summary>
        protected bool ForceOriginal { get; set; } = false;

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

        public static T Load<T>() where T : SP2, new()
        {
            Memory.Log.WriteLine($"{nameof(SP2)} :: {nameof(Load)} :: {typeof(T)} ");
            T r = new T();
            r.DefaultValues();
            r.Init();
            return r;
        }

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
            Entry entry = GetEntry(id);
            if (entry != null)
            {
                Rectangle src = entry.GetRectangle;
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
                tex?.Draw(dst, src, Color.White * fade);
            }
        }

        public virtual Entry GetEntry(Enum id) => GetEntry(Convert.ToUInt32(id));

        public virtual Entry GetEntry(UInt32 id)
        {
            if (EntriesPerTexture <= 0 && Entries.ContainsKey(id))
                return Entries[id];
            else if (Entries?.ContainsKey((uint)(id % EntriesPerTexture))??false)
                return Entries[(uint)(id % EntriesPerTexture)];
            return null;
        }

        public virtual TextureHandler GetTexture(Enum id, int file = -1)
        {
            int pos = Convert.ToInt32(id);
            file = file >= 0 ? file : GetEntry((uint)pos).File;
            //check if we set a custom file and we have a pos more then set entries per texture
            if (file <= 0)
            {
                if (EntriesPerTexture > 0 && pos >= EntriesPerTexture)
                    file = (pos / EntriesPerTexture);
            }
            if (file <= 0) return Textures.Count > file ? Textures[file] : null;
            int j = (int)Props.Sum(x => x.Count);
            if (file >= j)
            {
                file %= j;
            }
            return Textures.Count>file ? Textures[file] : null;
        }

        public virtual TextureHandler GetTexture(Enum id, out Vector2 scale)
        {
            scale = Scale[GetEntry(id).File];
            return GetTexture(id);
        }

        public virtual void Trim(Enum ic, byte pal)
        {
            Entry entry = this[ic];
            entry.SetTrimNonGroup(Textures[pal]);
        }

        protected virtual VertexPositionTexture_Texture2D Quad(Enum ic, byte pal, float scale = .25f, Box_Options options = Box_Options.Middle | Box_Options.Center, float z = 0f)
        {
            Trim(ic, pal);
            return Quad(this[ic], Textures[pal], scale, options, z);
        }

        protected virtual VertexPositionTexture_Texture2D Quad(Entry entry, TextureHandler texture, float scale = .25f, Box_Options options = Box_Options.Middle | Box_Options.Center, float z = 0f)
        {
            Rectangle rectangle = entry.GetRectangle;
            Vector2 scaleFactor = texture.ScaleFactor;
            Vector2 offset = options.HasFlag(Box_Options.UseOffset) ? entry.Offset : Vector2.Zero;
            VertexPositionTexture[] vpt = new VertexPositionTexture[6];
            float left, right, bottom, top;
            if (options.HasFlag(Box_Options.Left))
            {
                left = 0;
                right = rectangle.Width;
            }
            else if (options.HasFlag(Box_Options.Right))
            {
                left = -rectangle.Width;
                right = 0;
            }
            else// (options.HasFlag(Box_Options.Center))
            {
                left = -rectangle.Width / 2f;
                right = rectangle.Width / 2f;
            }
            if (options.HasFlag(Box_Options.Top))
            {
                bottom = 0;
                top = rectangle.Height;
            }
            else if (options.HasFlag(Box_Options.Buttom))
            {
                bottom = -rectangle.Height;
                top = 0;
            }
            else //(options.HasFlag(Box_Options.Middle))
            {
                bottom = -rectangle.Height / 2f;
                top = rectangle.Height / 2f;
            }

            VertexPositionTexture[] v = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(left+offset.X,top+offset.Y,z)*scale,new Vector2(rectangle.Right*scaleFactor.X/texture.Width,rectangle.Top*scaleFactor.Y/texture.Height)),
                new VertexPositionTexture(new Vector3(right+offset.X,top+offset.Y,z)*scale,new Vector2(rectangle.Left*scaleFactor.X/texture.Width,rectangle.Top*scaleFactor.Y/texture.Height)),
                new VertexPositionTexture(new Vector3(right+offset.X,bottom+offset.Y,z)*scale,new Vector2(rectangle.Left*scaleFactor.X/texture.Width,rectangle.Bottom*scaleFactor.Y/texture.Height)),
                new VertexPositionTexture(new Vector3(left+offset.X,bottom+offset.Y,z)*scale,new Vector2(rectangle.Right*scaleFactor.X/texture.Width,rectangle.Bottom*scaleFactor.Y/texture.Height)),
            };
            vpt[0] = v[0];
            vpt[1] = v[1];
            vpt[2] = v[3];

            vpt[3] = v[1];
            vpt[4] = v[2];
            vpt[5] = v[3];
            return new VertexPositionTexture_Texture2D(vpt, texture);
        }

        protected virtual void DefaultValues()
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

        protected virtual void Init()
        {
            if (Entries == null)
            {
                ArchiveBase aw = ArchiveWorker.Load(ArchiveString);
                InitEntries(aw);
                InsertCustomEntries();
                InitTextures<TEX>(aw);
            }
        }

        protected virtual void InitEntries(ArchiveBase aw = null)
        {
            if (Entries != null) return;
            if (aw == null)
                aw = ArchiveWorker.Load(ArchiveString);
            MemoryStream ms;

            byte[] buffer = aw.GetBinaryFile(IndexFilename);
            if (buffer == null) return;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                Count = br.ReadUInt32();
                ushort[] locations = new ushort[Count];
                Entries = new Dictionary<uint, Entry>((int)Count);
                for (uint i = 0; i < Count; i++)
                {
                    locations[i] = br.ReadUInt16();
                    ms.Seek(2, SeekOrigin.Current);
                }
                byte fid = 0;
                Entry last = null;
                for (uint i = 0; i < Count; i++)
                {
                    ms.Seek(locations[i] + 6, SeekOrigin.Begin);
                    byte t = br.ReadByte();
                    if (t == 0 || t == 96) // known invalid entries in sp2 files have this value. there might be more to it.
                    {
                        Count = i;
                        break;
                    }

                    Entries[i] = new Entry();
                    Entries[i].LoadfromStreamSP2(br, locations[i], last, ref fid);

                    last = Entries[i];
                }
            }
        }

        protected virtual void InitTextures<T>(ArchiveBase aw = null) where T : Texture_Base, new()
        {
            int count = (int)Props.Sum(x => x.Count);
            if (Textures == null)
                Textures = new List<TextureHandler>(count);
            if (Textures.Count <= 0)
            {
                if (aw == null)
                    aw = ArchiveWorker.Load(ArchiveString);
                T tex;
                Scale = new Dictionary<uint, Vector2>(count);
                int b = 0;
                for (int j = 0; j < Props.Count; j++)
                    for (uint i = 0; i < Props[j].Count; i++)
                    {
                        tex = new T();
                        byte[] buffer = aw.GetBinaryFile(string.Format(Props[j].Filename, i + TextureStartOffset));
                        if (buffer != null)
                        {
                            tex.Load(buffer);

                            if (Props[j].Big != null && ForceOriginal == false && b < Props[j].Big.Count)
                            {
                                TextureHandler th = TextureHandler.Create(Props[j].Big[b].Filename, tex, 2, Props[j].Big[b++].Split / 2);

                                Textures.Add(th);
                                Scale[i] = Vector2.One;
                            }
                            else
                            {
                                TextureHandler th = TextureHandler.Create(Props[j].Filename, tex);
                                Textures.Add(th);
                                Scale[i] = th.GetScale(); //scale might not be used outside of texturehandler.
                            }
                        }
                    }
            }
        }

        protected virtual void InsertCustomEntries()
        {
        }

        #endregion Methods
    }
}