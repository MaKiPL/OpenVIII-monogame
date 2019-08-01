using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public abstract class Input2
    {
        #region Fields

        protected static bool bLimitInput;
        public static InputKeyboard Keyboard { get; private set; }
        public static InputGamePad GamePad { get; private set; }
        public static InputMouse Mouse { get; private set; }
        protected static double msDelay;
        private static readonly int msDelayLimit = 100;

        #endregion Fields

        #region Constructors

        public Input2()
        {
            if (Keyboard != null)
                Keyboard = new InputKeyboard();
            if (Mouse != null)
                Mouse = new InputMouse();
            if (GamePad != null)
                GamePad = new InputGamePad();
            if (InputList != null)
                InputList = new List<Inputs>
                {
                    new Inputs_OpenVIII(),
                    new Inputs_FF8PSX(),
                    new Inputs_FF8Steam(),
                    new Inputs_FF82000()
                };
        }

        #endregion Constructors

        #region Methods

        public static bool Update()
        {
            CheckInputLimit();
            Keyboard.UpdateOnce();
            GamePad.UpdateOnce();
            Mouse.UpdateOnce();
            return false;
        }

        protected bool ButtonTriggered(FF8TextTagKey key)
        {
            foreach (var list in InputList.Where(x => x.Data.Any(y => y.Key.Contains(key))))
            {
                foreach (var kvp in list.Data.Where(y => y.Key.Contains(key)))
                {
                    foreach (var test in kvp.Value)
                    {
                        ButtonTriggered(test);
                    }
                }
            }

            return false;
        }

        protected virtual bool ButtonTriggered(InputButton test)
        {
            if (Keyboard.ButtonTriggered(test))
                return true;
            if (Mouse.ButtonTriggered(test))
                return true;
            if (GamePad.ButtonTriggered(test))
                return true;
            return false;
        }
        protected abstract bool UpdateOnce();

        private static void CheckInputLimit()
        {
            //issue here if CheckInputLimit is checked more than once per update cycle this will be wrong.
            if (Memory.gameTime != null)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.TotalMilliseconds) < msDelayLimit;
            if (!bLimitInput) msDelay = 0;
        }

        public static List<Inputs> InputList { get; private set; }

        #endregion Methods
    }
}