using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    [Serializable]
    public class InputButton
    {
        #region Fields

        public List<InputButton> Combo;
        public GamePadButtons GamePadButton;
        public double HoldMS;
        public Keys Key;
        public MouseButtons MouseButton;
        public ButtonTrigger Trigger = ButtonTrigger.OnPress;

        #endregion Fields

        #region Methods

        public InputButton Clone()
        {
            InputButton i = (InputButton)MemberwiseClone();
            i.Combo = new List<InputButton>(Combo.Count);
            foreach (InputButton item in Combo)
            {
                i.Combo.Add(item.Clone());
            }
            return i;
        }

        public override bool Equals(object obj)
        {
            InputButton o = (InputButton)obj;
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
            int hashCode = 37823841;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<InputButton>>.Default.GetHashCode(Combo);
            hashCode = hashCode * -1521134295 + GamePadButton.GetHashCode();
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + MouseButton.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            string s = "";
            if (Key != Keys.None)
                s = Key.ToString();
            if (GamePadButton != GamePadButtons.None)
                s = GamePadButton.ToString();
            if (MouseButton != MouseButtons.None)
                s = MouseButton.ToString();

            if (Combo != null)
            {
                foreach (InputButton item in Combo)
                {
                    s = $"{s}+{item}";
                }
            }
            return s;
        }

        #endregion Methods
    }
}