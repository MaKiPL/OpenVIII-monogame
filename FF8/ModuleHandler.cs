using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF8
{
    public static class ModuleHandler
    {
        private static int module = Memory.module;
        private static int lastModule = Memory.module;

        public static async void Update(GameTime gameTime)
        {
            if (lastModule != module)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                lastModule = module;
            }
            module = Memory.module;


            //#if DEBUG
            if (Input.Button(Buttons.Back) || Input.Button(Buttons.Cancel))
            {
                Memory.module = Memory.MODULE_MAINMENU_DEBUG;
                Input.OverrideLockMouse = false;
                Input.CurrentMode = Input.MouseLockMode.Screen;
            }
            //#endif


            switch (module)
            {
                //doesn't need memory
                case Memory.MODULE_OVERTURE_DEBUG:
                case Memory.MODULE_MOVIETEST:
                    break;
                default:
                    //requires memory to be loaded.
                    if ((Memory.InitTask != null) && (Memory.InitTask.IsCompleted == false ||
                           Memory.InitTask.Status == TaskStatus.Running ||
                           Memory.InitTask.Status == TaskStatus.WaitingToRun ||
                           Memory.InitTask.Status == TaskStatus.WaitingForActivation))
                    {
                        //task is still running loading assets blank screen and wait.
                        Memory.SuppressDraw = true;
                        await Memory.InitTask;
                        //fade in doesn't happen because time was set before the await.
                        //ending here causes update to be run again with new time
                        return;
                    }
                    break;
            }
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
                case Memory.MODULE_FACE_TEST:
                    Module_face_test.Update();
                    break;
                case Memory.MODULE_ICON_TEST:
                    Module_icon_test.Update();
                    break;
                case Memory.MODULE_CARD_TEST:
                    Module_card_test.Update();
                    break;
            }
        }

        public static void Draw(GameTime gameTime)
        {

            switch (module)
            {
                //doesn't need memory
                case Memory.MODULE_OVERTURE_DEBUG:
                case Memory.MODULE_MOVIETEST:
                    break;
                default:
                    //requires memory to be loaded.
                    if ((Memory.InitTask != null) && (Memory.InitTask.IsCompleted == false ||
                           Memory.InitTask.Status == TaskStatus.Running ||
                           Memory.InitTask.Status == TaskStatus.WaitingToRun ||
                           Memory.InitTask.Status == TaskStatus.WaitingForActivation))
                    {
                        //suppress draw in update but if draw happens before update, blank screen, and end here
                        Memory.graphics.GraphicsDevice.Clear(Color.Black);
                        return;
                    }
                    break;
            }
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
                case Memory.MODULE_FACE_TEST:
                    Module_face_test.Draw();
                    break;
                case Memory.MODULE_ICON_TEST:
                    Module_icon_test.Draw();
                    break;
                case Memory.MODULE_CARD_TEST:
                    Module_card_test.Draw();
                    break;
            }
        }

        public static void ResetBS()
            => Module_battle_debug.ResetState();
    }
}
