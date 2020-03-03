using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenVIII
{
    /// <summary>
    /// This contains functions to Load High res mod versions of textures and get scale vector.
    /// </summary>
    public class TextureHandler : IDisposable
    {
        #region Fields

        private static readonly ConcurrentDictionary<string, Texture2D> PngCache = new ConcurrentDictionary<string, Texture2D>();
        private static readonly ConcurrentDictionary<string, TextureHandler> TextureHandlerCache = new ConcurrentDictionary<string, TextureHandler>();
        private static string[] _pngStrings;
        private Texture_Base _classic;

        private bool _disposedValue;

        /// <summary>
        /// If i'm expecting a 256x256 and get a 128x256 pad the pixels with transparent ones.
        /// </summary>
        private bool _enforceSquare;

        #endregion Fields

        #region Properties

        public IEnumerable<Texture2D> AllTexture2Ds => Grid.Select(x => Textures[x.col, x.row]);

        /// <summary>
        /// Original sub 256x265 texture, required for fallback when issues happen.
        /// </summary>
        public Texture_Base Classic { get => _classic; private set { _classic = value; if (value != null) ClassicSize = new Vector2(value.GetWidth, value.GetHeight); } }

        public int ClassicHeight => (int)(ClassicSize == Vector2.Zero ? Size.Y : ClassicSize.Y);

        /// <summary>
        /// X = width and Y = height. The Size of original texture. Will be used in scaling
        /// </summary>
        public Vector2 ClassicSize { get; private set; }

        public int ClassicWidth => (int)(ClassicSize == Vector2.Zero ? Size.X : ClassicSize.X);

        public Color[] Colors { get; private set; }

        public uint Count { get; protected set; }

        public IEnumerable<(int col, int row)> Grid =>
            (from row in Enumerable.Range(0, (int)Rows)
             from col in Enumerable.Range(0, (int)Cols)
             select (col, row));

        public int Height => (int)Size.Y;

        public bool Modded { get; private set; }

        public string ModdedFilename { get; private set; }
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

        public int Width => (int)Size.X;

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

        public static TextureHandler Create(string filename, uint cols) => Create(filename, cols, 1);

        public static TextureHandler Create(string filename, uint cols, uint rows) => Create(filename, null, cols, rows);

        public static TextureHandler Create(string filename, Texture_Base classic, ushort palette = 0, Color[] colors = null) => Create(filename, classic, 1, 1, palette: palette, colors: colors);

        public static TextureHandler Create(string filename, Texture_Base classic, uint cols, uint rows, ushort palette = 0, Color[] colors = null)
        {
            string pngPath = FindPng(filename, palette);
            if (!string.IsNullOrWhiteSpace(pngPath) && TextureHandlerCache.TryGetValue(pngPath, out TextureHandler ret)) return ret;
            ret = new TextureHandler
            {
                ModdedFilename = pngPath,
                Filename = filename,
                Classic = classic,
                Cols = cols,
                Rows = rows,
                Palette = palette,
                Colors = colors
            };
            ret.Init();
            if (ret.Modded && !string.IsNullOrWhiteSpace(ret.ModdedFilename))
            {
                TextureHandlerCache.TryAdd(ret.ModdedFilename, ret);
            }
            return ret;
        }

        public static TextureHandler CreateFromPng(string filename, int classicWidth, int classicHeight, ushort palette, bool enforceSquare, bool forceReload = false)
        {
            string s = FindPng(filename, palette);
            if (string.IsNullOrWhiteSpace(s) || !TextureHandlerCache.TryGetValue(s, out TextureHandler ret))
            {
                ret = new TextureHandler
                {
                    _enforceSquare = enforceSquare,
                    ModdedFilename = filename,
                    //Modded = string.IsNullOrWhiteSpace(s),
                    Filename = filename,
                    ClassicSize = new Vector2(classicWidth, classicHeight),
                    Cols = 1,
                    Rows = 1,
                    Palette = palette
                };
                ret.Init();
                if (ret.Modded && !string.IsNullOrWhiteSpace(ret.ModdedFilename))
                {
                    TextureHandlerCache.TryAdd(ret.ModdedFilename, ret);
                }
            }
            else if (forceReload && ret.Modded)
            {
                ret.Reload();
            }

            return ret;
        }

        public static explicit operator Texture2D(TextureHandler t)
        {
            if (t.Count == 0)
                return null;
            if (t.Count == 1)
                return t[0, 0];
            throw new Exception("TextureHandler can only be cast to Texture2D if there is only one texture in the array use [cols,rows] instead");
            //return null;
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static string FindPng(string path, int palette = -1)
        {
            if (File.Exists(path))
                return path;

            string bn = Regex.Escape(Path.GetFileNameWithoutExtension(Regex.Replace(path, @"\{[\d\.\:]+\}", "", RegexOptions.IgnoreCase | RegexOptions.Compiled)));
            string textures = Path.Combine(Memory.FF8DIR, "textures");
            if (Directory.Exists(textures))
            {
                if (_pngStrings == null)
                    _pngStrings = Directory.GetFiles(textures, "*.png", SearchOption.AllDirectories);

                IOrderedEnumerable<string> limited = _pngStrings.Where(x => x.IndexOf(bn, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.InvariantCultureIgnoreCase);
                if (!limited.Any()) return null;

                Regex re1 = new Regex(@".+[\\/]+" + bn + @"_(\d{1,2})\.png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex re2 = new Regex(@".+[\\/]+" + bn + @"\.png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                IEnumerable<Match> matches = (limited.Select(x => new { x, m1 = re1.Match(x) })
                    .Select(t => new { t, m2 = re2.Match(t.x) })
                    .Where(t => (t.t.m1.Success || t.m2.Success))
                    .OrderByDescending(t => t.t.m1.Success)
                    .Select(t => t.t.m1.Success ? t.t.m1 : t.m2));
                string tex = matches.FirstOrDefault(x =>
                        (x.Groups.Count == 3 && int.TryParse(x.Groups[1].Value, out int p) && p == palette) ||
                        x.Groups.Count == 2)
                    ?.Value;
                //string tex = limited.FirstOrDefault(x => (t = re1.Match(x)).Success && t.Groups.Count > 1 && int.TryParse(t.Groups[1].Value, out int p) && p == palette);
                //if (palette < 0 || (tex = _findPNG($"{bn}+ _{ (palette)}")) == null)
                //    tex = _findPNG(bn);
                //if (tex != null)
                //if (string.IsNullOrWhiteSpace(tex))
                //{
                //    tex = limited.FirstOrDefault(x => (t = re2.Match(x)).Success);
                //}

                return tex;
            }
            return null;
        }

        public static Vector2 GetOffset(Rectangle old, Rectangle @new) => GetOffset(old.Location.ToVector2(), @new.Location.ToVector2());

        public static Vector2 GetOffset(Vector2 oldLoc, Vector2 newLoc) => Abs(oldLoc - newLoc);

        public static Vector2 GetScale(Vector2 old, Vector2 @new)
        {
            if (Math.Abs(@new.Y) < float.Epsilon && Math.Abs(@new.X) > float.Epsilon)
                return new Vector2(@new.X / old.X);
            else if (Math.Abs(@new.Y) > float.Epsilon && Math.Abs(@new.X) < float.Epsilon)
                return new Vector2(@new.Y / old.Y);
            else if (Math.Abs(@new.Y) < float.Epsilon && Math.Abs(@new.X) < float.Epsilon)
                return Vector2.One;
            return @new / old;
        }

        public static Vector2 GetScale(Texture_Base old, Texture2D @new) => new Vector2((float)@new.Width / old.GetWidth, (float)@new.Height / old.GetHeight);

        public static Vector2 GetScale(Point oldSize, Point newSize) => GetScale(oldSize.ToVector2(), newSize.ToVector2());

        public static implicit operator Rectangle(TextureHandler v) => new Rectangle(new Point(0), v.Size.ToPoint());

        /// <summary>
        /// Load Texture from a mod
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static Texture2D LoadPNG(string path, int palette = -1, bool forceSquare = false)
        {
            string pngPath = File.Exists(path) ? path : FindPng(path, palette);
            Texture2D tex = null;
            if (!string.IsNullOrWhiteSpace(pngPath) && !PngCache.TryGetValue(pngPath, out tex))
            {
                using (FileStream fs = new FileStream(pngPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    tex = Texture2D.FromStream(Memory.graphics.GraphicsDevice, fs);
                    PngCache.TryAdd(pngPath, tex);
                }
            }
            if (tex != null && forceSquare && tex.Width != tex.Height)
            {
                int s = Math.Max(tex.Width, tex.Height);
                RenderTarget2D tmp = new RenderTarget2D(Memory.graphics.GraphicsDevice, s, s);
                using (tex)
                {
                    Memory.graphics.GraphicsDevice.SetRenderTarget(tmp);
                    Memory.SpriteBatchStartAlpha();
                    Memory.graphics.GraphicsDevice.Clear(Color.TransparentBlack);
                    Memory.spriteBatch.Draw(tex, new Rectangle(0, 0, tex.Width, tex.Height), Color.White);
                    Memory.SpriteBatchEnd();
                    Memory.graphics.GraphicsDevice.SetRenderTarget(null);
                }
                tex = tmp;
            }
            return tex;
        }

        public static Rectangle Scale(Rectangle src, Vector2 scale)
        {
            src.Location = (src.Location.ToVector2() * scale).ToPoint();//.RoundedPoint();
            src.Size = (src.Size.ToVector2() * scale).ToPoint();//.RoundedPoint();
            return src;
        }

        public static Rectangle ToRectangle(Texture2D t) => new Rectangle(0, 0, t?.Width ?? 0, t?.Height ?? 0);

        public static Rectangle ToRectangle(TextureHandler t) => new Rectangle(0, 0, (int)t.ClassicSize.X, (int)t.ClassicSize.Y);

        public static Rectangle ToRectangle(Vector2 loc, Vector2 size) => new Rectangle(loc.ToPoint(), size.ToPoint());

        public static Vector2 ToVector2(Texture2D t) => t != null ? new Vector2(t.Width, t.Height) : Vector2.Zero;

        public static Texture2D UseBest(Texture_Base old, Texture2D @new, ushort palette = 0, Color[] colors = null) => UseBest(old, @new, out Vector2 _, palette, colors);

        public static Texture2D UseBest(Texture_Base old, Texture2D @new, out Vector2 scale, ushort palette = 0, Color[] colors = null)
        {
            if (@new == null && old != null)
            {
                scale = Vector2.One;
                if (old.GetClutCount <= 1)
                    return old.GetTexture();
                Texture2D tex = colors != null ? old.GetTexture(colors) : old.GetTexture(palette);
                return tex;
            }
            else
            {
                scale = old != null ? GetScale(old, @new) : Vector2.Zero;
                return @new;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

        public void Draw(Rectangle dst, Rectangle? src, Color color)
        {
            if (src != null)
            {
                _Draw(dst, src.Value, color);
            }
            //drawing texture directly
            else
            {
                _Draw(dst, color);
            }
        }

        public void Draw(Rectangle dst, Color color) => Draw(dst, null, color);

        public void Draw(Rectangle dst, Color color, float rotation, Vector2 origin, SpriteEffects effects, float depth)
        {
            if (Rows == 1 && Cols == 1)
                Memory.spriteBatch.Draw(Textures[0, 0], dst, null, color, rotation, origin, effects, depth);
            else
            {
                throw new Exception("had not coded this to draw from multiple textures");
            }
        }

        public Vector2 GetScale(int cols = 0, int rows = 0) => ScaleFactor;

        public void Merge()
        {
            if (Rows * Cols <= 1 || Textures == null || Textures.Length < Rows * Cols) return;
            if (Memory.IsMainThread)
            {
                int width = 0;
                int height = 0;
                if (!(AllTexture2Ds.Any(x => x == null)))
                {
                    for (int r = 0; r < (int)Rows; r++)
                    {
                        int rowWidth = 0;
                        int rowHeight = 0;
                        for (int c = 0; c < (int)Cols && Textures[c, r] != null; c++)
                        {
                            rowWidth += Textures[c, r].Width;
                            if (rowHeight < Textures[c, r].Height)
                                rowHeight = Textures[c, r].Height;
                        }
                        if (width < rowWidth)
                            width = rowWidth;
                        height += rowHeight;
                    }
                }

                if (width == 0 || height == 0)
                {
                    Rows = 0;
                    Cols = 0;
                    Count = 0;
                    Textures = null;
                    return;
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
                        Textures[c, r].GetData(buffer);
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
                Count = 1;
            }
            else
            {
                Memory.MainThreadOnlyActions.Enqueue(this.Merge);
            }
        }

        public void Save() => Save("", true);

        public void Save(string outPath, bool replace)
        {
            string clean = Path.GetFileNameWithoutExtension(Regex.Replace(Filename, @"{[^}]+}", ""));
            clean = $"{Path.GetFileName(clean)}_{(Palette).ToString()}.png";
            outPath = Path.Combine(string.IsNullOrWhiteSpace(outPath) ? Path.GetTempPath() : outPath, clean);
            if ((!File.Exists(outPath) || replace)
            && (Textures != null && Textures.Length > 0 && Textures[0, 0] != null))
                using (FileStream fs = File.Create(outPath))
                    Textures[0, 0].SaveAsPng(fs, Textures[0, 0].Width, Textures[0, 0].Height);
            else
                Debug.WriteLine($"{this} :: Textures is null or empty! :: {Filename}");
        }

        /// <summary>
        /// Remove all transparent rows and cols of pixels
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public Rectangle Trim(Rectangle src) => _process(Rectangle.Empty, src, Color.TransparentBlack, _Trim_SingleTexture);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }
            if (!string.IsNullOrWhiteSpace(ModdedFilename) && PngCache.TryRemove(ModdedFilename, out Texture2D tex))
            {
                if (!tex.IsDisposed)
                {
                    tex.Dispose();
                }
            }
            if (!string.IsNullOrWhiteSpace(Filename) && TextureHandlerCache.TryRemove(Filename, out TextureHandler textureHandler))
            {
                foreach (Texture2D t in textureHandler.Textures)
                {
                    if (!t.IsDisposed)
                    {
                        t.Dispose();
                    }
                }
            }
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            // TODO need an easy way to remove old textures from cache.

            _disposedValue = true;
        }

        protected void Process()
        {
            if (Memory.graphics?.GraphicsDevice == null) return;
            Vector2 size = Vector2.Zero;
            Vector2 oldSize = Vector2.Zero;
            Texture_Base tex = null;
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_MENU); // TODO remove this should be done outside of texture handler.
            string[] listOfFiles = aw.GetListOfFiles(); // TODO remove this.
            uint c2 = 0;
            uint r2 = 0;
            uint total = Rows * Cols;
            for (uint r = 0; r < Rows; r++)
            {
                for (uint c = 0; c < Cols; c++)
                {
                    Texture2D pngTex;
                    string path = "";
                    if (listOfFiles != null)
                    {
                        string value = string.Format(Filename, c + r * Cols + StartOffset);
                        path = listOfFiles.FirstOrDefault(x => (x.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0));
                    }

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        tex = Texture_Base.Open(aw.GetBinaryFile(path));
                        if (Classic == null && c2 < Cols) oldSize.X += tex?.GetWidth ?? ClassicWidth;
                        pngTex = LoadPNG(path, Palette, _enforceSquare);
                    }
                    else
                    {
                        pngTex = !string.IsNullOrWhiteSpace(ModdedFilename) ? LoadPNG(ModdedFilename, Palette, _enforceSquare) : LoadPNG(Filename, Palette, _enforceSquare);
                    }

                    if (tex == null && total == 1) tex = Classic;
                    Textures[c, r] = (UseBest(tex, pngTex, Palette, Colors));
                    if (pngTex != null) Modded = true;
                    if (c2 < Cols && Textures[c2, r2] != null) size.X += Textures[c2++, r2].Width;
                }
                if (Classic == null && r2 < Rows) oldSize.Y += tex?.GetHeight ?? ClassicHeight;
                if (r2 < Rows && Textures.LongLength > r2 + c2 - 1 && Textures[c2 - 1, r2] != null) size.Y += Textures[c2 - 1, r2++].Height;
            }
            Size = size;
            if (Classic == null && ClassicSize == Vector2.Zero) ClassicSize = oldSize;
        }

        private void _Draw(Rectangle dst, Rectangle src, Color color)
        => _process(dst, src, color, _Draw);

        private static Rectangle _Draw(Texture2D tex, Rectangle dst, Rectangle src, Color color)
        {
            Memory.spriteBatch.Draw(tex, dst, src, color);
            return src;
        }

        private void _Draw(Rectangle dst, Color color)
        {
            if (Rows == 1 && Cols == 1 && dst.Height > 0 && dst.Width > 0)
                Memory.spriteBatch.Draw(Textures[0, 0], dst, color);
            else
            {
                //throw new Exception($"{this}::code broken for multiple pcs. I think");
                Vector2 dstOffset = Vector2.Zero;
                Vector2 dstV = Vector2.Zero;
                dstOffset.X = dst.X;
                dstOffset.Y = dst.Y;
                for (uint r = 0; r < Rows; r++)
                {
                    for (uint c = 0; c < Cols; c++)
                    {
                        Vector2 scale = GetScale(Size, dst.Size.ToVector2());
                        Texture2D texture = Textures[c, r];
                        if (texture != null)
                        {
                            dstV = ToVector2(texture) * scale;
                            Memory.spriteBatch.Draw(texture, dstOffset, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                        }

                        dstOffset.X += dstV.X;
                    }
                    dstOffset.Y += dstV.Y;
                }
            }
        }

        /// <summary>
        /// Process the texture with the given variables.
        /// <para>Only used by trim and draw.</para>
        /// <para>Trim really only needs src variable</para>
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        /// <param name="color"></param>
        /// <param name="single"></param>
        /// <returns></returns>
        private Rectangle _process(
            Rectangle dst, Rectangle src, Color color,
            Func<Texture2D, Rectangle, Rectangle, Color, Rectangle> single)
        {
            Rectangle ret = Rectangle.Empty;

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
                        Rectangle source2 = Scale(src, ScaleFactor);
                        cnt = ContainerRectangle(offset, Textures[c, r]);
                        if (cnt.Contains(source2))
                        {
                            source2.Location = (GetOffset(cnt, source2)).ToPoint();
                            return single(Textures[c, r], dst, source2, color);
                        }
                        //End
                        //This part is for if a given src rectangle overlaps >=2 textures
                        else if (cnt.Intersects(source2))
                        {
                            Rectangle src2 = Rectangle.Intersect(cnt, source2);
                            src2.Location = (GetOffset(cnt, src2)).ToPoint();
                            dst2 = Scale(dst, GetScale(source2.Size, src2.Size));
                            dst2.Location = (dst.Location);
                            dst2.Offset(dstOffset);
                            if (ret == Rectangle.Empty)
                                ret = single(Textures[c, r], dst2, src2, color);
                            else
                                Rectangle.Union(ret, single(Textures[c, r], dst2, src2, color));
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

        private Rectangle _Trim_SingleTexture(Texture2D tex, Rectangle dst, Rectangle src, Color color)
        {
            Rectangle ret = Rectangle.Empty;
            ret.Offset(-1, -1);
            // storage of colors.
            Color[] tmp = new Color[src.Width * src.Height];
            // colors of all pixels
            tex.GetData(0, src, tmp, 0, tmp.Length);
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
            //src = Scale(src, ReverseScaleFactor);
            return ret;
        }

        private Rectangle ContainerRectangle(Vector2 offset, Texture2D tex)
        {
            Rectangle cnt = ToRectangle(tex);
            cnt.Offset(offset);
            return cnt;
        }

        private void Init()
        {
            Size = Vector2.Zero;
            Count = Cols * Rows;
            Textures = new Texture2D[Cols, Rows];
            StartOffset = 0;

            //load textures;
            Process();
            //unload Classic
            Classic = null;
            //Merge the texture pieces into one.
            Merge();
            if (!Modded && Memory.EnableDumpingData)
                Memory.MainThreadOnlyActions.Enqueue(this.Save);
            //if(ScaleFactor.X > ScaleFactor.Y && ScaleFactor.X/ScaleFactor.Y == 2f)
            //{
            //    var t = new Texture2D(Memory.graphics.GraphicsDevice, (int)(ClassicWidth * ScaleFactor.Y), Height);
            //    var c = new Color[t.Width * t.Height];
            //    Textures[0, 0].GetData(0, new Rectangle(0, 0, t.Width, t.Height), c, 0, c.Length);
            //    t.SetData(c);
            //    Textures[0, 0] = t;
            //    Memory.MainThreadOnlyActions.Enqueue(this.Save);
            //}
        }

        private void Reload()
        {
            if (Rows * Cols == 1)
            {
                Textures[0, 0].Dispose();
                if (PngCache.TryRemove(ModdedFilename, out Texture2D value) && !value.IsDisposed)
                    value.Dispose();
                Textures[0, 0] = LoadPNG(ModdedFilename, Palette, _enforceSquare);
            }
            else
                throw new Exception("too many textures reload not setup for >1 texture");
        }

        #endregion Methods

        // To detect redundant calls
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TextureHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }
}