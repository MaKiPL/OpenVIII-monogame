using FF8.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        private static Dictionary<Enum, Item> strSideBar;
        private static Dictionary<Enum, Item> strHeaderText;
        private static Rectangle IGM_Size;
        private static Rectangle IGM_Header_Size;
        private static Rectangle IGM_Footer_Size;
        private static Rectangle IGM_Clock_Size;
        private static Matrix IGM_focus;
        private static FF8String IGM_Footer_Text;
        private static FF8String IGM_Header_Text;

        private static void Init_InGameMenu()
        {
            strSideBar = new Dictionary<Enum, Item>()
            {
                { IGMItems.Junction, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,0) } },
                { IGMItems.Item, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,2) } },
                { IGMItems.Magic, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,4) } },
                { IGMItems.Status, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,8) } },
                { IGMItems.GF, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,6) } },
                { IGMItems.Ability, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,62) } },
                { IGMItems.Switch, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,64) } },
                { IGMItems.Card, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,10) } },
                { IGMItems.Config, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,16) } },
                { IGMItems.Tutorial, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,67) } },
                { IGMItems.Save, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,14) } },
            };
            strHeaderText = new Dictionary<Enum, Item>()
            {
                { IGMItems.Junction, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,1) } },
                { IGMItems.Item, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,3) } },
                { IGMItems.Magic, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,5) } },
                { IGMItems.Status, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,9) } },
                { IGMItems.GF, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,7) } },
                { IGMItems.Ability, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,63) } },
                { IGMItems.Switch, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,65) } },
                { IGMItems.Card, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,11) } },
                { IGMItems.Config, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,17) } },
                { IGMItems.Tutorial, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,68) } },
                { IGMItems.Save, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,15) } },
            };
        }


        private static void UpdateInGameMenu()
        {
            IGM_Size = new Rectangle { Width = 843, Height = 630 }.Scale(scale);
            IGM_focus = Matrix.CreateTranslation(-IGM_Size.X - (IGM_Size.Width / 2), -IGM_Size.Y - (IGM_Size.Width / 2), 0)
                * Matrix.CreateTranslation(vpWidth / 2, vpHeight / 2, 0);

            IGM_Header_Size = new Rectangle { Width = 610, Height = 75 }.Scale(scale);
            IGM_Footer_Size = new Rectangle { Width = 610, Height = 75,Y=630-75 }.Scale(scale);
            IGM_Clock_Size = new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 };
            

            IGM_Footer_Text = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID).ReplaceRegion();
            IGM_Header_Text = strHeaderText[IGMItems.Junction];

        }
        private static void DrawInGameMenu()
        {
            Memory.SpriteBatchStartAlpha(tm: IGM_focus);

            Draw_IGM_Header();
            Draw_IGM_SideBar();
            Draw_IGM_ClockBox();
            Draw_IGM_FooterBox();
            for (sbyte i = 0; i < 3; i++)
                if (Memory.State.Party[i] != 0xFF)
                {
                    Draw_PartyStatus_Box(i, (Faces.ID)Memory.State.Party[i]);
                }
            Draw_IGM_ExtraBox();
            Memory.SpriteBatchEnd();
        }
        private static void Draw_IGM_Header()
        {
            DrawBox(IGM_Header_Size, IGM_Header_Text, Icons.ID.HELP, false);
        }

        private static void Draw_IGM_SideBar(bool Save = false)
        {
        }
        private static void Draw_IGM_ClockBox()
        {
            DrawBox(IGM_Clock_Size);
        }

        private static void Draw_IGM_FooterBox()
        {
            DrawBox(IGM_Footer_Size, IGM_Footer_Text, null, false);
        }

        private static void Draw_IGM_ExtraBox()
        {
            for (byte i = 0; i <= (byte)Faces.ID.Eden; i++)
            {
                if(Memory.State.Party.Where(x=>x>=0 && x<= (byte)Faces.ID.Eden).Count()>=1 && !Memory.State.Party.Contains(i))
                Draw_NonPartyStatus((sbyte)i, (Faces.ID)i);
            }
                
        }
        private static void Draw_PartyStatus_Box(sbyte pos, Faces.ID character)
        {

        }
        private static void Draw_NonPartyStatus(sbyte pos, Faces.ID character)
        {

        }

    }
}

namespace FF8.Menu
{
    public enum IGMItems
    {
        Junction,
        Item,
        Magic,
        Status,
        GF,
        Ability,
        Switch,
        Card,
        Config,
        Tutorial,
        Save
    }
}