using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        private static void Init_InGameMenu()
        { }


        private static void UpdateInGameMenu() => throw new NotImplementedException();
        private static void DrawInGameMenu()
        {
            Draw_IGM_Header(new FF8String("Junction"));
            Draw_IGM_SideBar();
            Draw_IGM_ClockBox();
            Draw_IGM_LocationBox();
        }
        private static void Draw_IGM_Header(FF8String help)
        {

        }

        private static void Draw_IGM_SideBar(bool Save = false)
        {

        }
        private static void Draw_IGM_ClockBox()
        {
        }

        private static void Draw_IGM_LocationBox()
        {
        }

        private static void Draw_IGM_ExtraBox()
        {

        }
        private static void Draw_PartyStatus_Box(sbyte pos, Faces.ID character)
        {

        }
        private static void Draw_NonPartyStatus()
        {

        }

    }
}
