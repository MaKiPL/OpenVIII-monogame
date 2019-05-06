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
            for (sbyte i = 0; i < 3; i++)
                Draw_PartyStatus_Box(i, (Faces.ID)Memory.State.Party[0]);
            Draw_IGM_ExtraBox();
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
            FF8String loc = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID).ReplaceRegion();
            Rectangle dst = new Rectangle { };
            DrawBox(loc, null, dst, false);
        }

        private static void Draw_IGM_ExtraBox()
        {
            for (sbyte i = 0; i < 3; i++)
                Draw_NonPartyStatus(i, (Faces.ID)Memory.State.Party[0]);
        }
        private static void Draw_PartyStatus_Box(sbyte pos, Faces.ID character)
        {

        }
        private static void Draw_NonPartyStatus(sbyte pos, Faces.ID character)
        {

        }

    }
}
