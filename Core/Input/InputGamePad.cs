using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OpenVIII
{
    public class InputGamePad : Input2
    {
        #region Fields

        private static GamePadState last_state;
        private static GamePadState state;

        #endregion Fields

        #region Properties

        protected static GamePadState Last_State => last_state;
        protected static GamePadState State
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
            State = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            return false;
        }

        #endregion Methods
    }
}