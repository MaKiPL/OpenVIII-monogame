using Microsoft.Xna.Framework;
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
        private static Rectangle IGM_SideBox_Size;
        private static Rectangle[] IGM_Party_Size;
        private static Rectangle IGM_NonPartyBox_Size;
        private static Rectangle[] IGM_NonParty_Size;
        private static Matrix IGM_focus;
        private static FF8String IGM_Footer_Text;
        private static FF8String IGM_Header_Text;
        private static IGMItems choSideBar;

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
            IGM_focus = Matrix.CreateTranslation(-IGM_Size.X - (IGM_Size.Width / 2), -IGM_Size.Y - (IGM_Size.Height / 2), 0)
                * Matrix.CreateTranslation(vpWidth / 2, vpHeight / 2, 0);

            IGM_Header_Size = new Rectangle { Width = 610, Height = 75 }.Scale(scale);
            IGM_Footer_Size = new Rectangle { Width = 610, Height = 75, Y = 630 - 75 }.Scale(scale);
            IGM_Clock_Size = new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 }.Scale(scale);
            IGM_SideBox_Size = new Rectangle { Width = 226, Height = 492, X = 843 - 226 }.Scale(scale);

            for (int i = 0; i < strSideBar.Count; i++)
            {
                Item l = strSideBar[(IGMItems)i];
                Rectangle r = new Rectangle
                {
                    Width = IGM_SideBox_Size.Width,
                    Height = IGM_SideBox_Size.Height / strSideBar.Count(),
                    X = IGM_SideBox_Size.X,
                    Y = IGM_SideBox_Size.Y + (IGM_SideBox_Size.Height / strSideBar.Count()) * i,
                }.Scale();
                r.Inflate(-26 * scale.X, -12 * scale.Y);
                l.Loc = r;
                l.Point = new Point(l.Loc.X, l.Loc.Center.Y);
                strSideBar[(IGMItems)i] = l;
            }
            IGM_Party_Size = new Rectangle[3];
            for (int i = 0; i < 3; i++)
                IGM_Party_Size[i] = new Rectangle { Width = 580, Height = 78, X = 20, Y = 84 + 78 * i }.Scale(scale);

            IGM_NonPartyBox_Size = new Rectangle { Width = 580, Height = 231, X = 20, Y = 318 }.Scale(scale);
            IGM_NonParty_Size = new Rectangle[6];
            int row = 0, col = 0;
            for (int i = 0; i < IGM_NonParty_Size.Length; i++)
            {
                int width = IGM_NonPartyBox_Size.Width / 2;
                int height = IGM_NonPartyBox_Size.Height / 3;
                row = i / 2;
                col = i % 2;
                IGM_NonParty_Size[i] = new Rectangle
                {
                    Width = width,
                    Height = height,
                    X = IGM_NonPartyBox_Size.X + col * width,
                    Y = IGM_NonPartyBox_Size.Y + row * height
                }.Scale(scale);
                IGM_NonParty_Size[i].Inflate(-26 * scale.X, -12 * scale.Y);
            }

            IGM_Footer_Text = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID).ReplaceRegion();
            IGM_Header_Text = strHeaderText[choSideBar];
            UpdateInGameMenuInput();
        }

        private static bool UpdateInGameMenuInput()
        {
            bool ret = false;
            Point ml = Input.MouseLocation;

            if (strSideBar != null && strSideBar.Count > 0)
            {
                foreach (KeyValuePair<Enum, Item> item in strSideBar)
                {
                    Rectangle r = item.Value.Loc;
                    r.Offset(IGM_focus.Translation.X, IGM_focus.Translation.Y);
                    if (r.Contains(ml))
                    {
                        choSideBar = (IGMItems)item.Key;
                        ret = true;

                        if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
                        {
                            return ret;
                        }
                        break;
                    }
                }

                if (Input.Button(Buttons.Down))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    if (++choSideBar > Enum.GetValues(typeof(IGMItems)).Cast<IGMItems>().Max())
                        choSideBar = Enum.GetValues(typeof(IGMItems)).Cast<IGMItems>().Min();
                    ret = true;
                }
                else if (Input.Button(Buttons.Up))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    if (--choSideBar < 0)
                        choSideBar = Enum.GetValues(typeof(IGMItems)).Cast<IGMItems>().Max();
                    ret = true;
                }
                else if (Input.Button(Buttons.Cancel))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(8);
                    init_debugger_Audio.StopAudio();
                    Dchoose = 0;
                    Fade = 0.0f;
                    State = MainMenuStates.LoadGameChooseGame;
                    ret = true;
                }
                else if (Input.Button(Buttons.Okay))
                {
                    //PercentLoaded = 0f;
                    ////State = MainMenuStates.LoadGameCheckingSlot;
                    //State = MainMenuStates.LoadGameLoading;
                }
            }
            return ret;
        }

        private static void DrawInGameMenu()
        {
            Memory.SpriteBatchStartAlpha(tm: IGM_focus);

            Draw_IGM_Header();
            Draw_IGM_SideBar();
            Draw_IGM_ClockBox();
            Draw_IGM_FooterBox();
            Draw_IGM_NonPartyBox();
            Draw_IGM_PartyStatus_Boxes();
            DrawPointer(strSideBar[choSideBar].Point);

            Memory.SpriteBatchEnd();
        }

        private static void Draw_IGM_PartyStatus_Boxes()
        {
            for (sbyte i = 0; i < 3; i++)
                Draw_IGM_PartyStatus_Box(i, Memory.State.Party[i]);
        }

        private static void Draw_IGM_Header() => DrawBox(IGM_Header_Size, IGM_Header_Text, Icons.ID.HELP, false);

        private static void Draw_IGM_SideBar(bool Save = false)
        {
            DrawBox(IGM_SideBox_Size);
            for (int i = 0; i < strSideBar.Count; i++)
                Memory.font.RenderBasicText(strSideBar[(IGMItems)i], strSideBar[(IGMItems)i].Loc.Location, TextScale, 1, Fade: fade);
        }

        private static void Draw_IGM_ClockBox() => DrawBox(IGM_Clock_Size);

        private static void Draw_IGM_FooterBox() => DrawBox(IGM_Footer_Size, IGM_Footer_Text, indent: false);

        private static void Draw_IGM_NonPartyBox()
        {
            DrawBox(IGM_NonPartyBox_Size);
            sbyte pos = 0;
            for (byte i = 0; i <= (byte)Faces.ID.Edea_Kramer && IGM_NonParty_Size != null && pos < IGM_NonParty_Size.Length; i++)
            {
                if (!Memory.State.Party.Contains((Faces.ID)i) && Memory.State.Characters[i].Exists != 0 && Memory.State.Characters[i].Exists != 6)//15,9,7,4 shows on menu, 0 locked, 6 hidden
                    Draw_NonPartyStatus(pos++, (Faces.ID)i);
            }
        }

        private static void Draw_IGM_PartyStatus_Box(sbyte pos, Faces.ID character)
        {
            if (IGM_NonParty_Size != null)
            {
                if (character != Faces.ID.Blank)
                    DrawBox(IGM_Party_Size[pos], Memory.Strings.GetName(character), Icons.ID.STATUS, indent: false);
                else
                    DrawBox(IGM_Party_Size[pos]);
            }
        }

        private static void Draw_NonPartyStatus(sbyte pos, Faces.ID character)
        {
            if (IGM_NonParty_Size != null && pos < IGM_NonParty_Size.Length)
                Memory.font.RenderBasicText(Memory.Strings.GetName(character), IGM_NonParty_Size[pos].Location, TextScale, 1, Fade: fade);
        }
    }
}

namespace FF8
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