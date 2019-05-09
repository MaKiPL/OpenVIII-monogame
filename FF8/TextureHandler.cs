using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

namespace FF8
{
    /// <summary>
    /// This contains functions to Load Highres mod versions of textures and get scale vector.
    /// </summary>
    internal class TextureHandler
    {
        #region Fields

        private TEX _classic;

        #endregion Fields

        #region Constructors
        public bool Modded { get; private set; } = false;
        public TextureHandler(string filename, uint cols = 1, uint rows = 1, int pallet = -1)
        {
            if (cols == 1 && rows == 1)
            {
                ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                filename = aw.GetListOfFiles().First(x => x.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0);
                TEX tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, filename));
                Init(filename, tex, cols, rows);
            }
            else
                Init(filename, null, cols, rows);
        }

        public TextureHandler(string filename, TEX classic, uint cols = 1, uint rows = 1, int pallet = -1) => Init(filename, classic, cols, rows, pallet);

        public void Init(string filename, TEX classic, uint cols = 1, uint rows = 1, int pallet = -1)
        {
            Classic = classic;
            Size = Vector2.Zero;
            Count = cols * rows;
            Textures = new Texture2D[cols, rows];
            StartOffset = 0;
            Rows = rows;
            Cols = cols;
            Filename = filename;
            Pallet = pallet;

            //load textures;
            Init();
            //unload Classic
            Classic = null;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Original sub 256x265 texture, required for fallback when issues happen.
        /// </summary>
        public TEX Classic { get => _classic; private set { _classic = value; if (value != null) ClassicSize = new Vector2(value.TextureData.Width, value.TextureData.Height); } }

        /// <summary>
        /// X = width and Y = height. The Size of original texture. Will be used in scaling
        /// </summary>
        public Vector2 ClassicSize { get; private set; }

        public uint Count { get; protected set; }

        /// <summary>
        /// Scale vector from original to big
        /// </summary>
        public Vector2 ScaleFactor => Size == Vector2.Zero || ClassicSize == Vector2.Zero ? Vector2.One : Size / ClassicSize;

        /// <summary>
        /// X = width and Y = height. The Size of big version texture. Will be used in scaling
        /// </summary>
        public Vector2 Size { get; private set; }

        protected uint Cols { get; set; }
        protected string Filename { get; set; }
        public int Pallet { get; protected set; }
        protected uint Rows { get; set; }

        /// <summary>
        /// Scale vector big to modded or orignal to modded.
        /// </summary>
        //protected Vector2[,] Scales { get; private set; }

        protected uint StartOffset { get; set; }
        protected Texture2D[,] Textures { get; private set; }

        #endregion Properties

        #region Indexers

        public Texture2D this[int c, int r] => Textures[c, r];

        #endregion Indexers

        #region Methods

        public static explicit operator Texture2D(TextureHandler t)
        {
            if (t.Count == 1)
                return t[0, 0];
            throw new Exception("TextureHandler can only be cast to Texture2D if there is only one texture in the array use [cols,rows] instead");
            //return null;
        }

        public static implicit operator Rectangle(TextureHandler v) => new Rectangle(new Point(0), v.Size.ToPoint());

        public static Vector2 GetScale(Vector2 _old, Vector2 _new) => _new / _old;

        public static Vector2 GetScale(Vector2 _old, Texture2D _new) => new Vector2(_new.Width / _old.X, _new.Height / _old.Y);

        public static Vector2 GetScale(TEX _old, Texture2D _new) => new Vector2((float)_new.Width / _old.TextureData.Width, (float)_new.Height / _old.TextureData.Height);

        public static Vector2 GetScale(Texture2D _old, Texture2D _new) => new Vector2((float)_new.Width / _old.Width, (float)_new.Height / _old.Height);

        /// <summary>
        /// Load Texture from a mod
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D LoadPNG(string path, int pallet = -1)
        {
            string bn = Path.GetFileNameWithoutExtension(path);
            string prefix = bn.Substring(0, 2);
            string pngpath = Path.Combine(Memory.FF8DIR, "textures", prefix, bn);
            // this isn't working correctly unless mod authors have the this-> 13=0, 14=1... for pallets
            //https://github.com/MaKiPL/OpenVIII/issues/73
            string suffix = pallet > -1 ? $"{pallet + 13}" : "";
            suffix += ".png";
            if (Directory.Exists(pngpath))
            {
                try
                {
                    pngpath = Directory.GetFiles(pngpath).Last(x => 
                    (x.IndexOf(bn, StringComparison.OrdinalIgnoreCase) >= 0 && 
                    x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)));
                    using (FileStream fs = File.OpenRead(pngpath))
                    {
                        return Texture2D.FromStream(Memory.graphics.GraphicsDevice, fs);
                    }
                }
                catch (InvalidOperationException)
                {
                    // couldn't find a match.
                }
            }
            return null;
        }

        public static Rectangle Scale(Rectangle mat1, Vector2 mat2)
        {
            mat1.Location = (mat1.Location.ToVector2() * mat2).ToPoint();
            mat1.Size = (mat1.Size.ToVector2() * mat2).ToPoint();
            return mat1;
        }

        public static Vector2 ToVector2(Texture2D t) => new Vector2(t.Width, t.Height);

        public static Vector2 ToVector2(TextureHandler t) => new Vector2(t.ClassicSize.X, t.ClassicSize.Y);

        public static Rectangle ToRectangle(Texture2D t) => new Rectangle(0, 0, t.Width, t.Height);

        public static Rectangle ToRectangle(TextureHandler t) => new Rectangle(0, 0, (int)t.ClassicSize.X, (int)t.ClassicSize.Y);

        public static Rectangle ToRectangle(Vector2 loc, Vector2 size) => new Rectangle(loc.ToPoint(), size.ToPoint());

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
            Texture2D tex;
            if (_new == null)
            {
                scale = Vector2.One;
                if (_old.TextureData.NumOfPalettes <= 1)
                    return _old.GetTexture();
                tex = _old.GetTexture(pallet);
                return tex;
            }
            else
            {
                scale = GetScale(_old, _new);
                return _new;
            }
        }

        public void Draw(Rectangle dst, Rectangle? src, Color color)
        {
            Vector2 dstOffset = Vector2.Zero;
            if (src != null)
            {
                bool drawn = false;
                Vector2 offset = Vector2.Zero;
                Rectangle dst2 = new Rectangle();
                Rectangle cnt = new Rectangle();
                for (uint r = 0; r < Rows; r++)
                {
                    offset.X = 0;
                    //dstOffset.X = 0;
                    for (uint c = 0; c < Cols; c++)
                    {
                        drawn = false;
                        dst2 = new Rectangle();
                        //if all the pieces of Scales are correct they should all have the same scale.
                        Rectangle _src = Scale(src.Value, ScaleFactor);
                        cnt = ToRectangle(Textures[c, r]);
                        cnt.Offset(offset);
                        if (cnt.Contains(_src))
                        {
                            //got lucky the whole thing is in this rectangle
                            _src.Location = (GetOffset(cnt, _src)).ToPoint();
                            Memory.spriteBatch.Draw(Textures[c, r], dst,_src, color);
                            return;
                        }
                        else if (cnt.Intersects(_src))
                        {
                            //Gotta draw more than once.
                            Rectangle src2 = Rectangle.Intersect(cnt, _src);
                            src2.Location = (GetOffset(cnt, src2)).ToPoint();
                            dst2 = Scale(dst, GetScale(_src.Size, src2.Size));
                            dst2.Location = (dst.Location);
                            dst2.Offset(dstOffset);
                            Memory.spriteBatch.Draw(Textures[c, r], dst2, src2, color);
                            drawn = true;
                            dstOffset.X += dst2.Width;
                        }
                        offset.X += cnt.Width;
                    }
                    offset.Y += cnt.Height;
                    if (drawn)
                        dstOffset.Y += dst2.Height;
                }
            }
            //drawing texture directly
            else
            {
                Vector2 dstV = Vector2.Zero;
                dstOffset.X += dst.X;
                dstOffset.Y += dst.X;
                for (uint r = 0; r < Rows; r++)
                {
                    for (uint c = 0; c < Cols; c++)
                    {
                        Vector2 scale = GetScale(Size, dst.Size.ToVector2());
                        dstV = ToVector2(Textures[c, r]) * scale;
                        Memory.spriteBatch.Draw(Textures[c, r], dstOffset, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                        dstOffset.X += dstV.X;
                    }
                    dstOffset.Y += dstV.Y;
                }
            }
        }

        public Vector2 GetScale(int cols = 0, int rows = 0) => ScaleFactor;

        protected void Init()
        {
            Vector2 size = Vector2.Zero;
            Vector2 oldsize = Vector2.Zero;
            TEX tex = null;
            uint c2 = 0;
            uint r2 = 0;
            for (uint r = 0; r < Rows; r++)
            {
                for (uint c = 0; c < Cols; c++)
                {
                    ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                    string path = aw.GetListOfFiles().First(x => (x.IndexOf(string.Format(Filename, c + r * Cols + StartOffset), StringComparison.OrdinalIgnoreCase) >= 0));
                    tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, path));
                    if (Classic == null && c2 < Cols) oldsize.X += tex.TextureData.Width;
                    Texture2D pngTex = LoadPNG(path, Pallet);
                    Textures[c, r] = (UseBest(tex, pngTex, Pallet));
                    if (pngTex != null) Modded = true;
                    if (c2 < Cols) size.X += Textures[c2++, r2].Width;
                }
                if (Classic == null && r2 < Rows) oldsize.Y += tex.TextureData.Height;
                if (r2 < Rows) size.Y += Textures[c2 - 1, r2++].Height;
            }
            Size = size;
            if (Classic == null) ClassicSize = oldsize;
        }

        public static Vector2 GetOffset(Rectangle old, Rectangle @new) => GetOffset(old.Location.ToVector2(), @new.Location.ToVector2());

        public static Vector2 GetOffset(Point oldLoc, Point newLoc) => GetOffset(oldLoc.ToVector2(), newLoc.ToVector2());
        public static Vector2 Abs(Vector2 v) => new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
        public static Vector2 GetOffset(Vector2 oldLoc, Vector2 newLoc) => Abs(oldLoc-newLoc);

        public static Vector2 GetScale(Point oldSize, Point newSize) => GetScale(oldSize.ToVector2(), newSize.ToVector2());

        #endregion Methods
    }
}