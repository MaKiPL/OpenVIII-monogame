using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public abstract class Menu : Menu_Base
    {
        #region Fields

        public static EventHandler FadedInHandler;
        public static EventHandler FadedOutHandler;
        public Dictionary<Enum, IGMData> Data;

        public EventHandler<Enum> ModeChangeHandler;
        protected Enum _mode;
        protected bool skipdata;

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private const int _blinkinspeed = 500;

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private const int _blinkoutspeed = 900;

        private const float _fadedin = 1f;
        private const float _fadedout = 0f;

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private const int _fadeinspeed = 700;

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private const int _fadeoutspeed = 1500;

        /// <summary>
        /// Scale of Menu Items (Background and Cursor)
        /// </summary>
        private const float _menuitemscale = 2f;

        private static BattleMenus _battlemenus;
        //private static bool _blinkstate;
        //private static bool _fadeout = false;
        private static IGM _igm;

        private static IGM_Items _igm_items;

        private static IGM_Junction _igm_junction;

        private static IGM_Lobby _igm_lobby;

        private static object _igm_lock = new object();
        private bool _backup = false;
        private Vector2 _size;

        #endregion Fields

        #region Constructors

        public Menu() => InitConstructor();

        public Menu(Damageable damageable)
        {
            _damageable = damageable;
            InitConstructor(); // because base() would always run first :(
        }

        #endregion Constructors

        #region Properties

        public static BattleMenus BattleMenus => _battlemenus;

        /// <summary>
        /// Blink works like Fade except it goes up to 1f then to 0f and back.
        /// </summary>
        public static float Blink_Amount { get; private set; } = _fadedin;

        /// <summary>
        /// Fade by default scales from 0f to 1f. Unless FadeOut then it goes from 1f to 0f.
        /// </summary>
        public static float Fade { get; private set; } = _fadedin;

        public static bool FadingOut => FadeSlider.Reversed; //_fadeout;
        /// <summary>
        /// Focus scales and centers the menu.
        /// </summary>
        public static Matrix Focus { get; protected set; }

        /// <summary>
        /// In Game Menu
        /// </summary>
        public static IGM IGM => _igm;

        /// <summary>
        /// In Game Menu - Items Menu
        /// </summary>
        public static IGM_Items IGM_Items => _igm_items;

        /// <summary>
        /// In Game Menu - Junction Menu
        /// </summary>
        public static IGM_Junction IGM_Junction => _igm_junction;

        /// <summary>
        /// Lobby Menu
        /// </summary>
        public static IGM_Lobby IGM_Lobby => _igm_lobby;

        /// <summary>
        /// Adjusted mouse location used to determine if mouse is highlighting a button.
        /// </summary>
        public static Point MouseLocation => InputMouse.Location.Transform(Menu.Focus);

        public static Vector2 StaticSize { get; protected set; }
        /// <summary>
        /// Size of text the real game doesn't use a 1:1 ratio.
        /// </summary>
        public static Vector2 TextScale { get; } = new Vector2(2.545455f, 3.0375f);

        /// <summary>
        /// If true Inputs won't be called from Update(). So will need to be called sepperately.
        /// </summary>
        public bool NoInputOnUpdate { get; set; } = false;

        /// <summary>
        /// Size of the menu. If kept in a 4:3 region it won't scale down till after losing enough width.
        /// </summary>
        public Vector2 Size { get => _size; protected set => _size = value; }

        /// <summary>
        /// Viewport dimensions
        /// </summary>
        protected Vector2 vp => new Vector2(Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height);

        /// <summary>
        /// if canceled don't init menu.
        /// </summary>
        private bool cancel => Memory.Token.IsCancellationRequested;

        #endregion Properties

        #region Methods

        public static Tuple<Rectangle, Point, Rectangle> DrawBox(Rectangle dst, FF8String buffer = null, Icons.ID? title = null, Vector2? textScale = null, Vector2? boxScale = null, Box_Options options = Box_Options.Default)
        {
            if (textScale == null) textScale = Vector2.One;
            if (boxScale == null) boxScale = Vector2.One;
            Point cursor = Point.Zero;
            Rectangle font = new Rectangle();
            Rectangle backup = dst;
            if (buffer != null && buffer.Length > 0)
            {
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: Fade, skipdraw: true);
                if (dst.Size == Point.Zero)
                {
                    dst.Size = font.Size;
                    if (title == null)
                    {
                        dst.Inflate(20, 10);
                    }
                    else
                        dst.Inflate(20, 30);
                    dst.Location = backup.Location;
                    backup = dst;
                }
            }
            Vector2 bgscale = new Vector2(_menuitemscale) * textScale.Value;
            Rectangle box = dst.Scale(boxScale.Value);
            Rectangle hotspot = dst;
            if ((options & Box_Options.SkipDraw) == 0 && dst.Size != Point.Zero)
            {
                if (dst.Width > 256 * bgscale.X)
                    Memory.Icons.Draw(Icons.ID.Menu_BG_368, 0, box, bgscale, Fade);
                else
                    Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, box, bgscale, Fade);
                if (title != null)
                {
                    dst.Offset(15, 0);
                    dst.Y = (int)(dst.Y * boxScale.Value.Y);
                    Memory.Icons.Draw(title.Value, 2, dst, (bgscale + new Vector2(.5f)), Fade);
                }
                dst = backup;
            }
            if (buffer != null && buffer.Length > 0)
            {
                //font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: Fade, skipdraw: true);
                if ((options & Box_Options.Indent) != 0)
                    dst.Offset(70 * textScale.Value.X, 0);
                else if ((options & Box_Options.Center) != 0)
                    dst.Offset(dst.Width / 2 - font.Width / 2, 0);
                else
                    dst.Offset(25 * textScale.Value.X, 0);

                if ((options & Box_Options.Buttom) != 0)
                    dst.Offset(0, (dst.Height - 48));
                else if ((options & Box_Options.Middle) != 0)
                    dst.Offset(0, dst.Height / 2 - font.Height / 2);
                else
                    dst.Offset(0, 21);

                dst.Y = (int)(dst.Y * boxScale.Value.Y);
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: Fade, skipdraw: (options & Box_Options.SkipDraw) != 0);
                cursor = dst.Location;
                cursor.Y += (int)(TextScale.Y * 6); // 12 * (3.0375/2)
            }

            return new Tuple<Rectangle, Point, Rectangle>(hotspot, cursor, font);
        }

        public static void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false)
        {
            if (offset == null)
                offset = new Vector2(-1.15f, -.3f);
            Vector2 scale = new Vector2(_menuitemscale);
            Vector2 size = Memory.Icons.GetEntry(Icons.ID.Finger_Right, 0).Size * scale;
            Rectangle dst = new Rectangle(cursor, Point.Zero);
            byte pallet = 2;
            byte fadedpallet = 7;
            dst.Offset(size * offset.Value);
            Memory.Icons.Trim(Icons.ID.Finger_Right, pallet);
            if (blink)
            {
                Memory.Icons.Draw(Icons.ID.Finger_Right, fadedpallet, dst, scale, Fade);
            }
            Memory.Icons.Draw(Icons.ID.Finger_Right, pallet, dst, scale, blink ? Fade * Blink_Amount : Fade);
        }

        public static void FadeIn()
        {
            if (FadeSlider.Reversed)
                FadeSlider.ReverseRestart();
            else
                FadeSlider.Restart();

            //Fade = _fadedout;
            //_fadeout = false;
        }

        public static void FadeOut()
        {
            if (!FadeSlider.Reversed)
                FadeSlider.ReverseRestart();
            else
                FadeSlider.Restart();

            //Fade = _fadedin;
            //_fadeout = true;
        }

        public static void InitStaticMembers()
        {
            lock (_igm_lock)
            {
                if (_igm_lobby == null)
                    _igm_lobby = new IGM_Lobby();
                if (_igm == null)
                    _igm = new IGM();
                if (_igm_junction == null)
                    _igm_junction = new IGM_Junction();
                if (_igm_items == null)
                    _igm_items = new IGM_Items();
                if (_battlemenus == null)
                    _battlemenus = new BattleMenus();
                Fade = 0;
            }
        }

        public static Slide<float> BlinkSlider = new Slide<float>(_fadedout, _fadedin, _blinkinspeed, MathHelper.Lerp) { ReverseMS = _blinkoutspeed };
        public static Slide<float> FadeSlider = new Slide<float>(_fadedout, _fadedin, _fadeinspeed, MathHelper.Lerp) { ReverseMS = _fadeoutspeed };

        public override void Reset()
        {
            if (!skipdata)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                {
                    i.Value?.Reset();
                }
            base.Reset();
        }
        private static void UpdateFade(object sender = null)
        {
            if (!BlinkSlider.Done)
            {
                Blink_Amount = BlinkSlider.Update();

                if (BlinkSlider.Done)
                {
                    BlinkSlider.Reverse();
                    BlinkSlider.Restart();
                }
            }
            if (!FadeSlider.Done)
            {

                Fade = FadeSlider.Update();
                if (FadeSlider.Reversed)
                {
                    if (FadeSlider.Done)
                    {
                        FadedOutHandler?.Invoke(sender, null);
                        FadeSlider.ReverseRestart();
                    }
                }
                else
                {
                    Fade = FadeSlider.Update();
                    if (FadeSlider.Done)
                    {
                        FadedInHandler?.Invoke(sender, null);
                    }
                }
            }
    }

        public override void Draw()
        {
            StartDraw();
            DrawData();
            EndDraw();
        }

        public virtual void DrawData()
        {
            if (!skipdata && Enabled)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                    i.Value.Draw();
        }

        public virtual void EndDraw()
        {
            if (Enabled)
                Memory.SpriteBatchEnd();
        }

        public Enum GetMode() => _mode;

        public override bool Inputs() => false;

        public override void Refresh()
        {
            Backup();
            base.Refresh();
        }

        public void Refresh(bool backup) => Refresh(Damageable, backup);

        public override void Refresh(Damageable damageable) => Refresh(damageable, false);

        public virtual void Refresh(Damageable damageable, bool backup)
        {
            _backup = backup;
            base.Refresh(damageable);
        }

        public virtual bool SetMode(Enum mode)
        {
            if (!mode.Equals(_mode))
            {
                _mode = mode;
                ModeChangeHandler?.Invoke(this, mode);
                return true;
            }
            return false;
        }

        public virtual void StartDraw()
        {
            if (Enabled)
                Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp, tm: Focus);
        }
        public static void UpdateOnce()
        {
            UpdateFade(null);
        }
        public override bool Update()
        {
            bool ret = false;            
            GenerateFocus();
            StaticSize = Size;
            if (Enabled)
            {
                //todo detect when there is no saves detected.
                //check for null
                if (!skipdata)
                    foreach (KeyValuePair<Enum, IGMData> i in Data)
                    {
                        ret = i.Value.Update() || ret;
                    }
            }
            if (!NoInputOnUpdate)
                return Inputs() || ret;
            else return ret;
        }

        protected void GenerateFocus(Vector2? inputsize = null, Box_Options options = Box_Options.Default)
        {
            Vector2 size = inputsize ?? Size;
            Vector2 Zoom = Memory.Scale(size.X, size.Y, Memory.ScaleMode.FitBoth);
            size /= 2;
            Vector2 t = new Vector2(vp.X / 2, vp.Y / 2);
            if ((options & Box_Options.Top) != 0)
            {
                t.Y = 0;
                size.Y = 0;
            }
            else if ((options & Box_Options.Buttom) != 0)
            {
                t.Y = vp.Y - (size.Y * 2 * Zoom.Y);
                size.Y = 0;
            }
            Focus = Matrix.CreateTranslation(-size.X, -size.Y, 0) *
                Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                Matrix.CreateTranslation(t.X, t.Y, 0);
        }

        protected override void Init()
        {
        }

        protected override void RefreshChild()
        {
            if (!skipdata)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                    i.Value.Refresh(Damageable);
        }

        private void Backup()
        {
            //backup memory
            if (_backup)
                Memory.PrevState = Memory.State.Clone();
            _backup = false;
        }

        private void InitConstructor()
        {
            //WaitForInit();
            if (!cancel)
            {
                Data = new Dictionary<Enum, IGMData>();
                Init();
                skipdata = true;
                Refresh();
                skipdata = false;
            }
        }

        #endregion Methods
    }
}