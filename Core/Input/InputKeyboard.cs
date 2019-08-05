using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System.Linq;

namespace OpenVIII
{
    public class InputKeyboard : Input2
    {
        #region Fields

        private static KeyboardState last_state;
        private static KeyboardState state;

        public InputKeyboard(bool skip = true) : base(skip)
        {
        }

        #endregion Fields

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

        private bool Press(Keys k) => state.IsKeyDown(k);
        private bool OnPress(Keys k) => state.IsKeyDown(k) && last_state.IsKeyUp(k);
        private bool OnRelease(Keys k) => state.IsKeyUp(k) && last_state.IsKeyDown(k);
        protected override bool ButtonTriggered(InputButton test)
        {
            if(test != null && test.Key != Keys.None)
            {
                return ((test.Combo == null || base.ButtonTriggered(test.Combo)) &&
                    ((test.Trigger & ButtonTrigger.OnPress) != 0 && OnPress(test.Key)) ||
                    ((test.Trigger & ButtonTrigger.OnRelease) != 0 && OnRelease(test.Key)) ||
                    ((test.Trigger & ButtonTrigger.Press) != 0 && Press(test.Key)));
            }
            return false;
        }



        #endregion Properties

        #region Methods

        protected override bool UpdateOnce()
        {
            State = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            return false;
        }

        #endregion Methods
    }
}