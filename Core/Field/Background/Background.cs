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

        private const int bytesPerPalette = 2 * colorsPerPalette;

        private const int colorsPerPalette = 256;

        /// <summary>
        /// 4 bit has 2 columns per every byte so it expands to twice the width.
        /// </summary>
        private const int fourBitTexturePageWidth = 2 * texturePageWidth;

        /// <summary>
        /// Standard texture page width.
        /// </summary>
        private const int texturePageWidth = 128;

        private Dictionary<byte, List<TileQuadTexture>> Animations;

        private AlphaTestEffect ate;

        private Vector3 camPosition;

        private Vector3 camTarget;

        /// <summary>
        /// Palettes/Color Lookup Tables
        /// </summary>
        private Cluts Cluts;

        private float degrees;
        private bool disposedValue = false;

        private BasicEffect effect;

        private FPS_Camera fps_camera;
        private Rectangle OutputDims;

        private Matrix projectionMatrix, viewMatrix, worldMatrix;

        private List<TileQuadTexture> quads;

        private Dictionary<byte, TextureHandler> TextureIDs;

        private Dictionary<TextureIDPaletteID, TextureHandler> TextureIDsPalettes;

        private ConcurrentDictionary<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>>> Textures;

        private BackgroundTextureType TextureType;

        private Tiles tiles;

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

        public TimeSpan CurrentTime { get; private set; }
        public Tiles GetTiles => tiles;
        public bool HasSpriteBatchTexturesLoaded => GetTexturesReadyToDrawClassicSpriteBatch()?.Count > 0;
        public int Height { get => OutputDims.Height; private set => OutputDims.Height = value; }
        public bool Is4Bit => tiles?.Any(x => x.Is4Bit) ?? false;
        public bool IsAddBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.add) ?? false;
        public bool IsHalfBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.halfadd) ?? false;
        public bool IsQuarterBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.quarteradd) ?? false;
        public bool IsSubtractBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.subtract) ?? false;
        public Vector3 MouseLocation { get; private set; }
        public TimeSpan TotalTime { get; private set; }
        public int Width { get => OutputDims.Width; private set => OutputDims.Width = value; }

        #endregion Properties

        #region Methods

        public static Background Load(byte[] mimb, byte[] mapb)
        {
            if (mimb == null || mapb == null)
                return null;
            Background r = new Background
            {
                TextureType = BackgroundTextureType.GetTextureType(mimb),
                camTarget = Vector3.Zero,
                camPosition = new Vector3(0f, 0f, -10f),
                fps_camera = new FPS_Camera(),
                degrees = 90f
            };
            if (Memory.graphics != null)
            {
                r.ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice);
                r.effect = new BasicEffect(Memory.graphics.GraphicsDevice);
            }
            r.worldMatrix = Matrix.CreateWorld(r.camPosition, Vector3.
                          Forward, Vector3.Up);
            r.viewMatrix = Matrix.CreateLookAt(r.camPosition, r.camTarget,
                     Vector3.Up);
            r.LoadTiles(mapb);
            r.LoadPalettes(mimb);
            r.DumpRawTexture(mimb);
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                if (!r.ParseBackgroundQuads(mimb))
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
                if (!r.ParseBackgroundClassicSpriteBatch(mimb))
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
            using (Texture2D mask = new Texture2D(Memory.graphics.GraphicsDevice, 4, 4))
            {
                mask.SetData(Enumerable.Range(0, 16).Select(x => Color.White).ToArray());
                string fieldname = Module.GetFieldName();
                string folder = Module.GetFolder(fieldname, "deswizzle");
                Vector2 scale = quads[0].Texture.ScaleFactor;
                int Width = (int)(tiles.Width * scale.X);
                int Height = (int)(tiles.Height * scale.Y);
                //Matrix backup = projectionMatrix;
                //projectionMatrix = Matrix.CreateOrthographic(tiles.Width, tiles.Height, 0f, 100f);
                tiles.UniquePupuIDs();// make sure each layer has it's own id.
                foreach (IGrouping<uint, TileQuadTexture> pupuIDgroup in quads.GroupBy(x => x.GetTile.PupuID)) //group the quads by their pupu id.
                {
                    using (RenderTarget2D outTex = new RenderTarget2D(Memory.graphics.GraphicsDevice, Width, Height))
                    {
                        //start drawing
                        Memory.graphics.GraphicsDevice.SetRenderTarget(outTex);
                        Memory.graphics.GraphicsDevice.Clear(Color.TransparentBlack);
                        Memory.SpriteBatchStartAlpha();
                        foreach (TileQuadTexture quad in pupuIDgroup)
                        {
                            Tile tile = (Tile)quad;
                            //DrawBackgroundQuadsStart();
                            //DrawBackgroundQuad(quad, true);
                            Rectangle dst = tile.GetRectangle;
                            dst.Offset(Math.Abs(tiles.TopLeft.X), Math.Abs(tiles.TopLeft.Y));
                            Rectangle src = tile.Source;
                            //src = src.Scale(scale);
                            dst = dst.Scale(scale);
                            quad.Texture.Draw(dst, src, Color.White);
                        }
                        Memory.SpriteBatchEnd();
                        //end drawing
                        Memory.graphics.GraphicsDevice.SetRenderTarget(null);
                        //set path
                        string path = Path.Combine(folder,
                            $"{fieldname}_{pupuIDgroup.Key.ToString("X8")}.png");
                        //save image.
                        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            outTex.SaveAsPng(fs, Width, Height);
                        }
                    }
                    using (RenderTarget2D outTex = new RenderTarget2D(Memory.graphics.GraphicsDevice, Width, Height))
                    {
                        //start drawing
                        Memory.graphics.GraphicsDevice.SetRenderTarget(outTex);
                        Memory.graphics.GraphicsDevice.Clear(Color.Black);
                        Memory.SpriteBatchStartAlpha();
                        Rectangle src = new Rectangle(0, 0, 4, 4);
                        foreach (TileQuadTexture quad in pupuIDgroup)
                        {
                            Tile tile = (Tile)quad;
                            //DrawBackgroundQuadsStart();
                            //DrawBackgroundQuad(quad, true);
                            Rectangle dst = tile.GetRectangle;
                            dst.Offset(Math.Abs(tiles.TopLeft.X), Math.Abs(tiles.TopLeft.Y));
                            //src = src.Scale(scale);
                            dst = dst.Scale(scale);
                            Memory.spriteBatch.Draw(mask, dst, src, Color.White);
                        }
                        Memory.SpriteBatchEnd();
                        //end drawing
                        Memory.graphics.GraphicsDevice.SetRenderTarget(null);
                        //set path
                        string path = Path.Combine(folder,
                            $"{fieldname}_{pupuIDgroup.Key.ToString("X8")}_MASK.png");
                        //save image.
                        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            outTex.SaveAsPng(fs, Width, Height);
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
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            DrawBackgroundQuads();
            DrawWalkMesh();
            DrawSpriteBatch();
        }

        public void Reswizzle()
        {
            tiles.UniquePupuIDs();// make sure each layer has it's own id.

            string fieldname = Module.GetFieldName();
            string folder = Module.GetFolder(fieldname, "deswizzle"); //goes from deswizzle folder
            if (Directory.Exists(folder))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(folder, "*.png");
                folder = Module.GetFolder(fieldname, "reswizzle");

                Dictionary<byte, HashSet<byte>> overlap = tiles.Select(x => x.TextureID).Distinct().ToDictionary(x => x, x => new HashSet<byte>());
                ConcurrentDictionary<byte, TextureBuffer> texids = new ConcurrentDictionary<byte, TextureBuffer>();
                ConcurrentDictionary<TextureIDPaletteID, TextureBuffer> texidspalette = new ConcurrentDictionary<TextureIDPaletteID, TextureBuffer>();
                int Width = 0; int Height = 0;

                //Vector2 origin = tiles.Origin;
                Point lowest = tiles.TopLeft;
                Vector2 size = new Vector2(tiles.Width, tiles.Height);//new Point(Math.Abs(lowest.X) + highest.X + Tile.size, Math.Abs(lowest.Y) + highest.Y + Tile.size);
                Regex re = new Regex(@".+_([0-9A-F]{8}).png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                process();
                if (overlap.Any(x => x.Value.Count > 1))
                    process(true);
                void process(bool dooverlap = false)
                {
                    foreach (string file in files)
                    {
                        //Point highest = new Point(tiles.Max(x => x.X), tiles.Max(x => x.Y));

                        Match match = re.Match(file);
                        if (match.Groups.Count > 1 && UInt32.TryParse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint pupuid))
                        {
                            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (Texture2D tex = Texture2D.FromStream(Memory.graphics.GraphicsDevice, fs))
                            {
                                Vector2 scale = new Vector2(tex.Width, tex.Height) / size;
                                Width = (int)(256 * scale.X);
                                Height = (int)(256 * scale.Y);
                                TextureBuffer inTex = new TextureBuffer(tex.Width, tex.Height);
                                inTex.GetData(tex);

                                foreach (TileQuadTexture quad in quads.Where(x =>
                                x.GetTile.PupuID == pupuid &&
                                (!dooverlap || overlap[x.GetTile.TextureID].Contains(x.GetTile.PaletteID))))
                                {
                                    Tile tile = (Tile)quad;

                                    texids.TryAdd(tile.TextureID, new TextureBuffer(Width, Height, false));
                                    Point src = (new Vector2(Math.Abs(lowest.X) + tile.X, Math.Abs(lowest.Y) + tile.Y) * scale).ToPoint();
                                    Point dst = (new Vector2(tile.SourceX, tile.SourceY) * scale).ToPoint();

                                    if (!dooverlap)
                                        foreach (Point p in (from x in Enumerable.Range(0, (int)(Tile.size * scale.X))
                                                             from y in Enumerable.Range(0, (int)(Tile.size * scale.Y))
                                                             orderby y, x ascending
                                                             select new Point(x, y)))
                                        {
                                            Color input = inTex[src.X + p.X, src.Y + p.Y];
                                            Color current = texids[tile.TextureID][dst.X + p.X, dst.Y + p.Y];

                                            Point unscaledLocation = tile.Source.Location;
                                            unscaledLocation.Offset(p.ToVector2() / scale);
                                            Color? output = ChangeColor(current, input, unscaledLocation, tile.TextureID, overlap);
                                            if (output.HasValue)
                                                if (output.Value.A != 0)
                                                    texids[tile.TextureID][dst.X + p.X, dst.Y + p.Y] = output.Value;
                                                else continue;
                                            else break;

                                            //if (input.A != 0)
                                            //{
                                            //    if (current.A == 0)
                                            //        texids[tile.TextureID][dst.X + p.X, dst.Y + p.Y] = input;
                                            //    else if (current == input)
                                            //        continue; // colors are same don't fret.
                                            //    else
                                            //    {
                                            //        Point unscaledLocation = tile.Source.Location;
                                            //        unscaledLocation.Offset(p.ToVector2() / scale);

                                            //        IEnumerable<byte> o = (from t1 in tiles
                                            //                               where t1.TextureID == tile.TextureID && t1.ExpandedSource.Contains(unscaledLocation)
                                            //                               select t1.PaletteID).Distinct();
                                            //        if (o.Count() > 1)
                                            //        {
                                            //            o.ForEach(x => overlap[tile.TextureID].Add(x));
                                            //            break;
                                            //        }
                                            //        else texids[tile.TextureID][dst.X + p.X, dst.Y + p.Y] = input;
                                            //    }
                                            //}
                                        }
                                    else if (overlap[tile.TextureID].Count > 1 && dooverlap)
                                    {
                                        TextureIDPaletteID key = new TextureIDPaletteID { PaletteID = tile.PaletteID, TextureID = tile.TextureID };
                                        texidspalette.TryAdd(key, new TextureBuffer(Width, Height));

                                        foreach (Point p in (from x in Enumerable.Range(0, (int)(Tile.size * scale.X))
                                                             from y in Enumerable.Range(0, (int)(Tile.size * scale.Y))
                                                             select new Point(x, y)))
                                        {
                                            Color input = inTex[src.X + p.X, src.Y + p.Y];
                                            if (input.A != 0)
                                            {
                                                texidspalette[key][dst.X + p.X, dst.Y + p.Y] = inTex[src.X + p.X, src.Y + p.Y];
                                            }
                                        }
                                    }
                                    else continue;
                                }
                            }
                        }
                    }
                }
                //save new reswizzles
                foreach (KeyValuePair<byte, TextureBuffer> tid in texids)
                {
                    string path = Path.Combine(folder,
                        $"{fieldname}_{tid.Key}.png");
                    //save image.
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (Texture2D outTex = (Texture2D)tid.Value)
                            outTex.SaveAsPng(fs, Width, Height);
                    }
                }
                foreach (KeyValuePair<TextureIDPaletteID, TextureBuffer> tid in texidspalette)
                {
                    string path = Path.Combine(folder,
                        $"{fieldname}_{tid.Key.TextureID}_{tid.Key.PaletteID}.png");
                    //save image.
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (Texture2D outTex = (Texture2D)tid.Value)
                            outTex.SaveAsPng(fs, Width, Height);
                    }
                }
                Process.Start(folder);
            }
        }

        public Tiles TilesUnderMouse() => new Tiles(tiles.Where(x =>
        x.X < MouseLocation.X && x.X + 16 > MouseLocation.X &&
        x.Y < MouseLocation.Y && x.Y + 16 > MouseLocation.Y).ToList());

        public void Update()
        {
            if ((CurrentTime += Memory.ElapsedGameTime) > TotalTime)
            {
                CurrentTime = TimeSpan.Zero;
                foreach (KeyValuePair<byte, List<TileQuadTexture>> a in Animations)
                {
                    int i = a.Value.FirstOrDefault(x => x.Enabled)?.AnimationState ?? 0;
                    int max = a.Value.Max(k => k.AnimationState);
                    a.Value.Where(x => x.AnimationState == i).ForEach(x => x.Hide());
                    if (++i >= max)
                        i = 0;
                    a.Value.Where(x => x.AnimationState == i).ForEach(x => x.Show());
                }
            }

            float Width = tiles.Width;
            float Height = tiles.Height;

            if (Module.Toggles.HasFlag(Module._Toggles.Perspective)) //perspective mode shows gabs in the tiles.
            {
                //finds the min zoom out to fit the entire image in frame.
                Vector2 half = new Vector2(Width / 2f, Height / 2f);
                float fieldOfView = MathHelper.ToRadians(70);
                float getOppositeSide(float side, float angle)
                {
                    return (float)(Math.Tan(angle) * side);
                }
                half.X = getOppositeSide(half.X, MathHelper.ToRadians(45));
                half.Y = getOppositeSide(half.Y, MathHelper.ToRadians(45));
                float minDistancefromBG = -Math.Max(half.X, half.Y);
                if (camPosition.Z > minDistancefromBG)
                    camPosition.Z = minDistancefromBG;

                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView, Memory.graphics.GraphicsDevice.Viewport.AspectRatio, float.Epsilon, 1000f);
                if (!Module.Toggles.HasFlag(Module._Toggles.Menu))
                    viewMatrix = fps_camera.Update(ref camPosition, ref camTarget, ref degrees);
                else viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);
            }
            else
            {
                Viewport vp = Memory.graphics.GraphicsDevice.Viewport;
                Vector2 scale = Memory.Scale(Width, Height, Memory.ScaleMode.FitBoth);
                projectionMatrix = Matrix.CreateOrthographic(vp.Width / scale.X, vp.Height / scale.Y, 0f, 100f);
                viewMatrix = Matrix.CreateLookAt(Vector3.Forward * 10f, Vector3.Zero, Vector3.Up);
            }
            Vector2 ml = InputMouse.Location.ToVector2();
            Vector3 ml3d = Memory.graphics.GraphicsDevice.Viewport.Unproject(ml.ToVector3(), projectionMatrix, viewMatrix, worldMatrix);
            ml3d.Y *= -1;
            MouseLocation = ml3d;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                TextureIDs?.ForEach(x => x.Value?.Dispose());
                TextureIDsPalettes?.ForEach(x => x.Value?.Dispose());
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        private static Color blend0(Color baseColor, Color color)
        {//ClassicSpriteBatch
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R + color.R / 2, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.R + color.G / 2, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B + color.B / 2, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private static Color blend1(Color baseColor, Color color)
        {//ClassicSpriteBatch
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R + color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G + color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B + color.B, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private static Color blend2(Color baseColor, Color color)
        {//ClassicSpriteBatch
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R - color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G - color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B - color.B, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private static Color blend3(Color baseColor, Color color)
        {//ClassicSpriteBatch
            Color r;
            r.R = (byte)MathHelper.Clamp((byte)(baseColor.R + (color.R / 4)), 0, 255);
            r.G = (byte)MathHelper.Clamp((byte)(baseColor.G + (color.G / 4)), 0, 255);
            r.B = (byte)MathHelper.Clamp((byte)(baseColor.B + (color.B / 4)), 0, 255);
            r.A = 0xFF;
            return r;
        }

        private Color? ChangeColor(Color current, Color input, Point _p, byte TextureID, Dictionary<byte, HashSet<byte>> overlap)
        {
            if (input.A != 0)
            {
                if (current.A == 0 || current == input)
                    return input;
                else
                {
                    IEnumerable<byte> o = (from t1 in tiles
                                           where t1.TextureID == TextureID && t1.ExpandedSource.Contains(_p)
                                           select t1.PaletteID).Distinct();
                    if (o.Count() > 1)
                    {
                        o.ForEach(x => overlap[TextureID].Add(x));
                        return null;
                    }
                    else return input; // two tiles same palette is drawing to same place
                }
            }
            return Color.TransparentBlack;
        }

        private void DrawBackgroundQuad(TileQuadTexture quad, bool forceBlendModeNone = false, Vector2 scale2 = default)
        {
            VertexPositionTexture[] temp = quad;
            Tile tile = (Tile)quad;
            ate.Texture = quad;
            if (scale2 != default)
            {
                temp = (VertexPositionTexture[])temp.Clone();
                Matrix scale = Matrix.CreateScale(scale2.ToVector3());
                for (int i = 0; i < temp.Length; i++)
                {
                    VertexPositionTexture t = temp[i];
                    t.Position = Vector3.Transform(temp[i].Position, scale);
                    temp[i] = t;
                }
            }
            DrawBackgroundQuadsSetBendMode(forceBlendModeNone ? BlendMode.none : tile.BlendMode);
            foreach (EffectPass pass in ate.CurrentTechnique.Passes)
            {
                pass.Apply();

                Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                vertexData: temp, vertexOffset: 0, primitiveCount: 2);
            }
        }

        private void DrawBackgroundQuads()
        {
            if (!Module.Toggles.HasFlag(Module._Toggles.Quad)) return;
            DrawBackgroundQuadsStart();
            foreach (TileQuadTexture quad in quads.Where(x => x.Enabled))
            {
                DrawBackgroundQuad(quad);
            }
        }

        private void DrawBackgroundQuadsSetBendMode(BlendMode bm)
        {
            ate.Alpha = 1f;
            Color half = new Color(.5f, .5f, .5f, 1f);
            Color quarter = new Color(.25f, .25f, .25f, 1f);
            Color full = Color.White;
            switch (bm)
            {
                //If we deswizzled and merged the (BlendModes != BlendMode.none) tiles
                // we can change SamplerState to Anisotropic.
                //But swizzled textures are a Texture Atlas so it will draw bad pixels from near by.
                case BlendMode.none:
                default:
                    Memory.graphics.GraphicsDevice.BlendFactor = full;
                    Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.add:
                    Memory.graphics.GraphicsDevice.BlendFactor = full;
                    Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Add;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.subtract:
                    Memory.graphics.GraphicsDevice.BlendFactor = full;
                    Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Subtract;
                    ate.Alpha = .85f; //doesn't darken so much.
                                      //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.halfadd:
                    Memory.graphics.GraphicsDevice.BlendFactor = half;
                    Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Add_BlendFactor;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;

                case BlendMode.quarteradd:
                    Memory.graphics.GraphicsDevice.BlendFactor = quarter;
                    Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Add_BlendFactor;
                    //Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    break;
            }
        }

        private void DrawBackgroundQuadsStart()
        {
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;

            effect.Projection = projectionMatrix; effect.View = viewMatrix; effect.World = worldMatrix;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            effect.TextureEnabled = true;
            ate.VertexColorEnabled = false;
            effect.VertexColorEnabled = false;
        }

        private void DrawSpriteBatch()
        {
            if (!Module.Toggles.HasFlag(Module._Toggles.ClassicSpriteBatch)) return;
            List<KeyValuePair<BlendMode, Texture2D>> _drawtextures = GetTexturesReadyToDrawClassicSpriteBatch();
            bool open = false;
            BlendMode lastbm = BlendMode.none;
            float alpha = 1f;
            if (_drawtextures != null)
                foreach (KeyValuePair<BlendMode, Texture2D> kvp in _drawtextures)
                {
                    if (!open || lastbm != kvp.Key)
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

                            case BlendMode.halfadd:
                                Memory.SpriteBatchStart(bs: Memory.blendState_Add, ss: SamplerState.AnisotropicClamp);
                                break;

                            case BlendMode.quarteradd:
                                Memory.SpriteBatchStart(bs: Memory.blendState_Add, ss: SamplerState.AnisotropicClamp);
                                break;

                            case BlendMode.add:
                                Memory.SpriteBatchStart(bs: Memory.blendState_Add, ss: SamplerState.AnisotropicClamp);
                                break;

                            case BlendMode.subtract:
                                alpha = .9f;
                                Memory.SpriteBatchStart(bs: Memory.blendState_Subtract, ss: SamplerState.AnisotropicClamp);
                                break;
                        }
                        lastbm = kvp.Key;
                    }
                    Texture2D tex = kvp.Value;
                    Rectangle src = new Rectangle(0, 0, tex.Width, tex.Height);
                    Rectangle dst = src;

                    dst.Size = (dst.Size.ToVector2() * Memory.Scale(tex.Width, tex.Height, Memory.ScaleMode.FitBoth)).ToPoint();
                    //In game I think we'd keep the field from leaving the screen edge but would center on the Squall and the party when it can.
                    //I setup scaling after noticing the field didn't size with the screen. I set it to center on screen.
                    dst.Offset(Memory.Center.X - dst.Center.X, Memory.Center.Y - dst.Center.Y);
                    Memory.spriteBatch.Draw(tex, dst, src, Color.White * alpha);
                    //new Microsoft.Xna.Framework.Rectangle(0, 0, 1280 + (width - 320), 720 + (height - 224)),
                    //new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height)
                }

            if (open)
                Memory.SpriteBatchEnd();
        }

        private void DrawWalkMesh()
        {//todo move into walkmesh class. was only because at the time I thought i'd need the background data.
            if (!Module.Toggles.HasFlag(Module._Toggles.WalkMesh)) return;

            effect.TextureEnabled = false;
            Memory.graphics.GraphicsDevice.BlendFactor = Color.White;
            Memory.graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            //using (DepthStencilState depthStencilState = new DepthStencilState() { DepthBufferEnable = true })
            using (RasterizerState rasterizerState = new RasterizerState() { CullMode = CullMode.None })
            {
                Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;//depthStencilState;
                Memory.graphics.GraphicsDevice.RasterizerState = rasterizerState;
                ate.Texture = null;
                ate.VertexColorEnabled = true;
                effect.VertexColorEnabled = true;
                //camPosition = Module.Cameras[0].Position;
                effect.World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);// Module.Cameras[0].CreateWorld();
                float fieldOfView = MathHelper.ToRadians(70);
                //effect.View = //Module.Cameras[0].CreateLookAt();
                //effect.Projection = Module.Cameras[0].CreateProjection();

                effect.Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, Memory.graphics.GraphicsDevice.Viewport.AspectRatio, float.Epsilon, 1000f);
                if (!Module.Toggles.HasFlag(Module._Toggles.Menu))
                    effect.View = fps_camera.Update(ref camPosition, ref camTarget, ref degrees);
                else
                    effect.View = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                    vertexData: Module.WalkMesh.Vertices.ToArray(), vertexOffset: 0, primitiveCount: Module.WalkMesh.Count);
                }
            }
        }

        private void DumpRawTexture(byte[] mimb)
        {
            if (Memory.EnableDumpingData || Module.Toggles.HasFlag(Module._Toggles.DumpingData))
            {
                MemoryStream ms;

                string path = Path.Combine(Module.GetFolder(),
                    $"{Module.GetFieldName()}_raw_{{0}}bit_{{1}}.png");
                using (BinaryReader br = new BinaryReader(ms = new MemoryStream(mimb)))
                {

                    long startPixel = TextureType.PaletteSectionSize;
                    

                    process(8);
                    process(4);
                    process(16);
                    process(24);
                    process(24,true);
                    void process(byte bit, bool alt  = false)
                    {
                        float adj = (bit / 8f);
                        int Width = (bit == 24)? Width = TextureType.Width :(int)(TextureType.Width / adj);
                        int Height = checked((int)Math.Ceiling((mimb.Length - TextureType.PaletteSectionSize) / TextureType.Width / (bit == 24 ? adj : 1f)));

                        if (bit == 24 && alt)
                        {
                            Width = (int)Math.Ceiling(TextureType.Width / adj);
                            Height *= (int)adj;
                        }

                        TextureBuffer buffer = new TextureBuffer(Width, Height, false);
                        foreach (KeyValuePair<byte, Color[]> clut in Cluts)
                        {
                            ms.Seek(startPixel, SeekOrigin.Begin);
                            int i = 0;
                            byte colorkey = 0;
                            int lastrow = 0;
                            while (ms.Position + Math.Ceiling(adj) < ms.Length)
                            {
                                int row = (i / Width);
                                Color input = Color.TransparentBlack;
                                if (bit == 24) //just to see if anything is there. don't think there is a real usage of 24 bit.
                                {
                                    if (alt && lastrow != row && row % 3 ==0) i++;
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
                                    colorkey = br.ReadByte();
                                    input = clut.Value[colorkey];
                                }
                                else if (bit == 4)
                                {
                                    //i++;
                                    if (i % 2 == 0)
                                    {
                                        colorkey = br.ReadByte();
                                        input = clut.Value[colorkey & 0xf];
                                    }
                                    else
                                    {
                                        input = clut.Value[(colorkey & 0xf0) >> 4];
                                    }
                                }
                                else throw new ArgumentException($"{nameof(bit)} is {bit}, it may only be 4 or 8.");
                                if (i < buffer.Count)
                                    buffer[i] = input;
                                else break;
                                i++;
                                lastrow = row;
                            }

                            using (Texture2D tex = (Texture2D)buffer)
                            using (FileStream fs = new FileStream(string.Format(path, bit, $"{clut.Key}{(alt?"a":"")}"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                            {
                                tex.SaveAsPng(fs, Width, Height);
                            }
                            if (bit > 8) break;
                        }
                    }
                }
            }
        }

        private void FindOverlappingTilesClassicSpriteBatch() => (from t1 in tiles
                                                                  from t2 in tiles
                                                                  where t1.TileID < t2.TileID
                                                                  where t1.BlendMode == BlendMode.none
                                                                  where t1.Intersect(t2)
                                                                  orderby t1.TileID, t2.TileID ascending
                                                                  select new[] { t1, t2 }

                                     ).ForEach(x => x[1].OverLapID = checked((byte)(x[0].OverLapID + 1)));

        private byte GetColorKeyClassicSpriteBatch(byte[] mimb, int textureWidth, int startPixel, int x, int y, bool is8Bit)
        {
            if (is8Bit)
                return mimb[startPixel + x + (y * textureWidth)];
            else
            {
                byte tempKey = mimb[startPixel + x / 2 + (y * textureWidth)];
                if (x % 2 == 1)
                    return checked((byte)((tempKey & 0xf0) >> 4));
                else
                    return checked((byte)(tempKey & 0xf));
            }
        }

        private List<KeyValuePair<BlendMode, Texture2D>> GetTexturesReadyToDrawClassicSpriteBatch() =>
                                                                                    Textures?.OrderByDescending(kvp_Z => kvp_Z.Key)
            .SelectMany(kvp_LayerID => kvp_LayerID.Value.OrderBy(x => kvp_LayerID.Key)
            .SelectMany(kvp_AnimationID => kvp_AnimationID.Value.OrderBy(x => kvp_AnimationID.Key))
            .SelectMany(kvp_AnimationState => kvp_AnimationState.Value.OrderBy(x => kvp_AnimationState.Key))
            .SelectMany(kvp_OverlapID => kvp_OverlapID.Value.OrderBy(x => kvp_OverlapID.Key))
            .SelectMany(kvp_BlendMode => kvp_BlendMode.Value)).ToList();

        private bool GetTextureType(byte[] mimb, byte[] mapb)
        {
            if (mimb == null || mapb == null)
                return false;
            TextureType = BackgroundTextureType.GetTextureType(mimb);
            if (TextureType == default)
            {//unsupported feild dump data so can check it.
                string path = Path.Combine(Path.GetTempPath(), "Fields", $"{Memory.FieldHolder.fields[Memory.FieldHolder.FieldID]}");
                using (BinaryWriter bw = new BinaryWriter(new FileStream($"{path}.mim", FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    bw.Write(mimb);
                    Debug.WriteLine($"Saved {path}.mim");
                }
                using (BinaryWriter bw = new BinaryWriter(new FileStream($"{path}.map", FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
                {
                    bw.Write(mapb);
                    Debug.WriteLine($"Saved {path}.map");
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the TextureHandler used by the tile.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>Texture handler used by tile</returns>
        private TextureHandler GetTextureUsedByTile(Tile tile)
        {
            if (TextureIDsPalettes != null)
            {
                TextureHandler tidp = TextureIDsPalettes.FirstOrDefault(x => x.Key.PaletteID == tile.PaletteID && x.Key.TextureID == tile.TextureID).Value;
                if (tidp != default)
                    return tidp;
            }

            if (TextureIDs != null)
            {
                TextureHandler tid = TextureIDs.FirstOrDefault(x => x.Key == tile.TextureID).Value;
                if (tid != default)
                    return tid;
            }
            return null;
        }

        private void LoadPalettes(byte[] mimb)
        {
            int Offset = /*Memory.FieldHolder.FieldID == 76 ? 0 :*/ TextureType?.BytesSkippedPalettes ?? 0;
            Cluts CLUT = tiles != null ? new Cluts(tiles.Select(x => x.PaletteID).Distinct().ToDictionary(x => x, x => new Color[colorsPerPalette]), false) :
                new Cluts(Enumerable.Range(0, 16).Select(x => (byte)x).ToDictionary(x => x, x => new Color[colorsPerPalette]), false);
            using (BinaryReader br = new BinaryReader(new MemoryStream(mimb)))
                foreach (KeyValuePair<byte, Color[]> clut in CLUT)
                {
                    int palettePointer = Offset + ((clut.Key) * bytesPerPalette);
                    br.BaseStream.Seek(palettePointer, SeekOrigin.Begin);
                    for (int i = 0; i < colorsPerPalette; i++)
                        clut.Value[i] = Texture_Base.ABGR1555toRGBA32bit(br.ReadUInt16());
                }
            Cluts = CLUT;
            SaveCluts();
        }

        private void LoadTiles(byte[] mapb) => tiles = mapb == null ? default : Tiles.Load(mapb, TextureType.Type);

        private int LoadUpscaleBackgrounds(string path)
        {
            if (Directory.Exists(path))
            {
                List<string> files = Directory.EnumerateFiles(path, $"*{Module.GetFieldName()}*.png", SearchOption.AllDirectories).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                if (files.Count > 0)
                {
                    this.TextureIDs = new Dictionary<byte, TextureHandler>();
                    string escapedname = Regex.Escape(Module.GetFieldName());
                    Regex regex = new Regex(@".+" + escapedname + @"_(\d{1,2})\.png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    foreach (Match file in files.Select(x => regex.Match(x)))
                    {
                        if (file.Groups.Count > 1 && byte.TryParse(file.Groups[1].Value, out byte b))
                        {
                            if (b >= 13) b -= 13;
                            if (!this.TextureIDs.ContainsKey(b))
                            {
                                string alt = $"{Module.GetFieldName()}_{b + 13}.png";
                                this.TextureIDs.Add(b, TextureHandler.CreateFromPng(File.Exists(alt) ? alt : file.Value, 256, 256, 0, true, true));
                            }
                        }
                    }
                    SaveSwizzled(this.TextureIDs.ToDictionary(x => x.Key, x => (Texture2D)x.Value));
                    this.TextureIDsPalettes = new Dictionary<TextureIDPaletteID, TextureHandler>();
                    Regex regex2 = new Regex(@".+" + escapedname + @"_(\d{1,2})_(\d{1,2})\.png", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    foreach (Match file in files.Select(x => regex2.Match(x)))
                    {
                        TextureIDPaletteID tipi;
                        if (file.Groups.Count > 1 && byte.TryParse(file.Groups[1].Value, out byte b) && byte.TryParse(file.Groups[2].Value, out byte b2))
                        {
                            if (b >= 13) b -= 13;
                            if (!this.TextureIDsPalettes.ContainsKey(tipi = new TextureIDPaletteID { PaletteID = b2, TextureID = b }))
                            {
                                string alt = $"{Module.GetFieldName()}_{b + 13}_{b2}.png";
                                this.TextureIDsPalettes.Add(tipi, TextureHandler.CreateFromPng(File.Exists(alt) ? alt : file.Value, 256, 256, b2, true, true));
                            }
                        }
                        foreach (IGrouping<byte, KeyValuePair<TextureIDPaletteID, TextureHandler>> groups in TextureIDsPalettes.Where(x => TextureIDsPalettes.Count(y => y.Key.TextureID == x.Key.TextureID) > 1).GroupBy(x => x.Key.PaletteID))

                            foreach (KeyValuePair<TextureIDPaletteID, TextureHandler> kvp_group in groups)
                            {
                                Dictionary<byte, Texture2D> _TextureIDs = groups.ToDictionary(x => x.Key.TextureID, x => (Texture2D)x.Value);
                                SaveSwizzled(_TextureIDs, $"_{kvp_group.Key.PaletteID}");
                                break;
                            }
                    }
                }
            }
            int count = (this.TextureIDs?.Count ?? 0) + (this.TextureIDsPalettes?.Count ?? 0);
            return count;
        }

        //private void SaveSwizzled(string suf = "") => SaveSwizzled(TextureIDs, suf);
        private void OldSaveDeswizzled()
        {
            if (Memory.EnableDumpingData || (Module.Toggles.HasFlag(Module._Toggles.DumpingData) && Module.Toggles.HasFlag(Module._Toggles.ClassicSpriteBatch)))
            {
                string fieldname = Module.GetFieldName();
                string folder = Module.GetFolder(fieldname);
                string path;
                //List<KeyValuePair<BlendModes, Texture2D>> _drawtextures = drawtextures();
                foreach (KeyValuePair<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>>> kvp_Z in Textures)
                    foreach (KeyValuePair<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>> kvp_Layer in kvp_Z.Value)
                        foreach (KeyValuePair<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>> kvp_AnimationID in kvp_Layer.Value)
                            foreach (KeyValuePair<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>> kvp_AnimationState in kvp_AnimationID.Value)
                                foreach (KeyValuePair<byte, ConcurrentDictionary<BlendMode, Texture2D>> kvp_OverlapID in kvp_AnimationState.Value)
                                    foreach (KeyValuePair<BlendMode, Texture2D> kvp in kvp_OverlapID.Value)
                                    {
                                        path = Path.Combine(folder,
                                            $"{fieldname}_{kvp_Z.Key.ToString("D4")}.{kvp_Layer.Key}.{kvp_AnimationID.Key}.{kvp_AnimationState.Key}.{kvp_OverlapID.Key}.{(int)kvp.Key}.png");
                                        using (FileStream fs = new FileStream(path,
                                            FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                                            kvp.Value.SaveAsPng(
                                                fs,
                                                kvp.Value.Width, kvp.Value.Height);
                                    }
            }
        }

        private bool ParseBackgroundClassicSpriteBatch(byte[] mimb)
        {
            if (!Module.Toggles.HasFlag(Module._Toggles.ClassicSpriteBatch)) return true;
            if (mimb == null || (tiles?.Count ?? 0) == 0)
                return false;

            FindOverlappingTilesClassicSpriteBatch();
            //FindSameXYTilesSource();
            Point lowest = new Point(tiles.Min(x => x.X), tiles.Min(x => x.Y));
            Point maximum = new Point(tiles.Max(x => x.X), tiles.Max(x => x.Y));

            Height = Math.Abs(lowest.Y) + maximum.Y + Tile.size; //224
            Width = Math.Abs(lowest.X) + maximum.X + Tile.size; //320
                                                                //Color[] finalImage = new Color[height * width]; //ARGB;
                                                                //Color[] finalOverlapImage = new Color[height * width];
                                                                //tex = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
                                                                //texOverlap = new Texture2D(Memory.graphics.GraphicsDevice, width, height);
            IOrderedEnumerable<byte> layers = tiles.Select(x => x.LayerID).Distinct().OrderBy(x => x);
            Debug.WriteLine($"FieldID: {Memory.FieldHolder.FieldID}, Layers: {layers.Count()}, ({string.Join(",", layers.ToArray())}) ");
            byte MaximumLayer = layers.Max();
            byte MinimumLayer = layers.Min();
            IOrderedEnumerable<ushort> BufferDepth = tiles.Select(x => x.Z).Distinct().OrderByDescending(x => x); // larger number is farther away.

            List<Tile> sortedtiles = tiles.OrderBy(x => x.OverLapID).ThenByDescending(x => x.Z).ThenBy(x => x.LayerID).ThenBy(x => x.AnimationID).ThenBy(x => x.AnimationState).ThenBy(x => x.BlendMode).ToList();

            if (Textures != null)
            {
                foreach (Texture2D tex in GetTexturesReadyToDrawClassicSpriteBatch().Select(x => x.Value))
                    tex.Dispose();
            }
            Textures = new ConcurrentDictionary<ushort, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>>>();
            ushort z = 0;
            byte layerID = 0;
            byte animationID = 0;
            byte animationState = 0;
            byte overlapID = 0;
            BlendMode blendmode = BlendMode.none;
            TextureBuffer texturebuffer = null;
            bool hasColor = false;
            void convertColorToTexture2d()
            {
                if (!hasColor || texturebuffer == null) return;
                hasColor = false;
                ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>> dictLayerID = Textures.GetOrAdd(z, new ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>>());
                ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>> dictAnimationID = dictLayerID.GetOrAdd(layerID, new ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>>());
                ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>> dictAnimationState = dictAnimationID.GetOrAdd(animationID, new ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>>());
                ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>> dictOverlapID = dictAnimationState.GetOrAdd(animationState, new ConcurrentDictionary<byte, ConcurrentDictionary<BlendMode, Texture2D>>());
                ConcurrentDictionary<BlendMode, Texture2D> dictblend = dictOverlapID.GetOrAdd(overlapID, new ConcurrentDictionary<BlendMode, Texture2D>());
                Texture2D tex = dictblend.GetOrAdd(blendmode, new Texture2D(Memory.graphics.GraphicsDevice, Width, Height));
                texturebuffer.SetData(tex);
            }
            for (int i = 0; i < sortedtiles.Count; i++)
            {
                Tile previousTile = (i > 0) ? previousTile = sortedtiles[i - 1] : null;
                Tile tile = sortedtiles[i];
                if (texturebuffer == null || previousTile == null ||
                    (previousTile.Z != tile.Z ||
                    previousTile.LayerID != tile.LayerID ||
                    previousTile.BlendMode != tile.BlendMode ||
                    previousTile.AnimationID != tile.AnimationID ||
                    previousTile.AnimationState != tile.AnimationState ||
                    previousTile.OverLapID != tile.OverLapID))
                {
                    convertColorToTexture2d();
                    texturebuffer = new TextureBuffer(Width, Height, false);
                    z = tile.Z;
                    layerID = tile.LayerID;
                    blendmode = tile.BlendMode;
                    animationID = tile.AnimationID;
                    animationState = tile.AnimationState;
                    overlapID = tile.OverLapID;
                }

                int palettePointer = TextureType.BytesSkippedPalettes + ((tile.PaletteID) * bytesPerPalette);
                int sourceImagePointer = bytesPerPalette * TextureType.Palettes;
                const int texturewidth = 128;
                int startPixel = sourceImagePointer + tile.SourceX + texturewidth * tile.TextureID + (TextureType.Width * tile.SourceY);
                Point real = new Point(Math.Abs(lowest.X) + tile.X, Math.Abs(lowest.Y) + tile.Y);
                int realDestinationPixel = ((real.Y * Width) + real.X);

                Rectangle dst = new Rectangle(real.X, real.Y, Tile.size, Tile.size);
                if (tile.Is4Bit)
                {
                    startPixel -= tile.SourceX / 2;
                }
                for (int y = 0; y < Tile.size; y++)
                    for (int x = 0; x < Tile.size; x++)
                    {
                        byte colorKey = GetColorKeyClassicSpriteBatch(mimb, TextureType.Width, startPixel, x, y, tile.Is8Bit);
                        ushort color16bit = BitConverter.ToUInt16(mimb, 2 * colorKey + palettePointer);
                        if (color16bit == 0) // 0 is Color.TransparentBlack So we skip it.
                            continue;
                        Color color = Texture_Base.ABGR1555toRGBA32bit(color16bit);

                        int pos = realDestinationPixel + (x) + (y * Width);
                        Color bufferedcolor = texturebuffer[pos];
                        if (blendmode < BlendMode.none)
                        {
                            if (color == Color.Black)
                                continue;

                            if (blendmode == BlendMode.subtract)
                            {
                                if (bufferedcolor.A != 0)
                                    color = blend2(bufferedcolor, color);
                            }
                            else
                            {
                                if (blendmode == BlendMode.quarteradd)
                                    color = Color.Multiply(color, .25f);
                                else if (blendmode == BlendMode.halfadd)
                                    color = Color.Multiply(color, .5f);
                                if (bufferedcolor.A != 0)
                                    color = blend1(bufferedcolor, color);
                            }
                        }
                        else if (bufferedcolor.A != 0)
                        {
                            throw new Exception("Color is already set something may be wrong.");
                        }
                        color.A = 0xFF;
                        texturebuffer[pos] = color;
                        hasColor = true;
                    }
            }
            convertColorToTexture2d(); // gets leftover colors from last batch and makes a texture.
            OldSaveDeswizzled();
            return true;
        }

        /// <summary>
        /// <para>Create a swizzeled Textures with one palette.</para>
        /// <para>Few exceptions where tiles conflict and need seperate files.</para>
        /// </summary>
        /// <param name="mimb">Image Data .mim file</param>
        /// <param name="mapb">Image/Tile Map .map file</param>
        /// <returns></returns>
        private bool ParseBackgroundQuads(byte[] mimb)
        {
            if (mimb == null || (tiles?.Count ?? 0) == 0)
                return false;
            string path = Path.Combine(Memory.FF8DIR, "textures");
            int count = LoadUpscaleBackgrounds(path);
            if (count > 0)
            {
                Debug.WriteLine($"Loaded {count} Textures from {path}");
            }
            else
            {
                var UniqueSetOfTileData = tiles.Where(x => x.Draw).Select(x => new { x.TextureID, x.BlendMode, loc = new Point(x.SourceX, x.SourceY), x.Depth, x.PaletteID, x.AnimationID }).Distinct().ToList();
                Width = UniqueSetOfTileData.Max(x => x.loc.X + Tile.size);
                Height = UniqueSetOfTileData.Max(x => x.loc.Y + Tile.size);
                Dictionary<byte, Texture2D> TextureIDs = UniqueSetOfTileData.Select(x => x.TextureID).Distinct().ToDictionary(x => x, x => Memory.graphics != null ? new Texture2D(Memory.graphics.GraphicsDevice, 256, 256):null);
                Dictionary<byte, HashSet<byte>> overlap = tiles.Select(x => x.TextureID).Distinct().ToDictionary(x => x, x => new HashSet<byte>());
                using (BinaryReader br = new BinaryReader(new MemoryStream(mimb)))
                {
                    foreach (KeyValuePair<byte, Texture2D> kvp in TextureIDs)
                    {
                        GenTexture(kvp.Key, kvp.Value);
                    }
                    SaveSwizzled(TextureIDs);
                    string fieldname = Module.GetFieldName();
                    this.TextureIDs = TextureIDs.ToDictionary(x => x.Key, x => TextureHandler.Create($"{ fieldname }_{x.Key}", new Texture2DWrapper(x.Value), ushort.MaxValue));

                    if (overlap.Any(x => x.Value.Count > 1))
                    {
                        Dictionary<TextureIDPaletteID, Texture2D> TextureIDsPalettes = UniqueSetOfTileData.Where(x => overlap[x.TextureID].Contains(x.PaletteID)).Select(x => new TextureIDPaletteID { TextureID = x.TextureID, PaletteID = x.PaletteID }).Distinct().ToDictionary(x => x, x => new Texture2D(Memory.graphics.GraphicsDevice, 256, 256));
                        this.TextureIDsPalettes = TextureIDsPalettes.ToDictionary(x => x.Key, x => TextureHandler.Create($"{ fieldname }_{x.Key.TextureID}", new Texture2DWrapper(x.Value), x.Key.PaletteID));
                        foreach (KeyValuePair<TextureIDPaletteID, Texture2D> kvp in TextureIDsPalettes)
                        {
                            GenTexture(kvp.Key.TextureID, kvp.Value, kvp.Key.PaletteID);
                        }
                        foreach (IGrouping<byte, KeyValuePair<TextureIDPaletteID, Texture2D>> groups in TextureIDsPalettes.Where(x => TextureIDsPalettes.Count(y => y.Key.TextureID == x.Key.TextureID) > 1).GroupBy(x => x.Key.PaletteID))

                            foreach (KeyValuePair<TextureIDPaletteID, Texture2D> kvp_group in groups)
                            {
                                Dictionary<byte, Texture2D> _TextureIDs = groups.ToDictionary(x => x.Key.TextureID, x => x.Value);
                                SaveSwizzled(_TextureIDs, $"_{kvp_group.Key.PaletteID}");
                                break;
                            }
                    }
                    void GenTexture(byte texID, Texture2D tex2d, byte? inPaletteID = null)
                    {
                        if (tex2d == null) return;
                        TextureBuffer tex = new TextureBuffer(tex2d.Width, tex2d.Height, false);

                        foreach (var tile in UniqueSetOfTileData.Where(x => x.TextureID == texID && (!inPaletteID.HasValue || inPaletteID.Value == x.PaletteID)))
                        {
                            bool is4Bit = Tile.Test4Bit(tile.Depth);
                            bool is8Bit = Tile.Test8Bit(tile.Depth);
                            bool is16Bit = Tile.Test16Bit(tile.Depth);

                            long startPixel = TextureType.PaletteSectionSize + (tile.loc.X / (is4Bit ? 2 : 1)) + (texturePageWidth * tile.TextureID) + (TextureType.Width * tile.loc.Y);

                            byte Colorkey = 0;
                            foreach (Point p in (from x in Enumerable.Range(0, Tile.size)
                                                 from y in Enumerable.Range(0, Tile.size)
                                                 orderby y, x ascending
                                                 select new Point(x, y)))
                            {
                                br.BaseStream.Seek(startPixel + (p.Y * TextureType.Width / (is16Bit?2:1)) + (p.X / (is4Bit ? 2 : 1)), SeekOrigin.Begin);

                                Point _p = new Point(p.X + tile.loc.X, p.Y + tile.loc.Y);

                                byte paletteID = tile.PaletteID;
                                Color input = default;
                                if (is8Bit)
                                {
                                    input = Cluts[paletteID][br.ReadByte()];
                                }
                                else if(is16Bit)
                                {
                                    input = Texture_Base.ABGR1555toRGBA32bit(br.ReadUInt16());
                                }
                                else if(is4Bit)
                                {
                                    if (p.X % 2 == 0)
                                    {
                                        Colorkey = br.ReadByte();
                                        input = Cluts[paletteID][Colorkey & 0xf];
                                    }
                                    else
                                    {
                                        input = Cluts[paletteID][(Colorkey & 0xf0) >> 4];
                                    }
                                }

                                if (!inPaletteID.HasValue) // forcing a palette happens post overlap test. So shouldn't need to rerun test.
                                {
                                    Color current = tex[_p.X, _p.Y];
                                    Color? output = ChangeColor(current, input, _p, tile.TextureID, overlap);
                                    if (output.HasValue)
                                        if (output.Value.A != 0)
                                            tex[_p.X, _p.Y] = output.Value;
                                        else continue;
                                    else break;
                                }
                                else
                                {
                                    if (input.A != 0)
                                        tex[_p.X, _p.Y] = input;
                                }
                            }
                        }
                        tex.SetData(tex2d);
                    }
                }
            }
            //the sort here should be the default draw order. May need changed.
            quads = tiles.Select(x => new TileQuadTexture(x, GetTextureUsedByTile(x), 1f)).Where(x => x.Enabled)
                .OrderByDescending(x => x.GetTile.Z)
                .ThenByDescending(x => x.GetTile.TileID)
                .ThenBy(x => x.GetTile.LayerID)
                .ThenBy(x => x.GetTile.AnimationID)
                .ThenBy(x => x.GetTile.AnimationState)
                .ThenByDescending(x => x.GetTile.BlendMode).ToList();
            Animations = quads.Where(x => x.AnimationID != 0xFF).Select(x => x.AnimationID).Distinct().ToDictionary(x => x, x => quads.Where(y => y.AnimationID == x).OrderBy(y => y.AnimationState).ToList());
            Animations.ForEach(x => x.Value.Where(y => y.AnimationState != 0).ForEach(w => w.Hide()));
            TotalTime = TimeSpan.FromMilliseconds(1000f / 10f);
            CurrentTime = TimeSpan.Zero;
            return true;
        }

        private void SaveCluts()
        {
            if (Memory.EnableDumpingData || Module.Toggles.HasFlag(Module._Toggles.DumpingData))
            {
                string path = Path.Combine(Module.GetFolder(),
                    $"{Module.GetFieldName()}_Clut.png");
                Cluts.Save(path);
            }
        }

        private void SaveSwizzled(Dictionary<byte, Texture2D> _TextureIDs, string suf = "")
        {
            if (Memory.EnableDumpingData || Module.Toggles.HasFlag(Module._Toggles.DumpingData))
            {
                string fieldname = Module.GetFieldName();
                string folder = Module.GetFolder(fieldname);
                string path;
                foreach (KeyValuePair<byte, Texture2D> kvp in _TextureIDs)
                {
                    path = Path.Combine(folder,
                        $"{fieldname}_{kvp.Key}{suf}.png");
                    if (File.Exists(path))
                        continue;
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    { kvp.Value.SaveAsPng(fs, kvp.Value.Width, kvp.Value.Height); }
                }
            }
        }

        #endregion Methods

        // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }
}