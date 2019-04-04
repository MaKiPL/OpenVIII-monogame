using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    internal static class ModuleHandler
    {
        private static int module = Memory.module;
        private static int lastModule = Memory.module;

        public static void Update(GameTime gameTime)
        {
            if (lastModule != module)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                lastModule = module;
            }
            module = Memory.module;


//#if DEBUG
            if (Input.Button(Buttons.Back)||Input.Button(Buttons.Cancel))
            {
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;
                Module_main_menu_debug.Fade = 0.0f;
                Input.OverrideLockMouse = false;
                Input.CurrentMode = Input.MouseLockMode.Screen;
            }
//#endif


            switch (module)
            {
                case Memory.MODULE_BATTLE:
                    module_battle.Update();
                    break;
                case Memory.MODULE_BATTLE_DEBUG:
                    Module_battle_debug.Update();
                    break;
                case Memory.MODULE_MOVIETEST:
                    Module_movie_test.Update();
                    break;
                case Memory.MODULE_FIELD_DEBUG:
                    Module_field_debug.Update();
                    break;
                case Memory.MODULE_OVERTURE_DEBUG:
                    Module_overture_debug.Update();
                    break;
                case Memory.MODULE_MAINMENU_DEBUG:
                    Module_main_menu_debug.Update();
                    break;
                case Memory.MODULE_WORLD_DEBUG:
                    Module_world_debug.Update();
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
                    Module_battle_debug.Draw();
                    break;
                case Memory.MODULE_MOVIETEST:
                    Module_movie_test.Draw();
                    break;
                case Memory.MODULE_FIELD_DEBUG:
                    Module_field_debug.Draw();
                    break;
                case Memory.MODULE_OVERTURE_DEBUG:
                    Module_overture_debug.Draw();
                    break;
                case Memory.MODULE_MAINMENU_DEBUG:
                    Module_main_menu_debug.Draw();
                    break;
                case Memory.MODULE_WORLD_DEBUG:
                    Module_world_debug.Draw();
                    break;
            }
        }

        internal static void ResetBS()
            => Module_battle_debug.ResetState();
    }
}
