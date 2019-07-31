using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OpenVIII
{
    public class InputMouse : Input2
    {
        #region Fields

        private static MouseState last_state;
        private static MouseState state;

        #endregion Fields

        #region Properties

        public static MouseLockMode Mode { get; set; } = MouseLockMode.Screen;
        protected static MouseState Last_State => last_state;
        protected static MouseState State
        {
            get => state; set
            {
                last_state = state;
                state = value;
            }
        }

        #endregion Properties

        #region Methods

        public void LockMouse()
        {
            if (Memory.IsActive && Mode != MouseLockMode.Disabled) // check for focus to allow for tabbing out with out taking over mouse.
            {
                if (Mode == MouseLockMode.Center) //center mouse in screen after grabbing state, release mouse if alt tabbed out.
                {
                    Mouse.SetPosition(Memory.graphics.GraphicsDevice.Viewport.Bounds.Width / 2, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height / 2);
                }
                else if (Mode == MouseLockMode.Screen) //alt lock that clamps to viewport every frame. would be useful if using mouse to navigate menus and stuff.
                {
                    //there is a better way to clamp as if you move mouse fast enough it will escape for a short time.
                    Mouse.SetPosition(
                        MathHelper.Clamp(State.X, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Width),
                        MathHelper.Clamp(State.Y, 0, Memory.graphics.GraphicsDevice.Viewport.Bounds.Height));
                }
            }
        }

        protected override bool UpdateOnce()
        {
            State = Mouse.GetState();
            LockMouse();
            return false;
        }

        #endregion Methods
    }
}