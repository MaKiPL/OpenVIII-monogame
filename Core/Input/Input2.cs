using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public abstract class Input2
    {
        #region Fields

        protected static bool bLimitInput;
        protected static List<Input2> Data;
        protected static double msDelay;
        private static readonly int msDelayLimit = 100;

        #endregion Fields

        #region Constructors

        public Input2()
        {
            if (Data != null)
                Data = new List<Input2>
                {
                    new InputGamePad(),
                    new InputKeyboard(),
                    new InputMouse()
                };
        }

        #endregion Constructors

        #region Methods

        public static bool Update()
        {
            CheckInputLimit();
            Data.ForEach(a => a.UpdateOnce());
            return false;
        }

        protected abstract bool UpdateOnce();

        private static void CheckInputLimit()
        {
            //issue here if CheckInputLimit is checked more than once per update cycle this will be wrong.
            if (Memory.gameTime != null)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.TotalMilliseconds) < msDelayLimit;
            if (!bLimitInput) msDelay = 0;
        }

        #endregion Methods
    }

    public class InputGamePad : Input2
    {
        #region Fields

        protected static GamePadState m_gp_state;

        #endregion Fields

        #region Constructors

        public InputGamePad()
        {
            if (m_gp_state != null)
                m_gp_state = new GamePadState();
        }

        #endregion Constructors

        #region Methods

        protected override bool UpdateOnce()
        {
            m_gp_state = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            return false;
        }

        #endregion Methods
    }

    public class InputKeyboard : Input2
    {
        #region Fields

        protected static KeyboardState m_kb_state;

        #endregion Fields

        #region Constructors

        public InputKeyboard()
        {
            if (m_kb_state != null)
                m_kb_state = new KeyboardState();
        }

        #endregion Constructors

        #region Methods

        protected override bool UpdateOnce()
        {
            m_kb_state = Keyboard.GetState();
            return false;
        }

        #endregion Methods
    }

    public class InputMouse : Input2
    {
        #region Fields

        protected static MouseState m_m_state;

        #endregion Fields

        #region Constructors

        public InputMouse()
        {
            if (m_m_state != null)
                m_m_state = new MouseState();
            Mode = MouseLockMode.Screen;
        }

        #endregion Constructors

        #region Properties

        public static MouseLockMode Mode { get; private set; }

        #endregion Properties

        #region Methods

        public void LockMouse()
        {
            if (Memory.IsActive && Mode != MouseLockMode.Disabled) // check for focus to allow for tabbing out with out taking over mouse.
            {
                if (Mode == MouseLockMode.Center) //center mouse in screen after grabbing state, release mouse if alt tabbed out.
                {
                    Mouse.SetPosition(Memory.graphics.GraphicsDevice.Viewport.Bounds.Width / 2, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height / 2);
                }
                else if (Mode == MouseLockMode.Screen) //alt lock that clamps to viewport every frame. would be useful if using mouse to navigate menus and stuff.
                {
                    //there is a better way to clamp as if you move mouse fast enough it will escape for a short time.
                    Mouse.SetPosition(
                        MathHelper.Clamp(m_m_state.X, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Width),
                        MathHelper.Clamp(m_m_state.Y, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height));
                }
            }
        }

        protected override bool UpdateOnce()
        {
            m_m_state = Mouse.GetState();
            LockMouse();
            return false;
        }

        #endregion Methods
    }

    public abstract class Inputs_Keyboard
    {
        public abstract Dictionary<FF8TextTagKey, List<Keys>> Data { get; protected set; }
    }

    [Serializable]
    public class Inputs_FF82000 : Inputs_Keyboard
    {
        public Inputs_FF82000() => Data = new Dictionary<FF8TextTagKey, List<Keys>>
        {
            { FF8TextTagKey.Up, new List<Keys>{Keys.Up } },
            { FF8TextTagKey.Down, new List<Keys>{Keys.Down } },
            { FF8TextTagKey.Left, new List<Keys>{Keys.Left } },
            { FF8TextTagKey.Right, new List<Keys>{Keys.Right } },

            { FF8TextTagKey.Confirm, new List<Keys>{Keys.X } },
            { FF8TextTagKey.Cards, new List<Keys>{Keys.A } },
            { FF8TextTagKey.Cancel, new List<Keys>{Keys.W } },

            { FF8TextTagKey.RotateLeft, new List<Keys>{Keys.Q } },
            { FF8TextTagKey.RotateRight, new List<Keys>{Keys.E } },

            { FF8TextTagKey.EscapeKey1, new List<Keys>{ Keys.Z } },
            { FF8TextTagKey.EscapeKey2, new List<Keys>{ Keys.C } },

            { FF8TextTagKey.PartyKey, new List<Keys>{ Keys.D } },
            { FF8TextTagKey.Pause, new List<Keys>{ Keys.S } },

            //{ FF8TextTagKey.,  new List<Keys>{ Keys.F } },
        };

        public override Dictionary<FF8TextTagKey, List<Keys>> Data
        {
            get;
            protected set;
        }
    }
}