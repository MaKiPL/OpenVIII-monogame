using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Root class all menu objects can grow from.
    /// </summary>
    public abstract class Menu_Base
    {
        #region Fields

        protected Rectangle _pos;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Focus scales and centers the menu.
        /// </summary>
        public static Matrix Focus { get; protected set; }

        /// <summary>
        /// Adjusted mouse location used to determine if mouse is highlighting a button.
        /// </summary>
        public static Point MouseLocation => InputMouse.Location.Transform(Focus);

        public static Point ScreenBottomLeft => new Point(0, Memory.graphics.GraphicsDevice.Viewport.Height).Transform(Focus);
        public static Point ScreenBottomRight => new Point(Memory.graphics.GraphicsDevice.Viewport.Width, Memory.graphics.GraphicsDevice.Viewport.Height).Transform(Focus);
        public static Point ScreenTopLeft => new Point(0, 0).Transform(Focus);
        public static Point ScreenTopRight => new Point(Memory.graphics.GraphicsDevice.Viewport.Width, 0).Transform(Focus);
        public Menu_Base CONTAINER { get; set; }
        public Cursor_Status Cursor_Status { get; set; } = Cursor_Status.Disabled;

        /// <summary>
        /// Characters/Enemies/GF
        /// </summary>
        public virtual Damageable Damageable { get; protected set; }

        /// <summary>
        /// If enabled the menu is Visible and all functionality works. Else everything is hidden and
        /// nothing functions.
        /// </summary>
        public bool Enabled { get; private set; } = true;

        public virtual int Height { get => _pos.Height; set => _pos.Height = value; }
        public OffsetAnchor OffsetAnchor { get; set; }

        /// <summary>
        /// Position of party member 0,1,2. If -1 at the time of setting the character wasn't in the party.
        /// </summary>
        public sbyte PartyPos { get; protected set; } = sbyte.MaxValue;

        /// <summary>
        /// Where to draw this item.
        /// </summary>
        public virtual Rectangle Pos { get => _pos; set => _pos = value; }

        public virtual int Width { get => _pos.Width; set => _pos.Width = value; }

        public virtual int X { get => _pos.X; set => _pos.X = value; }

        public virtual int Y { get => _pos.Y; set => _pos.Y = value; }

        #endregion Properties

        #region Methods

        public static implicit operator Rectangle(Menu_Base v) => v.Pos;

        public abstract void Draw();

        /// <summary>
        /// Hide object prevents drawing, update, inputs.
        /// </summary>
        public virtual void Hide() => Enabled = false;

        public virtual void HideChildren()
        {
        }

        public abstract bool Inputs();

        public virtual void ModeChangeEvent(object sender, Enum e)
        {
        }

        /// <summary>
        /// Things that change rarely. Like a party member changes or Laguna dream happens.
        /// </summary>
        public virtual void Refresh() => RefreshChild();


        /// <summary>
        /// by default null damageable won't propogate to children. resets to false after refresh.
        /// </summary>
        public bool ForceNullDamageable { get; set; } = false;

        /// <summary>
        /// Update set characters and then refresh.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="Visiblecharacter"></param>
        public virtual void Refresh(Damageable damageable)
        {
            SetDamageable(damageable, null);         
            Refresh();
        }

        public void SetDamageable(Damageable damageable, sbyte? partypos = null, bool forcenull = false)
        {
            if ((Damageable != damageable) || (partypos.HasValue && partypos.Value != PartyPos))
            {
                if (partypos != null)
                {
                    Damageable = damageable;
                    PartyPos = partypos.Value;
                    if (Damageable == null)
                    {
                        if (PartyPos >= 0 && Memory.State?.PartyData != null && PartyPos < Memory.State.PartyData.Count)
                            Damageable = Memory.State[Memory.State.PartyData[PartyPos]];
                        else
                        {
                            int enemypos = (0 - PartyPos) - 1;
                            if (PartyPos < 0 && Enemy.Party != null && enemypos < Enemy.Party.Count)
                            {
                                Damageable = Enemy.Party[enemypos];
                            }
                        }
                    }

                }
                else if (damageable != null)
                {
                    Damageable = damageable;
                    if (Damageable.GetCharacterData(out Saves.CharacterData c))
                    {
                        PartyPos = (sbyte)(Memory.State?.PartyData?.Where(x => !x.Equals(Characters.Blank)).ToList().FindIndex(x => x.Equals(c.ID)) ?? -1);
                    }
                    else if (typeof(Enemy).Equals(Damageable.GetType()))
                    {
                        PartyPos = checked((sbyte)(0 - Enemy.Party.FindIndex(x => x.Equals(Damageable)) - 1));
                    }
                    else PartyPos = sbyte.MaxValue;
                }
                else if(ForceNullDamageable || forcenull)
                {
                    ForceNullDamageable = true;
                    Damageable = null;
                    PartyPos = sbyte.MaxValue;
                }
            }
        }

        /// <summary>
        /// Plan is to use this to reset values to a default state if done.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Show object enables drawing, update, inputs.
        /// </summary>
        public virtual void Show() => Enabled = true;

        public abstract bool Update();

        protected abstract void Init();

        /// <summary>
        /// For child items.
        /// </summary>
        protected virtual void RefreshChild()
        {
        }

        #endregion Methods
    }

    public class OffsetAnchor
    {
        #region Fields

        private Vector2 v = Vector2.Zero;

        #endregion Fields

        #region Constructors

        public OffsetAnchor(Vector2 pos) => this.v = pos;

        public OffsetAnchor(Point pos) => this.v = pos.ToVector2();

        public OffsetAnchor()
        {
        }

        #endregion Constructors

        #region Properties

        public Point pPos { get => v.ToPoint(); set => v = value.ToVector2(); }
        public Vector2 vPos { get => v; set => v = value; }
        public float X { get => v.X; set => v.X = value; }
        public float Y { get => v.Y; set => v.Y = value; }

        #endregion Properties

        #region Methods

        public static explicit operator Point(OffsetAnchor a) => a.v.ToPoint();

        public static implicit operator OffsetAnchor(Vector2 a) => new OffsetAnchor(a);

        public static implicit operator OffsetAnchor(Point a) => new OffsetAnchor(a);

        public static implicit operator Vector2(OffsetAnchor a) => a.v;

        public OffsetAnchor Offset(float x, float y)
        {
            v.X += x;
            v.Y += y;
            return this;
        }

        public OffsetAnchor Offset(Vector2 _v)
        {
            v += _v;
            return this;
        }

        internal void Set(Vector2 vector2) => v = vector2;

        #endregion Methods
    }
}