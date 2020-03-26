using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public abstract class Menu : Menu_Base
    {
        #region Fields

        public static Slide<float> BlinkSlider = new Slide<float>(_fadedout, _fadedin, _blinkinspeed, MathHelper.Lerp) { ReversedTime = _blinkoutspeed };

        public static EventHandler FadedInHandler;

        public static EventHandler FadedOutHandler;

        public static Slide<float> FadeSlider = new Slide<float>(_fadedout, _fadedin, _fadeinspeed, MathHelper.Lerp) { ReversedTime = _fadeoutspeed };

        public ConcurrentDictionary<Enum, Menu_Base> Data;

        protected Enum _mode;

        protected bool skipdata;

        private const float _fadedin = 1f;

        private const float _fadedout = 0f;

        /// <summary>
        /// Scale of Menu Items (Background and Cursor)
        /// </summary>
        private const float _menuitemscale = 2f;

        private static BattleMenus _battlemenus;

        private static Debug_Menu _debug_menu;

        //private static bool _blinkstate;
        //private static bool _fadeout = false;
        private static IGM _igm;

        private static IGM_Items _igm_items;

        private static IGM_Junction _igm_junction;

        private static IGM_LGSG _igm_lgsg;

        private static IGM_Lobby _igm_lobby;

        private static object _igm_lock = new object();

        private bool _backup = false;

        private Vector2 _size;
        private static MenuModule _module;

        #endregion Fields

        #region Events

        public event EventHandler<Enum> ModeChangeHandler;

        #endregion Events

        #region Properties

        public static BattleMenus BattleMenus => _battlemenus;

        //public Menu(Damageable damageable)
        //{
        //    _damageable = damageable;
        //    InitConstructor(); // because base() would always run first :(
        //}
        /// <summary>
        /// Blink works like Fade except it goes up to 1f then to 0f and back.
        /// </summary>
        public static float Blink_Amount { get; private set; } = _fadedin;

        /// <summary>
        /// Debug Menu
        /// </summary>
        public static Debug_Menu Debug_Menu => _debug_menu;

        /// <summary>
        /// Fade by default scales from 0f to 1f. Unless FadeOut then it goes from 1f to 0f.
        /// </summary>
        public static float Fade { get; private set; } = _fadedin;

        public static bool FadingOut => FadeSlider.Reversed;

        //_fadeout;
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
        public static IGM_LGSG IGM_LGSG => _igm_lgsg;

        /// <summary>
        /// Lobby Menu
        /// </summary>
        public static IGM_Lobby IGM_Lobby => _igm_lobby;

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

        public bool SkipFocus { get; set; } = false;

        /// <summary>
        /// Viewport dimensions
        /// </summary>
        protected Vector2 vp => new Vector2(Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height);

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private static TimeSpan _blinkinspeed => TimeSpan.FromMilliseconds(500d);

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private static TimeSpan _blinkoutspeed => TimeSpan.FromMilliseconds(900d);

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private static TimeSpan _fadeinspeed => TimeSpan.FromMilliseconds(700d);

        /// <summary>
        /// <para>Time to fade out in milliseconds</para>
        /// <para>Larger is slower</para>
        /// </summary>
        private static TimeSpan _fadeoutspeed => TimeSpan.FromMilliseconds(1500d);

        /// <summary>
        /// if canceled don't init menu.
        /// </summary>
        private bool cancel => Memory.Token.IsCancellationRequested;

        public static MenuModule Module { get => _module; set => _module = value; }

        #endregion Properties

        #region Methods

        //public Menu() => InitConstructor();
        public static T Create<T>(Damageable damageable = null) where T : Menu, new()
        {
            Memory.Log.WriteLine($"{nameof(Menu)} :: {nameof(Create)} :: {typeof(T)} :: {nameof(Damageable)} :: {damageable}");
            var r = new T
            {
                Damageable = damageable,
            };
            r.InitConstructor();
            return r;
        }

        public static BoxReturn DrawBox(Rectangle dst, FF8String buffer = null, Icons.ID? title = null, Vector2? textScale = null, Vector2? boxScale = null, Box_Options options = Box_Options.Default)
        {
            if (textScale == null) textScale = Vector2.One;
            if (boxScale == null) boxScale = Vector2.One;
            var cursor = Point.Zero;
            var font = new Rectangle();
            var backup = dst;
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
            var bgscale = new Vector2(_menuitemscale) * textScale.Value;
            var box = dst.Scale(boxScale.Value);
            var hotspot = dst;
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
                else if ((options & Box_Options.Right) != 0)
                    dst.Offset(dst.Width - font.Width - 5 * textScale.Value.X, 0);
                else
                    dst.Offset(25 * textScale.Value.X, 0);

                if ((options & Box_Options.Buttom) != 0)
                    dst.Offset(0, (dst.Height - 48));
                else if ((options & Box_Options.Middle) != 0)
                    dst.Offset(0, dst.Height / 2 - font.Height / 2 + 2);
                else
                    dst.Offset(0, 21);

                dst.Y = (int)(dst.Y * boxScale.Value.Y);
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), TextScale * textScale.Value, Fade: Fade, skipdraw: (options & Box_Options.SkipDraw) != 0);
                cursor = dst.Location;
                cursor.Y += (int)(TextScale.Y * 6); // 12 * (3.0375/2)
            }

            return new BoxReturn(hotspot, cursor, font);
        }

        public static void DrawPointer(Point cursor, Vector2? offset = null, bool blink = false)
        {
            if (offset == null)
                offset = new Vector2(-1.15f, -.3f);
            var scale = new Vector2(_menuitemscale);
            var Finger_Right = Memory.Icons.GetEntry(Icons.ID.Finger_Right, 0);
            if (Finger_Right != null)
            {
                var size = Finger_Right.Size * scale;
                var dst = new Rectangle(cursor, Point.Zero);
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
            Memory.Log.WriteLine($"{nameof(Menu)} :: {nameof(InitStaticMembers)}");
            lock (_igm_lock)
            {
                if (_igm_lobby == null)
                    _igm_lobby = IGM_Lobby.Create();
                if (_igm == null)
                    _igm = IGM.Create();
                if (_igm_junction == null)
                    _igm_junction = IGM_Junction.Create();
                if (_igm_items == null)
                    _igm_items = IGM_Items.Create();
                if (_battlemenus == null)
                    _battlemenus = BattleMenus.Create();
                if (_igm_lgsg == null)
                    _igm_lgsg = IGM_LGSG.Create();
                if (_debug_menu == null)
                    _debug_menu = Debug_Menu.Create();
                if (_module == null)
                    _module = MenuModule.Create();
                FadeIn();
            }
        }
        public static void UpdateOnce() => UpdateFade(null);

        public override void Draw()
        {
            StartDraw();
            DrawData();
            EndDraw();
        }

        public virtual void DrawData()
        {
            if (!skipdata && Enabled && Data != null)
                foreach (var i in Data.Where(x => x.Value != null && x.Value.Enabled).OrderBy(x => x.Key).Select(x => x.Value))
                    i?.Draw();
        }

        public virtual void EndDraw()
        {
            if (Enabled)
                Memory.SpriteBatchEnd();
        }

        public virtual Enum GetMode() => _mode;

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

        public override void Reset()
        {
            if (!skipdata)
                foreach (var i in Data)
                {
                    i.Value?.Reset();
                }
            base.Reset();
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

        public override bool Update()
        {
            var ret = false;
            if (!SkipFocus)
                GenerateFocus();
            if(Size != Vector2.Zero)
                StaticSize = Size;
            if (Enabled)
            {
                //todo detect when there is no saves detected.
                //check for null
                //if (!skipdata && Data != null)
                //{
                //    ret = ((from i in Data
                //           where i.Value != null && i.Value.Update()
                //           select new { isTrue = true }).FirstOrDefault()?.isTrue ?? false) || ret;
                //}
                foreach (var i in Data.Where(x => x.Value != null))
                {
                    ret = (i.Value.Update()) || ret;
                }
            }
            if (!NoInputOnUpdate)
                return Inputs() || ret;
            else return ret;
        }

        protected void GenerateFocus(Vector2? inputsize = null, Box_Options options = Box_Options.Default)
        {
            var size = inputsize ?? Size;
            var Zoom = Memory.Scale(size.X, size.Y, Memory.ScaleMode.FitBoth);
            var OffsetScreen = size/2f;
            var CenterOfScreen = new Vector2(vp.X, vp.Y ) / 2f;
            if ((options & Box_Options.Top) != 0)
            {
                CenterOfScreen.Y = 0;
                OffsetScreen.Y = 0;
            }
            else if ((options & Box_Options.Buttom) != 0)
            {
                CenterOfScreen.Y = vp.Y - (OffsetScreen.Y * 2 * Zoom.Y);
                OffsetScreen.Y = 0;
            }
            Focus = Matrix.CreateTranslation(-OffsetScreen.X, -OffsetScreen.Y, 0) *
                Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                Matrix.CreateTranslation(CenterOfScreen.X, CenterOfScreen.Y, 0);
        }

        protected override void Init()
        {
        }

        protected override void RefreshChild()
        {
            if (!skipdata)
                foreach (var i in Data.Where(x=>x.Value !=null))
                // children might have a damageable set.
                // if parents may not always have one set.
                {
                    if (ForceNullDamageable)
                        i.Value.ForceNullDamageable = ForceNullDamageable;
                    i.Value.Refresh(Damageable);
                }
            ForceNullDamageable = false;
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
                Data = new ConcurrentDictionary<Enum, Menu_Base>();
                Init();
                skipdata = true;
                Refresh();
                skipdata = false;
            }
        }

        #endregion Methods

        #region Structs

        public struct BoxReturn
        {
            #region Fields

            public Point Cursor;
            public Rectangle Font;
            public Rectangle HotSpot;

            #endregion Fields

            #region Constructors

            public BoxReturn(Rectangle hotSpot, Point cursor, Rectangle font)
            {
                HotSpot = hotSpot;
                Cursor = cursor;
                Font = font;
            }

            #endregion Constructors
        }

        #endregion Structs
    }
}