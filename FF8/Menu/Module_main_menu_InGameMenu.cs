using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static IGMItems IGM_choSideBar;
        private static IGMData IGM_Clock;
        private static Rectangle IGM_ClockBox;
        private static Matrix IGM_focus;
        private static Rectangle IGM_Footer_Size;
        private static FF8String IGM_Footer_Text;
        private static Rectangle IGM_Header_Size;
        private static FF8String IGM_Header_Text;
        private static IGMMode IGM_mode;
        private static Rectangle IGM_NonPartyBox;
        private static IGMData IGM_NonPartyStatus;
        private static IGMData IGM_PartyStatus;
        private static Rectangle IGM_SideBox_Size;
        private static Vector2 IGM_Size;
        private static Texture2D IGM_Red_Pixel;
        private static Dictionary<Enum, Item> strHeaderText;
        private static Dictionary<Enum, Item> strSideBar;
        private static int _IGM_choChar;

        public static int IGM_choChar
        {
            get
            {
                if (_IGM_choChar >= 0 && _IGM_choChar < IGM_PartyStatus.Count)
                {
                    if (IGM_PartyStatus.BLANKS[_IGM_choChar])
                        return IGM_choCharSet(_IGM_choChar + 1);
                }
                else if (_IGM_choChar < IGM_NonPartyStatus.Count + IGM_PartyStatus.Count && _IGM_choChar >= IGM_PartyStatus.Count)
                {
                    if (IGM_NonPartyStatus.BLANKS[_IGM_choChar - IGM_PartyStatus.Count])
                        return IGM_choCharSet(_IGM_choChar + 1);
                }
                return _IGM_choChar;
            }

            set => IGM_choCharSet(value);
        }

        private static int IGM_choCharSet(int value)
        {
            while (true)
            {
                if (value >= 0 && value < IGM_PartyStatus.Count)
                {
                    if (IGM_PartyStatus.BLANKS[value])
                    {
                        if (_IGM_choChar > value) value--;
                        else if (_IGM_choChar < value) value++;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (value < IGM_NonPartyStatus.Count + IGM_PartyStatus.Count && value >= IGM_PartyStatus.Count)
                {
                    if (IGM_NonPartyStatus.BLANKS[value - IGM_PartyStatus.Count])
                    {
                        if (_IGM_choChar > value) value--;
                        else if (_IGM_choChar < value) value++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (value < 0)
                    value = IGM_NonPartyStatus.Count + IGM_PartyStatus.Count - 1;
                else if (value >= IGM_NonPartyStatus.Count + IGM_PartyStatus.Count)
                    value = 0;
            }

            _IGM_choChar = value;
            return value;
        }

        #endregion Fields

        #region Enums

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

        #endregion Enums

        #region Methods

        private static void Draw_IGM_ClockBox()
        {
            if (IGM_Clock.POS != null && IGM_Clock.POS.Length > 0 && IGM_Clock.DATA != null && IGM_Clock.DATA.Length > 0)
            {
                int i = 0;
                DrawBox(IGM_ClockBox);
                Memory.Icons.Draw((Icons.ID)IGM_Clock.DATA[0, i], 13, IGM_Clock.POS[0, i++], TextScale, fade);//0
                Memory.Icons.Draw((int)IGM_Clock.DATA[0, i], 0, 2, "D1", IGM_Clock.POS[0, i++].Location.ToVector2(), TextScale, fade);//1
                Memory.Icons.Draw((Icons.ID)IGM_Clock.DATA[0, i], 13, IGM_Clock.POS[0, i], TextScale, fade);//2
                Memory.Icons.Draw((Icons.ID)IGM_Clock.DATA[0, i], 2, IGM_Clock.POS[0, i++], TextScale, fade * blink * .5f);//2
                Memory.Icons.Draw((int)IGM_Clock.DATA[0, i], 0, 2, "D2", IGM_Clock.POS[0, i++].Location.ToVector2(), TextScale, fade);//3
                Memory.Icons.Draw((Icons.ID)IGM_Clock.DATA[0, i], 13, IGM_Clock.POS[0, i++], TextScale, fade);//4
                Memory.Icons.Draw((int)IGM_Clock.DATA[0, i], 0, 2, "D1", IGM_Clock.POS[0, i++].Location.ToVector2(), TextScale, fade);//5
                Memory.Icons.Draw((int)IGM_Clock.DATA[0, i], 0, 2, "D1", IGM_Clock.POS[0, i++].Location.ToVector2(), TextScale, fade);//6
                Memory.Icons.Draw((Icons.ID)IGM_Clock.DATA[0, i], 2, IGM_Clock.POS[0, i++], TextScale, fade);//7
            }
        }

        private static void Draw_IGM_FooterBox() => DrawBox(IGM_Footer_Size, IGM_Footer_Text, indent: false);

        private static void Draw_IGM_Header() => DrawBox(IGM_Header_Size, IGM_Header_Text, Icons.ID.HELP, false);

        private static void Draw_IGM_NonPartyBox()
        {
            DrawBox(IGM_NonPartyBox);
            sbyte pos = 0;
            for (byte i = 0; i <= (byte)Faces.ID.Edea_Kramer && IGM_NonPartyStatus.SIZE != null && pos < IGM_NonPartyStatus.SIZE.Length; i++)
            {
                if (!Memory.State.Party.Contains((Saves.Characters)i) && Memory.State.Characters[i].Exists != 0 && Memory.State.Characters[i].Exists != 6)//15,9,7,4 shows on menu, 0 locked, 6 hidden
                {
                    IGM_NonPartyStatus.BLANKS[pos] = false;
                    Draw_NonPartyStatus(pos++);
                }
            }
            for (sbyte i = pos; i < IGM_NonPartyStatus.Count; i++)
            {
                IGM_NonPartyStatus.BLANKS[i] = true;
            }
        }

        private static void Draw_IGM_PartyStatus_Box(sbyte pos, bool blank = false)
        {
            IGM_PartyStatus.BLANKS[pos] = blank;
            if (IGM_NonPartyStatus.SIZE != null && !blank)
            {
                int i = 0;
                DrawBox(IGM_PartyStatus.POS[pos, i], (FF8String)IGM_PartyStatus.DATA[pos, i++], Icons.ID.STATUS, indent: false);
                Memory.Icons.Draw((Icons.ID)IGM_PartyStatus.DATA[pos, i], 13, IGM_PartyStatus.POS[pos, i++], TextScale, fade);
                Memory.Icons.Draw((int)IGM_PartyStatus.DATA[pos, i], 0, 2, "D1", IGM_PartyStatus.POS[pos, i++].Location.ToVector2(), TextScale, fade);
                Memory.Icons.Draw((Icons.ID)IGM_PartyStatus.DATA[pos, i], 13, IGM_PartyStatus.POS[pos, i++], TextScale, fade);
                Memory.Icons.Draw((int)IGM_PartyStatus.DATA[pos, i], 0, 2, "D1", IGM_PartyStatus.POS[pos, i++].Location.ToVector2(), TextScale, fade);
                Memory.Icons.Draw((Icons.ID)IGM_PartyStatus.DATA[pos, i], 13, IGM_PartyStatus.POS[pos, i++], TextScale, fade);
                Memory.Icons.Draw((int)IGM_PartyStatus.DATA[pos, i], 0, 2, "D1", IGM_PartyStatus.POS[pos, i++].Location.ToVector2(), TextScale, fade);
            }
            else
                DrawBox(IGM_PartyStatus.SIZE[pos]);
        }

        private static void Draw_IGM_PartyStatus_Boxes()
        {
            for (sbyte i = 0; i < IGM_PartyStatus.SIZE.Length; i++)
                Draw_IGM_PartyStatus_Box(i, (byte)Memory.State.Party[i] == 0xFF);
        }

        private static void Draw_IGM_SideBar(bool Save = false)
        {
            DrawBox(IGM_SideBox_Size);
            for (int i = 0; i < strSideBar.Count; i++)
                Memory.font.RenderBasicText(strSideBar[(IGMItems)i], strSideBar[(IGMItems)i].Loc.Location, TextScale, Fade: fade, lineSpacing: 1);
        }

        private static void Draw_NonPartyStatus(sbyte pos)
        {
            if (IGM_NonPartyStatus.SIZE != null && pos < IGM_NonPartyStatus.SIZE.Length)
            {
                int i = 0;
                Color color = new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, fade * .9f);

                Memory.font.RenderBasicText((FF8String)IGM_NonPartyStatus.DATA[pos, i], IGM_NonPartyStatus.POS[pos, i++].Location, TextScale, Fade: fade);//0
                Memory.Icons.Draw((Icons.ID)IGM_NonPartyStatus.DATA[pos, i], 13, IGM_NonPartyStatus.POS[pos, i++], TextScale, fade);//1
                Memory.Icons.Draw((int)IGM_NonPartyStatus.DATA[pos, i], 0, 2, "D1", IGM_NonPartyStatus.POS[pos, i++].Location.ToVector2(), TextScale, fade);//2
                Memory.Icons.Draw((Icons.ID)IGM_NonPartyStatus.DATA[pos, i], 13, IGM_NonPartyStatus.POS[pos, i++], TextScale, fade);//3
                Memory.spriteBatch.Draw((Texture2D)IGM_NonPartyStatus.DATA[pos, i], IGM_NonPartyStatus.POS[pos, i++], null, color);//4
                Memory.spriteBatch.Draw((Texture2D)IGM_NonPartyStatus.DATA[pos, i], IGM_NonPartyStatus.POS[pos, i++], null, color);//5
                Memory.Icons.Draw((int)IGM_NonPartyStatus.DATA[pos, i], 0, 2, "D1", IGM_NonPartyStatus.POS[pos, i++].Location.ToVector2(), TextScale, fade);//6
            }
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

                    if (IGM_choChar < IGM_PartyStatus.Count && IGM_choChar >= 0)
                        DrawPointer(IGM_PartyStatus.CURSOR[IGM_choChar]);
                    else if (IGM_choChar < IGM_NonPartyStatus.Count + IGM_PartyStatus.Count && IGM_choChar >= IGM_PartyStatus.Count)
                        DrawPointer(IGM_NonPartyStatus.CURSOR[IGM_choChar - IGM_PartyStatus.Count]);

                    break;

                default:
                    DrawPointer(strSideBar[IGM_choSideBar].Point);
                    break;
            }
            Memory.SpriteBatchEnd();
        }

        private static void Init_IGM_NonPartyStatus() => IGM_NonPartyStatus = new IGMData(6, 7);

        private static void Init_IGM_PartyStatus()
        {
            IGM_PartyStatus = new IGMData(3, 7);
            for (int i = 0; i < 3; i++)
                IGM_PartyStatus.SIZE[i] = new Rectangle { Width = 580, Height = 78, X = 20, Y = 84 + 78 * i };
        }

        private static void Init_IGMClockBox()
        {
            IGM_ClockBox = new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 };
            IGM_Clock = new IGMData(1, 8);
            Rectangle r;
            r = IGM_ClockBox;
            r.Offset(25, 14);
            IGM_Clock.DATA[0, 0] = Icons.ID.PLAY;
            IGM_Clock.POS[0, 0] = r;
            r = IGM_ClockBox;
            r.Offset(145, 14);
            IGM_Clock.DATA[0, 2] = Icons.ID.Colon;
            IGM_Clock.POS[0, 2] = r;
            r = IGM_ClockBox;
            r.Offset(25, 48);
            IGM_Clock.DATA[0, 4] = Icons.ID.SeeD;
            IGM_Clock.POS[0, 4] = r;
            r = IGM_ClockBox;
            r.Offset(185, 81);
            IGM_Clock.DATA[0, 7] = Icons.ID.G;
            IGM_Clock.POS[0, 7] = r;
        }

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

            IGM_Red_Pixel = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] color = new Color[] { new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, fade) };
            IGM_Red_Pixel.SetData<Color>(color, 0, IGM_Red_Pixel.Width * IGM_Red_Pixel.Height);

            IGM_Size = new Vector2 { X = 843, Y = 630 };
            IGM_Header_Size = new Rectangle { Width = 610, Height = 75 };
            IGM_Footer_Size = new Rectangle { Width = 610, Height = 75, Y = 630 - 75 };
            IGM_SideBox_Size = new Rectangle { Width = 226, Height = 492, X = 843 - 226 };

            Init_IGMClockBox();
            Init_IGM_NonPartyStatus();
            Init_IGM_PartyStatus();
            UpdateInGameMenu();
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
                l.Point = new Point(l.Loc.X, (int)(l.Loc.Y + 6 * TextScale.Y));
                //strSideBar[(IGMItems)i].Loc.Location
                strSideBar[(IGMItems)i] = l;
            }
        }

        private static void Update_IGM_ClockBox()
        {
            int num;
            int spaces;
            Rectangle r;

            r = IGM_ClockBox;
            num = Memory.State.timeplayed.TotalHours < 99 ? (int)(Memory.State.timeplayed.TotalHours) : 99;
            spaces = 2 - (num).ToString().Length;
            r.Offset(105 + spaces * 20, 14);
            IGM_Clock.DATA[0, 1] = num;
            IGM_Clock.POS[0, 1] = r;

            r = IGM_ClockBox;
            num = num >= 99 ? 99 : Memory.State.timeplayed.Minutes;
            spaces = 0;
            r.Offset(165 + spaces * 20, 14);
            IGM_Clock.DATA[0, 3] = num;
            IGM_Clock.POS[0, 3] = r;

            r = IGM_ClockBox;
            num = Memory.State.Fieldvars.SeedRankPts / 100;
            num = num < 99999 ? num : 99999;
            spaces = 5 - (num).ToString().Length;
            r.Offset(105 + spaces * 20, 48);
            IGM_Clock.DATA[0, 5] = num;
            IGM_Clock.POS[0, 5] = r;

            r = IGM_ClockBox;
            num = Memory.State.AmountofGil < 99999999 ? (int)(Memory.State.AmountofGil) : 99999999;
            spaces = 8 - (num).ToString().Length;
            r.Offset(25 + spaces * 20, 81);
            IGM_Clock.DATA[0, 6] = num;
            IGM_Clock.POS[0, 6] = r;
        }

        private static void Update_IGM_NonPartyStatus()
        {
            int row = 0, col = 0;
            IGM_NonPartyBox = new Rectangle { Width = 580, Height = 231, X = 20, Y = 318 };
            for (int i = 0; i < IGM_NonPartyStatus.SIZE.Length; i++)
            {
                int width = IGM_NonPartyBox.Width / 2;
                int height = IGM_NonPartyBox.Height / 3;
                row = i / 2;
                col = i % 2;
                IGM_NonPartyStatus.SIZE[i] = new Rectangle
                {
                    Width = width,
                    Height = height,
                    X = IGM_NonPartyBox.X + col * width,
                    Y = IGM_NonPartyBox.Y + row * height
                };
                IGM_NonPartyStatus.SIZE[i].Inflate(-26, -8);
                if (i > 1) IGM_NonPartyStatus.SIZE[i].Y -= 8;
            }
            sbyte pos = 0;
            for (byte i = 0; Memory.State.Party != null && i <= (byte)Faces.ID.Edea_Kramer && IGM_NonPartyStatus.SIZE != null && pos < IGM_NonPartyStatus.SIZE.Length; i++)
            {
                if (!Memory.State.Party.Contains((Saves.Characters)i) && Memory.State.Characters[i].Exists != 0 && Memory.State.Characters[i].Exists != 6)//15,9,7,4 shows on menu, 0 locked, 6 hidden
                    Update_IGM_NonPartyStatus(pos++, (Saves.Characters)i);
            }
        }

        private static void Update_IGM_NonPartyStatus(sbyte pos, Saves.Characters character)
        {
            float yoff = 39;
            int num = 0;
            int spaces = 0;
            Rectangle rbak = IGM_NonPartyStatus.SIZE[pos];

            IGM_NonPartyStatus.POS[pos, 0] = rbak;
            IGM_NonPartyStatus.DATA[pos, 0] = Memory.Strings.GetName((Faces.ID)character);
            IGM_NonPartyStatus.CURSOR[pos] = new Point(rbak.X, (int)(rbak.Y + (6 * TextScale.Y)));

            Rectangle r = rbak;
            r.Offset(7, yoff);
            IGM_NonPartyStatus.POS[pos, 1] = r;
            IGM_NonPartyStatus.DATA[pos, 1] = Icons.ID.Lv;

            r = rbak;
            num = Memory.State.Characters[(int)character].Level;
            spaces = 3 - num.ToString().Length;
            r.Offset((49 + spaces * 20), yoff);
            IGM_NonPartyStatus.POS[pos, 2] = r;
            IGM_NonPartyStatus.DATA[pos, 2] = num;

            r = rbak;
            r.Offset(126, yoff);
            IGM_NonPartyStatus.POS[pos, 3] = r;
            IGM_NonPartyStatus.DATA[pos, 3] = Icons.ID.HP2;

            r.Offset(0, 28);
            r.Width = 118;
            r.Height = 1;
            IGM_NonPartyStatus.POS[pos, 4] = r;
            IGM_NonPartyStatus.DATA[pos, 4] = IGM_Red_Pixel;

            r.Offset(0, 2);
            IGM_NonPartyStatus.POS[pos, 5] = r;
            IGM_NonPartyStatus.DATA[pos, 5] = IGM_Red_Pixel;

            r = rbak;
            num = Memory.State.Characters[(int)character].CurrentHP;
            spaces = 4 - num.ToString().Length;
            r.Offset((166 + spaces * 20), yoff);
            IGM_NonPartyStatus.POS[pos, 6] = r;
            IGM_NonPartyStatus.DATA[pos, 6] = num;
        }

        private static void Update_IGM_PartyStatus_Box(sbyte pos, Saves.Characters character)
        {
            if (IGM_NonPartyStatus.SIZE != null)
            {
                if (character != Saves.Characters.Blank)
                {
                    float yoff = 6;
                    int num = 0;
                    int spaces = 0;

                    IGM_PartyStatus.DATA[pos, 0] = Memory.Strings.GetName((Faces.ID)character);
                    Tuple<Rectangle, Point, Rectangle> dims = DrawBox(IGM_PartyStatus.SIZE[pos], (FF8String)IGM_PartyStatus.DATA[pos, 0], indent: false, skipdraw: true);
                    Rectangle r = dims.Item3;
                    IGM_PartyStatus.POS[pos, 0] = dims.Item1;
                    IGM_PartyStatus.CURSOR[pos] = dims.Item2;

                    r = dims.Item3;
                    r.Offset(184, yoff);
                    IGM_PartyStatus.POS[pos, 1] = r;
                    IGM_PartyStatus.DATA[pos, 1] = Icons.ID.Lv;

                    r = dims.Item3;
                    num = Memory.State.Characters[(int)character].Level;
                    spaces = 3 - num.ToString().Length;
                    r.Offset((229 + spaces * 20), yoff);
                    IGM_PartyStatus.POS[pos, 2] = r;
                    IGM_PartyStatus.DATA[pos, 2] = num;

                    r = dims.Item3;
                    r.Offset(304, yoff);
                    IGM_PartyStatus.POS[pos, 3] = r;
                    IGM_PartyStatus.DATA[pos, 3] = Icons.ID.HP2;

                    r = dims.Item3;
                    num = Memory.State.Characters[(int)character].CurrentHP;
                    spaces = 4 - num.ToString().Length;
                    r.Offset((354 + spaces * 20), yoff);
                    IGM_PartyStatus.POS[pos, 4] = r;
                    IGM_PartyStatus.DATA[pos, 4] = num;

                    r = dims.Item3;
                    r.Offset(437, yoff);
                    IGM_PartyStatus.POS[pos, 5] = r;
                    IGM_PartyStatus.DATA[pos, 5] = Icons.ID.Slash_Forward;

                    r = dims.Item3;

                    num = Memory.State.Party[0] == character ||
                        Memory.State.Party[1] == character && Memory.State.Party[0] == Saves.Characters.Blank ||
                        Memory.State.Party[2] == character && Memory.State.Party[0] == Saves.Characters.Blank && Memory.State.Party[1] == Saves.Characters.Blank
                        ? Memory.State.firstcharactersmaxHP : 0;
                    spaces = 4 - num.ToString().Length;
                    r.Offset((459 + spaces * 20), yoff);

                    IGM_PartyStatus.POS[pos, 6] = r;
                    IGM_PartyStatus.DATA[pos, 6] = num;
                }
            }
        }

        private static void Update_IGM_PartyStatus_Boxes()
        {
            for (sbyte i = 0; Memory.State.Party != null && i < IGM_PartyStatus.SIZE.Length; i++)
                Update_IGM_PartyStatus_Box(i, Memory.State.Party[i]);
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
            Update_IGM_ClockBox();
            Update_IGM_NonPartyStatus();
            Update_IGM_PartyStatus_Boxes();
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
                for (int i = 0; i < IGM_PartyStatus.Count; i++)
                {
                    if (IGM_PartyStatus.BLANKS[i]) continue;
                    Rectangle r = IGM_PartyStatus.SIZE[i];
                    //r.Offset(IGM_focus.Translation.X, IGM_focus.Translation.Y);
                    if (r.Contains(ml))
                    {
                        IGM_choChar = i;
                        ret = true;

                        if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
                        {
                            return ret;
                        }
                        break;
                    }
                }
                for (int i = IGM_PartyStatus.Count; i < IGM_NonPartyStatus.Count + IGM_PartyStatus.Count; i++)
                {
                    if (IGM_NonPartyStatus.BLANKS[i- IGM_PartyStatus.Count]) continue;
                    Rectangle r = IGM_NonPartyStatus.SIZE[i- IGM_PartyStatus.Count];
                    //r.Offset(IGM_focus.Translation.X, IGM_focus.Translation.Y);
                    if (r.Contains(ml))
                    {
                        IGM_choChar = i;
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
                    IGM_choChar++;
                    ret = true;
                }
                else if (Input.Button(Buttons.Up))
                {
                    Input.ResetInputLimit();
                    init_debugger_Audio.PlaySound(0);
                    IGM_choChar--;
                    ret = true;
                }
                else if (Input.Button(Buttons.Cancel))
                {
                    Input.ResetInputLimit();
                    ret = true;
                    init_debugger_Audio.PlaySound(8);
                    IGM_mode = IGMMode.ChooseItem;
                }
            }
            return ret;
        }

        #endregion Methods

        #region Classes

        public class IGMData
        {
            #region Fields

            /// <summary>
            /// location of where pointer finger will point.
            /// </summary>
            public Point[] CURSOR;

            /// <summary>
            /// Dynamic data that is passed from update to draw. (type must be cast to)
            /// </summary>
            public object[,] DATA;

            /// <summary>
            /// Where to draw this item.
            /// </summary>
            public Rectangle[,] POS;

            /// <summary>
            /// Size of the entire area
            /// </summary>
            public Rectangle[] SIZE;

            //private byte[] PALLET;
            public bool[] BLANKS;

            #endregion Fields

            #region Constructors

            public IGMData(int count, int depth)
            {
                SIZE = new Rectangle[count];
                POS = new Rectangle[count, depth];
                DATA = new object[count, depth];
                CURSOR = new Point[count];
                //PALLET = new byte[count];

                Count = (byte)count;
                Depth = (byte)depth;
                BLANKS = new bool[count];
            }

            #endregion Constructors

            #region Properties

            public byte Count { get; private set; }
            public byte Depth { get; private set; }

            #endregion Properties
        }

        #endregion Classes
    }
}