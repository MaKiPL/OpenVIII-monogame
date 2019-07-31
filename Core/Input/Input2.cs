using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    /// <summary>
    /// Triggers for actions
    /// </summary>
    [Flags]
    public enum ButtonTrigger : byte
    {
        /// <summary>
        /// Don't trigger
        /// </summary>
        None = 0x0,

        /// <summary>
        /// If was released now pressed
        /// </summary>
        OnPress = 0x1,

        /// <summary>
        /// If was pressed now released
        /// </summary>
        OnRelease = 0x2,

        /// <summary>
        /// If pressed, keeps triggering
        /// </summary>
        Press = 0x4,

        /// <summary>
        /// Trigger on value being out of the deadzone.
        /// </summary>
        Analog = 0x8,

        /// <summary>
        /// Don't check delay when triggering input.
        /// </summary>
        IgnoreDelay = 0x10,
    }

    public static class InputActions
    {
        #region Fields

        public static readonly List<FF8TextTagKey> Cancel = new List<FF8TextTagKey> { FF8TextTagKey.Cancel, FF8TextTagKey.x34 };
        public static readonly List<FF8TextTagKey> Cards = new List<FF8TextTagKey> { FF8TextTagKey.Cards, FF8TextTagKey.x37 };
        public static readonly List<FF8TextTagKey> Confirm = new List<FF8TextTagKey> { FF8TextTagKey.Confirm, FF8TextTagKey.x36 };
        public static readonly List<FF8TextTagKey> Down = new List<FF8TextTagKey> { FF8TextTagKey.Down, FF8TextTagKey.x3E };
        public static readonly List<FF8TextTagKey> EscapeLeft = new List<FF8TextTagKey> { FF8TextTagKey.EscapeLeft, FF8TextTagKey.x30 };
        public static readonly List<FF8TextTagKey> EscapeRight = new List<FF8TextTagKey> { FF8TextTagKey.EscapeRight, FF8TextTagKey.x31 };
        public static readonly List<FF8TextTagKey> Exit = new List<FF8TextTagKey> { FF8TextTagKey.Exit };
        public static readonly List<FF8TextTagKey> ExitMenu = new List<FF8TextTagKey> { FF8TextTagKey.ExitMenu };
        public static readonly List<FF8TextTagKey> Left = new List<FF8TextTagKey> { FF8TextTagKey.Left, FF8TextTagKey.x3F };
        public static readonly List<FF8TextTagKey> Menu = new List<FF8TextTagKey> { FF8TextTagKey.Menu, FF8TextTagKey.x35 };
        public static readonly List<FF8TextTagKey> Pause = new List<FF8TextTagKey> { FF8TextTagKey.Pause, FF8TextTagKey.x3B };
        public static readonly List<FF8TextTagKey> Reset = new List<FF8TextTagKey> { FF8TextTagKey.Reset };
        public static readonly List<FF8TextTagKey> Right = new List<FF8TextTagKey> { FF8TextTagKey.Right, FF8TextTagKey.x3D };
        public static readonly List<FF8TextTagKey> RotateLeft = new List<FF8TextTagKey> { FF8TextTagKey.RotateLeft, FF8TextTagKey.x32 };
        public static readonly List<FF8TextTagKey> RotateRight = new List<FF8TextTagKey> { FF8TextTagKey.RotateRight, FF8TextTagKey.x33 };
        public static readonly List<FF8TextTagKey> Select = new List<FF8TextTagKey> { FF8TextTagKey.Select, FF8TextTagKey.x38 };
        public static readonly List<FF8TextTagKey> Up = new List<FF8TextTagKey> { FF8TextTagKey.Up, FF8TextTagKey.x3C };

        #endregion Fields
    }

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

    [Serializable]
    public class InputButton
    {
        #region Fields

        public InputButton Combo;
        public ControllerButtons ControllerButton;
        public double HoldMS;
        public Keys Key;
        public MouseButtons MouseButton;
        public ButtonTrigger Trigger = ButtonTrigger.OnPress | ButtonTrigger.OnRelease | ButtonTrigger.Press;

        #endregion Fields
    }

    public class InputGamePad : Input2
    {
        #region Fields

        private static GamePadState last_state;
        private static GamePadState state;

        #endregion Fields

        #region Properties

        protected static GamePadState Last_State => last_state;
        protected static GamePadState State
        {
            get => state; set
            {
                last_state = state;
                state = value;
            }
        }

        #endregion Properties

        #region Methods

        protected override bool UpdateOnce()
        {
            State = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            return false;
        }

        #endregion Methods
    }

    public class InputKeyboard : Input2
    {
        #region Fields

        private static KeyboardState last_state;
        private static KeyboardState state;

        #endregion Fields

        #region Properties

        protected static KeyboardState Last_State => last_state;
        protected static KeyboardState State
        {
            get => state; set
            {
                last_state = state;
                state = value;
            }
        }

        #endregion Properties

        #region Methods

        protected override bool UpdateOnce()
        {
            State = Keyboard.GetState();
            return false;
        }

        #endregion Methods
    }

    public class InputMouse : Input2
    {
        #region Fields

        private static MouseState last_state;
        private static MouseState state;

        #endregion Fields

        #region Properties

        public static MouseLockMode Mode { get; set; } = MouseLockMode.Screen;
        protected static MouseState Last_State => last_state;
        protected static MouseState State
        {
            get => state; set
            {
                last_state = state;
                state = value;
            }
        }

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
                        MathHelper.Clamp(State.X, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Width),
                        MathHelper.Clamp(State.Y, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height));
                }
            }
        }

        protected override bool UpdateOnce()
        {
            State = Mouse.GetState();
            LockMouse();
            return false;
        }

        #endregion Methods
    }

    [Serializable]
    public class Inputs_FF82000 : Inputs
    {
        #region Constructors

        public Inputs_FF82000() => Data = new Dictionary<List<FF8TextTagKey>, List<InputButton>>
        {
            { InputActions.EscapeLeft , new List<InputButton>{ new InputButton { Key = Keys.Z } } },
            { InputActions.EscapeRight, new List<InputButton>{new InputButton { Key = Keys.C }} },

            { InputActions.RotateLeft, new List<InputButton>{new InputButton { Key = Keys.Q }} },
            { InputActions.RotateRight, new List<InputButton>{new InputButton { Key = Keys.E }} },

            { InputActions.Cancel, new List<InputButton>{new InputButton { Key = Keys.W }} },
            { InputActions.Menu, new List<InputButton>{new InputButton { Key = Keys.D }} },
            { InputActions.Confirm, new List<InputButton>{new InputButton { Key = Keys.X }} },
            { InputActions.Cards, new List<InputButton>{new InputButton { Key = Keys.A }} },

            { InputActions.Select, new List<InputButton>{new InputButton { Key = Keys.Q }} },
            { InputActions.Pause, new List<InputButton>{new InputButton { Key = Keys.S }} },

            { InputActions.Up, new List<InputButton>{new InputButton { Key = Keys.Up, Trigger = ButtonTrigger.Press }} },
            { InputActions.Down, new List<InputButton>{new InputButton { Key = Keys.Down, Trigger = ButtonTrigger.Press } } },
            { InputActions.Left, new List<InputButton>{new InputButton { Key = Keys.Left, Trigger = ButtonTrigger.Press } } },
            { InputActions.Right, new List<InputButton>{new InputButton { Key = Keys.Right, Trigger = ButtonTrigger.Press } } },

            { InputActions.Reset, new List<InputButton>{new InputButton { Key = Keys.R, Combo = new InputButton { Key = Keys.LeftControl } }} },
            { InputActions.Exit, new List<InputButton>{new InputButton { Key = Keys.Q, Combo = new InputButton { Key = Keys.LeftControl } } } },
        };

        #endregion Constructors

        #region Properties

        public override Dictionary<List<FF8TextTagKey>, List<InputButton>> Data
        {
            get;
            protected set;
        }

        public override bool DrawControllerButtons => false;

        #endregion Properties
    }

    [Serializable]
    public class Inputs_FF8Steam : Inputs
    {
        #region Constructors

        public Inputs_FF8Steam() => Data = new Dictionary<List<FF8TextTagKey>, List<InputButton>>
        {
            { InputActions.EscapeLeft , new List<InputButton>{ new InputButton { Key = Keys.D } } },
            { InputActions.EscapeRight, new List<InputButton>{new InputButton { Key = Keys.F }} },

            { InputActions.RotateLeft, new List<InputButton>{new InputButton { Key = Keys.H }} },
            { InputActions.RotateRight, new List<InputButton>{new InputButton { Key = Keys.G }} },

            { InputActions.Cancel, new List<InputButton>{new InputButton { Key = Keys.C }} },
            { InputActions.Menu, new List<InputButton>{new InputButton { Key = Keys.V }} },
            { InputActions.Confirm, new List<InputButton>{new InputButton { Key = Keys.X }} },
            { InputActions.Cards, new List<InputButton>{new InputButton { Key = Keys.S }} },

            { InputActions.Select, new List<InputButton>{new InputButton { Key = Keys.J }} },
            { InputActions.Pause, new List<InputButton>{new InputButton { Key = Keys.A }} },

            { InputActions.Up, new List<InputButton>{new InputButton { Key = Keys.Up, Trigger = ButtonTrigger.Press }} },
            { InputActions.Down, new List<InputButton>{new InputButton { Key = Keys.Down, Trigger = ButtonTrigger.Press } } },
            { InputActions.Left, new List<InputButton>{new InputButton { Key = Keys.Left, Trigger = ButtonTrigger.Press } } },
            { InputActions.Right, new List<InputButton>{new InputButton { Key = Keys.Right, Trigger = ButtonTrigger.Press } } },

            { InputActions.ExitMenu, new List<InputButton>{new InputButton { Key = Keys.Escape }} },
            { InputActions.Reset, new List<InputButton>{new InputButton { Key = Keys.R, Combo = new InputButton { Key = Keys.LeftControl } }} },
            { InputActions.Exit, new List<InputButton>{new InputButton { Key = Keys.Q, Combo = new InputButton { Key = Keys.LeftControl } } } },
        };

        #endregion Constructors

        #region Properties

        public override Dictionary<List<FF8TextTagKey>, List<InputButton>> Data
        {
            get;
            protected set;
        }

        public override bool DrawControllerButtons => false;

        #endregion Properties
    }

    [Serializable]
    public class Inputs_OpenVIII : Inputs
    {
        #region Constructors

        public Inputs_OpenVIII() => Data = new Dictionary<List<FF8TextTagKey>, List<InputButton>>
        {
            //{ InputActions.EscapeLeft , new List<InputButton>{ new InputButton { Key = Keys.D } } },
            //{ InputActions.EscapeRight, new List<InputButton>{new InputButton { Key = Keys.F }} },

            //{ InputActions.RotateLeft, new List<InputButton>{new InputButton { Key = Keys.H }} },
            //{ InputActions.RotateRight, new List<InputButton>{new InputButton { Key = Keys.G }} },

            { InputActions.Cancel, new List<InputButton>{new InputButton { Key = Keys.Enter }} },
            //{ InputActions.Menu, new List<InputButton>{new InputButton { Key = Keys.V }} },
            { InputActions.Confirm, new List<InputButton>{new InputButton { Key = Keys.Back }} },
            //{ InputActions.Cards, new List<InputButton>{new InputButton { Key = Keys.S }} },

            //{ InputActions.Select, new List<InputButton>{new InputButton { Key = Keys.J }} },
            { InputActions.Pause, new List<InputButton>{new InputButton { Key = Keys.Pause }} },

            { InputActions.Up, new List<InputButton>{
                new InputButton { Key = Keys.Up, Trigger = ButtonTrigger.Press },
                new InputButton { Key = Keys.W, Trigger = ButtonTrigger.Press },
            }},
            { InputActions.Down, new List<InputButton>{
                new InputButton { Key = Keys.Down, Trigger = ButtonTrigger.Press },
                new InputButton { Key = Keys.S, Trigger = ButtonTrigger.Press },
            }},
            { InputActions.Left, new List<InputButton>{
                new InputButton { Key = Keys.Left, Trigger = ButtonTrigger.Press },
                new InputButton { Key = Keys.A, Trigger = ButtonTrigger.Press },
            }},
            { InputActions.Right, new List<InputButton>{
                new InputButton { Key = Keys.Right, Trigger = ButtonTrigger.Press },
                new InputButton { Key = Keys.D, Trigger = ButtonTrigger.Press },
            }},

            { InputActions.ExitMenu, new List<InputButton>{new InputButton { Key = Keys.Escape }} },
            { InputActions.Reset, new List<InputButton>{new InputButton { Key = Keys.R, Combo = new InputButton { Key = Keys.LeftControl } }} },
            { InputActions.Exit, new List<InputButton>{new InputButton { Key = Keys.Q, Combo = new InputButton { Key = Keys.LeftControl } } } },
        };

        #endregion Constructors

        #region Properties

        public override Dictionary<List<FF8TextTagKey>, List<InputButton>> Data
        {
            get;
            protected set;
        }

        public override bool DrawControllerButtons => false;

        #endregion Properties
    }

    [Serializable]
    public class Inputs_FF8PSX : Inputs
    {
        #region Constructors

        public Inputs_FF8PSX() => Data = new Dictionary<List<FF8TextTagKey>, List<InputButton>>
        {
            { InputActions.EscapeLeft , new List<InputButton>{ new InputButton { ControllerButton = ControllerButtons.L2 } } },
            { InputActions.EscapeRight, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.R2 } } },

            { InputActions.RotateLeft, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.L1 } } },
            { InputActions.RotateRight, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.R1 } } },

            { InputActions.Cancel, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Triangle } } },
            { InputActions.Menu, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Circle } } },
            { InputActions.Confirm, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Cross } } },
            { InputActions.Cards, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Square } } },

            { InputActions.Select, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Select } } },
            { InputActions.Pause, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Start } } },

            { InputActions.Up, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Up, Trigger = ButtonTrigger.Press }} },
            { InputActions.Down, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Down, Trigger = ButtonTrigger.Press } } },
            { InputActions.Left, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Left, Trigger = ButtonTrigger.Press } } },
            { InputActions.Right, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Right, Trigger = ButtonTrigger.Press } } },

            //{ InputActions.ExitMenu, new List<InputButton>{new InputButton { Key = Keys.Escape }} },
            { InputActions.Reset, new List<InputButton>{
                new InputButton { ControllerButton = ControllerButtons.Start, Combo =
                new InputButton { ControllerButton = ControllerButtons.Select, Combo =
                new InputButton { ControllerButton = ControllerButtons.L1, Combo =
                new InputButton { ControllerButton = ControllerButtons.L2, Combo =
                new InputButton { ControllerButton = ControllerButtons.R1, Combo =
                new InputButton { ControllerButton = ControllerButtons.R2
            }}}}}}}},
            //{ InputActions.Exit, new List<InputButton>{new InputButton { Key = Keys.Q, Combo = new InputButton { Key = Keys.LeftControl } } } },
        };

        #endregion Constructors

        #region Properties

        public override Dictionary<List<FF8TextTagKey>, List<InputButton>> Data
        {
            get;
            protected set;
        }

        public override bool DrawControllerButtons => true;

        #endregion Properties
    }
    [Serializable]
    public class Inputs_FF7PSX : Inputs
    {
        #region Constructors

        public Inputs_FF7PSX() => Data = new Dictionary<List<FF8TextTagKey>, List<InputButton>>
        {
            { InputActions.EscapeLeft , new List<InputButton>{ new InputButton { ControllerButton = ControllerButtons.L2 } } },
            { InputActions.EscapeRight, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.R2 } } },

            { InputActions.RotateLeft, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.L1 } } },
            { InputActions.RotateRight, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.R1 } } },

            { InputActions.Cancel, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Cross } } },
            { InputActions.Menu, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Triangle } } },
            { InputActions.Confirm, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Circle } } },
            { InputActions.Cards, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Square } } },

            { InputActions.Select, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Select } } },
            { InputActions.Pause, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Start } } },

            { InputActions.Up, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Up, Trigger = ButtonTrigger.Press }} },
            { InputActions.Down, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Down, Trigger = ButtonTrigger.Press } } },
            { InputActions.Left, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Left, Trigger = ButtonTrigger.Press } } },
            { InputActions.Right, new List<InputButton>{new InputButton { ControllerButton = ControllerButtons.Right, Trigger = ButtonTrigger.Press } } },

            //{ InputActions.ExitMenu, new List<InputButton>{new InputButton { Key = Keys.Escape }} },
            { InputActions.Reset, new List<InputButton>{
                new InputButton { ControllerButton = ControllerButtons.Start, Combo =
                new InputButton { ControllerButton = ControllerButtons.Select, Combo =
                new InputButton { ControllerButton = ControllerButtons.L1, Combo =
                new InputButton { ControllerButton = ControllerButtons.L2, Combo =
                new InputButton { ControllerButton = ControllerButtons.R1, Combo =
                new InputButton { ControllerButton = ControllerButtons.R2
            }}}}}}}},
            //{ InputActions.Exit, new List<InputButton>{new InputButton { Key = Keys.Q, Combo = new InputButton { Key = Keys.LeftControl } } } },
        };

        #endregion Constructors

        #region Properties

        public override Dictionary<List<FF8TextTagKey>, List<InputButton>> Data
        {
            get;
            protected set;
        }

        public override bool DrawControllerButtons => true;

        #endregion Properties
    }
    public abstract class Inputs
    {
        #region Properties

        public abstract Dictionary<List<FF8TextTagKey>, List<InputButton>> Data { get; protected set; }

        public abstract bool DrawControllerButtons { get; }

        #endregion Properties
    }
}