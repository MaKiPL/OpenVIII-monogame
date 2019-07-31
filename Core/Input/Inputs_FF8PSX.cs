using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
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
}