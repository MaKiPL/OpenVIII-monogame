using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OpenVIII
{
    public class InputMouse : Input2
    {
        #region Fields

        private static MouseState last_state;
        private static MouseState state;

        #endregion Fields

        #region Constructors

        public InputMouse(bool skip = true) : base(skip)
        {
        }

        #endregion Constructors

        #region Properties

        public static Point Location => new Point(state.X, state.Y);
        public static MouseLockMode Mode { get; set; } = MouseLockMode.Screen;
        public Point Last_MouseLocation => new Point(last_state.X, last_state.Y);
        public Point MouseLocation => new Point(state.X, state.Y);
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

        public static Vector2 Distance(MouseButtons mouseToStick, float speed) => Translate_Stick(mouseToStick, state) * (float)Distance(speed);

        public void LockMouse()
        {
            if (Memory.IsActive && Mode != MouseLockMode.Disabled) // check for focus to allow for tabbing out with out taking over mouse.
            {
                if (Mode == MouseLockMode.Center) //center mouse in screen after grabbing state, release mouse if alt tabbed out.
                {
                    Microsoft.Xna.Framework.Input.Mouse.SetPosition(Memory.graphics.GraphicsDevice.Viewport.Bounds.Width / 2, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height / 2);
                }
                else if (Mode == MouseLockMode.Screen) //alt lock that clamps to viewport every frame. would be useful if using mouse to navigate menus and stuff.
                {
                    Rectangle vpb = Memory.graphics.GraphicsDevice.Viewport.Bounds;
                    //there is a better way to clamp as if you move mouse fast enough it will escape for a short time.
                    if (!(state.X >= 0 && state.X <= vpb.Width) || !(state.Y >= 0 && state.Y <= vpb.Height))
                    {
                        Microsoft.Xna.Framework.Input.Mouse.SetPosition(
                            MathHelper.Clamp(state.X, 0, vpb.Width),
                            MathHelper.Clamp(state.Y, 0, vpb.Height));
                    }
                }
            }
        }

        protected override bool ButtonTriggered(InputButton test, ButtonTrigger trigger = ButtonTrigger.None)
        {
            if (test != null && test.MouseButton != MouseButtons.None)
            {
                bool combotest = false;
                if (test.Combo != null)
                {
                    foreach (InputButton item in test.Combo)
                    {
                        item.Trigger = ButtonTrigger.Press;
                        if (!base.ButtonTriggered(item))
                        {
                            return false;
                        }
                    }
                    combotest = true;
                }
                ButtonTrigger triggertest = test.Trigger | trigger;
                return ((test.Combo == null || combotest) &&
                    ((triggertest & ButtonTrigger.OnPress) != 0 && OnPress(test.MouseButton)) ||
                    ((triggertest & ButtonTrigger.OnRelease) != 0 && OnRelease(test.MouseButton)) ||
                    ((triggertest & ButtonTrigger.Press) != 0 && Press(test.MouseButton, state)));
            }
            return false;
        }

        protected override bool UpdateOnce()
        {
            State = Microsoft.Xna.Framework.Input.Mouse.GetState();
            LockMouse();
            return false;
        }

        private static ButtonState Translate_Buttons(MouseButtons k, MouseState _state)
        {
            switch (k)
            {
                case MouseButtons.XButton1:
                    return _state.XButton1;

                case MouseButtons.XButton2:
                    return _state.XButton2;

                case MouseButtons.LeftButton:
                    return _state.LeftButton;

                case MouseButtons.MiddleButton:
                    return _state.MiddleButton;

                case MouseButtons.RightButton:
                    return _state.RightButton;

                case MouseButtons.MouseWheelup:
                    if (state.Equals(_state))
                        return state.ScrollWheelValue > last_state.ScrollWheelValue ? ButtonState.Pressed : ButtonState.Released;
                    break;

                case MouseButtons.MouseWheeldown:
                    if (state.Equals(_state))
                        return state.ScrollWheelValue < last_state.ScrollWheelValue ? ButtonState.Pressed : ButtonState.Released;
                    break;

                case MouseButtons.HorizMouseWheelup:
                    if (state.Equals(_state))
                        return state.HorizontalScrollWheelValue > last_state.HorizontalScrollWheelValue ? ButtonState.Pressed : ButtonState.Released;
                    break;

                case MouseButtons.HorizMouseWheeldown:
                    if (state.Equals(_state))
                        return state.HorizontalScrollWheelValue < last_state.HorizontalScrollWheelValue ? ButtonState.Pressed : ButtonState.Released;
                    break;
            }
            return ButtonState.Released;
        }

        private static Vector2 Translate_Stick(MouseButtons k, MouseState _state)
        {
            switch (k)
            {
                case MouseButtons.MouseToStick:
                    if (Mode == MouseLockMode.Center)
                    {
                        float tmpX = MathHelper.Clamp((_state.X - Memory.graphics.GraphicsDevice.Viewport.Bounds.Width / 2) / (50f), -1f, 1f);
                        float tmpY = MathHelper.Clamp((Memory.graphics.GraphicsDevice.Viewport.Bounds.Height / 2 - _state.Y) / (50f), -1f, 1f);
                        return new Vector2(tmpX, tmpY);
                    }
                    break;
            }
            return Vector2.Zero;
        }

        private bool OnPress(MouseButtons k) => Release(k, last_state) && Press(k, state);

        private bool OnRelease(MouseButtons k) => Press(k, last_state) && Release(k, state);

        private bool Press(MouseButtons k, MouseState _state)
        {
            ButtonState bs = Translate_Buttons(k, _state);
            if (bs == ButtonState.Pressed)
                return true;
            return false;
        }

        private bool Release(MouseButtons k, MouseState _state) => !Press(k, _state);

        #endregion Methods
    }
}