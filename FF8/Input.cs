using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FF8
{
    public enum Buttons { Up, Down, Left, Right, Okay, Cancel, Exit, Switch, Menu, Back, Start, X, Y, B, A, L1, L2, L3, R1, R2, R3, Select, LeftStickX, LeftStickY, RightStickX, RightStickY, MouseX, MouseY, MouseXjoy, MouseYjoy, MouseLeft, MouseMiddle, MouseRight, Mouse4, Mouse5, MouseWheelup, MouseWheeldown }

    internal class Input
    {
        //store current input states;
        private static GamePadState m_gp_state = new GamePadState();
        private static KeyboardState m_kb_state = new KeyboardState();
        private static MouseState m_m_state = new MouseState();
        //properties to assign current input states and back up last input states on the fly.
        protected static GamePadState CurrentGPState { get => m_gp_state; set { LastGPState = m_gp_state; m_gp_state = value; } }
        protected static KeyboardState CurrentKBState { get => m_kb_state; set { LastKBState = m_kb_state; m_kb_state = value; } }
        protected static MouseState CurrentMState { get => m_m_state; set { LastMState = m_m_state; m_m_state = value; } }
        //last states
        protected static GamePadState LastGPState { get; private set; } = new GamePadState();
        protected static KeyboardState LastKBState { get; private set; } = new KeyboardState();
        protected static MouseState LastMState { get; private set; }

        private static bool bLimitInput;
        private static int msDelay = 0;
        private static readonly int msDelayLimit = 100;


        public static bool GetInputDelayed(Keys key)
        {
            //if (bLimitInput)
            //    bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.Milliseconds) < msDelayLimit;
            if (Keyboard.GetState().IsKeyDown(key) && !bLimitInput)
            {
                ResetInputLimit();
                return true;
            }
            return false;
        }
        public static void Update()
        {
            CurrentGPState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            CurrentKBState = Keyboard.GetState();
            CurrentMState = Mouse.GetState();
            LockMouse();
            CheckInputLimit();
        }
        private static void CheckInputLimit()
        {
            if (bLimitInput)
            {
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.Milliseconds) < msDelayLimit;
            }
        }
        public static void ResetInputLimit()
        {
            msDelay = 0;
            bLimitInput = true;
        }
        private static bool IsPressed(Keys k, bool dblinput = false)
        {
            //This function checks if the key on KB is pressed
            //if dblinput is false it makes sure the key wasn't pressed last time.
            bool boolRT = CurrentKBState.IsKeyDown(k);
            if (!dblinput)
            {
                boolRT = boolRT && LastKBState.IsKeyUp(k);
            }
            else
            {
                boolRT = boolRT && !bLimitInput;
            }

            return boolRT;

        }
        private static bool IsPressed(Buttons b, bool dblinput = false)
        {
            //This function checks if the button on controller is pressed
            //if dblinput is false it makes sure the button wasn't pressed last time.
            bool boolRT = false;

            if (CurrentGPState.IsConnected)
            {
                switch (b)
                {
                    case Buttons.Up:
                        boolRT = CurrentGPState.DPad.Up == ButtonState.Pressed;
                        break;
                    case Buttons.Down:
                        boolRT = CurrentGPState.DPad.Down == ButtonState.Pressed;
                        break;
                    case Buttons.Left:
                        boolRT = CurrentGPState.DPad.Left == ButtonState.Pressed;
                        break;
                    case Buttons.Right:
                        boolRT = CurrentGPState.DPad.Right == ButtonState.Pressed;
                        break;
                    case Buttons.B:
                        boolRT = CurrentGPState.Buttons.B == ButtonState.Pressed;
                        break;
                    case Buttons.A:
                        boolRT = CurrentGPState.Buttons.A == ButtonState.Pressed;
                        break;
                    case Buttons.Y:
                        boolRT = CurrentGPState.Buttons.Y == ButtonState.Pressed;
                        break;
                    case Buttons.X:
                        boolRT = CurrentGPState.Buttons.X == ButtonState.Pressed;
                        break;
                    case Buttons.L1:
                        boolRT = CurrentGPState.Buttons.LeftShoulder == ButtonState.Pressed;
                        break;
                    case Buttons.L2:
                    case Buttons.R2:
                        boolRT = Analog(b) >= 0.25f; // treating the trigger like a button ignoring .25
                        break;
                    case Buttons.L3:
                        boolRT = CurrentGPState.Buttons.LeftStick == ButtonState.Pressed;
                        break;
                    case Buttons.R1:
                        boolRT = CurrentGPState.Buttons.RightShoulder == ButtonState.Pressed;
                        break;
                    case Buttons.R3:
                        boolRT = CurrentGPState.Buttons.RightStick == ButtonState.Pressed;
                        break;
                    case Buttons.Start:
                        boolRT = CurrentGPState.Buttons.Start == ButtonState.Pressed;
                        break;
                    case Buttons.Back:
                        boolRT = CurrentGPState.Buttons.Back == ButtonState.Pressed;
                        break;
                    // this would be used as a test to see if stick is currently used.
                    // not ment to control anything.
                    case Buttons.LeftStickX:
                    case Buttons.LeftStickY:
                    case Buttons.RightStickX:
                    case Buttons.RightStickY:
                        boolRT = Analog(b) != 0.0f;
                        break;
                }
                if (Memory.IsActive)
                {
                    switch (b)
                    {
                        case Buttons.MouseXjoy:
                        case Buttons.MouseYjoy:
                            boolRT = Analog(b) != 0.0f;
                            break;
                        case Buttons.MouseX:
                        case Buttons.MouseY:
                            boolRT = Analog(b) != Analog(b, true);
                            break;
                        case Buttons.MouseLeft:
                            boolRT = CurrentMState.LeftButton == ButtonState.Pressed;
                            break;
                        case Buttons.MouseMiddle:
                            boolRT = CurrentMState.MiddleButton == ButtonState.Pressed;
                            break;
                        case Buttons.MouseRight:
                            boolRT = CurrentMState.RightButton == ButtonState.Pressed;
                            break;
                        case Buttons.Mouse4:
                            boolRT = CurrentMState.XButton1 == ButtonState.Pressed;
                            break;
                        case Buttons.Mouse5:
                            boolRT = CurrentMState.XButton2 == ButtonState.Pressed;
                            break;
                        case Buttons.MouseWheelup:
                            boolRT = CurrentMState.ScrollWheelValue > LastMState.ScrollWheelValue;
                            break;
                        case Buttons.MouseWheeldown:
                            boolRT = CurrentMState.ScrollWheelValue < LastMState.ScrollWheelValue;
                            break;
                    }
                }

                if (!dblinput && boolRT)
                {
                    switch (b)
                    {
                        case Buttons.Up:
                            boolRT = LastGPState.DPad.Up == ButtonState.Released;
                            break;
                        case Buttons.Down:
                            boolRT = LastGPState.DPad.Down == ButtonState.Released;
                            break;
                        case Buttons.Left:
                            boolRT = LastGPState.DPad.Left == ButtonState.Released;
                            break;
                        case Buttons.Right:
                            boolRT = LastGPState.DPad.Right == ButtonState.Released;
                            break;
                        case Buttons.B:
                            boolRT = LastGPState.Buttons.B == ButtonState.Released;
                            break;
                        case Buttons.A:
                            boolRT = LastGPState.Buttons.A == ButtonState.Released;
                            break;
                        case Buttons.Y:
                            boolRT = LastGPState.Buttons.Y == ButtonState.Released;
                            break;
                        case Buttons.X:
                            boolRT = LastGPState.Buttons.X == ButtonState.Released;
                            break;
                        case Buttons.L1:
                            boolRT = LastGPState.Buttons.LeftShoulder == ButtonState.Released;
                            break;
                        case Buttons.L2:
                        case Buttons.R2:
                            boolRT = Analog(b, true) < 0.25f; // treating the trigger like a button ignoring .25
                            break;
                        case Buttons.L3:
                            boolRT = LastGPState.Buttons.LeftStick == ButtonState.Released;
                            break;
                        case Buttons.R1:
                            boolRT = LastGPState.Buttons.RightShoulder == ButtonState.Released;
                            break;
                        case Buttons.R3:
                            boolRT = LastGPState.Buttons.RightStick == ButtonState.Released;
                            break;
                        case Buttons.Start:
                            boolRT = LastGPState.Buttons.Start == ButtonState.Released;
                            break;
                        case Buttons.Back:
                            boolRT = LastGPState.Buttons.Back == ButtonState.Released;
                            break;
                        // this would be used as a test to see if stick was released previously
                        // not ment to control anything.
                        case Buttons.LeftStickX:
                        case Buttons.LeftStickY:
                        case Buttons.RightStickX:
                        case Buttons.RightStickY:
                            boolRT = Analog(b, true) == 0.0f;
                            break;
                    }
                    if (Memory.IsActive)
                    {
                        switch (b)
                        {
                            case Buttons.MouseXjoy:
                            case Buttons.MouseYjoy:
                                boolRT = Analog(b, true) == 0.0f;
                                break;
                            case Buttons.MouseLeft:
                                boolRT = LastMState.LeftButton == ButtonState.Released;
                                break;
                            case Buttons.MouseMiddle:
                                boolRT = LastMState.MiddleButton == ButtonState.Released;
                                break;
                            case Buttons.MouseRight:
                                boolRT = LastMState.RightButton == ButtonState.Released;
                                break;
                            case Buttons.Mouse4:
                                boolRT = LastMState.XButton1 == ButtonState.Released;
                                break;
                            case Buttons.Mouse5:
                                boolRT = LastMState.XButton2 == ButtonState.Released;
                                break;
                        }
                    }
                }
                else
                {
                    boolRT = boolRT && !bLimitInput;
                }
            }
            if (boolRT)
            {
            }
            return boolRT;
        }
        public static float Distance(float speed)
        {
            // no input throttle but still take the max speed * time;
            // for non analog controls
            return speed * Memory.gameTime.ElapsedGameTime.Milliseconds;
        }
        public static float Distance(Buttons b, float speed, bool last = false)
        {
            // (speed * stickvalue) * time = distance
            // idea is you get the distance traveled per ms value
            // the speed being the max speed. your sticks value being the throttle.
            return (speed * Analog(b, last)) * Memory.gameTime.ElapsedGameTime.Milliseconds;
        }
        public static float Analog(Buttons b, bool last = false)
        {
            //get output from analog controls
            //mousexjoy and mouseyjoy attempt to convert mouse input to a joystick like input 1.0f to -1.0f
            float tmp = 0f;
            if (last)
            {
                switch (b)
                {
                    case Buttons.LeftStickX:
                        return LastGPState.ThumbSticks.Left.X;
                    case Buttons.LeftStickY:
                        return LastGPState.ThumbSticks.Left.Y;
                    case Buttons.RightStickX:
                        return LastGPState.ThumbSticks.Right.X;
                    case Buttons.RightStickY:
                        return LastGPState.ThumbSticks.Right.Y;
                    case Buttons.L2:
                        return LastGPState.Triggers.Left;
                    case Buttons.R2:
                        return LastGPState.Triggers.Right;
                }
                if (Memory.IsActive)
                {
                    switch (b)
                    {
                        case Buttons.MouseX:
                            return LastMState.X;
                        case Buttons.MouseY:
                            return LastMState.Y;
                        case Buttons.MouseXjoy:
                            tmp = (LastMState.X - Memory.graphics.GraphicsDevice.Viewport.Bounds.Width / 2) / (50f);
                            return MathHelper.Clamp(tmp, -1f, 1f);
                        case Buttons.MouseYjoy:
                            tmp = (Memory.graphics.GraphicsDevice.Viewport.Bounds.Height / 2 - LastMState.Y) / (50f);
                            return MathHelper.Clamp(tmp, -1f, 1f);
                    }
                }
            }
            else
            {
                switch (b)
                {
                    case Buttons.LeftStickX:
                        return CurrentGPState.ThumbSticks.Left.X;
                    case Buttons.LeftStickY:
                        return CurrentGPState.ThumbSticks.Left.Y;
                    case Buttons.RightStickX:
                        return CurrentGPState.ThumbSticks.Right.X;
                    case Buttons.RightStickY:
                        return CurrentGPState.ThumbSticks.Right.Y;
                    case Buttons.L2:
                        return CurrentGPState.Triggers.Left;
                    case Buttons.R2:
                        return CurrentGPState.Triggers.Right;
                }
                if (Memory.IsActive)
                {
                    switch (b)
                    {
                        case Buttons.MouseX:
                            return LastMState.X;
                        case Buttons.MouseY:
                            return LastMState.Y;
                        case Buttons.MouseXjoy:
                            tmp = (CurrentMState.X - Memory.graphics.GraphicsDevice.Viewport.Bounds.Width / 2) / (50f);
                            return MathHelper.Clamp(tmp, -1f, 1f);
                        case Buttons.MouseYjoy:
                            tmp = (Memory.graphics.GraphicsDevice.Viewport.Bounds.Height / 2 - CurrentMState.Y) / (50f);
                            return MathHelper.Clamp(tmp, -1f, 1f);
                    }
                }
            }
            return 0.0f;
        }
        public static MouseLockMode CurrentMode;

        public static bool OverrideLockMouse { get; set; } = false;

        public enum MouseLockMode
        {
            Center,
            Screen
        }
        public static void LockMouse()
        {
            if (Memory.IsActive && OverrideLockMouse) // check for focus to allow for tabbing out with out taking over mouse.
            {
                if (CurrentMode == MouseLockMode.Center) //center mouse in screen after grabbing state, release mouse if alt tabbed out.
                {
                    Mouse.SetPosition(Memory.graphics.GraphicsDevice.Viewport.Bounds.Width / 2, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height / 2);
                }
                else if (CurrentMode == MouseLockMode.Screen) //alt lock that clamps to viewport every frame. would be useful if using mouse to navigate menus and stuff.
                {
                    //there is a better way to clamp as if you move mouse fast enough it will escape for a short time.
                    Mouse.SetPosition(
                        MathHelper.Clamp(CurrentMState.X, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Width),
                        MathHelper.Clamp(CurrentMState.Y, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height));
                }
            }
        }
        public static bool Button(Keys k, bool dblinput = false)
        {
            return IsPressed(k, dblinput); // fail over to IsPressed
        }
        public static bool Button(Buttons b, bool dblinput = false)
        {
            // To add support for controller I was extracting the boolean bits from the if statements.
            // Maybe it could be a scheme in future. When these are configureable.
            // This function mostly translates the function to the button(s) or key(s).
            switch (b)
            {
                case Buttons.Up:
                    return (IsPressed(Keys.Up, true) || IsPressed(Buttons.MouseWheelup) || IsPressed(Keys.W, true) || (CurrentGPState.IsConnected && (IsPressed(b, true) || Analog(Buttons.LeftStickY) > 0.0f))) && !bLimitInput;
                case Buttons.Down:
                    return (IsPressed(Keys.Down, true) || IsPressed(Buttons.MouseWheeldown) || IsPressed(Keys.S, true) || (CurrentGPState.IsConnected && (IsPressed(b, true) || Analog(Buttons.LeftStickY) < 0.0f))) && !bLimitInput;
                case Buttons.Left:
                    return (IsPressed(Keys.Left, true) || IsPressed(Keys.A, true) || (CurrentGPState.IsConnected && (IsPressed(b, true) || Analog(Buttons.LeftStickX) < 0.0f))) && !bLimitInput;
                case Buttons.Right:
                    return (IsPressed(Keys.Right, true) || IsPressed(Keys.D, true) || (CurrentGPState.IsConnected && (IsPressed(b, true) || Analog(Buttons.LeftStickX) > 0.0f))) && !bLimitInput;
                case Buttons.Okay:
                    return IsPressed(Keys.Enter, dblinput) || IsPressed(Buttons.B, dblinput) || IsPressed(Buttons.MouseLeft);
                case Buttons.Cancel:
                    return IsPressed(Keys.Back, dblinput) || IsPressed(Buttons.A, dblinput) || IsPressed(Buttons.MouseRight);
                case Buttons.Menu:
                    return IsPressed(Keys.PageUp, dblinput) || IsPressed(Buttons.Y, dblinput);
                case Buttons.Switch:
                    return IsPressed(Keys.PageDown, dblinput) || IsPressed(Buttons.X, dblinput);
                case Buttons.Start:
                    return IsPressed(Keys.Home, dblinput) || IsPressed(Buttons.Start, dblinput);
                case Buttons.Select:
                    return IsPressed(Keys.End, dblinput) || IsPressed(Buttons.Back, dblinput);
                case Buttons.Exit:
                    return IsPressed(Keys.Escape, dblinput);
                default:
                    return IsPressed(b, dblinput); // fail over to IsPressed if no custom input is avalible above
            }
        }
    }
}
