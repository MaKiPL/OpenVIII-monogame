using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        private static void DrawLG_Loading() => DrawLGSG_ing(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.Loading].Text);
        private static void DrawSG_Saving() => DrawLGSG_ing(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.Saving].Text);
        private static void DrawLGSG_ing(FF8String topright, FF8String help)
        {
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, help);
            DrawLGSGLoadBar();
        }
        private static void UpdateLG_Loading()
        {
            if (PercentLoaded < 1.0f)
            {
                PercentLoaded += Memory.gameTime.ElapsedGameTime.Milliseconds / 1000.0f * 3;
            }
            else if(Menu.IGM != null)
            {
                State = MainMenuStates.IGM; // start loaded game.
                Memory.State = Saves.FileList[SlotLoc, BlockLoc + blockpage * 3].Clone();

                Menu.IGM.ReInit();
                //till we have a game to load i'm going to display ingame menu.

                init_debugger_Audio.PlaySound(36);
            }
        }
        private static void UpdateSG_Saving() => throw new NotImplementedException();
    }
}