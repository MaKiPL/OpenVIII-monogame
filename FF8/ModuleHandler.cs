using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    class ModuleHandler
    {
        private static int module = Memory.module;
        static int msDelay = 0;
        static int msDelayLimit = 100;
        static bool bLimitInput = false;

        public static void Update(GameTime gameTime)
        {
            module = Memory.module;
            if (bLimitInput)
                bLimitInput = (msDelay += gameTime.ElapsedGameTime.Milliseconds) < msDelayLimit;
            else
            {
#if DEBUG
                if (Keyboard.GetState().IsKeyDown(Keys.F2))
                {
                    Memory.module = Memory.MODULE_BATTLE_DEBUG;
                    bLimitInput = true;
                    msDelay = 0;
                    //if (Memory.bat_sceneID == 006)
                    //    Memory.bat_sceneID = 000;
                    //else Memory.bat_sceneID = 006;
                    Memory.bat_sceneID++;
                    module_battle_debug.ResetState();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.F3))
                {
                    bLimitInput = true;
                    msDelay = 0;
                    Memory.musicIndex++;
                    if (Memory.musicIndex >= Memory.musices.Length)
                        Memory.musicIndex = 0;
                    init_debugger_Audio.PlayMusic();
                }
                if(Keyboard.GetState().IsKeyDown(Keys.F4))
                {
                    Memory.module = Memory.MODULE_FIELD_DEBUG;
                    bLimitInput = true;
                    msDelay = 0;
                    Memory.FieldHolder.FieldID++;
                    module_field_debug.ResetField();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.F5))
                {
                    Memory.module = Memory.MODULE_FIELD_DEBUG;
                    bLimitInput = true;
                    msDelay = 0;
                    Memory.FieldHolder.FieldID--;
                    module_field_debug.ResetField();
                }
            }
#endif
            switch(module)
            {
                case Memory.MODULE_BATTLE:
                    module_battle.Update();
                    break;
                case Memory.MODULE_BATTLE_DEBUG:
                    module_battle_debug.Update();
                    break;
                case Memory.MODULE_MOVIETEST:
                    module_movie_test.Update();
                    break;
                case Memory.MODULE_FIELD_DEBUG:
                    module_field_debug.Update();
                    break;
            }
        }

        public static void Draw(GameTime gameTime)
        {
            switch (module)
            {
                case Memory.MODULE_BATTLE:
                    module_battle.Draw();
                    break;
                case Memory.MODULE_BATTLE_DEBUG:
                    module_battle_debug.Draw();
                    break;
                case Memory.MODULE_MOVIETEST:
                    module_movie_test.Draw();
                    break;
                case Memory.MODULE_FIELD_DEBUG:
                    module_field_debug.Draw();
                    break;
            }
        }

        internal static void ResetBS()
            => module_battle_debug.ResetState();
    }
}
