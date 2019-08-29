using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        private static void DrawLGCheckSlot() => DrawLGSGCheckSlot(strLoadScreen[Litems.Load].Text);
        private static void DrawSGCheckSlot() => DrawLGSGCheckSlot(strLoadScreen[Litems.Save].Text);
        private static void DrawLGSGCheckSlot(FF8String topright)
        {
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, strLoadScreen[Litems.CheckGameFolder].Text);
            DrawLGSGLoadBar();
        }
        private static void UpdateLGCheckSlot()
        {
            if (PercentLoaded == 0) LoadBarSlide.Restart();
            if (!LoadBarSlide.Done)
            {
                PercentLoaded = LoadBarSlide.Update();
            }
            else
            {
                State = MainMenuStates.LoadGameChooseGame;
                Memory.SuppressDraw = true;
                init_debugger_Audio.PlaySound(35);
            }
        }
        private static void UpdateSGCheckSlot() => throw new NotImplementedException();
    }
} 