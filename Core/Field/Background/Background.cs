using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

        /// <summary>
        /// Palettes/Color Lookup Tables
        /// </summary>
        private Cluts Cluts;

        private bool disposedValue = false;

        private BasicEffect effect;

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
        public int Height { get => OutputDims.Height; private set => OutputDims.Height = value; }
        public bool Is4Bit => tiles?.Any(x => x.Is4Bit) ?? false;
        public bool IsAddBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.add) ?? false;
        public bool IsHalfBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.halfadd) ?? false;
        public bool IsQuarterBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.quarteradd) ?? false;
        public bool IsSubtractBlendMode => tiles?.Any(x => x.BlendMode == BlendMode.subtract) ?? false;
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
                ate = new AlphaTestEffect(Memory.graphics.GraphicsDevice),
                effect = new BasicEffect(Memory.graphics.GraphicsDevice),
                camTarget = Vector3.Zero,
                camPosition = new Vector3(0f, 0f, -10f),
                fps_camera = new FPS_Camera(),
                degrees = 90f
            };
            r.worldMatrix = Matrix.CreateWorld(r.camPosition, Vector3.
                          Forward, Vector3.Up);
            r.viewMatrix = Matrix.CreateLookAt(r.camPosition, r.camTarget,
                     Vector3.Up);
            r.GetTiles(mapb);
            r.GetPalettes(mimb);
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                if (!r.ParseBackgroundQuads(mimb, mapb))
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
                if (!r.ParseBackground2D(mimb, mapb))
                {
                    return null;
                }
            }
            finally
            {
                watch.Stop();
                Debug.WriteLine($"{nameof(ParseBackground2D)} took {watch.ElapsedMilliseconds / 1000f} seconds.");
            }
            return r;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

        public void Draw()
        {
            DrawGeometry();
            List<KeyValuePair<BlendMode, Texture2D>> _drawtextures = drawtextures();
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

        public void Update()
        {
            if ((CurrentTime += Memory.gameTime.ElapsedGameTime) > TotalTime)
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
        }

        private float degrees;
        private FPS_Camera fps_camera;
        private Vector3 camTarget;
        private Vector3 camPosition;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //TextureIDs?.ForEach(x => x.Value?.Dispose());
                //TextureIDsPalettes?.ForEach(x => x.Value?.Dispose());
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        private static Color blend0(Color baseColor, Color color)
        {
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R + color.R / 2, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.R + color.G / 2, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B + color.B / 2, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private static Color blend1(Color baseColor, Color color)
        {
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R + color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G + color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B + color.B, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private static Color blend2(Color baseColor, Color color)
        {
            Color r;
            r.R = (byte)MathHelper.Clamp(baseColor.R - color.R, 0, 255);
            r.G = (byte)MathHelper.Clamp(baseColor.G - color.G, 0, 255);
            r.B = (byte)MathHelper.Clamp(baseColor.B - color.B, 0, 255);
            r.A = 0xFF;
            return r;
        }

        private static Color blend3(Color baseColor, Color color)
        {
            Color r;
            r.R = (byte)MathHelper.Clamp((byte)(baseColor.R + (color.R / 4)), 0, 255);
            r.G = (byte)MathHelper.Clamp((byte)(baseColor.G + (color.G / 4)), 0, 255);
            r.B = (byte)MathHelper.Clamp((byte)(baseColor.B + (color.B / 4)), 0, 255);
            r.A = 0xFF;
            return r;
        }

        private void DrawBackground()
        {
            if (!Module.Toggles.HasFlag(Module._Toggles.Quad)) return;
            Memory.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            ate.Projection = projectionMatrix; ate.View = viewMatrix; ate.World = worldMatrix;

            effect.Projection = projectionMatrix; effect.View = viewMatrix; effect.World = worldMatrix;
            Memory.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            effect.TextureEnabled = true;
            //seeing if sorting will matter.
            IOrderedEnumerable<TileQuadTexture> sorted = quads.Where(x => x.Enabled).OrderByDescending(x => x.GetTile.Z).ThenBy(x => x.GetTile.LayerID).ThenBy(x => x.GetTile.AnimationID).ThenBy(x => x.GetTile.AnimationState).ThenBy(x => x.GetTile.BlendMode);
            //foreach (IGrouping<BlendMode, TileQuadTexture> BlendModeGroup in quads.Where(x => x.Enabled).GroupBy(x => x.BlendMode))
            ate.VertexColorEnabled = false;
            effect.VertexColorEnabled = false;
            foreach (TileQuadTexture quad in sorted)
            {
                Color half = new Color(.5f, .5f, .5f, 1f);
                Color quarter = new Color(.25f, .25f, .25f, 1f);
                Color full = Color.White;
                Tile tile = (Tile)quad;
                ate.Texture = quad;
                switch (tile.BlendMode)
                {
                    case BlendMode.none:
                    default:
                        Memory.graphics.GraphicsDevice.BlendFactor = full;
                        Memory.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                        Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                        break;

                    case BlendMode.add:
                        Memory.graphics.GraphicsDevice.BlendFactor = full;
                        Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Add;
                        Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                        break;

                    case BlendMode.subtract:
                        Memory.graphics.GraphicsDevice.BlendFactor = full;
                        Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Subtract;
                        Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                        break;

                    case BlendMode.halfadd:
                        Memory.graphics.GraphicsDevice.BlendFactor = half;
                        Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Add_BlendFactor;
                        Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                        break;

                    case BlendMode.quarteradd:
                        Memory.graphics.GraphicsDevice.BlendFactor = quarter;
                        Memory.graphics.GraphicsDevice.BlendState = Memory.blendState_Add_BlendFactor;
                        Memory.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                        break;
                }
                foreach (EffectPass pass in ate.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    Memory.graphics.GraphicsDevice.DrawUserPrimitives(primitiveType: PrimitiveType.TriangleList,
                    vertexData: (VertexPositionTexture[])quad, vertexOffset: 0, primitiveCount: 2);
                }
            }
        }

        private void DrawGeometry()
        {
            Memory.spriteBatch.GraphicsDevice.Clear(Color.Black);
            DrawBackground();

            DrawWalkMesh();
        }

        private List<KeyValuePair<BlendMode, Texture2D>> drawtextures() =>
                                                            Textures?.OrderByDescending(kvp_Z => kvp_Z.Key)
            .SelectMany(kvp_LayerID => kvp_LayerID.Value.OrderBy(x => kvp_LayerID.Key)
            .SelectMany(kvp_AnimationID => kvp_AnimationID.Value.OrderBy(x => kvp_AnimationID.Key))
            .SelectMany(kvp_AnimationState => kvp_AnimationState.Value.OrderBy(x => kvp_AnimationState.Key))
            .SelectMany(kvp_OverlapID => kvp_OverlapID.Value.OrderBy(x => kvp_OverlapID.Key))
            .SelectMany(kvp_BlendMode => kvp_BlendMode.Value)).ToList();

        private void DrawWalkMesh()
        {
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

        private void FindOverlappingTiles() => (from t1 in tiles
                                                from t2 in tiles
                                                where t1.TileID < t2.TileID
                                                where t1.BlendMode == BlendMode.none
                                                where t1.Intersect(t2)
                                                orderby t1.TileID, t2.TileID ascending
                                                select new[] { t1, t2 }

                                     ).ForEach(x => x[1].OverLapID = checked((byte)(x[0].OverLapID + 1)));

        private byte GetColorKey(byte[] mimb, int textureWidth, int startPixel, int x, int y, bool is8Bit)
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

        private void GetPalettes(byte[] mimb)
        {
            int Offset = TextureType?.BytesSkippedPalettes ?? 0;
            Cluts CLUT = tiles != null ? new Cluts(tiles.Select(x => x.PaletteID).Distinct().ToDictionary(x => x, x => new Color[colorsPerPalette]), false) :
                new Cluts(Enumerable.Range(0, 15).Select(x => (byte)x).ToDictionary(x => x, x => new Color[colorsPerPalette]), false);
            using (BinaryReader br = new BinaryReader(new MemoryStream(mimb)))
                foreach (KeyValuePair<byte, Color[]> clut in CLUT)
                {
                    int palettePointer = Offset + ((clut.Key) * bytesPerPalette);
                    br.BaseStream.Seek(palettePointer, SeekOrigin.Begin);
                    for (int i = 0; i < colorsPerPalette; i++)
                        clut.Value[i] = Texture_Base.ABGR1555toRGBA32bit(br.ReadUInt16());
                }

            Cluts = CLUT;
        }

        private TextureHandler GetTexture(Tile tile)
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

        private void GetTiles(byte[] mapb) => tiles = mapb == null ? default : Tiles.Load(mapb, TextureType.Type);

        private bool ParseBackground2D(byte[] mimb, byte[] mapb)
        {
            if (!Module.Toggles.HasFlag(Module._Toggles.ClassicSpriteBatch)) return true;
            if (mimb == null || mapb == null)
                return false;

            FindOverlappingTiles();
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
                foreach (Texture2D tex in drawtextures().Select(x => x.Value))
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
                        byte colorKey = GetColorKey(mimb, TextureType.Width, startPixel, x, y, tile.Is8Bit);
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
                                if (bufferedcolor != Color.TransparentBlack)
                                    color = blend2(bufferedcolor, color);
                            }
                            else
                            {
                                if (blendmode == BlendMode.quarteradd)
                                    color = Color.Multiply(color, .25f);
                                else if (blendmode == BlendMode.halfadd)
                                    color = Color.Multiply(color, .5f);
                                if (bufferedcolor != Color.TransparentBlack)
                                    color = blend1(bufferedcolor, color);
                            }
                        }
                        else if (bufferedcolor != Color.TransparentBlack)
                        {
                            throw new Exception("Color is already set something may be wrong.");
                        }
                        color.A = 0xFF;
                        texturebuffer[pos] = color;
                        hasColor = true;
                    }
            }
            convertColorToTexture2d(); // gets leftover colors from last batch and makes a texture.
            SaveTextures();
            return true;
        }

        private bool ParseBackgroundQuads(byte[] mimb, byte[] mapb)
        {
            if (mimb == null || mapb == null)
                return false;
            //FindOverlappingTiles();

            var UniqueSetOfTileData = tiles.Select(x => new { x.TextureID, loc = new Point(x.SourceX, x.SourceY), x.Is4Bit, x.PaletteID, x.AnimationID }).Distinct().ToList();
            // Create a swizzeled Textures with one palette.
            // 4bit has 2 pixels per byte. So will need a seperate texture for those.
            Width = UniqueSetOfTileData.Max(x => x.loc.X + Tile.size);
            Height = UniqueSetOfTileData.Max(x => x.loc.Y + Tile.size);
            Dictionary<byte, Texture2D> TextureIDs = UniqueSetOfTileData.Select(x => x.TextureID).Distinct().ToDictionary(x => x, x => new Texture2D(Memory.graphics.GraphicsDevice, 256, 256));

            //var dup = (from t1 in UniqueSetOfTileData
            //           from t2 in UniqueSetOfTileData
            //           where t1 != t2 && t1.TextureID == t1.TextureID && t1.loc == t2.loc
            //           select new[] { t1, t2 }).ToList();
            //foreach(var i in UniqueSetOfTileData.GroupBy(x => x.TextureID))
            //{
            //    foreach(var j in i.GroupBy(x=>x.loc))
            //    {
            //        var m = j.ToList();
            //        if(m.Count>1 && m[0].loc.X == 240 && m[0].loc.Y == 192)
            //        {
            //            m[1].PaletteID = 5;
            //        }
            //    }
            //}
            bool overlap = false;
            using (BinaryReader br = new BinaryReader(new MemoryStream(mimb)))
            {
                foreach (KeyValuePair<byte, Texture2D> kvp in TextureIDs)
                {
                    GenTexture(kvp.Key, kvp.Value);
                }
                SaveSwizzled(TextureIDs);
                string fieldname = Module.GetFieldName();
                this.TextureIDs = TextureIDs.ToDictionary(x => x.Key, x => TextureHandler.Create($"{ fieldname }_{x.Key}", new Texture2DWrapper(x.Value), ushort.MaxValue));
                SaveCluts();
                if (overlap)
                {
                    Dictionary<TextureIDPaletteID, Texture2D> TextureIDsPalettes = UniqueSetOfTileData.Where(x => x.AnimationID != 0xFF || x.Is4Bit).Select(x => new TextureIDPaletteID { TextureID = x.TextureID, PaletteID = x.PaletteID }).Distinct().ToDictionary(x => x, x => new Texture2D(Memory.graphics.GraphicsDevice, 256, 256));
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
                void GenTexture(byte texID, Texture2D tex2d, byte? inpaletteID = null)
                {
                    TextureBuffer tex = new TextureBuffer(tex2d.Width, tex2d.Height, true);

                    //foreach (var textureID in UniqueSetOfTileData.GroupBy(x=>x.TextureID == kvp.Key))
                    foreach (var tile in UniqueSetOfTileData.Where(x => x.TextureID == texID && (!inpaletteID.HasValue || inpaletteID.Value == x.PaletteID)))
                    {
                        long startPixel = TextureType.PaletteSectionSize + (tile.loc.X / (tile.Is4Bit ? 2 : 1)) + (texturePageWidth * tile.TextureID) + (TextureType.Width * tile.loc.Y);
                        //int readlength = Tile.size + (Tile.size * TextureType.Width);

                        for (int y = 0; y < 16; y++)
                        {
                            br.BaseStream.Seek(startPixel + (y * TextureType.Width), SeekOrigin.Begin);

                            byte Colorkey = 0;
                            int _y = y + tile.loc.Y;
                            for (int x = 0; x < 16; x++)
                            {
                                int _x = x + tile.loc.X;
                                byte paletteID = tile.PaletteID;
                                //if (tile.loc.X == 240 && tile.loc.Y == 192)
                                //    paletteID = 9;
                                Color color = default;
                                if (!tile.Is4Bit)
                                {
                                    color = Cluts[paletteID][br.ReadByte()];
                                }
                                else
                                {
                                    if (x % 2 == 0)
                                    {
                                        Colorkey = br.ReadByte();
                                        color = Cluts[paletteID][Colorkey & 0xf];
                                    }
                                    else
                                    {
                                        color = Cluts[paletteID][(Colorkey & 0xf0) >> 4];
                                    }
                                }
                                if (color != Color.TransparentBlack)
                                {
                                    if (tex[_x, _y] != Color.TransparentBlack)
                                    {
                                        //if ()//excluding 8bit overlap for now.
                                        overlap = true;
                                        if (tex[_x, _y] != color)
                                        {
                                            Debug.WriteLine($"x={_x},y={_y} :: {Memory.FieldHolder.fields[Memory.FieldHolder.FieldID]} :: {tile} \n   existed_color {tex[_x, _y]} :: failed_color={color}");
                                            break;
                                        }
                                    }
                                    else
                                        tex[_x, _y] = color;
                                }
                            }
                        }
                        //(from b in br.ReadBytes(readlength)
                        // select dictPalettes[tile.PaletteID][b]).ToArray();
                    }
                    tex.SetData(tex2d);
                }
            }
            //Memory.Scale(Width, Height, Memory.ScaleMode.FitBoth).X
            quads = tiles.Select(x => new TileQuadTexture(x, GetTexture(x), 1f)).ToList();
            Animations = quads.Where(x => x.AnimationID != 0xFF).Select(x => x.AnimationID).Distinct().ToDictionary(x => x, x => quads.Where(y => y.AnimationID == x).OrderBy(y => y.AnimationState).ToList());
            Animations.ForEach(x => x.Value.Where(y => y.AnimationState != x.Value.Max(k => k.AnimationState)).ForEach(y => y.Hide()));
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

        //private void SaveSwizzled(string suf = "") => SaveSwizzled(TextureIDs, suf);
        private void SaveTextures()
        {
            if (Memory.EnableDumpingData || (Module.Toggles.HasFlag(Module._Toggles.DumpingData) && Module.Toggles.HasFlag(Module._Toggles.Deswizzle)))
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

        #endregion Methods

        // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }
}