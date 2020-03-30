using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Threading.Tasks;

namespace OpenVIII
{
    public static class ModuleHandler
    {
        private static Module module = Memory.Module;
        private static Module lastModule = Memory.Module;

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
                if (Memory.Module != Module.MainMenuDebug && Memory.Module != Module.BattleDebug)
                {
                    Memory.Module = Module.MainMenuDebug;
                    InputMouse.Mode = MouseLockMode.Screen;
                }
            }
#endif

            switch (module)
            {
                //doesn't need memory
                case Module.OvertureDebug:
                case Module.MovieTest:
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
                case Module.Battle:
                    ModuleBattle.Update();
                    break;

                case Module.BattleDebug:
                    Menu.UpdateOnce();
                    ModuleBattleDebug.Update();
                    break;

                case Module.MovieTest:
                    ModuleMovieTest.Update();
                    break;

                case Module.FieldDebug:
                    Fields.Module.Update();
                    break;

                case Module.OvertureDebug:
                    Module_overture_debug.Update();
                    break;

                case Module.MainMenuDebug:
                    Menu.UpdateOnce();
                    Menu.Module.Update();
                    break;

                case Module.WorldDebug:
                    Module_world_debug.Update(gameTime);
                    break;

                case Module.FaceTest:
                    Module_face_test.Update();
                    break;

                case Module.IconTest:
                    Module_icon_test.Update();
                    break;

                case Module.CardTest:
                    Module_card_test.Update();
                    break;

                case Module.FieldModelTest:
                    Fields.ModuleFieldObjectTest.Update();
                    break;
            }
        }

        public static void Draw(GameTime gameTime)
        {
            switch (module)
            {
                //doesn't need memory
                case Module.OvertureDebug:
                case Module.MovieTest:
                    break;

                default:
                    //requires memory to be loaded.
                    if ((Memory.InitTask != null) && (Memory.InitTask.IsCompleted == false ||
                           Memory.InitTask.Status == TaskStatus.Running ||
                           Memory.InitTask.Status == TaskStatus.WaitingToRun ||
                           Memory.InitTask.Status == TaskStatus.WaitingForActivation))
                    {
                        //suppress draw in update but if draw happens before update, blank screen, and end here
                        Memory.Graphics.GraphicsDevice.Clear(Color.Black);
                        return;
                    }
                    break;
            }
            switch (module)
            {
                case Module.Battle:
                    ModuleBattle.Draw();
                    break;

                case Module.BattleDebug:
                    ModuleBattleDebug.Draw();
                    break;

                case Module.MovieTest:
                    ModuleMovieTest.Draw();
                    break;

                case Module.FieldDebug:
                    Fields.Module.Draw();
                    break;

                case Module.OvertureDebug:
                    Module_overture_debug.Draw();
                    break;

                case Module.MainMenuDebug:
                    Menu.Module.Draw();
                    break;

                case Module.WorldDebug:
                    Module_world_debug.Draw();
                    break;

                case Module.FaceTest:
                    Module_face_test.Draw();
                    break;

                case Module.IconTest:
                    Module_icon_test.Draw();
                    break;

                case Module.CardTest:
                    Module_card_test.Draw();
                    break;

                case Module.BattleSwirl:
                    BattleSwirl.Draw();
                    break;

                case Module.FieldModelTest:
                    Fields.ModuleFieldObjectTest.Draw();
                    break;
            }
        }

        public static void ResetBS()
            => ModuleBattleDebug.ResetState();
    }
}