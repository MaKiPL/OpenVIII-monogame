using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Background Tiles for field
    /// </summary>
    /// <see cref="https://github.com/myst6re/deling/blob/master/files/BackgroundFile.cpp"/>
    /// <seealso cref="https://github.com/myst6re/deling/blob/master/files/BackgroundFile.h"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_MAP"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_MIM"/>
    /// <seealso cref="http://forums.qhimm.com/index.php?topic=13444.msg264595#msg264595"/>
    /// <seealso cref="http://forums.qhimm.com/index.php?topic=13444.0"/>
    public partial class Background : IDisposable
    {
        #region Fields

        private const int BytesPerPalette = 2 * ColorsPerPalette;

        private const int ColorsPerPalette = 256;

        /// <summary>
        /// 4 bit has 2 columns per every byte so it expands to twice the width.
        /// </summary>
        private const int FourBitTexturePageWidth = 2 * TexturePageWidth;

        /// <summary>
        /// Standard texture page width.
        /// </summary>
        private const int TexturePageWidth = 128;

        private Dictionary<byte, List<TileQuadTexture>> _animations;

        private AlphaTestEffect _ate;

        private Vector3 _camPosition;

        private Vector3 _camTarget;

        /// <summary>
        /// Palettes/Color Lookup Tables
        /// </summary>
        private Cluts _cluts;

        public float Degrees;
        private bool _disposedValue;

        private BasicEffect _effect;

        private FPS_Camera _fpsCamera;
        private Rectangle _outputDims;

        private Matrix _projectionMatrix, _viewMatrix, _worldMatrix;

        private List<TileQuadTexture> _quads;

        private Dictionary<byte, TextureHandler> _textureIDs;

        private Dictionary<TextureIDPaletteID, TextureHandler> _textureIDsPalettes;

        private ConcurrentDictionary<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<byte,
                ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>>>
            _textures;

        private BackgroundTextureType _textureType;

        #endregion Fields

        #region Destructors

        // To detect redundant calls
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Background()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        #endregion Destructors

        #region Properties

        public TimeSpan CurrentTime { get; set; }
        public Tiles GetTiles { get; private set; }

        public bool HasSpriteBatchTexturesLoaded => GetTexturesReadyToDrawClassicSpriteBatch()?.Count > 0;
        public int Height { get => _outputDims.Height; private set => _outputDims.Height = value; }
        /*
                public bool Is4Bit => GetTiles?.Any(x => x.Is4Bit) ?? false;

                public bool IsAddBlendMode => GetTiles?.Any(x => x.BlendMode == BlendMode.add) ?? false;
                public bool IsHalfBlendMode => GetTiles?.Any(x => x.BlendMode == BlendMode.halfadd) ?? false;
                public bool IsQuarterBlendMode => GetTiles?.Any(x => x.BlendMode == BlendMode.quarteradd) ?? false;
                public bool IsSubtractBlendMode => GetTiles?.Any(x => x.BlendMode == BlendMode.subtract) ?? false;
                */
        public Vector3 MouseLocation { get; set; }
        public TimeSpan TotalTime { get; set; }
        public int Width { get => _outputDims.Width; private set => _outputDims.Width = value; }

        #endregion Properties

        #region Methods

        public static Background Load(byte[] mim, byte[] map)
        {
            if (mim == null || map == null)
                return null;
            var r = new Background
            {
                _textureType = BackgroundTextureType.GetTextureType(mim),
                _camTarget = Vector3.Zero,
                _camPosition = new Vector3(0f, 0f, -10f),
                _fpsCamera = new FPS_Camera(),
                Degrees = 90f
            };
            if (Memory.Graphics != null)
            {
                r._ate = new AlphaTestEffect(Memory.Graphics.GraphicsDevice);
                r._effect = new BasicEffect(Memory.Graphics.GraphicsDevice);
            }
            r._worldMatrix = Matrix.CreateWorld(r._camPosition, Vector3.
                          Forward, Vector3.Up);
            r._viewMatrix = Matrix.CreateLookAt(r._camPosition, r._camTarget,
                     Vector3.Up);
            r.LoadTiles(map);
            r.LoadPalettes(mim);
            r.DumpRawTexture(mim);
            var watch = Stopwatch.StartNew();
            try
            {
                if (!r.ParseBackgroundQuads(mim))
                {
                    return null;
                }
            }
            finally
            {
                watch.Stop();
                Debug.WriteLine($"{nameof(ParseBackgroundQuads)} took {watch.ElapsedMilliseconds / 1000f} seconds.");
            }
            try
            {
                if (!r.ParseBackgroundClassicSpriteBatch(mim))
                {
                    return null;
                }
            }
            finally
            {
                watch.Stop();
                Debug.WriteLine($"{nameof(ParseBackgroundClassicSpriteBatch)} took {watch.ElapsedMilliseconds / 1000f} seconds.");
            }
            return r;
        }

        public void Deswizzle()
        {
            using (var mask = new Texture2D(Memory.Graphics.GraphicsDevice, 4, 4))
            {
                mask.SetData(Enumerable.Range(0, 16).Select(x => Color.White).ToArray());
                var fieldName = Module.GetFieldName();
                var folder = Module.GetFolder(fieldName, "deswizzle");
                var scale = _quads[0].Texture.ScaleFactor;
                var tilesWidth = (int)(GetTiles.Width * scale.X);
                var tilesHeight = (int)(GetTiles.Height * scale.Y);
                //Matrix backup = projectionMatrix;
                //projectionMatrix = Matrix.CreateOrthographic(tiles.Width, tiles.Height, 0f, 100f);
                GetTiles.UniquePupuIDs();// make sure each layer has it's own ID.
                foreach (var pupuGroup in _quads.GroupBy(x => x.GetTile.PupuID)
                ) //group the quads by their pupu ID.
                {
                    using (var outTex = new RenderTarget2D(Memory.Graphics.GraphicsDevice, tilesWidth, tilesHeight))
                    {
                        //start drawing
                        Memory.Graphics.GraphicsDevice.SetRenderTarget(outTex);
                        Memory.Graphics.GraphicsDevice.Clear(Color.TransparentBlack);
                        Memory.SpriteBatchStartAlpha();
                        foreach (var quad in pupuGroup)
                        {
                            var tile = (Tile)quad;
                            //DrawBackgroundQuadsStart();
                            //DrawBackgroundQuad(quad, true);
                            var dst = tile.GetRectangle;
                            dst.Offset(Math.Abs(GetTiles.TopLeft.X), Math.Abs(GetTiles.TopLeft.Y));
                            var src = tile.Source;
                            //src = src.Scale(scale);
                            dst = dst.Scale(scale);
                            quad.Texture.Draw(dst, src, Color.White);
                        }
                        Memory.SpriteBatchEnd();
                        //end drawing
                        Memory.Graphics.GraphicsDevice.SetRenderTarget(null);
                        //set path
                        var path = Path.Combine(folder,
                            $"{fieldName}_{pupuGroup.Key:X8}.png");
                        //save image.
                        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            outTex.SaveAsPng(fs, tilesWidth, tilesHeight);
                        }
                    }
                    using (var outTex = new RenderTarget2D(Memory.Graphics.GraphicsDevice, tilesWidth, tilesHeight))
                    {
                        //start drawing
                        Memory.Graphics.GraphicsDevice.SetRenderTarget(outTex);
                        Memory.Graphics.GraphicsDevice.Clear(Color.Black);
                        Memory.SpriteBatchStartAlpha();
                        var src = new Rectangle(0, 0, 4, 4);
                        foreach (var quad in pupuGroup)
                        {
                            var tile = (Tile)quad;
                            //DrawBackgroundQuadsStart();
                            //DrawBackgroundQuad(quad, true);
                            var dst = tile.GetRectangle;
                            dst.Offset(Math.Abs(GetTiles.TopLeft.X), Math.Abs(GetTiles.TopLeft.Y));
                            //src = src.Scale(scale);
                            dst = dst.Scale(scale);
                            Memory.SpriteBatch.Draw(mask, dst, src, Color.White);
                        }
                        Memory.SpriteBatchEnd();
                        //end drawing
                        Memory.Graphics.GraphicsDevice.SetRenderTarget(null);
                        //set path
                        var path = Path.Combine(folder,
                            $"{fieldName}_{pupuGroup.Key:X8}_MASK.png");
                        //save image.
                        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            outTex.SaveAsPng(fs, tilesWidth, tilesHeight);
                        }
                    }
                }
                Process.Start(folder);
                //projectionMatrix = backup;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

        public void Draw()
        {
            //Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            DrawBackgroundQuads();
            DrawWalkMesh();
            DrawSpriteBatch();
        }

        public void Reswizzle()
        {
            GetTiles.UniquePupuIDs();// make sure each layer has it's own ID.

            var fieldName = Module.GetFieldName();
            var folder = Module.GetFolder(fieldName, "deswizzle"); //goes from deswizzle folder
            if (!Directory.Exists(folder)) return;
            IEnumerable<string> files = Directory.EnumerateFiles(folder, "*.png").ToArray();
            folder = Module.GetFolder(fieldName, "reswizzle");

            var overlap = GetTiles.Select(x => x.TextureID).Distinct().ToDictionary(x => x, x => new HashSet<byte>());
            var texIDs = new ConcurrentDictionary<byte, TextureBuffer>();
            var texIDsPalette = new ConcurrentDictionary<TextureIDPaletteID, TextureBuffer>();
            var width = 0; var height = 0;

            //Vector2 origin = tiles.Origin;
            var lowest = GetTiles.TopLeft;
            var size = new Vector2(GetTiles.Width, GetTiles.Height);//new Point(Math.Abs(lowest.X) + highest.X + Tile.size, Math.Abs(lowest.Y) + highest.Y + Tile.size);
            var re = new Regex(@".+_([0-9A-F]{8}).png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            process();
            if (overlap.Any(x => x.Value.Count > 1))
                process(true);
            void process(bool doOverLap = false)
            {
                foreach (var file in files)
                {
                    //Point highest = new Point(tiles.Max(x => x.X), tiles.Max(x => x.Y));

                    var match = re.Match(file);
                    if (match.Groups.Count > 1 && uint.TryParse(match.Groups[1].Value, NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture, out var pupuID))
                    {
                        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var tex = Texture2D.FromStream(Memory.Graphics.GraphicsDevice, fs))
                        {
                            var scale = new Vector2(tex.Width, tex.Height) / size;
                            width = (int)(256 * scale.X);
                            height = (int)(256 * scale.Y);
                            var inTex = new TextureBuffer(tex.Width, tex.Height);
                            inTex.GetData(tex);

                            foreach (var quad in _quads.Where(x =>
                                x.GetTile.PupuID == pupuID &&
                                (!doOverLap || overlap[x.GetTile.TextureID].Contains(x.GetTile.PaletteID))))
                            {
                                var tile = (Tile)quad;

                                texIDs.TryAdd(tile.TextureID, new TextureBuffer(width, height, false));
                                var src = (new Vector2(Math.Abs(lowest.X) + tile.X, Math.Abs(lowest.Y) + tile.Y) * scale).ToPoint();
                                var dst = (new Vector2(tile.SourceX, tile.SourceY) * scale).ToPoint();

                                if (!doOverLap)
                                    foreach (var p in from x in Enumerable.Range(0, (int)(Tile.Size * scale.X))
                                        from y in Enumerable.Range(0, (int)(Tile.Size * scale.Y))
                                        orderby y, x
                                        select new Point(x, y))
                                    {
                                        var input = inTex[src.X + p.X, src.Y + p.Y];
                                        var current = texIDs[tile.TextureID][dst.X + p.X, dst.Y + p.Y];

                                        var unscaledLocation = tile.Source.Location;
                                        unscaledLocation.Offset(p.ToVector2() / scale);
                                        var output = ChangeColor(current, input, unscaledLocation, tile.TextureID, overlap);
                                        if (!output.HasValue) break;
                                        if (output.Value.A != 0)
                                            texIDs[tile.TextureID][dst.X + p.X, dst.Y + p.Y] = output.Value;
                                        
                                    }
                                else if (overlap[tile.TextureID].Count > 1)
                                {
                                    var key = new TextureIDPaletteID { PaletteID = tile.PaletteID, TextureID = tile.TextureID };
                                    texIDsPalette.TryAdd(key, new TextureBuffer(width, height));

                                    foreach (var p in from x in Enumerable.Range(0, (int)(Tile.Size * scale.X))
                                        from y in Enumerable.Range(0, (int)(Tile.Size * scale.Y))
                                        select new Point(x, y))
                                    {
                                        var input = inTex[src.X + p.X, src.Y + p.Y];
                                        if (input.A != 0)
                                        {
                                            texIDsPalette[key][dst.X + p.X, dst.Y + p.Y] = inTex[src.X + p.X, src.Y + p.Y];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //save new reswizzles
            foreach (var tid in texIDs)
            {
                var path = Path.Combine(folder,
                    $"{fieldName}_{tid.Key}.png");
                //save image.
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var outTex = (Texture2D)tid.Value)
                        outTex.SaveAsPng(fs, width, height);
                }
            }
            foreach (var tid in texIDsPalette)
            {
                var path = Path.Combine(folder,
                    $"{fieldName}_{tid.Key.TextureID}_{tid.Key.PaletteID}.png");
                //save image.
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var outTex = (Texture2D)tid.Value)
                        outTex.SaveAsPng(fs, width, height);
                }
            }
            Process.Start(folder);
        }

        public Tiles TilesUnderMouse() => new Tiles(GetTiles.Where(x =>
        x.X < MouseLocation.X && x.X + 16 > MouseLocation.X &&
        x.Y < MouseLocation.Y && x.Y + 16 > MouseLocation.Y).ToList());

        public void Update()
        {
            if ((CurrentTime += Memory.ElapsedGameTime) > TotalTime)
            {
                CurrentTime = TimeSpan.Zero;
                foreach (var a in _animations)
                {
                    int i = a.Value.FirstOrDefault(x => x.Enabled)?.AnimationState ?? 0;
                    int max = a.Value.Max(k => k.AnimationState);
                    var i1 = i;
                    a.Value.Where(x => x.AnimationState == i1).ForEach(x => x.Hide());
                    if (++i >= max)
                        i = 0;
                    a.Value.Where(x => x.AnimationState == i).ForEach(x => x.Show());
                }
            }

            float tilesWidth = GetTiles.Width;
            float tilesHeight = GetTiles.Height;

            if (Module.Toggles.HasFlag(Toggles.Perspective)) //perspective mode shows gabs in the tiles.
            {
                //finds the min zoom out to fit the entire image in frame.
                var half = new Vector2(tilesWidth / 2f, tilesHeight / 2f);
                var fieldOfView = MathHelper.ToRadians(70);
                float getOppositeSide(float side, float angle)
                {
                    return (float)(Math.Tan(angle) * side);
                }
                half.X = getOppositeSide(half.X, MathHelper.ToRadians(45));
                half.Y = getOppositeSide(half.Y, MathHelper.ToRadians(45));
                var minDistanceFromBG = -Math.Max(half.X, half.Y);
                if (_camPosition.Z > minDistanceFromBG)
                    _camPosition.Z = minDistanceFromBG;

                _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView, Memory.Graphics.GraphicsDevice.Viewport.AspectRatio, float.Epsilon, 1000f);
                _viewMatrix = !Module.Toggles.HasFlag(Toggles.Menu)
                    ? _fpsCamera.Update(ref _camPosition, ref _camTarget, ref Degrees)
                    : Matrix.CreateLookAt(_camPosition, _camTarget, Vector3.Up);
            }
            else
            {
                var vp = Memory.Graphics.GraphicsDevice.Viewport;
                var scale = Memory.Scale(tilesWidth, tilesHeight, ScaleMode.FitBoth);
                _projectionMatrix = Matrix.CreateOrthographic(vp.Width / scale.X, vp.Height / scale.Y, 0f, 100f);
                _viewMatrix = Matrix.CreateLookAt(Vector3.Forward * 10f, Vector3.Zero, Vector3.Up);
            }
            var ml = InputMouse.Location.ToVector2();
            var ml3d = Memory.Graphics.GraphicsDevice.Viewport.Unproject(ml.ToVector3(), _projectionMatrix, _viewMatrix, _worldMatrix);
            ml3d.Y *= -1;
            MouseLocation = ml3d;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            _textureIDs?.ForEach(x => x.Value?.Dispose());
            _textureIDsPalettes?.ForEach(x => x.Value?.Dispose());
            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposedValue = true;
        }

        private static Color Blend1(Color baseColor, Color color)
        {//ClassicSpriteBatch
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R + color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G + color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B + color.B, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private static Color Blend2(Color baseColor, Color color)
        {//ClassicSpriteBatch
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R - color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G - color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B - color.B, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private Color? ChangeColor(Color current, Color input, Point p, byte textureID, IReadOnlyDictionary<byte, HashSet<byte>> overlap)
        {
            if (input.A == 0) return Color.TransparentBlack;
            if (current.A == 0 || current == input)
                return input;
            IEnumerable<byte> o = (from tile in GetTiles
                // ReSharper disable once ImplicitlyCapturedClosure
                where tile.TextureID.Equals(textureID) && tile.ExpandedSource.Contains(p)
                select tile.PaletteID).Distinct().ToArray();
            if (o.Count() <= 1) return input; // two tiles same palette is drawing to same place
            o.ForEach(x => overlap[textureID].Add(x));
            return null;

        }

        private void DrawBackgroundQuad(TileQuadTexture quad, bool forceBlendModeNone = false, Vector2 scale2 = default)
        {
            VertexPositionTexture[] temp = quad;
            var tile = (Tile)quad;
            _ate.Texture = quad;
            if (scale2 != default)
            {
                temp = (VertexPositionTexture[])temp.Clone();
                var scale = Matrix.CreateScale(scale2.ToVector3());
                for (var i = 0; i < temp.Length; i++)
                {
                    var t = temp[i];
                    t.Position = Vector3.Transform(temp[i].Position, scale);
                    temp[i] = t;
                }
            }
            DrawBackgroundQuadsSetBendMode(forceBlendModeNone ? BlendMode.None : tile.BlendMode);
            foreach (var pass in _ate.CurrentTechnique.Passes)
            {
                pass.Apply();

                Memory.Graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                vertexData: temp, vertexOffset: 0, primitiveCount: 2);
            }
        }

        private void DrawBackgroundQuads()
        {
            if (!Module.Toggles.HasFlag(Toggles.Quad)) return;
            DrawBackgroundQuadsStart();
            foreach (var quad in _quads.Where(x => x.Enabled))
            {
                DrawBackgroundQuad(quad);
            }
        }

        private void DrawBackgroundQuadsSetBendMode(BlendMode bm)
        {
            _ate.Alpha = 1f;
            var half = new Color(.5f, .5f, .5f, 1f);
            var quarter = new Color(.25f, .25f, .25f, 1f);
            var full = Color.White;
            switch (bm)
            {
                //If we deswizzled and merged the (BlendModes != BlendMode.none) tiles
                // we can change SamplerState to Anisotropic.
                //But swizzled textures are a Texture Atlas so it will draw bad pixels from near by.
                default:
                    Memory.Graphics.GraphicsDevice.BlendFactor = full;
                    Memory.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.Add:
                    Memory.Graphics.GraphicsDevice.BlendFactor = full;
                    Memory.Graphics.GraphicsDevice.BlendState = Memory.BlendStateAdd;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.Subtract:
                    Memory.Graphics.GraphicsDevice.BlendFactor = full;
                    Memory.Graphics.GraphicsDevice.BlendState = Memory.BlendStateSubtract;
                    _ate.Alpha = .85f; //doesn't darken so much.
                                      //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.HalfAdd:
                    Memory.Graphics.GraphicsDevice.BlendFactor = half;
                    Memory.Graphics.GraphicsDevice.BlendState = Memory.BlendStateAddBlendFactor;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.QuarterAdd:
                    Memory.Graphics.GraphicsDevice.BlendFactor = quarter;
                    Memory.Graphics.GraphicsDevice.BlendState = Memory.BlendStateAddBlendFactor;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;
            }
        }

        private void DrawBackgroundQuadsStart()
        {
            Memory.Graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            _ate.Projection = _projectionMatrix; _ate.View = _viewMatrix; _ate.World = _worldMatrix;

            _effect.Projection = _projectionMatrix; _effect.View = _viewMatrix; _effect.World = _worldMatrix;
            Memory.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _effect.TextureEnabled = true;
            _ate.VertexColorEnabled = false;
            _effect.VertexColorEnabled = false;
        }

        private void DrawSpriteBatch()
        {
            if (!Module.Toggles.HasFlag(Toggles.ClassicSpriteBatch)) return;
            var drawTextures = GetTexturesReadyToDrawClassicSpriteBatch();
            var open = false;
            var lastBlendMode = BlendMode.None;
            var alpha = 1f;
            if (drawTextures != null)
                foreach (var kvp in drawTextures)
                {
                    if (!open || lastBlendMode != kvp.Key)
                    {
                        if (open)
                            Memory.SpriteBatchEnd();
                        open = true;
                        alpha = 1f;
                        switch (kvp.Key)
                        {
                            default:
                                Memory.SpriteBatchStartAlpha();
                                break;

                            case BlendMode.HalfAdd:
                                Memory.SpriteBatchStart(bs: Memory.BlendStateAdd, ss: SamplerState.AnisotropicClamp);
                                break;

                            case BlendMode.QuarterAdd:
                                Memory.SpriteBatchStart(bs: Memory.BlendStateAdd, ss: SamplerState.AnisotropicClamp);
                                break;

                            case BlendMode.Add:
                                Memory.SpriteBatchStart(bs: Memory.BlendStateAdd, ss: SamplerState.AnisotropicClamp);
                                break;

                            case BlendMode.Subtract:
                                alpha = .9f;
                                Memory.SpriteBatchStart(bs: Memory.BlendStateSubtract, ss: SamplerState.AnisotropicClamp);
                                break;
                        }
                        lastBlendMode = kvp.Key;
                    }
                    var tex = kvp.Value;
                    var src = new Rectangle(0, 0, tex.Width, tex.Height);
                    var dst = src;

                    dst.Size = (dst.Size.ToVector2() * Memory.Scale(tex.Width, tex.Height, ScaleMode.FitBoth)).ToPoint();
                    //In game I think we'd keep the field from leaving the screen edge but would center on the Squall and the party when it can.
                    //I setup scaling after noticing the field didn't size with the screen. I set it to center on screen.
                    dst.Offset(Memory.Center.X - dst.Center.X, Memory.Center.Y - dst.Center.Y);
                    Memory.SpriteBatch.Draw(tex, dst, src, Color.White * alpha);
                    //new Microsoft.Xna.Framework.Rectangle(0, 0, 1280 + (width - 320), 720 + (height - 224)),
                    //new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height)
                }

            if (open)
                Memory.SpriteBatchEnd();
        }

        private void DrawWalkMesh()
        {//todo move into walk mesh class. was only because at the time I thought i'd need the background data.
            if (!Module.Toggles.HasFlag(Toggles.WalkMesh)) return;

            _effect.TextureEnabled = false;
            Memory.Graphics.GraphicsDevice.BlendFactor = Color.White;
            Memory.Graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            Memory.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            //using (DepthStencilState depthStencilState = new DepthStencilState() { DepthBufferEnable = true })
            using (var rasterizerState = new RasterizerState() { CullMode = CullMode.None })
            {
                Memory.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;//depthStencilState;
                Memory.Graphics.GraphicsDevice.RasterizerState = rasterizerState;
                _ate.Texture = null;
                _ate.VertexColorEnabled = true;
                _effect.VertexColorEnabled = true;
                //camPosition = Module.Cameras[0].Position;
                _effect.World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);// Module.Cameras[0].CreateWorld();
                var fieldOfView = MathHelper.ToRadians(70);
                //effect.View = //Module.Cameras[0].CreateLookAt();
                //effect.Projection = Module.Cameras[0].CreateProjection();

                _effect.Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView,
                    Memory.Graphics.GraphicsDevice.Viewport.AspectRatio, float.Epsilon, 1000f);
                _effect.View = !Module.Toggles.HasFlag(Toggles.Menu)
                    ? _fpsCamera.Update(ref _camPosition, ref _camTarget, ref Degrees)
                    : Matrix.CreateLookAt(_camPosition, _camTarget, Vector3.Up);
                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    Memory.Graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                    vertexData: Module.WalkMesh.Vertices.ToArray(), vertexOffset: 0, primitiveCount: Module.WalkMesh.Count);
                }
            }
        }

        private void DumpRawTexture(byte[] mim)
        {
            if (!Memory.EnableDumpingData && !Module.Toggles.HasFlag(Toggles.DumpingData)) return;
            MemoryStream ms;

            var path = Path.Combine(Module.GetFolder(),
                $"{Module.GetFieldName()}_raw_{{0}}bit_{{1}}.png");
            using (var br = new BinaryReader(ms = new MemoryStream(mim)))
            {

                long startPixel = _textureType.PaletteSectionSize;
                    

                process(8);
                process(4);
                process(16);
                process(24);
                process(24,true);
                void process(byte bit, bool alt  = false)
                {
                    var adj = bit / 8f;
                    var textureTypeWidth = bit == 24? _textureType.Width :(int)(_textureType.Width / adj);
                    var height = checked((int)Math.Ceiling(((float)mim.Length - _textureType.PaletteSectionSize) / _textureType.Width / (bit == 24 ? adj : 1f)));

                    if (bit == 24 && alt)
                    {
                        textureTypeWidth = (int)Math.Ceiling(_textureType.Width / adj);
                        height *= (int)adj;
                    }

                    var buffer = new TextureBuffer(textureTypeWidth, height, false);
                    foreach (var clut in _cluts)
                    {
                        ms.Seek(startPixel, SeekOrigin.Begin);
                        var i = 0;
                        byte colorKey = 0;
                        var lastRow = 0;
                        while (ms.Position + Math.Ceiling(adj) < ms.Length)
                        {
                            var row = i / textureTypeWidth;
                            Color input;
                            if (bit == 24) //just to see if anything is there. don't think there is a real usage of 24 bit.
                            {
                                if (alt && lastRow != row && row % 3 ==0) i++;
                                //i += 1;
                                input = new Color
                                {
                                    B = br.ReadByte(),
                                    G = br.ReadByte(),
                                    R = br.ReadByte(),
                                    A = 0xFF,
                                };
                            }
                            else if (bit == 16)
                            {
                                //i += 1;
                                input = Texture_Base.ABGR1555toRGBA32bit(br.ReadUInt16());
                            }
                            else if (bit == 8)
                            {
                                //i = checked((int)(ms.Position - startPixel));
                                colorKey = br.ReadByte();
                                input = clut.Value[colorKey];
                            }
                            else if (bit == 4)
                            {
                                //i++;
                                if (i % 2 == 0)
                                {
                                    colorKey = br.ReadByte();
                                    input = clut.Value[colorKey & 0xf];
                                }
                                else
                                {
                                    input = clut.Value[(colorKey & 0xf0) >> 4];
                                }
                            }
                            else throw new ArgumentException($"{nameof(bit)} is {bit}, it may only be 4 or 8.");
                            if (i < buffer.Count)
                                buffer[i] = input;
                            else break;
                            i++;
                            lastRow = row;
                        }

                        using (var tex = (Texture2D)buffer)
                        using (var fs = new FileStream(string.Format(path, bit, $"{clut.Key}{(alt?"a":"")}"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            tex.SaveAsPng(fs, textureTypeWidth, height);
                        }
                        if (bit > 8) break;
                    }
                }
            }
        }

        private void FindOverlappingTilesClassicSpriteBatch() => (from t1 in GetTiles
                                                                  from t2 in GetTiles
                                                                  where t1.TileID < t2.TileID
                                                                  where t1.BlendMode == BlendMode.None
                                                                  where t1.Intersect(t2)
                                                                  orderby t1.TileID, t2.TileID ascending
                                                                  select new[] { t1, t2 }

                                     ).ForEach(x => x[1].OverLapID = checked((byte)(x[0].OverLapID + 1)));

        private static byte GetColorKeyClassicSpriteBatch(IReadOnlyList<byte> mim, int textureWidth, int startPixel, int x, int y, bool is8Bit)
        {
            if (is8Bit)
                return mim[startPixel + x + y * textureWidth];
            var tempKey = mim[startPixel + x / 2 + y * textureWidth];
            if (x % 2 == 1)
                return checked((byte)((tempKey & 0xf0) >> 4));
            return checked((byte)(tempKey & 0xf));
        }

        private List<KeyValuePair<BlendMode, Texture2D>> GetTexturesReadyToDrawClassicSpriteBatch() =>
            _textures?.OrderByDescending(kvpZ => kvpZ.Key)
            .SelectMany(kvpLayerID => kvpLayerID.Value.OrderBy(x => kvpLayerID.Key)
            .SelectMany(kvpAnimationID => kvpAnimationID.Value.OrderBy(x => kvpAnimationID.Key))
            .SelectMany(kvpAnimationState => kvpAnimationState.Value.OrderBy(x => kvpAnimationState.Key))
            .SelectMany(kvpOverlapID => kvpOverlapID.Value.OrderBy(x => kvpOverlapID.Key))
            .SelectMany(kvpBlendMode => kvpBlendMode.Value)).ToList();

        /// <summary>
        /// Gets the TextureHandler used by the tile.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>Texture handler used by tile</returns>
        private TextureHandler GetTextureUsedByTile(Tile tile)
        {
            var textureIDsPalette = _textureIDsPalettes
                ?.FirstOrDefault(x => x.Key.PaletteID == tile.PaletteID && x.Key.TextureID == tile.TextureID).Value;
            if (textureIDsPalette != null)
                return textureIDsPalette;

            var tid = _textureIDs?.FirstOrDefault(x => x.Key == tile.TextureID).Value;
            return tid;
        }

        private void LoadPalettes(byte[] mim)
        {
            var offset = /*Memory.FieldHolder.FieldID == 76 ? 0 :*/ _textureType?.BytesSkippedPalettes ?? 0;
            var cluts = GetTiles != null
                ? new Cluts(
                    GetTiles.Select(x => x.PaletteID).Distinct().ToDictionary(x => x, x => new Color[ColorsPerPalette]),
                    false)
                :
                new Cluts(
                    Enumerable.Range(0, 16).Select(x => (byte) x)
                        .ToDictionary(x => x, x => new Color[ColorsPerPalette]), false);
            using (var br = new BinaryReader(new MemoryStream(mim)))
                foreach (var clut in cluts)
                {
                    var palettePointer = offset + clut.Key * BytesPerPalette;
                    br.BaseStream.Seek(palettePointer, SeekOrigin.Begin);
                    for (var i = 0; i < ColorsPerPalette; i++)
                        clut.Value[i] = Texture_Base.ABGR1555toRGBA32bit(br.ReadUInt16());
                }
            _cluts = cluts;
            SaveCluts();
        }

        private void LoadTiles(byte[] map) => GetTiles = map == null ? default : Tiles.Load(map, _textureType.Type);

        private int LoadUpscaleBackgrounds(string path)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.EnumerateFiles(path, $"*{Module.GetFieldName()}*.png", SearchOption.AllDirectories).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                if (files.Count > 0)
                {
                    _textureIDs = new Dictionary<byte, TextureHandler>();
                    var escapedName = Regex.Escape(Module.GetFieldName());
                    var regex = new Regex(@".+" + escapedName + @"_(\d{1,2})\.png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    foreach (var file in files.Select(x => regex.Match(x)))
                    {
                        if (file.Groups.Count <= 1 || !byte.TryParse(file.Groups[1].Value, out var b)) continue;
                        if (b >= 13) b -= 13;
                        if (_textureIDs.ContainsKey(b)) continue;
                        var alt = $"{Module.GetFieldName()}_{b + 13}.png";
                        _textureIDs.Add(b, TextureHandler.CreateFromPng(File.Exists(alt) ? alt : file.Value, 256, 256, 0, true, true));
                    }
                    SaveSwizzled(_textureIDs.ToDictionary(x => x.Key, x => (Texture2D)x.Value));
                    _textureIDsPalettes = new Dictionary<TextureIDPaletteID, TextureHandler>();
                    var regex2 = new Regex(@".+" + escapedName + @"_(\d{1,2})_(\d{1,2})\.png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    foreach (var file in files.Select(x => regex2.Match(x)))
                    {
                        if (file.Groups.Count > 1 && byte.TryParse(file.Groups[1].Value, out var b) && byte.TryParse(file.Groups[2].Value, out var b2))
                        {
                            if (b >= 13) b -= 13;
                            TextureIDPaletteID textureIDPaletteID;
                            if (!_textureIDsPalettes.ContainsKey(textureIDPaletteID = new TextureIDPaletteID { PaletteID = b2, TextureID = b }))
                            {
                                var alt = $"{Module.GetFieldName()}_{b + 13}_{b2}.png";
                                _textureIDsPalettes.Add(textureIDPaletteID, TextureHandler.CreateFromPng(File.Exists(alt) ? alt : file.Value, 256, 256, b2, true, true));
                            }
                        }
                        foreach (var groups in _textureIDsPalettes.Where(x => _textureIDsPalettes.Count(y => y.Key.TextureID == x.Key.TextureID) > 1).GroupBy(x => x.Key.PaletteID))

                            foreach (var kvpGroup in groups)
                            {
                                var textureIDs =
                                    groups.ToDictionary(x => x.Key.TextureID, x => (Texture2D) x.Value);
                                SaveSwizzled(textureIDs, $"_{kvpGroup.Key.PaletteID}");
                                break;
                            }
                    }
                }
            }
            var count = (_textureIDs?.Count ?? 0) + (_textureIDsPalettes?.Count ?? 0);
            return count;
        }

        //private void SaveSwizzled(string suf = "") => SaveSwizzled(TextureIDs, suf);
        private void OldSaveDeswizzled()
        {
            if (!Memory.EnableDumpingData && (!Module.Toggles.HasFlag(Toggles.DumpingData) ||
                                              !Module.Toggles.HasFlag(Toggles.ClassicSpriteBatch))) return;
            var fieldName = Module.GetFieldName();
            var folder = Module.GetFolder(fieldName);
            foreach (var kvpZ in _textures)
            foreach (var kvpLayer in kvpZ
                .Value)
            foreach (var kvpAnimationID
                in
                kvpLayer.Value)
            foreach (var
                kvpAnimationState in kvpAnimationID.Value)
            foreach (var kvpOverlapID in kvpAnimationState.Value)
            foreach (var kvp in kvpOverlapID.Value)
            {
                var path = Path.Combine(folder,
                    $"{fieldName}_{kvpZ.Key:D4}.{kvpLayer.Key}.{kvpAnimationID.Key}.{kvpAnimationState.Key}.{kvpOverlapID.Key}.{(int) kvp.Key}.png");
                using (var fs = new FileStream(path,
                    FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    kvp.Value.SaveAsPng(
                        fs,
                        kvp.Value.Width, kvp.Value.Height);
            }
        }

        private bool ParseBackgroundClassicSpriteBatch(byte[] mim)
        {
            if (!Module.Toggles.HasFlag(Toggles.ClassicSpriteBatch)) return true;
            if (mim == null || (GetTiles?.Count ?? 0) == 0)
                return false;

            FindOverlappingTilesClassicSpriteBatch();
            //FindSameXYTilesSource();
            var lowest = new Point(GetTiles.Min(x => x.X), GetTiles.Min(x => x.Y));
            var maximum = new Point(GetTiles.Max(x => x.X), GetTiles.Max(x => x.Y));

            Height = Math.Abs(lowest.Y) + maximum.Y + Tile.Size; //224
            Width = Math.Abs(lowest.X) + maximum.X + Tile.Size; //320
                                                                //Color[] finalImage = new Color[height * width]; //ARGB;
                                                                //Color[] finalOverlapImage = new Color[height * width];
                                                                //tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
                                                                //texOverlap = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            IEnumerable<byte> layers = GetTiles.Select(x => x.LayerID).Distinct().OrderBy(x => x).ToArray();
            Debug.WriteLine($"FieldID: {Memory.FieldHolder.FieldID}, Layers: {layers.Count()}, ({string.Join(",", layers)}) ");

            var sortedTiles = GetTiles.OrderBy(x => x.OverLapID).ThenByDescending(x => x.Z).ThenBy(x => x.LayerID).ThenBy(x => x.AnimationID).ThenBy(x => x.AnimationState).ThenBy(x => x.BlendMode).ToList();

            if (_textures != null)
            {
                foreach (var tex in GetTexturesReadyToDrawClassicSpriteBatch().Select(x => x.Value))
                    tex.Dispose();
            }
            _textures = new ConcurrentDictionary<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>>>();
            const ushort z = 0;
            byte layerID = 0;
            byte animationID = 0;
            byte animationState = 0;
            byte overlapID = 0;
            var blendMode = BlendMode.None;
            TextureBuffer textureBuffer = null;
            var hasColor = false;
            var dictLayerID = _textures.GetOrAdd(z, new ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>>());

            void convertColorToTexture2d()
            {
                if (!hasColor || textureBuffer == null) return;
                hasColor = false;
                var dictAnimationID =
                    dictLayerID.GetOrAdd(layerID,
                        new ConcurrentDictionary<byte, ConcurrentDictionary<byte,
                            ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>());
                var
                    dictAnimationState = dictAnimationID.GetOrAdd(animationID,
                        new ConcurrentDictionary<byte,
                            ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>());
                var dictOverlapID =
                    dictAnimationState.GetOrAdd(animationState,
                        new ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>());
                var dictBlend =
                    dictOverlapID.GetOrAdd(overlapID, new ConcurrentDictionary<BlendMode, Texture2D>());
                var tex = dictBlend.GetOrAdd(blendMode, new Texture2D(Memory.Graphics.GraphicsDevice, Width, Height));
                textureBuffer.SetData(tex);
            }
            for (var i = 0; i < sortedTiles.Count; i++)
            {
                var previousTile = i > 0 ? sortedTiles[i - 1] : null;
                var tile = sortedTiles[i];
                if (textureBuffer == null || previousTile == null || previousTile.Z != tile.Z ||
                    previousTile.LayerID != tile.LayerID || previousTile.BlendMode != tile.BlendMode ||
                    previousTile.AnimationID != tile.AnimationID ||
                    previousTile.AnimationState != tile.AnimationState || previousTile.OverLapID != tile.OverLapID)
                {
                    convertColorToTexture2d();
                    textureBuffer = new TextureBuffer(Width, Height, false);
                    layerID = tile.LayerID;
                    blendMode = tile.BlendMode;
                    animationID = tile.AnimationID;
                    animationState = tile.AnimationState;
                    overlapID = tile.OverLapID;
                }

                var palettePointer = _textureType.BytesSkippedPalettes + tile.PaletteID * BytesPerPalette;
                var sourceImagePointer = BytesPerPalette * _textureType.Palettes;
                const int textureWidth = 128;
                var startPixel = sourceImagePointer + tile.SourceX + textureWidth * tile.TextureID + _textureType.Width * tile.SourceY;
                var real = new Point(Math.Abs(lowest.X) + tile.X, Math.Abs(lowest.Y) + tile.Y);
                var realDestinationPixel = real.Y * Width + real.X;

                
                if (tile.Is4Bit)
                {
                    startPixel -= tile.SourceX / 2;
                }
                for (var y = 0; y < Tile.Size; y++)
                    for (var x = 0; x < Tile.Size; x++)
                    {
                        var colorKey = GetColorKeyClassicSpriteBatch(mim, _textureType.Width, startPixel, x, y, tile.Is8Bit);
                        var color16Bit = BitConverter.ToUInt16(mim, 2 * colorKey + palettePointer);
                        if (color16Bit == 0) // 0 is Color.TransparentBlack So we skip it.
                            continue;
                        var color = Texture_Base.ABGR1555toRGBA32bit(color16Bit);

                        var pos = realDestinationPixel + x + y * Width;
                        var bufferedColor = textureBuffer[pos];
                        if (blendMode < BlendMode.None)
                        {
                            if (color == Color.Black)
                                continue;

                            if (blendMode == BlendMode.Subtract)
                            {
                                if (bufferedColor.A != 0)
                                    color = Blend2(bufferedColor, color);
                            }
                            else
                            {
                                switch (blendMode)
                                {
                                    case BlendMode.QuarterAdd:
                                        color = Color.Multiply(color, .25f);
                                        break;
                                    case BlendMode.HalfAdd:
                                        color = Color.Multiply(color, .5f);
                                        break;
                                    case BlendMode.Add:
                                        break;
                                    case BlendMode.Subtract:
                                        break;
                                    case BlendMode.None:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                if (bufferedColor.A != 0)
                                    color = Blend1(bufferedColor, color);
                            }
                        }
                        else if (bufferedColor.A != 0)
                        {
                            throw new Exception("Color is already set something may be wrong.");
                        }
                        color.A = 0xFF;
                        textureBuffer[pos] = color;
                        hasColor = true;
                    }
            }
            convertColorToTexture2d(); // gets leftover colors from last batch and makes a texture.
            OldSaveDeswizzled();
            return true;
        }

        /// <summary>
        /// <para>Create a swizzeled Textures with one palette.</para>
        /// <para>Few exceptions where tiles conflict and need separate files.</para>
        /// </summary>
        /// <param name="mim">Image Data .mim file</param>
        /// <returns></returns>
        private bool ParseBackgroundQuads(byte[] mim)
        {
            if (mim == null || (GetTiles?.Count ?? 0) == 0)
                return false;
            var path = Path.Combine(Memory.FF8Dir, "textures");
            var count = LoadUpscaleBackgrounds(path);
            if (count <= 0)
            {
                var uniqueSetOfTileData = GetTiles.Where(x => x.Draw).Select(x => new
                {
                    x.TextureID, x.BlendMode, loc = new Point(x.SourceX, x.SourceY), x.Depth, x.PaletteID, x.AnimationID
                }).Distinct().ToList().AsReadOnly();
                Width = uniqueSetOfTileData.Max(x => x.loc.X) + Tile.Size;
                Height = uniqueSetOfTileData.Max(x => x.loc.Y) + Tile.Size;
                IReadOnlyDictionary<byte, Texture2D> textureIDs = uniqueSetOfTileData.Select(x => x.TextureID)
                    .Distinct()
                    .ToDictionary(x => x,
                        x => Memory.Graphics != null ? new Texture2D(Memory.Graphics.GraphicsDevice, 256, 256) : null);
                IReadOnlyDictionary<byte, HashSet<byte>> overlap = GetTiles.Select(x => x.TextureID).Distinct()
                    .ToDictionary(x => x, x => new HashSet<byte>());
                using (var br = new BinaryReader(new MemoryStream(mim)))
                {
                    foreach (var kvp in textureIDs)
                    {
                        GenTexture(kvp.Key, kvp.Value);
                    }

                    SaveSwizzled(textureIDs);
                    var fieldName = Module.GetFieldName();
                    _textureIDs = textureIDs.ToDictionary(x => x.Key,
                        x => TextureHandler.Create($"{fieldName}_{x.Key}", new Texture2DWrapper(x.Value),
                            ushort.MaxValue));

                    if (overlap.Any(x => x.Value.Count > 1))
                    {
                        IReadOnlyDictionary<TextureIDPaletteID, Texture2D> textureIDsPalettes = uniqueSetOfTileData
                            .Where(x => overlap[x.TextureID].Contains(x.PaletteID))
                            .Select(x => new TextureIDPaletteID {TextureID = x.TextureID, PaletteID = x.PaletteID})
                            .Distinct().ToDictionary(x => x,
                                x => new Texture2D(Memory.Graphics.GraphicsDevice, 256, 256));
                        _textureIDsPalettes = textureIDsPalettes.ToDictionary(x => x.Key,
                            x => TextureHandler.Create($"{fieldName}_{x.Key.TextureID}", new Texture2DWrapper(x.Value),
                                x.Key.PaletteID));
                        foreach (var kvp in textureIDsPalettes)
                        {
                            GenTexture(kvp.Key.TextureID, kvp.Value, kvp.Key.PaletteID);
                        }

                        foreach (var groups in
                            textureIDsPalettes
                                .Where(x => textureIDsPalettes.Count(y => y.Key.TextureID == x.Key.TextureID) > 1)
                                .GroupBy(x => x.Key.PaletteID))

                        foreach (var kvpGroup in groups)
                        {
                            textureIDs =
                                groups.ToDictionary(x => x.Key.TextureID, x => x.Value);
                            SaveSwizzled(textureIDs, $"_{kvpGroup.Key.PaletteID}");
                            break;
                        }
                    }

                    void GenTexture(byte texID, Texture2D tex2d, byte? inPaletteID = null)
                    {
                        if (tex2d == null) return;
                        var tex = new TextureBuffer(tex2d.Width, tex2d.Height, false);

                        foreach (var tile in uniqueSetOfTileData.Where(x =>
                            x.TextureID == texID && (!inPaletteID.HasValue || inPaletteID.Value == x.PaletteID)))
                        {
                            var is4Bit = Tile.Test4Bit(tile.Depth);
                            var is8Bit = Tile.Test8Bit(tile.Depth);
                            var is16Bit = Tile.Test16Bit(tile.Depth);


                            byte colorKey = 0;
                            foreach (var p in from x in Enumerable.Range(0, Tile.Size)
                                from y in Enumerable.Range(0, Tile.Size)
                                orderby y, x
                                select new Point(x, y))
                            {
                                (var trueX, var trueY) = (tile.loc.X+p.X, tile.loc.Y + p.Y);
                                var offsetX = is16Bit
                                    ? trueX * 2
                                    : trueX / (is4Bit ? 2 : 1);
                                var texturePageOffset = TexturePageWidth * tile.TextureID;
                                var offsetY = _textureType.Width * trueY ;
                                var offset = _textureType.PaletteSectionSize +
                                             offsetX +
                                             texturePageOffset + 
                                             offsetY;
                                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                                var point = new Point(p.X + tile.loc.X, p.Y + tile.loc.Y);

                                var paletteID = tile.PaletteID;
                                Color input = default;
                                if (is8Bit)
                                {
                                    input = _cluts[paletteID][br.ReadByte()];
                                }
                                else if (is16Bit)
                                {
                                    input = Texture_Base.ABGR1555toRGBA32bit(br.ReadUInt16());
                                }
                                else if (is4Bit)
                                {
                                    if (p.X % 2 == 0)
                                    {
                                        colorKey = br.ReadByte();
                                        input = _cluts[paletteID][colorKey & 0xf];
                                    }
                                    else
                                    {
                                        input = _cluts[paletteID][(colorKey & 0xf0) >> 4];
                                    }
                                }

                                if (!inPaletteID.HasValue) 
                                    // forcing a palette happens post overlap test. So shouldn't need to rerun test.
                                {
                                    var current = tex[point.X, point.Y];
                                    var output = ChangeColor(current, input, point, tile.TextureID, overlap);
                                    if (!output.HasValue) break;
                                    if (output.Value.A != 0)
                                        tex[point.X, point.Y] = output.Value;
                                }
                                else
                                {
                                    if (input.A != 0)
                                        tex[point.X, point.Y] = input;
                                }
                            }
                        }

                        tex.SetData(tex2d);
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Loaded {count} Textures from {path}");
            }

            //the sort here should be the default draw order. May need changed.
            _quads = GetTiles.Select(x => new TileQuadTexture(x, GetTextureUsedByTile(x), 1f)).Where(x => x.Enabled)
                .OrderByDescending(x => x.GetTile.Z)
                .ThenByDescending(x => x.GetTile.TileID)
                .ThenBy(x => x.GetTile.LayerID)
                .ThenBy(x => x.GetTile.AnimationID)
                .ThenBy(x => x.GetTile.AnimationState)
                .ThenByDescending(x => x.GetTile.BlendMode).ToList();
            _animations = _quads.Where(x => x.AnimationID != 0xFF).Select(x => x.AnimationID).Distinct().ToDictionary(x => x, x => _quads.Where(y => y.AnimationID == x).OrderBy(y => y.AnimationState).ToList());
            _animations.ForEach(x => x.Value.Where(y => y.AnimationState != 0).ForEach(w => w.Hide()));
            TotalTime = TimeSpan.FromMilliseconds(1000f / 10f);
            CurrentTime = TimeSpan.Zero;
            return true;
        }

        private void SaveCluts()
        {
            if (Memory.EnableDumpingData || Module.Toggles.HasFlag(Toggles.DumpingData))
            {
                var path = Path.Combine(Module.GetFolder(),
                    $"{Module.GetFieldName()}_Clut.png");
                _cluts.Save(path);
            }
        }

        private static void SaveSwizzled(IReadOnlyDictionary<byte, Texture2D> textureIDs, string suf = "")
        {
            if (!Memory.EnableDumpingData && !Module.Toggles.HasFlag(Toggles.DumpingData)) return;
            var fieldName = Module.GetFieldName();
            var folder = Module.GetFolder(fieldName);
            foreach (var kvp in textureIDs)
            {
                var path = Path.Combine(folder,
                    $"{fieldName}_{kvp.Key}{suf}.png");
                if (File.Exists(path))
                    continue;
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                { kvp.Value.SaveAsPng(fs, kvp.Value.Width, kvp.Value.Height); }
            }
        }

        #endregion Methods

        // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }
}