using Microsoft.Xna.Framework.Input;
using System;

namespace OpenVIII
{
    [Serializable]
    public class InputButton
    {
        #region Fields

        public InputButton Combo;
        public GamePadButtons GamePadButton;
        public double HoldMS;
        public Keys Key;
        public MouseButtons MouseButton;
        public ButtonTrigger Trigger = ButtonTrigger.OnPress;

        #endregion Fields

        // | ButtonTrigger.OnRelease | ButtonTrigger.Press;
    }
}