using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
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
}