using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        private static Dictionary<Enum, Item> strSideBar;
        private static Dictionary<Enum, Item> strHeaderText;
        private static Vector2 IGM_Size;
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
        private static IGMItems IGM_choSideBar;
        private static Texture2D SimpleTexture;
        private static IGMMode IGM_mode;

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

            SimpleTexture = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] color = new Color[] { new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, fade) };
            SimpleTexture.SetData<Color>(color, 0, SimpleTexture.Width * SimpleTexture.Height);

            IGM_Size = new Vector2 { X = 843, Y = 630 };
            IGM_Header_Size = new Rectangle { Width = 610, Height = 75 };
            IGM_Footer_Size = new Rectangle { Width = 610, Height = 75, Y = 630 - 75 };
            IGM_SideBox_Size = new Rectangle { Width = 226, Height = 492, X = 843 - 226 };

            for (int i = 0; i < strSideBar.Count; i++)
            {
                Item l = strSideBar[(IGMItems)i];
                Rectangle r = new Rectangle
                {
                    Width = IGM_SideBox_Size.Width,
                    Height = IGM_SideBox_Size.Height / strSideBar.Count(),
                    X = IGM_SideBox_Size.X,
                    Y = IGM_SideBox_Size.Y + (IGM_SideBox_Size.Height / strSideBar.Count()) * i,
                };
                r.Inflate(-26, -12);
                l.Loc = r;
                l.Point = new Point(l.Loc.X, l.Loc.Center.Y);
                strSideBar[(IGMItems)i] = l;
            }
            IGM_Party_Size = new Rectangle[3];
            for (int i = 0; i < 3; i++)
                IGM_Party_Size[i] = new Rectangle { Width = 580, Height = 78, X = 20, Y = 84 + 78 * i };

            IGM_NonPartyBox_Size = new Rectangle { Width = 580, Height = 231, X = 20, Y = 318 };

            Init_IGMClockBox();
            Init_NonPartyStatus();
            UpdateInGameMenu();
        }

        private static void UpdateInGameMenu()
        {
            Vector2 Zoom = Memory.Scale(IGM_Size.X, IGM_Size.Y, Memory.ScaleMode.FitBoth);

            IGM_focus = Matrix.CreateTranslation((IGM_Size.X / -2), (IGM_Size.Y / -2), 0) *
                Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                Matrix.CreateTranslation(vp.X / 2, vp.Y / 2, 0);

            TextScale = new Vector2(2.545455f, 3.0375f);

            IGM_Footer_Text = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID).ReplaceRegion();
            IGM_Header_Text = strHeaderText[IGM_choSideBar];
            Update_IGMClockBox();
            Update_NonPartyStatus();
            UpdateInGameMenuInput();
        }

        private static bool UpdateInGameMenuInput()
        {
            bool ret = false;
            ml = Input.MouseLocation.Transform(IGM_focus);

            if (IGM_mode == IGMMode.ChooseItem)
            {
                if (strSideBar != null && strSideBar.Count > 0)
                {
                    foreach (KeyValuePair<Enum, Item> item in strSideBar)
                    {
                        Rectangle r = item.Value.Loc;
                        //r.Offset(IGM_focus.Translation.X, IGM_focus.Translation.Y);
                        if (r.Contains(ml))
                        {
                            IGM_choSideBar = (IGMItems)item.Key;
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
                        if (++IGM_choSideBar > Enum.GetValues(typeof(IGMItems)).Cast<IGMItems>().Max())
                            IGM_choSideBar = Enum.GetValues(typeof(IGMItems)).Cast<IGMItems>().Min();
                        ret = true;
                    }
                    else if (Input.Button(Buttons.Up))
                    {
                        Input.ResetInputLimit();
                        init_debugger_Audio.PlaySound(0);
                        if (--IGM_choSideBar < 0)
                            IGM_choSideBar = Enum.GetValues(typeof(IGMItems)).Cast<IGMItems>().Max();
                        ret = true;
                    }
                    else if (Input.Button(Buttons.Cancel))
                    {
                        Input.ResetInputLimit();
                        init_debugger_Audio.PlaySound(8);
                        Fade = 0.0f;
                        State = MainMenuStates.LoadGameChooseGame;
                        ret = true;
                    }
                    else if (Input.Button(Buttons.Okay))
                    {
                        Input.ResetInputLimit();
                        init_debugger_Audio.PlaySound(0);
                        ret = true;
                        switch (IGM_choSideBar)
                        {
                            //Select Char Mode
                            case IGMItems.Junction:
                            case IGMItems.Magic:
                            case IGMItems.Status:
                                IGM_mode = IGMMode.ChooseChar;
                                break;
                        }
                    }
                }
            }
            else if (IGM_mode == IGMMode.ChooseChar)
            {
                if (Input.Button(Buttons.Cancel))
                {
                    Input.ResetInputLimit();
                    ret = true;
                    init_debugger_Audio.PlaySound(8);
                    IGM_mode = IGMMode.ChooseItem;
                }
            }
            return ret;
        }

        private enum IGMMode
        {
            ChooseItem,
            ChooseChar,
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

        private static void DrawInGameMenu()
        {
            Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp, tm: IGM_focus);

            switch (IGM_mode)
            {
                case IGMMode.ChooseChar:
                case IGMMode.ChooseItem:
                default:
                    Draw_IGM_Header();
                    Draw_IGM_SideBar();
                    Draw_IGM_ClockBox();
                    Draw_IGM_FooterBox();
                    Draw_IGM_NonPartyBox();
                    Draw_IGM_PartyStatus_Boxes();
                    break;
            }
            switch (IGM_mode)
            {
                case IGMMode.ChooseChar:
                    DrawPointer(strSideBar[IGM_choSideBar].Point, blink: true);
                    break;

                default:
                    DrawPointer(strSideBar[IGM_choSideBar].Point);
                    break;
            }
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
                Memory.font.RenderBasicText(strSideBar[(IGMItems)i], strSideBar[(IGMItems)i].Loc.Location, TextScale, Fade: fade, lineSpacing: 1);
        }

        private static Rectangle[] IGM_Clock_DIMs;
        private static object[] IGM_Clock_Vals;

        private static void Init_IGMClockBox()
        {
            IGM_Clock_Size = new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 };
            IGM_Clock_DIMs = new Rectangle[8];
            IGM_Clock_Vals = new object[8];
            Rectangle r;
            r = IGM_Clock_Size;
            r.Offset(25, 14);
            IGM_Clock_Vals[0] = Icons.ID.PLAY;
            IGM_Clock_DIMs[0] = r;
            r = IGM_Clock_Size;
            r.Offset(145, 14);
            IGM_Clock_Vals[2] = Icons.ID.Colon;
            IGM_Clock_DIMs[2] = r;
            r = IGM_Clock_Size;
            r.Offset(25, 48);
            IGM_Clock_Vals[4] = Icons.ID.SeeD;
            IGM_Clock_DIMs[4] = r;
            r = IGM_Clock_Size;
            r.Offset(185, 81);
            IGM_Clock_Vals[7] = Icons.ID.G;
            IGM_Clock_DIMs[7] = r;
        }

        private static void Update_IGMClockBox()
        {
            int num;
            int spaces;
            Rectangle r;

            r = IGM_Clock_Size;
            num = Memory.State.timeplayed.TotalHours < 99 ? (int)(Memory.State.timeplayed.TotalHours) : 99;
            spaces = 2 - (num).ToString().Length;
            r.Offset(105 + spaces * 20, 14);
            IGM_Clock_Vals[1] = num;
            IGM_Clock_DIMs[1] = r;

            r = IGM_Clock_Size;
            num = num >= 99 ? 99 : Memory.State.timeplayed.Minutes;
            spaces = 0;
            r.Offset(165 + spaces * 20, 14);
            IGM_Clock_Vals[3] = num;
            IGM_Clock_DIMs[3] = r;

            r = IGM_Clock_Size;
            num = Memory.State.Fieldvars.SeedRankPts / 100;
            num = num < 99999 ? num : 99999;
            spaces = 5 - (num).ToString().Length;
            r.Offset(105 + spaces * 20, 48);
            IGM_Clock_Vals[5] = num;
            IGM_Clock_DIMs[5] = r;

            r = IGM_Clock_Size;
            num = Memory.State.AmountofGil < 99999999 ? (int)(Memory.State.AmountofGil) : 99999999;
            spaces = 8 - (num).ToString().Length;
            r.Offset(25 + spaces * 20, 81);
            IGM_Clock_Vals[6] = num;
            IGM_Clock_DIMs[6] = r;
        }

        private static void Draw_IGM_ClockBox()
        {
            if (IGM_Clock_DIMs != null && IGM_Clock_DIMs.Length > 0 && IGM_Clock_Vals != null && IGM_Clock_Vals.Length > 0)
            {
                int i = 0;
                DrawBox(IGM_Clock_Size);
                Memory.Icons.Draw((Icons.ID)IGM_Clock_Vals[i], 13, IGM_Clock_DIMs[i++], TextScale, fade);//0
                Memory.Icons.Draw((int)IGM_Clock_Vals[i], 0, 2, "D1", IGM_Clock_DIMs[i++].Location.ToVector2(), TextScale, fade);//1
                Memory.Icons.Draw((Icons.ID)IGM_Clock_Vals[i], 13, IGM_Clock_DIMs[i], TextScale, fade);//2
                Memory.Icons.Draw((Icons.ID)IGM_Clock_Vals[i], 2, IGM_Clock_DIMs[i++], TextScale, fade * blink * .5f);//2
                Memory.Icons.Draw((int)IGM_Clock_Vals[i], 0, 2, "D2", IGM_Clock_DIMs[i++].Location.ToVector2(), TextScale, fade);//3
                Memory.Icons.Draw((Icons.ID)IGM_Clock_Vals[i], 13, IGM_Clock_DIMs[i++], TextScale, fade);//4
                Memory.Icons.Draw((int)IGM_Clock_Vals[i], 0, 2, "D1", IGM_Clock_DIMs[i++].Location.ToVector2(), TextScale, fade);//5
                Memory.Icons.Draw((int)IGM_Clock_Vals[i], 0, 2, "D1", IGM_Clock_DIMs[i++].Location.ToVector2(), TextScale, fade);//6
                Memory.Icons.Draw((Icons.ID)IGM_Clock_Vals[i], 2, IGM_Clock_DIMs[i++], TextScale, fade);//7
            }
        }

        private static void Draw_IGM_FooterBox() => DrawBox(IGM_Footer_Size, IGM_Footer_Text, indent: false);

        private static void Draw_IGM_NonPartyBox()
        {
            DrawBox(IGM_NonPartyBox_Size);
            sbyte pos = 0;
            for (byte i = 0; i <= (byte)Faces.ID.Edea_Kramer && IGM_NonParty_Size != null && pos < IGM_NonParty_Size.Length; i++)
            {
                if (!Memory.State.Party.Contains((Saves.Characters)i) && Memory.State.Characters[i].Exists != 0 && Memory.State.Characters[i].Exists != 6)//15,9,7,4 shows on menu, 0 locked, 6 hidden
                    Draw_NonPartyStatus(pos++);
            }
        }

        private static void Draw_IGM_PartyStatus_Box(sbyte pos, Saves.Characters character)
        {
            if (IGM_NonParty_Size != null)
            {
                if (character != Saves.Characters.Blank)
                {
                    Tuple<Rectangle, Point, Rectangle> dims = DrawBox(IGM_Party_Size[pos], Memory.Strings.GetName((Faces.ID)character), Icons.ID.STATUS, indent: false);
                    Rectangle r = dims.Item3;
                    float yoff = 6;
                    r = dims.Item3;
                    r.Offset(184, yoff);
                    Memory.Icons.Draw(Icons.ID.Lv, 13, r, TextScale, fade);
                    r = dims.Item3;
                    int lvl = Memory.State.Characters[(int)character].Level;
                    int spaces = 3 - lvl.ToString().Length;
                    r.Offset((229 + spaces * 20), yoff);
                    Memory.Icons.Draw(lvl, 0, 2, "D1", r.Location.ToVector2(), TextScale, fade);
                    r = dims.Item3;
                    r.Offset(304, yoff);
                    Memory.Icons.Draw(Icons.ID.HP2, 13, r, TextScale, fade);
                    r = dims.Item3;
                    lvl = Memory.State.Characters[(int)character].CurrentHP;
                    spaces = 4 - lvl.ToString().Length;
                    r.Offset((354 + spaces * 20), yoff);
                    Memory.Icons.Draw(lvl, 0, 2, "D1", r.Location.ToVector2(), TextScale, fade);
                    r = dims.Item3;
                    r.Offset(437, yoff);
                    Memory.Icons.Draw(Icons.ID.Slash_Forward, 13, r, TextScale, fade);
                    r = dims.Item3;

                    lvl = Memory.State.Party[0] == character ||
                        Memory.State.Party[1] == character && Memory.State.Party[0] == Saves.Characters.Blank ||
                        Memory.State.Party[2] == character && Memory.State.Party[0] == Saves.Characters.Blank && Memory.State.Party[1] == Saves.Characters.Blank
                        ? Memory.State.firstcharactersmaxHP : 0;
                    spaces = 4 - lvl.ToString().Length;
                    r.Offset((459 + spaces * 20), yoff);
                    Memory.Icons.Draw(lvl, 0, 2, "D1", r.Location.ToVector2(), TextScale, fade);
                }
                else
                    DrawBox(IGM_Party_Size[pos]);
            }
        }

        private static void Update_IGM_PartyStatus_Box(sbyte pos, Saves.Characters character)
        {
            if (IGM_NonParty_Size != null)
            {
                if (character != Saves.Characters.Blank)
                {
                    Tuple<Rectangle, Point, Rectangle> dims = DrawBox(IGM_Party_Size[pos], Memory.Strings.GetName((Faces.ID)character), Icons.ID.STATUS, indent: false);
                    Rectangle r = dims.Item3;
                    float yoff = 6;
                    r = dims.Item3;
                    r.Offset(184, yoff);
                    Memory.Icons.Draw(Icons.ID.Lv, 13, r, TextScale, fade);
                    r = dims.Item3;
                    int lvl = Memory.State.Characters[(int)character].Level;
                    int spaces = 3 - lvl.ToString().Length;
                    r.Offset((229 + spaces * 20), yoff);
                    Memory.Icons.Draw(lvl, 0, 2, "D1", r.Location.ToVector2(), TextScale, fade);
                    r = dims.Item3;
                    r.Offset(304, yoff);
                    Memory.Icons.Draw(Icons.ID.HP2, 13, r, TextScale, fade);
                    r = dims.Item3;
                    lvl = Memory.State.Characters[(int)character].CurrentHP;
                    spaces = 4 - lvl.ToString().Length;
                    r.Offset((354 + spaces * 20), yoff);
                    Memory.Icons.Draw(lvl, 0, 2, "D1", r.Location.ToVector2(), TextScale, fade);
                    r = dims.Item3;
                    r.Offset(437, yoff);
                    Memory.Icons.Draw(Icons.ID.Slash_Forward, 13, r, TextScale, fade);
                    r = dims.Item3;

                    lvl = Memory.State.Party[0] == character ||
                        Memory.State.Party[1] == character && Memory.State.Party[0] == Saves.Characters.Blank ||
                        Memory.State.Party[2] == character && Memory.State.Party[0] == Saves.Characters.Blank && Memory.State.Party[1] == Saves.Characters.Blank
                        ? Memory.State.firstcharactersmaxHP : 0;
                    spaces = 4 - lvl.ToString().Length;
                    r.Offset((459 + spaces * 20), yoff);
                    Memory.Icons.Draw(lvl, 0, 2, "D1", r.Location.ToVector2(), TextScale, fade);
                }
                else
                    DrawBox(IGM_Party_Size[pos]);
            }
        }

        private static Rectangle[,] Update_NonPartyStatus_POS;
        private static Point[] Update_NonPartyStatus_CURSOR;
        private static object[,] Update_NonPartyStatus_DATA;

        private static void Init_NonPartyStatus()
        {
            IGM_NonParty_Size = new Rectangle[6];
            Update_NonPartyStatus_POS = new Rectangle[IGM_NonParty_Size.Length, 7];
            Update_NonPartyStatus_DATA = new object[IGM_NonParty_Size.Length, 7];
            Update_NonPartyStatus_CURSOR = new Point[IGM_NonParty_Size.Length];
        }

        private static void Update_NonPartyStatus()
        {
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
                };
                IGM_NonParty_Size[i].Inflate(-26, -8);
                if (i > 1) IGM_NonParty_Size[i].Y -= 8;
            }
            sbyte pos = 0;
            for (byte i = 0; Memory.State.Party != null && i <= (byte)Faces.ID.Edea_Kramer && IGM_NonParty_Size != null && pos < IGM_NonParty_Size.Length; i++)
            {
                if (!Memory.State.Party.Contains((Saves.Characters)i) && Memory.State.Characters[i].Exists != 0 && Memory.State.Characters[i].Exists != 6)//15,9,7,4 shows on menu, 0 locked, 6 hidden
                    Update_NonPartyStatus(pos++, (Saves.Characters)i);
            }
        }

        private static void Update_NonPartyStatus(sbyte pos, Saves.Characters character)
        {
            float yoff = 39;
            int num = 0;
            int spaces = 0;
            Rectangle rbak = IGM_NonParty_Size[pos];

            Update_NonPartyStatus_POS[pos, 0] = rbak;
            Update_NonPartyStatus_DATA[pos, 0] = Memory.Strings.GetName((Faces.ID)character);
            Update_NonPartyStatus_CURSOR[pos] = new Point(0, (int)(rbak.Y + (3 * TextScale.Y)));

            Rectangle r = rbak;
            r.Offset(7, yoff);
            Update_NonPartyStatus_POS[pos, 1] = r;
            Update_NonPartyStatus_DATA[pos, 1] = Icons.ID.Lv;

            r = rbak;
            num = Memory.State.Characters[(int)character].Level;
            spaces = 3 - num.ToString().Length;
            r.Offset((49 + spaces * 20), yoff);
            Update_NonPartyStatus_POS[pos, 2] = r;
            Update_NonPartyStatus_DATA[pos, 2] = num;

            r = rbak;
            r.Offset(126, yoff);
            Update_NonPartyStatus_POS[pos, 3] = r;
            Update_NonPartyStatus_DATA[pos, 3] = Icons.ID.HP2;

            r.Offset(0, 28);
            r.Width = 118;
            r.Height = 1;
            Update_NonPartyStatus_POS[pos, 4] = r;
            Update_NonPartyStatus_DATA[pos, 4] = SimpleTexture;

            r.Offset(0, 2);
            Update_NonPartyStatus_POS[pos, 5] = r;
            Update_NonPartyStatus_DATA[pos, 5] = SimpleTexture;

            r = rbak;
            num = Memory.State.Characters[(int)character].CurrentHP;
            spaces = 4 - num.ToString().Length;
            r.Offset((166 + spaces * 20), yoff);
            Update_NonPartyStatus_POS[pos, 6] = r;
            Update_NonPartyStatus_DATA[pos, 6] = num;
        }

        private static void Draw_NonPartyStatus(sbyte pos)
        {
            if (IGM_NonParty_Size != null && pos < IGM_NonParty_Size.Length)
            {
                int i = 0;
                Color color = new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, fade * .9f);

                Memory.font.RenderBasicText((FF8String)Update_NonPartyStatus_DATA[pos, i], Update_NonPartyStatus_POS[pos, i++].Location, TextScale, Fade: fade);//0
                Memory.Icons.Draw((Icons.ID)Update_NonPartyStatus_DATA[pos, i], 13, Update_NonPartyStatus_POS[pos, i++], TextScale, fade);//1
                Memory.Icons.Draw((int)Update_NonPartyStatus_DATA[pos, i], 0, 2, "D1", Update_NonPartyStatus_POS[pos, i++].Location.ToVector2(), TextScale, fade);//2
                Memory.Icons.Draw((Icons.ID)Update_NonPartyStatus_DATA[pos, i], 13, Update_NonPartyStatus_POS[pos, i++], TextScale, fade);//3
                Memory.spriteBatch.Draw((Texture2D)Update_NonPartyStatus_DATA[pos, i], Update_NonPartyStatus_POS[pos, i++], null, color);//4
                Memory.spriteBatch.Draw((Texture2D)Update_NonPartyStatus_DATA[pos, i], Update_NonPartyStatus_POS[pos, i++], null, color);//5
                Memory.Icons.Draw((int)Update_NonPartyStatus_DATA[pos, i], 0, 2, "D1", Update_NonPartyStatus_POS[pos, i++].Location.ToVector2(), TextScale, fade);//6
            }
        }

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
}