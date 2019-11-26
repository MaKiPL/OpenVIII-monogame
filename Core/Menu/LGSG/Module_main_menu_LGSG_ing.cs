using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        #region Fields

        private static Slide<float> LoadBarSlide = new Slide<float>(0f, 1f, 1000d, MathHelper.SmoothStep);

        #endregion Fields

        #region Methods

        private static void DrawLG_Loading() => DrawLGSG_ing(strLoadScreen[Litems.Load].Text, strLoadScreen[Litems.Loading].Text);

        private static void DrawLGSG_ing(FF8String topright, FF8String help)
        {
            DrawLGSGHeader(strLoadScreen[Litems.GameFolderSlot1 + SlotLoc].Text, topright, help);
            DrawLGSGLoadBar();
        }

        private static void DrawSG_Saving() => DrawLGSG_ing(strLoadScreen[Litems.Save].Text, strLoadScreen[Litems.Saving].Text);

        private static void UpdateLG_Loading()
        {
            if (PercentLoaded == 0) LoadBarSlide.Restart();
            if (!LoadBarSlide.Done)
            {
                PercentLoaded = LoadBarSlide.Update();
            }
            else if (Menu.IGM != null)
            {
                State = MainMenuStates.IGM; // start loaded game.
                Memory.State = Saves.FileList[SlotLoc, BlockLoc + blockpage * 3].Clone();

                Menu.IGM.Refresh();
                //till we have a game to load i'm going to display ingame menu.

                init_debugger_Audio.PlaySound(36);
            }
        }

        private static void UpdateSG_Saving() => throw new NotImplementedException();

        #endregion Methods
    }
}