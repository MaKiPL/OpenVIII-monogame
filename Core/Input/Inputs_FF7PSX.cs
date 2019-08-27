using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    [Serializable]
    public class Inputs_FF7PSX : Inputs
    {
        #region Constructors

        public Inputs_FF7PSX() => Data = new Dictionary<List<FF8TextTagKey>, List<InputButton>>
        {
            { InputActions.EscapeLeft , new List<InputButton>{ new InputButton { GamePadButton = GamePadButtons.L2 } } },
            { InputActions.EscapeRight, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.R2 } } },

            { InputActions.RotateLeft, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.L1 } } },
            { InputActions.RotateRight, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.R1 } } },

            { InputActions.Cancel, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Cross } } },
            { InputActions.Menu, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Triangle } } },
            { InputActions.Confirm, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Circle } } },
            { InputActions.Cards, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Square } } },

            { InputActions.Select, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Select } } },
            { InputActions.Pause, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Start } } },

            { InputActions.Up, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Up, Trigger = ButtonTrigger.Press }} },
            { InputActions.Down, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Down, Trigger = ButtonTrigger.Press } } },
            { InputActions.Left, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Left, Trigger = ButtonTrigger.Press } } },
            { InputActions.Right, new List<InputButton>{new InputButton { GamePadButton = GamePadButtons.Right, Trigger = ButtonTrigger.Press } } },

            //{ InputActions.ExitMenu, new List<InputButton>{new InputButton { Key = Keys.Escape }} },
            { InputActions.Reset, new List<InputButton>{
                new InputButton { GamePadButton = GamePadButtons.Start, Combo = new List<InputButton>
                {
                new InputButton { GamePadButton = GamePadButtons.Select },
                new InputButton { GamePadButton = GamePadButtons.L1 },
                new InputButton { GamePadButton = GamePadButtons.L2 },
                new InputButton { GamePadButton = GamePadButtons.R1 },
                new InputButton { GamePadButton = GamePadButtons.R2 }
                }
            }}},
            //{ InputActions.Exit, new List<InputButton>{new InputButton { Key = Keys.Q, Combo = new InputButton { Key = Keys.LeftControl } } } },
        };

        #endregion Constructors

        #region Properties

        public override Dictionary<List<FF8TextTagKey>, List<InputButton>> Data
        {
            get;
            protected set;
        }

        public override bool DrawGamePadButtons => true;

        #endregion Properties
    }
}