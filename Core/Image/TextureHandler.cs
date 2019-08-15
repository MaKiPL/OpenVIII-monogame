using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenVIII
{
    /// <summary>
    /// This contains functions to Load Highres mod versions of textures and get scale vector.
    /// </summary>
    public class TextureHandler
    {
        #region Fields

        private Texture_Base _classic;

        #endregion Fields

        #region Constructors

        public TextureHandler(string filename, uint cols) : this(filename, cols, 1)
        {
        }

        public TextureHandler(string filename, uint cols, uint rows) =>
            //if (cols == 1 && rows == 1)
            //{
            //    ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
            //    filename = aw.GetListOfFiles().First(x => x.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0);
            //    Texture_Base tex = new TEX(aw.GetBinaryFile(filename));
            //    Init(filename, tex, cols, rows);
            //}
            //else
            Init(filename, null, cols, rows);

        public TextureHandler(string filename, Texture_Base classic, ushort palette = 0, Color[] colors = null) => Init(filename, classic, 1, 1, palette: palette, colors: colors);

        public TextureHandler(string filename, Texture_Base classic, uint cols, uint rows, ushort palette = 0, Color[] colors = null) => Init(filename, classic, cols, rows, palette, colors);

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Original sub 256x265 texture, required for fallback when issues happen.
        /// </summary>
        public Texture_Base Classic { get => _classic; private set { _classic = value; if (value != null) ClassicSize = new Vector2(value.GetWidth, value.GetHeight); } }

        /// <summary>
        /// X = width and Y = height. The Size of original texture. Will be used in scaling
        /// </summary>
        public Vector2 ClassicSize { get; private set; }

        public Color[] Colors { get; private set; }
        public uint Count { get; protected set; }
        public bool Modded { get; private set; } = false;
        public ushort Palette { get; protected set; }

        /// <summary>
        /// Scale vector from big to original
        /// </summary>
        public Vector2 ReverseScaleFactor => Size == Vector2.Zero || ClassicSize == Vector2.Zero ? Vector2.One : ClassicSize / Size;

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

        protected uint Rows { get; set; }

        protected uint StartOffset { get; set; }

        protected Texture2D[,] Textures { get; private set; }

        #endregion Properties

        #region Indexers

        public Texture2D this[int c, int r] => Textures[c, r];

        #endregion Indexers

        #region Methods

        public static Vector2 Abs(Vector2 v) => new Vector2(Math.Abs(v.X), Math.Abs(v.Y));

        public static explicit operator Texture2D(TextureHandler t)
        {
            if (t.Count == 1)
                return t[0, 0];
            throw new Exception("TextureHandler can only be cast to Texture2D if there is only one texture in the array use [cols,rows] instead");
            //return null;
        }

        public static Vector2 GetOffset(Rectangle old, Rectangle @new) => GetOffset(old.Location.ToVector2(), @new.Location.ToVector2());

        public static Vector2 GetOffset(Point oldLoc, Point newLoc) => GetOffset(oldLoc.ToVector2(), newLoc.ToVector2());

        public static Vector2 GetOffset(Vector2 oldLoc, Vector2 newLoc) => Abs(oldLoc - newLoc);

        public static Vector2 GetScale(Vector2 _old, Vector2 _new)
        {
            if (_new.Y == 0 && _new.X != 0)
                return new Vector2(_new.X / _old.X);
            else if (_new.Y != 0 && _new.X == 0)
                return new Vector2(_new.Y / _old.Y);
            else if (_new.Y == 0 && _new.X == 0)
                return Vector2.One;
            return _new / _old;
        }

        public static Vector2 GetScale(Vector2 _old, Texture2D _new) => new Vector2(_new.Width / _old.X, _new.Height / _old.Y);

        public static Vector2 GetScale(Texture_Base _old, Texture2D _new) => new Vector2((float)_new.Width / _old.GetWidth, (float)_new.Height / _old.GetHeight);

        public static Vector2 GetScale(Texture2D _old, Texture2D _new) => new Vector2((float)_new.Width / _old.Width, (float)_new.Height / _old.Height);

        public static Vector2 GetScale(Point oldSize, Point newSize) => GetScale(oldSize.ToVector2(), newSize.ToVector2());

        public static implicit operator Rectangle(TextureHandler v) => new Rectangle(new Point(0), v.Size.ToPoint());

        /// <summary>
        /// Load Texture from a mod
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D LoadPNG(string path, int palette = -1)
        {
            string bn = Path.GetFileNameWithoutExtension(path);
            string prefix = bn.Substring(0, 2);
            string pngpath = Path.Combine(Memory.FF8DIR, "textures", prefix, bn);
            // this isn't working correctly unless mod authors have the this-> 13=0, 14=1... for palettes
            //https://github.com/MaKiPL/OpenVIII/issues/73
            string suffix = palette > -1 ? $"{palette + 13}" : "";
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
            mat1.Location = (mat1.Location.ToVector2() * mat2).RoundedPoint();
            mat1.Size = (mat1.Size.ToVector2() * mat2).RoundedPoint();
            return mat1;
        }

        public static Rectangle ToRectangle(Texture2D t) => new Rectangle(0, 0, t.Width, t.Height);

        public static Rectangle ToRectangle(TextureHandler t) => new Rectangle(0, 0, (int)t.ClassicSize.X, (int)t.ClassicSize.Y);

        public static Rectangle ToRectangle(Vector2 loc, Vector2 size) => new Rectangle(loc.ToPoint(), size.ToPoint());

        public static Vector2 ToVector2(Texture2D t) => new Vector2(t.Width, t.Height);

        public static Vector2 ToVector2(TextureHandler t) => new Vector2(t.ClassicSize.X, t.ClassicSize.Y);

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

        public static Texture2D UseBest(Texture_Base _old, Texture2D _new, ushort palette = 0, Color[] colors = null) => UseBest(_old, _new, out Vector2 scale, palette, colors);

        public static Texture2D UseBest(Texture_Base _old, Texture2D _new, out Vector2 scale, ushort palette = 0, Color[] colors = null)
        {
            Texture2D tex;
            if (_new == null)
            {
                scale = Vector2.One;
                if (_old.GetClutCount <= 1)
                    return _old.GetTexture();
                tex = colors != null ? _old.GetTexture(colors) : _old.GetTexture(palette);
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
            if (src != null)
            {
                _Draw(dst, src.Value, color);
                return;
            }
            //drawing texture directly
            else
            {
                _Draw(dst, color);
            }
        }

        public Vector2 GetScale(int cols = 0, int rows = 0) => ScaleFactor;

        public void Merge()
        {
            if (Rows * Cols > 1)
            {
                if (Memory.IsMainThread)
                {
                    int width = 0;
                    int height = 0;
                    for (int r = 0; r < (int)Rows; r++)
                    {
                        int rowwidth = 0;
                        int rowheight = 0;
                        for (int c = 0; c < (int)Cols; c++)
                        {
                            rowwidth += Textures[c, r].Width;
                            if (rowheight < Textures[c, r].Height)
                                rowheight = Textures[c, r].Height;
                        }
                        if (width < rowwidth)
                            width = rowwidth;
                        height += rowheight;
                    }

                    Texture2D tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color);
                    Rectangle dst = new Rectangle();
                    for (int r = 0; r < (int)Rows; r++)
                    {
                        dst.Y += r > 0 ? Textures[0, r - 1].Height : 0;
                        for (int c = 0; c < (int)Cols; c++)
                        {
                            dst.Height = Textures[c, r].Height;
                            dst.Width = Textures[c, r].Width;
                            dst.X += c > 0 ? Textures[c - 1, r].Width : 0;
                            Color[] buffer = new Color[dst.Height * dst.Width];
                            Textures[c, r].GetData<Color>(buffer);
                            tex.SetData(0, dst, buffer, 0, buffer.Length);
                            //Textures[c, r].Dispose();
                        }
                        dst.X = 0;
                    }
                    foreach (Texture2D t in Textures)
                        t.Dispose();
                    Textures = new Texture2D[1, 1];
                    Textures[0, 0] = tex;
                    Rows = 1;
                    Cols = 1;
                }
                else
                {
                    Memory.MainThreadOnlyActions.Enqueue(this.Merge);
                    //Memory.MainThreadOnlyActions.Enqueue(this.Save);
                }
            }
        }

        public void Save()
        {
            string clean = Regex.Replace(Filename, @"{[^}]+}", "");
            string outpath = Path.Combine(Path.GetTempPath(), Path.GetFileName(clean) + $"{Palette}.png");
            using (FileStream fs = File.Create(outpath))
                Textures[0, 0].SaveAsPng(fs, Textures[0, 0].Width, Textures[0, 0].Height);
        }

        /// <summary>
        /// Remove all transparenct rows and cols of pixels
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public Rectangle Trim(Rectangle src)
        {
            List<Tuple<Texture2D, Rectangle, Rectangle>> ret = _process(Rectangle.Empty, src);

            Rectangle rect = Rectangle.Empty;
            foreach (Tuple<Texture2D, Rectangle, Rectangle> r in ret)
            {
                Rectangle tmp = _Trim_SingleTexture(r.Item1, r.Item3);
                if (rect == Rectangle.Empty)
                    rect = tmp;
                else
                    Rectangle.Union(rect, tmp);
            }
            return rect;
        }

        protected void Init()
        {
            Vector2 size = Vector2.Zero;
            Vector2 oldsize = Vector2.Zero;
            Texture_Base tex = null;
            uint c2 = 0;
            uint r2 = 0;
            for (uint r = 0; r < Rows; r++)
            {
                for (uint c = 0; c < Cols && Memory.graphics.GraphicsDevice != null; c++)
                {
                    ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
                    string path = aw.GetListOfFiles().First(x => (x.IndexOf(string.Format(Filename, c + r * Cols + StartOffset), StringComparison.OrdinalIgnoreCase) >= 0));
                    tex = Texture_Base.Open(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, path));
                    if (Classic == null && c2 < Cols) oldsize.X += tex.GetWidth;
                    Texture2D pngTex = LoadPNG(path, Palette);
                    Textures[c, r] = (UseBest(tex, pngTex, Palette, Colors));
                    if (pngTex != null) Modded = true;
                    if (c2 < Cols && Textures[c2, r2] != null) size.X += Textures[c2++, r2].Width;
                }
                if (Classic == null && r2 < Rows) oldsize.Y += tex.GetHeight;
                if (r2 < Rows && Textures.LongLength > r2 + c2 - 1 && Textures[c2 - 1, r2] != null) size.Y += Textures[c2 - 1, r2++].Height;
            }
            Size = size;
            if (Classic == null) ClassicSize = oldsize;
        }
        private Func<Rectangle, Rectangle, List<Tuple<Texture2D, Rectangle, Rectangle>>> b;
        private void _Draw(Rectangle dst, Rectangle src, Color color)
        {
            //Func<Rectangle,Rectangle, List<Tuple<Texture2D, Rectangle, Rectangle>>> a = _process;
            if (b == null)
                b = PointEx.Memoize<Rectangle, Rectangle, List<Tuple<Texture2D, Rectangle, Rectangle>>>(_process);

            List<Tuple<Texture2D, Rectangle, Rectangle>> ret = b(dst, src);

            foreach (Tuple<Texture2D, Rectangle, Rectangle> r in ret)
            {
                _Draw(r.Item1, r.Item2, r.Item3, color);
            }
        }

        private Rectangle _Draw(Texture2D tex, Rectangle dst, Rectangle src, Color color)
        {
            Memory.spriteBatch.Draw(tex, dst, src, color);
            return src;
        }

        private void _Draw(Rectangle dst, Color color)
        {
            Vector2 dstOffset = Vector2.Zero;
            Vector2 dstV = Vector2.Zero;
            dstOffset.X = dst.X;
            dstOffset.Y = dst.Y;
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

        /// <summary>
        /// Process the texture with the given varibles.
        /// <para>Only used by trim and draw.</para>
        /// <para>Trim really only needs src varible</para>
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="color"></param>
        /// <param name="single"></param>
        /// <returns></returns>
        private List<Tuple<Texture2D, Rectangle, Rectangle>> _process(
            Rectangle dst, Rectangle src)
        {
            List<Tuple<Texture2D, Rectangle, Rectangle>> ret = new List<Tuple<Texture2D, Rectangle, Rectangle>>(2);
            //Rectangle rect = Rectangle.Empty;

            if (Memory.IsMainThread) // Some code may only work on main thread.
            {
                //all extra code is only used for multiple pcs
                Vector2 dstOffset = Vector2.Zero; // only if Intersects
                Rectangle dst2 = Rectangle.Empty; // only if Intersects
                Vector2 offset = Vector2.Zero;
                Rectangle cnt = Rectangle.Empty;
                for (uint r = 0; r < Rows; r++)
                {
                    bool drawn = false; // only if Intersects
                    offset.X = 0;
                    for (uint c = 0; c < Cols; c++)
                    {
                        //Start: if were always only one texture pcs
                        Rectangle _src = Scale(src, ScaleFactor);
                        cnt = ContainerRectangle(offset, Textures[c, r]);
                        if (cnt.Contains(_src))
                        {
                            _src.Location = (GetOffset(cnt, _src)).ToPoint();
                            //return single(, dst, _src, color);
                            return new List<Tuple<Texture2D, Rectangle, Rectangle>>
                            {
                                new Tuple<Texture2D, Rectangle, Rectangle>(Textures[c, r],dst,_src)
                            };
                        }
                        //End
                        //This part is for if a given src rectangle overlaps >=2 textures
                        else if (cnt.Intersects(_src))
                        {
                            Rectangle src2 = Rectangle.Intersect(cnt, _src);
                            src2.Location = (GetOffset(cnt, src2)).ToPoint();
                            dst2 = Scale(dst, GetScale(_src.Size, src2.Size));
                            dst2.Location = (dst.Location);
                            dst2.Offset(dstOffset);
                            ret.Add(new Tuple<Texture2D, Rectangle, Rectangle>(Textures[c, r], dst2, src2));
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
            else throw new InvalidOperationException($"{this} Must run in main thread.");
            return ret;
        }

        private Rectangle _Trim_SingleTexture(Texture2D tex, Rectangle src)
        {
            Rectangle ret = Rectangle.Empty;
            ret.Offset(-1, -1);
            // storage of colors.
            Color[] tmp = new Color[src.Width * src.Height];
            // colors of all pixels
            tex.GetData<Color>(0, src, tmp, 0, tmp.Length);
            // max x and y values
            int x2 = src.Width;
            int y2 = src.Height;
            // check each pixel's color
            for (int y1 = 0; y1 < y2; y1++)
            {
                for (int x1 = 0; x1 < x2; x1++)
                {
                    Color a = tmp[x1 + y1 * src.Width];
                    if (a.A != 0)
                    {
                        // grab high and low bounds of non transparent pixels.
                        if (ret.Y < 0 || ret.X > x1)
                        {
                            ret.X = x1;
                        }
                        else if (ret.Width == 0 || ret.Width < x1)
                            ret.Width = x1;
                        // do same for Y axis.
                        if (ret.Y < 0 || ret.Y > y1)
                        {
                            ret.Y = y1;
                        }
                        else if (ret.Height == 0 || ret.Height < y1)
                            ret.Height = y1;
                    }
                }
            }
            //using height and width as a bottom and right x/y
            //converting them back to height and width.
            ret.Width -= ret.X;
            ret.Height -= ret.Y;
            ret.Width += 1;
            ret.Height += 1;
            ret.Offset(src.X, src.Y);
            ret = Scale(ret, ReverseScaleFactor);
            src = Scale(src, ReverseScaleFactor);
            return ret;
        }

        private Rectangle ContainerRectangle(Vector2 offset, Texture2D tex)
        {
            Rectangle cnt = ToRectangle(tex);
            cnt.Offset(offset);
            return cnt;
        }

        private void Init(string filename, Texture_Base classic, uint cols, uint rows, ushort palette = 0, Color[] colors = null)
        {
            Classic = classic;
            Size = Vector2.Zero;
            Count = cols * rows;
            Textures = new Texture2D[cols, rows];
            StartOffset = 0;
            Rows = rows;
            Cols = cols;
            Filename = filename;
            Palette = palette;
            Colors = colors;

            //load textures;
            Init();
            //unload Classic
            Classic = null;
            //Merge the texture pieces into one.
            Merge();
        }

        #endregion Methods
    }
}