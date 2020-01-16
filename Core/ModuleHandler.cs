using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Threading.Tasks;

namespace OpenVIII
{
    public static class ModuleHandler
    {
        private static MODULE module = Memory.Module;
        private static MODULE lastModule = Memory.Module;

        public static async void Update(GameTime gameTime)
        {
            if (lastModule != module)
            {
                //got stuck on this once had to force close.
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                lastModule = module;
            }
            module = Memory.Module;

#if DEBUG
            if (Input2.DelayedButton(FF8TextTagKey.Reset) || Input2.DelayedButton(FF8TextTagKey.Cancel))
            {
                if (Memory.Module != MODULE.MAINMENU_DEBUG && Memory.Module != MODULE.BATTLE_DEBUG)
                {
                    Memory.Module = MODULE.MAINMENU_DEBUG;
                    InputMouse.Mode = MouseLockMode.Screen;
                }
            }
#endif

            switch (module)
            {
                //doesn't need memory
                case MODULE.OVERTURE_DEBUG:
                case MODULE.MOVIETEST:
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
                case MODULE.BATTLE:
                    module_battle.Update();
                    break;

                case MODULE.BATTLE_DEBUG:
                    Menu.UpdateOnce();
                    Module_battle_debug.Update();
                    break;

                case MODULE.MOVIETEST:
                    Module_movie_test.Update();
                    break;

                case MODULE.FIELD_DEBUG:
                    Fields.Module.Update();
                    break;

                case MODULE.OVERTURE_DEBUG:
                    Module_overture_debug.Update();
                    break;

                case MODULE.MAINMENU_DEBUG:
                    Menu.UpdateOnce();
                    Menu.Module.Update();
                    break;

                case MODULE.WORLD_DEBUG:
                    Module_world_debug.Update(gameTime);
                    break;

                case MODULE.FACE_TEST:
                    Module_face_test.Update();
                    break;

                case MODULE.ICON_TEST:
                    Module_icon_test.Update();
                    break;

                case MODULE.CARD_TEST:
                    Module_card_test.Update();
                    break;

                case MODULE.FIELD_MODEL_TEST:
                    Fields.Module_field_object_test.Update();
                    break;
            }
        }

        public static void Draw(GameTime gameTime)
        {
            switch (module)
            {
                //doesn't need memory
                case MODULE.OVERTURE_DEBUG:
                case MODULE.MOVIETEST:
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
                case MODULE.BATTLE:
                    module_battle.Draw();
                    break;

                case MODULE.BATTLE_DEBUG:
                    Module_battle_debug.Draw();
                    break;

                case MODULE.MOVIETEST:
                    Module_movie_test.Draw();
                    break;

                case MODULE.FIELD_DEBUG:
                    Fields.Module.Draw();
                    break;

                case MODULE.OVERTURE_DEBUG:
                    Module_overture_debug.Draw();
                    break;

                case MODULE.MAINMENU_DEBUG:
                    Menu.Module.Draw();
                    break;

                case MODULE.WORLD_DEBUG:
                    Module_world_debug.Draw();
                    break;

                case MODULE.FACE_TEST:
                    Module_face_test.Draw();
                    break;

                case MODULE.ICON_TEST:
                    Module_icon_test.Draw();
                    break;

                case MODULE.CARD_TEST:
                    Module_card_test.Draw();
                    break;

                case MODULE.BATTLE_SWIRL:
                    BattleSwirl.Draw();
                    break;

                case MODULE.FIELD_MODEL_TEST:
                    Fields.Module_field_object_test.Draw();
                    break;
            }
        }

        public static void ResetBS()
            => Module_battle_debug.ResetState();
    }
}