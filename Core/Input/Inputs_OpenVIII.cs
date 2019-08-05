using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    [Serializable]
    public class Inputs_OpenVIII : Inputs
    {
        #region Constructors

        public Inputs_OpenVIII() => Data = new Dictionary<List<FF8TextTagKey>, List<InputButton>>
        {
            { InputActions.EscapeLeft , new List<InputButton>{ new InputButton { Key = Keys.Home } } },
            { InputActions.EscapeRight, new List<InputButton>{new InputButton { Key = Keys.End }} },

            { InputActions.RotateLeft, new List<InputButton>{new InputButton { Key = Keys.PageUp }} },
            { InputActions.RotateRight, new List<InputButton>{new InputButton { Key = Keys.PageDown }} },

            { InputActions.Cancel, new List<InputButton>{
                new InputButton { Key = Keys.Back },
                new InputButton { MouseButton = MouseButtons.RightButton }
            }},
            { InputActions.Menu, new List<InputButton>{new InputButton { Key = Keys.M }} },
            { InputActions.Confirm, new List<InputButton>{
                new InputButton { Key = Keys.Enter },
                new InputButton { MouseButton = MouseButtons.LeftButton, Trigger = ButtonTrigger.MouseOver | ButtonTrigger.OnPress }
            }},
            { InputActions.Cards, new List<InputButton>{new InputButton { Key = Keys.Space }} },

            { InputActions.Select, new List<InputButton>{new InputButton { Key = Keys.Tab }} },
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
                new InputButton { MouseButton = MouseButtons.MouseWheelup, Trigger = ButtonTrigger.MouseOver | ButtonTrigger.OnPress | ButtonTrigger.Scrolling },
            }},
            { InputActions.Right, new List<InputButton>{
                new InputButton { Key = Keys.Right, Trigger = ButtonTrigger.Press },
                new InputButton { Key = Keys.D, Trigger = ButtonTrigger.Press },
                new InputButton { MouseButton = MouseButtons.MouseWheeldown, Trigger = ButtonTrigger.MouseOver | ButtonTrigger.OnPress | ButtonTrigger.Scrolling },
            }},

            { InputActions.ExitMenu, new List<InputButton>{new InputButton { Key = Keys.Escape }} },
            { InputActions.Reset, new List<InputButton>{new InputButton { Key = Keys.R, Combo = new List<InputButton>{ new InputButton { Key = Keys.LeftControl } }} } },
            { InputActions.Exit, new List<InputButton>{new InputButton { Key = Keys.Q, Combo = new List<InputButton>{new InputButton { Key = Keys.LeftControl } } } } },
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
}