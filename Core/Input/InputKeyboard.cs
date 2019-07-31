using Microsoft.Xna.Framework.Input;

namespace OpenVIII
{
    public class InputKeyboard : Input2
    {
        #region Fields

        private static KeyboardState last_state;
        private static KeyboardState state;

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

        #endregion Properties

        #region Methods

        protected override bool UpdateOnce()
        {
            State = Keyboard.GetState();
            return false;
        }

        #endregion Methods
    }
}