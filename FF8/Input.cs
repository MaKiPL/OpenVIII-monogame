using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class Input
    {
        private static bool bLimitInput = false;
        static int msDelay = 0;
        static int msDelayLimit = 100;

        public static bool GetInputDelayed(Keys key)
        {
            if (bLimitInput)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.Milliseconds) < msDelayLimit;
            if (Keyboard.GetState().IsKeyDown(key) && !bLimitInput)
            {
                bLimitInput = true;
                msDelay = 0;
                return true;
            }
            return false;
        }
    }
}
