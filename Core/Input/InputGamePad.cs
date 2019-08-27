using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public class InputGamePad : Input2
    {
        protected struct Iap
        {
            public Icons.ID id;
            public byte palette;

            public Iap(Icons.ID id = Icons.ID.None, byte palette = 2)
            {
                this.id = id;
                this.palette = palette;
            }
        }
        #region Fields

        private static readonly float deadzone_r = 1f / 100;
        private static GamePadState last_state;
        private static GamePadState state;

        private static Dictionary<GamePadButtons,Iap> iap;

        #endregion Fields

        #region Constructors

        public InputGamePad(bool skip = true) : base(skip)
        {
            if(iap == null)
            {
                iap = new Dictionary<GamePadButtons, Iap>
                {
                    { GamePadButtons.Up, new Iap{ id= Icons.ID.D_Pad_Up, palette = 2 } },
                    { GamePadButtons.Down, new Iap{ id= Icons.ID.D_Pad_Down, palette = 2 } },
                    { GamePadButtons.Left, new Iap{ id= Icons.ID.D_Pad_Left, palette = 2 } },
                    { GamePadButtons.Right, new Iap{ id= Icons.ID.D_Pad_Right, palette = 2 } },
                    { GamePadButtons.X, new Iap {id = Icons.ID.Size_16x16_PSX_Square, palette = 4} },
                    { GamePadButtons.Y, new Iap {id = Icons.ID.Size_16x16_PSX_Triangle, palette = 4} },
                    { GamePadButtons.A, new Iap {id = Icons.ID.Size_16x16_PSX_Cross, palette = 4} },
                    { GamePadButtons.B, new Iap {id = Icons.ID.Size_16x16_PSX_Circle, palette = 4} },
                    { GamePadButtons.Back, new Iap {id = Icons.ID.SELECT, palette = 2} },
                    { GamePadButtons.Start, new Iap {id = Icons.ID.START, palette = 2} },
                    { GamePadButtons.Left_Shoulder, new Iap {id = Icons.ID.Size_16x08_PSX_L1, palette = 2} },
                    { GamePadButtons.Right_Shoulder, new Iap {id = Icons.ID.Size_16x08_PSX_R1, palette = 2} },
                    { GamePadButtons.Left_Trigger, new Iap {id = Icons.ID.Size_16x08_PSX_L2, palette = 2} },
                    { GamePadButtons.Right_Trigger, new Iap {id = Icons.ID.Size_16x08_PSX_R2, palette = 2} },
                };
            }
        }

        #endregion Constructors

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

        public static Vector2 Distance(GamePadButtons stick, float speed) => Translate_Stick(stick, state) * (float)Distance(speed);

        public override bool ButtonTriggered(InputButton test, ButtonTrigger trigger = ButtonTrigger.None)
        {
            if (test != null && test.GamePadButton != GamePadButtons.None)
            {
                bool combotest = false;
                if (test.Combo != null)
                {
                    foreach (var item in test.Combo)
                    {
                        item.Trigger = ButtonTrigger.Press;
                        if(!base.ButtonTriggered(item))
                        {
                            return false;
                        }
                    }
                    combotest = true;
                }
                var triggertest = test.Trigger | trigger;
                return ((test.Combo == null || combotest) &&
                    ((triggertest & ButtonTrigger.OnPress) != 0 && OnPress(test.GamePadButton)) ||
                    ((triggertest & ButtonTrigger.OnRelease) != 0 && OnRelease(test.GamePadButton)) ||
                    ((triggertest & ButtonTrigger.Press) != 0 && Press(test.GamePadButton, state)));
            }
            return false;
        }

        protected override bool UpdateOnce()
        {
            State = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            return false;
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
                case GamePadButtons.Left_Trigger:
                    return _state.Triggers.Left > deadzone_r ? _state.Triggers.Left : 0f;

                case GamePadButtons.Right_Trigger:
                    return _state.Triggers.Right > deadzone_r ? _state.Triggers.Right : 0f;
            }
            return 0f;
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
        public FF8String ButtonString(GamePadButtons gamePadButton, FF8TextTagKey key)
        {
            if (iap.ContainsKey(gamePadButton))
            {
                return (ButtonString(iap[gamePadButton]));
            }
            return "";
        }
        protected FF8String ButtonString(Iap i)
        {
            short id = (short)i.id;
            return new FF8String(new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.CustomICON, (byte)(id & 0xFF), (byte)((id & 0xFF00) >> 8), i.palette });
        }

        #endregion Methods
    }
}