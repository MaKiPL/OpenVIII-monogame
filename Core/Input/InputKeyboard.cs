using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace OpenVIII
{
    public class InputKeyboard : Input2
    {
        #region Fields

        private static KeyboardState last_state;
        private static KeyboardState state;

        #endregion Fields

        #region Constructors

        public InputKeyboard(bool skip = true) : base(skip)
        {
        }

        #endregion Constructors

        #region Properties

        protected static KeyboardState Last_State => last_state;
        protected static KeyboardState State
        {
            get => state; set
            {
                last_state = state;
                state = value;
            }
        }

        #endregion Properties

        #region Methods

        public override bool ButtonTriggered(InputButton test,ButtonTrigger trigger = ButtonTrigger.None)
        {
            if (test != null && test.Key != Keys.None &&
                (state.GetPressedKeys().Contains(test.Key) || last_state.GetPressedKeys().Contains(test.Key)))
            {
                bool combotest = false;
                if (test.Combo != null)
                {
                    foreach (var item in test.Combo)
                    {
                        item.Trigger = ButtonTrigger.Press;
                        if (!base.ButtonTriggered(item))
                        {
                            return false;
                        }
                    }
                    combotest = true;
                }
                var triggertest = test.Trigger | trigger;
                return ((test.Combo == null || combotest) &&
                    ((triggertest & ButtonTrigger.OnPress) != 0 && OnPress(test.Key)) ||
                    ((triggertest & ButtonTrigger.OnRelease) != 0 && OnRelease(test.Key)) ||
                    ((triggertest & ButtonTrigger.Press) != 0 && Press(test.Key)));
            }
            return false;
        }

        protected override bool UpdateOnce()
        {
            State = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            return false;
        }

        private bool OnPress(Keys k) => Press(k) && last_state.IsKeyUp(k);

        private bool OnRelease(Keys k) => !OnPress(k);

        private bool Press(Keys k)
        {
            if (state.IsKeyDown(k))
                return true;
            return false;
        }

        #endregion Methods
    }
}