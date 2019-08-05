using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj)
        {
            var o = (InputButton)obj;
            if (o.Key == Key &&
                o.MouseButton == MouseButton &&
                o.GamePadButton == GamePadButton &&
                //(o.Trigger & Trigger)!=0 &&
                (o.Combo == null || o.Combo.Equals(Combo)))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 37823841;
            hashCode = hashCode * -1521134295 + EqualityComparer<InputButton>.Default.GetHashCode(Combo);
            hashCode = hashCode * -1521134295 + GamePadButton.GetHashCode();
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + MouseButton.GetHashCode();
            return hashCode;
        }

        #endregion Fields

    }
}