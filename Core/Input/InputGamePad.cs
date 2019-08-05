using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace OpenVIII
{
    public class InputGamePad : Input2
    {
        #region Fields

        private static readonly float deadzone_r = 1f / 100;
        private static GamePadState last_state;
        private static GamePadState state;

        public InputGamePad(bool skip = true) : base(skip)
        {
        }

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

        protected override bool ButtonTriggered(InputButton test)
        {
            if (test != null && test.GamePadButton != GamePadButtons.None)
            {
                return ((test.Combo == null || base.ButtonTriggered(test.Combo)) &&
                    ((test.Trigger & ButtonTrigger.OnPress) != 0 && OnPress(test.GamePadButton)) ||
                    ((test.Trigger & ButtonTrigger.OnRelease) != 0 && OnRelease(test.GamePadButton)) ||
                    ((test.Trigger & ButtonTrigger.Press) != 0 && Press(test.GamePadButton, state)));
            }
            return false;
        }

        protected override bool UpdateOnce()
        {
            State = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            return false;
        }

        private Vector2 DeadZoneTest(Vector2 v)
        {
            if (Vector2.Distance(Vector2.Zero, v) > deadzone_r &&
                Math.Abs(v.X) > deadzone_r && Math.Abs(v.Y) > deadzone_r)
            {
                return v;
            }
            return Vector2.Zero;
        }

        private bool OnPress(GamePadButtons k) => Release(k, last_state) && Press(k, state);

        private bool OnRelease(GamePadButtons k) => Release(k, last_state) && Press(k, state);

        private bool Press(GamePadButtons k, GamePadState _state)
        {
            ButtonState bs = Translate_Button(k, _state);
            float ts = Translate_Trigger(k, _state);
            Vector2 ss = Translate_Stick(k, _state);
            if ((ss != Vector2.Zero) ||
                (ts != 0f) ||
                (bs == ButtonState.Pressed))
                return true;
            return false;
        }

        private bool Release(GamePadButtons k, GamePadState _state) => !Press(k, _state);
        public static Vector2 Distance(GamePadButtons stick, float speed)
        {
            return Translate_Stick(stick, state) * (float)Distance(speed);
        }
        private static ButtonState Translate_Button(GamePadButtons k, GamePadState _state)
        {
            switch (k)
            {
                case GamePadButtons.A:
                    return _state.Buttons.A;

                case GamePadButtons.B:
                    return _state.Buttons.B;

                case GamePadButtons.X:
                    return _state.Buttons.X;

                case GamePadButtons.Y:
                    return _state.Buttons.Y;

                case GamePadButtons.Back:
                    return _state.Buttons.Back;

                case GamePadButtons.L1:
                    return _state.Buttons.LeftShoulder;

                case GamePadButtons.L3:
                    return _state.Buttons.LeftStick;

                case GamePadButtons.R1:
                    return _state.Buttons.RightShoulder;

                case GamePadButtons.R3:
                    return _state.Buttons.RightStick;

                case GamePadButtons.Start:
                    return _state.Buttons.Start;

                case GamePadButtons.Up:
                    return _state.DPad.Up;

                case GamePadButtons.Down:
                    return _state.DPad.Down;

                case GamePadButtons.Left:
                    return _state.DPad.Left;

                case GamePadButtons.Right:
                    return _state.DPad.Right;
            }
            return ButtonState.Released;
        }

        private static Vector2 Translate_Stick(GamePadButtons k, GamePadState _state)
        {
            switch (k)
            {
                case GamePadButtons.ThumbSticks_Left:
                    return _state.ThumbSticks.Left;

                case GamePadButtons.ThumbSticks_Right:
                    return _state.ThumbSticks.Right;
            }
            return Vector2.Zero;
        }

        private static float Translate_Trigger(GamePadButtons k, GamePadState _state)
        {
            switch (k)
            {
                case GamePadButtons.Triggers_Left:
                    return _state.Triggers.Left > deadzone_r ? _state.Triggers.Left : 0f;

                case GamePadButtons.Triggers_Right:
                    return _state.Triggers.Right > deadzone_r ? _state.Triggers.Right : 0f;
            }
            return 0f;
        }

        #endregion Methods
    }
}