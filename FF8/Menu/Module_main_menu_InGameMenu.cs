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
                {
                    value = IGM_NonPartyStatus.Count + IGM_PartyStatus.Count - 1;
                    _IGM_choChar = int.MaxValue;
                }
                else if (value >= IGM_NonPartyStatus.Count + IGM_PartyStatus.Count)
                {
                    value = 0;
                    _IGM_choChar = int.MinValue;
                }
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
            //if (IGM_Clock.ITEM != null && IGM_Clock.ITEM.Length > 0)
            //{
                //int i = 0;
                //int pos = 0;
                //DrawBox(IGM_ClockBox);
                IGM_Clock.Draw();
                //IGM_Clock.ITEM[pos, i++].Draw();//0
                ////Memory.Icons.Draw((Icons.ID)IGM_Clock.ITEM[0, i].Data, 13, IGM_Clock.ITEM[0, i++].Pos, TextScale, fade);//0
                //IGM_Clock.ITEM[pos, i++].Draw();//1
                ////Memory.Icons.Draw((int)IGM_Clock.ITEM[0, i].Data, 0, 2, "D1", IGM_Clock.ITEM[0, i++].Pos.Location.ToVector2(), TextScale, fade);//1
                //IGM_Clock.ITEM[pos, i++].Draw();//2
                ////Memory.Icons.Draw((Icons.ID)IGM_Clock.ITEM[0, i].Data, 13, IGM_Clock.ITEM[0, i].Pos, TextScale, fade);//2
                ////Memory.Icons.Draw((Icons.ID)IGM_Clock.ITEM[0, i].Data, 2, IGM_Clock.ITEM[0, i++].Pos, TextScale, fade * blink_Amount * .5f);//2
                //IGM_Clock.ITEM[pos, i++].Draw();//3
                ////Memory.Icons.Draw((int)IGM_Clock.ITEM[0, i].Data, 0, 2, "D2", IGM_Clock.ITEM[0, i++].Pos.Location.ToVector2(), TextScale, fade);//3
                //IGM_Clock.ITEM[pos, i++].Draw();//4
                ////Memory.Icons.Draw((Icons.ID)IGM_Clock.ITEM[0, i].Data, 13, IGM_Clock.ITEM[0, i++].Pos, TextScale, fade);//4
                //IGM_Clock.ITEM[pos, i++].Draw();//5
                ////Memory.Icons.Draw((int)IGM_Clock.ITEM[0, i].Data, 0, 2, "D1", IGM_Clock.ITEM[0, i++].Pos.Location.ToVector2(), TextScale, fade);//5
                //IGM_Clock.ITEM[pos, i++].Draw();//6
                ////Memory.Icons.Draw((int)IGM_Clock.ITEM[0, i].Data, 0, 2, "D1", IGM_Clock.ITEM[0, i++].Pos.Location.ToVector2(), TextScale, fade);//6
                //IGM_Clock.ITEM[pos, i++].Draw();//7
                ////Memory.Icons.Draw((Icons.ID)IGM_Clock.ITEM[0, i].Data, 2, IGM_Clock.ITEM[0, i++].Pos, TextScale, fade);//7
            //}
        }

        private static void Draw_IGM_FooterBox() => DrawBox(IGM_Footer_Size, IGM_Footer_Text, indent: false);

        private static void Draw_IGM_Header() => DrawBox(IGM_Header_Size, IGM_Header_Text, Icons.ID.HELP, false);

        private static void Draw_IGM_NonPartyBox()
        {
            //DrawBox(IGM_NonPartyBox);
            //sbyte pos = 0;
            //for (byte i = 0; i <= (byte)Faces.ID.Edea_Kramer && IGM_NonPartyStatus.SIZE != null && pos < IGM_NonPartyStatus.SIZE.Length; i++)
            //{
            //    if (!Memory.State.Party.Contains((Saves.Characters)i) && Memory.State.Characters[i].Exists != 0 && Memory.State.Characters[i].Exists != 6)//15,9,7,4 shows on menu, 0 locked, 6 hidden
            //    {
            //        IGM_NonPartyStatus.BLANKS[pos] = false;
            //        Draw_NonPartyStatus(pos++);
            //    }
            //}
            //for (sbyte i = pos; i < IGM_NonPartyStatus.Count; i++)
            //{
            //    IGM_NonPartyStatus.BLANKS[i] = true;
            //}
            IGM_NonPartyStatus.Draw();
        }

        private static void Draw_IGM_PartyStatus_Box(sbyte pos)
        {
            
            //if (IGM_PartyStatus.SIZE != null && !blank)
            //{
            //    int i = 0;
            //    //DrawBox(IGM_PartyStatus.ITEM[pos, i].Pos, (FF8String)IGM_PartyStatus.ITEM[pos, i++].Data, Icons.ID.STATUS, indent: false);
            //    IGM_PartyStatus.ITEM[pos, i++].Draw();//0
            //    //Memory.Icons.Draw((Icons.ID)IGM_PartyStatus.ITEM[pos, i].Data, 13, IGM_PartyStatus.ITEM[pos, i++].Pos, TextScale, fade);
            //    IGM_PartyStatus.ITEM[pos, i++].Draw();//1
            //    //Memory.Icons.Draw((int)IGM_PartyStatus.ITEM[pos, i].Data, 0, 2, "D1", IGM_PartyStatus.ITEM[pos, i++].Pos.Location.ToVector2(), TextScale, fade);
            //    IGM_PartyStatus.ITEM[pos, i++].Draw();//2
            //    //Memory.Icons.Draw((Icons.ID)IGM_PartyStatus.ITEM[pos, i].Data, 13, IGM_PartyStatus.ITEM[pos, i++].Pos, TextScale, fade);
            //    IGM_PartyStatus.ITEM[pos, i++].Draw();//3
            //    //Memory.Icons.Draw((int)IGM_PartyStatus.ITEM[pos, i].Data, 0, 2, "D1", IGM_PartyStatus.ITEM[pos, i++].Pos.Location.ToVector2(), TextScale, fade);
            //    IGM_PartyStatus.ITEM[pos, i++].Draw();//4
            //    //Memory.Icons.Draw((Icons.ID)IGM_PartyStatus.ITEM[pos, i].Data, 13, IGM_PartyStatus.ITEM[pos, i++].Pos, TextScale, fade);
            //    IGM_PartyStatus.ITEM[pos, i++].Draw();//5
            //    //Memory.Icons.Draw((int)IGM_PartyStatus.ITEM[pos, i].Data, 0, 2, "D1", IGM_PartyStatus.ITEM[pos, i++].Pos.Location.ToVector2(), TextScale, fade);
            //    IGM_PartyStatus.ITEM[pos, i++].Draw();//6
            //}
            //else
            //    DrawBox(IGM_PartyStatus.SIZE[pos]);
            IGM_PartyStatus.Draw();
        }

        private static void Draw_IGM_PartyStatus_Boxes()
        {
            for (sbyte i = 0; i < IGM_PartyStatus.SIZE.Length; i++)
                Draw_IGM_PartyStatus_Box(i/*, (byte)Memory.State.Party[i] == 0xFF*/);
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

                //Memory.font.RenderBasicText((FF8String)IGM_NonPartyStatus.ITEM[pos, i].Data, IGM_NonPartyStatus.ITEM[pos, i++].Pos.Location, TextScale, Fade: fade);//0
                IGM_NonPartyStatus.ITEM[pos, i++].Draw();//0
                //Memory.Icons.Draw((Icons.ID)IGM_NonPartyStatus.ITEM[pos, i].Data, 13, IGM_NonPartyStatus.ITEM[pos, i++].Pos, TextScale, fade);//1
                IGM_NonPartyStatus.ITEM[pos, i++].Draw();//1
                //Memory.Icons.Draw((int)IGM_NonPartyStatus.ITEM[pos, i].Data, 0, 2, "D1", IGM_NonPartyStatus.ITEM[pos, i++].Pos.Location.ToVector2(), TextScale, fade);//2
                IGM_NonPartyStatus.ITEM[pos, i++].Draw();//2
                //Memory.Icons.Draw((Icons.ID)IGM_NonPartyStatus.ITEM[pos, i].Data, 13, IGM_NonPartyStatus.ITEM[pos, i++].Pos, TextScale, fade);//3
                IGM_NonPartyStatus.ITEM[pos, i++].Draw();//3
                //Memory.spriteBatch.Draw((Texture2D)IGM_NonPartyStatus.ITEM[pos, i].Data, IGM_NonPartyStatus.ITEM[pos, i++].Pos, null, color);//4
                IGM_NonPartyStatus.ITEM[pos, i++].Draw();//4
                //Memory.spriteBatch.Draw((Texture2D)IGM_NonPartyStatus.ITEM[pos, i].Data, IGM_NonPartyStatus.ITEM[pos, i++].Pos, null, color);//5
                IGM_NonPartyStatus.ITEM[pos, i++].Draw();//5
                //Memory.Icons.Draw((int)IGM_NonPartyStatus.ITEM[pos, i].Data, 0, 2, "D1", IGM_NonPartyStatus.ITEM[pos, i++].Pos.Location.ToVector2(), TextScale, fade);//6
                IGM_NonPartyStatus.ITEM[pos, i++].Draw();//6
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

        private static void Init_IGM_NonPartyStatus()
        {
            IGM_NonPartyBox = new Rectangle { Width = 580, Height = 231, X = 20, Y = 318 };
            IGM_NonPartyStatus = new IGMData(6, 7, new IGMDataItem_Box(pos: IGM_NonPartyBox));

            int row = 0, col = 0;
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
        }

        private static void Init_IGM_PartyStatus()
        {
            IGM_PartyStatus = new IGMData(3, 7);
            for (int i = 0; i < 3; i++)
                IGM_PartyStatus.SIZE[i] = new Rectangle { Width = 580, Height = 78, X = 20, Y = 84 + 78 * i };
        }

        private static void Init_IGMClockBox()
        {
            IGM_ClockBox = new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 };
            IGM_Clock = new IGMData(1, 9);
            Rectangle r;
            r = IGM_ClockBox;
            IGM_Clock.ITEM[0, 0] = new IGMDataItem_Box(pos: r);

            r = IGM_ClockBox;
            r.Offset(25, 14);
            IGM_Clock.ITEM[0, 1] = new IGMDataItem_Icon(Icons.ID.PLAY, r,13);

            r = IGM_ClockBox;
            r.Offset(145, 14);
            IGM_Clock.ITEM[0, 3] = new IGMDataItem_Icon(Icons.ID.Colon, r,13, 2, .5f);

            r = IGM_ClockBox;
            r.Offset(25, 48);
            IGM_Clock.ITEM[0, 5] = new IGMDataItem_Icon(Icons.ID.SeeD, r,13);

            r = IGM_ClockBox;
            r.Offset(185, 81);
            IGM_Clock.ITEM[0, 8] = new IGMDataItem_Icon(Icons.ID.G, r,2);

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
            IGM_Clock.ITEM[0, 2] = new IGMDataItem_Int(num, r, 2, 0, 1);

            r = IGM_ClockBox;
            num = num >= 99 ? 99 : Memory.State.timeplayed.Minutes;
            spaces = 0;
            r.Offset(165 + spaces * 20, 14);
            IGM_Clock.ITEM[0, 4] = new IGMDataItem_Int(num, r, 2, 0, 2);

            r = IGM_ClockBox;
            num = Memory.State.Fieldvars.SeedRankPts / 100;
            num = num < 99999 ? num : 99999;
            spaces = 5 - (num).ToString().Length;
            r.Offset(105 + spaces * 20, 48);
            IGM_Clock.ITEM[0, 6] = new IGMDataItem_Int(num, r, 2, 0, 1);

            r = IGM_ClockBox;
            num = Memory.State.AmountofGil < 99999999 ? (int)(Memory.State.AmountofGil) : 99999999;
            spaces = 8 - (num).ToString().Length;
            r.Offset(25 + spaces * 20, 81);
            IGM_Clock.ITEM[0, 7] = new IGMDataItem_Int(num, r, 2, 0, 1);
        }

        private static void Update_IGM_NonPartyStatus()
        {
            sbyte pos = 0;
            for (byte i = 0; Memory.State.Party != null && i <= (byte)Faces.ID.Edea_Kramer && IGM_NonPartyStatus.SIZE != null && pos < IGM_NonPartyStatus.SIZE.Length; i++)
            {
                if (!Memory.State.Party.Contains((Saves.Characters)i) && Memory.State.Characters[i].Exists != 0 && Memory.State.Characters[i].Exists != 6)//15,9,7,4 shows on menu, 0 locked, 6 hidden
                {
                    IGM_NonPartyStatus.BLANKS[pos] = false;
                    Update_IGM_NonPartyStatus(pos++, (Saves.Characters)i);
                }
            }
            for (;pos<IGM_NonPartyStatus.Count;pos++)
            {
                for (int i = 0; i < IGM_NonPartyStatus.Depth; i++)
                {
                    IGM_NonPartyStatus.BLANKS[pos] = true;
                    IGM_NonPartyStatus.ITEM[pos, i] = null;
                }
            }

        }

        private static void Update_IGM_NonPartyStatus(sbyte pos, Saves.Characters character)
        {
            float yoff = 39;
            int num = 0;
            int spaces = 0;
            Rectangle rbak = IGM_NonPartyStatus.SIZE[pos];
            Rectangle r = rbak;
            Color color = new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, .9f);
            IGM_NonPartyStatus.ITEM[pos, 0] = new IGMDataItem_String(Memory.Strings.GetName((Faces.ID)character), rbak);
            IGM_NonPartyStatus.CURSOR[pos] = new Point(rbak.X, (int)(rbak.Y + (6 * TextScale.Y)));

            r.Offset(7, yoff);
            IGM_NonPartyStatus.ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.Lv, r,13);

            r = rbak;
            num = Memory.State.Characters[(int)character].Level;
            spaces = 3 - num.ToString().Length;
            r.Offset((49 + spaces * 20), yoff);
            IGM_NonPartyStatus.ITEM[pos, 2] = new IGMDataItem_Int(num, r, 2, 0, 1);

            r = rbak;
            r.Offset(126, yoff);
            IGM_NonPartyStatus.ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.HP2, r,13);

            r.Offset(0, 28);
            r.Width = 118;
            r.Height = 1;
            IGM_NonPartyStatus.ITEM[pos, 4] = new IGMDataItem_Texture(IGM_Red_Pixel, r) { Color = color };

            r.Offset(0, 2);
            IGM_NonPartyStatus.ITEM[pos, 5] = new IGMDataItem_Texture(IGM_Red_Pixel, r) { Color = color };

            r = rbak;
            num = Memory.State.Characters[(int)character].CurrentHP;
            spaces = 4 - num.ToString().Length;
            r.Offset((166 + spaces * 20), yoff);
            IGM_NonPartyStatus.ITEM[pos, 6] = new IGMDataItem_Int(num, r, 2, 0, 1);
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

                    IGM_PartyStatus.ITEM[pos, 0] = new IGMDataItem_Box(Memory.Strings.GetName((Faces.ID)character), title: Icons.ID.STATUS);
                    Tuple<Rectangle, Point, Rectangle> dims = DrawBox(IGM_PartyStatus.SIZE[pos], ((IGMDataItem_Box)IGM_PartyStatus.ITEM[pos, 0]).Data, indent: false, skipdraw: true);
                    Rectangle r = dims.Item3;
                    IGM_PartyStatus.ITEM[pos, 0].Pos = dims.Item1;
                    IGM_PartyStatus.CURSOR[pos] = dims.Item2;

                    r = dims.Item3;
                    r.Offset(184, yoff);
                    IGM_PartyStatus.ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.Lv, r, 13);

                    r = dims.Item3;
                    num = Memory.State.Characters[(int)character].Level;
                    spaces = 3 - num.ToString().Length;
                    r.Offset((229 + spaces * 20), yoff);
                    IGM_PartyStatus.ITEM[pos, 2] = new IGMDataItem_Int(num, r, 2, 0, 1);

                    r = dims.Item3;
                    r.Offset(304, yoff);
                    IGM_PartyStatus.ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.HP2, r, 13);

                    r = dims.Item3;
                    num = Memory.State.Characters[(int)character].CurrentHP;
                    spaces = 4 - num.ToString().Length;
                    r.Offset((354 + spaces * 20), yoff);
                    IGM_PartyStatus.ITEM[pos, 4] = new IGMDataItem_Int(num, r, 2, 0, 1);

                    r = dims.Item3;
                    r.Offset(437, yoff);
                    IGM_PartyStatus.ITEM[pos, 5] = new IGMDataItem_Icon(Icons.ID.Slash_Forward, r, 13);

                    r = dims.Item3;

                    num = Memory.State.Party[0] == character ||
                        Memory.State.Party[1] == character && Memory.State.Party[0] == Saves.Characters.Blank ||
                        Memory.State.Party[2] == character && Memory.State.Party[0] == Saves.Characters.Blank && Memory.State.Party[1] == Saves.Characters.Blank
                        ? Memory.State.firstcharactersmaxHP : 0;
                    spaces = 4 - num.ToString().Length;
                    r.Offset((459 + spaces * 20), yoff);
                    IGM_PartyStatus.ITEM[pos, 6] = new IGMDataItem_Int(num, r, 2, 0, 1);

                    IGM_NonPartyStatus.BLANKS[pos] = false;
                }
                else
                {
                    IGM_PartyStatus.ITEM[pos, 0] = new IGMDataItem_Box(pos:IGM_PartyStatus.SIZE[pos]);
                    IGM_NonPartyStatus.BLANKS[pos] = true;
                    for (int i = 1; i < IGM_PartyStatus.Depth; i++)
                    {
                        IGM_PartyStatus.ITEM[pos, i] = null;
                    }
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
            //todo detect when there is no saves detected.
            //check for null
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
                    if (IGM_NonPartyStatus.BLANKS[i - IGM_PartyStatus.Count]) continue;
                    Rectangle r = IGM_NonPartyStatus.SIZE[i - IGM_PartyStatus.Count];
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

            public IGMDataItem[,] ITEM;

            /// <summary>
            /// Size of the entire area
            /// </summary>
            public Rectangle[] SIZE;

            //private byte[] PALLET;
            public bool[] BLANKS;
            public IGMDataItem CONTAINER;

            #endregion Fields

            #region Constructors

            public IGMData(int count, int depth,IGMDataItem container=null)
            {
                SIZE = new Rectangle[count];
                //POS = new Rectangle[count, depth];
                //DATA = new object[count, depth];
                ITEM = new IGMDataItem[count, depth];
                CURSOR = new Point[count];
                //PALLET = new byte[count];

                Count = (byte)count;
                Depth = (byte)depth;
                BLANKS = new bool[count];
                CONTAINER = container;
            }
            public void Draw()
            {
                if (CONTAINER != null)
                    CONTAINER.Draw();
                foreach(IGMDataItem i in ITEM)
                {
                    if(i!=null)
                        i.Draw();
                }
            }

            #endregion Constructors

            #region Properties

            public byte Count { get; private set; }
            public byte Depth { get; private set; }

            #endregion Properties
        }

        public abstract class IGMDataItem//<T>
        {
            //protected T _data;
            protected Rectangle _pos;

            public IGMDataItem(Rectangle? pos = null) =>
                //_data = data;
                _pos = pos ?? Rectangle.Empty;

            /// <summary>
            /// Dynamic data that is passed from update to draw.
            /// </summary>
            //public virtual T Data { get => _data; set => _data = value; }
            /// <summary>
            /// Where to draw this item.
            /// </summary>
            public virtual Rectangle Pos
            {
                get => _pos; set => _pos = value;

                //public static implicit operator IGMDataItem<T>(IGMDataItem_Icon v) => throw new NotImplementedException();
            }
            public Color Color { get; internal set; } = Color.White;

            //public virtual object Data { get; internal set; }
            //public virtual FF8String Data { get; internal set; }
            public abstract void Draw();
        }

        public class IGMDataItem_Icon : IGMDataItem//<Icons.ID>
        {
            private byte _pallet;
            private byte _faded_pallet;

            public Icons.ID Data { get; set; }
            public byte Pallet
            {
                get => _pallet; set
                {
                    if (value >= 16) value = 2;
                    _pallet = value;
                }
            }
            public byte Faded_Pallet
            {
                get => _faded_pallet; set
                {
                    if (value >= 16) value = 2;
                    _faded_pallet = value;
                }
            }

            public bool Blink => Faded_Pallet!=Pallet;
            public float Blink_Adjustment { get; set; }

            public IGMDataItem_Icon(Icons.ID data, Rectangle? pos = null, byte? pallet = null, byte? faded_pallet = null, float blink_adjustment = 1f) : base(pos)
            {
                Data = data;
                Pallet = pallet ?? 2;
                Faded_Pallet = faded_pallet ?? Pallet;
                Blink_Adjustment = blink_adjustment;
            }

            public override void Draw()
            {
                Memory.Icons.Draw(Data, Pallet, Pos, TextScale, fade);
                if (Blink)
                    Memory.Icons.Draw(Data, Faded_Pallet, Pos, TextScale, fade * blink_Amount * Blink_Adjustment);
            }
        }

        public class IGMDataItem_Int : IGMDataItem//<Int>
        {
            private byte _pallet;

            public int Data { get; set; }
            public byte Padding { get; set; }
            public byte Pallet
            {
                get => _pallet; set
                {
                    if (value >= 16) value = 2;
                    _pallet = value;
                }
            }
            public Icons.NumType NumType { get; set; }

            public IGMDataItem_Int(int data, Rectangle? pos = null, byte? pallet = null, Icons.NumType? numtype = null, byte? padding = null) : base(pos)
            {
                Data = data;
                Padding = padding ?? 1;
                Pallet = pallet ?? 2;
                NumType = numtype ?? 0;
            }

            public override void Draw() => Memory.Icons.Draw(Data, NumType, Pallet, $"D{Padding}", Pos.Location.ToVector2(), TextScale, fade);
        }

        private class IGMDataItem_String : IGMDataItem
        {
            public FF8String Data { get; set; }

            public IGMDataItem_String(FF8String data, Rectangle? pos = null) : base(pos) => this.Data = data;

            public override void Draw() => Memory.font.RenderBasicText(Data, Pos.Location, TextScale, Fade: fade);
        }
        private class IGMDataItem_Box : IGMDataItem
        {
            public FF8String Data { get; set; }
            public Icons.ID? Title { get; set; }
            public bool Indent { get; set; }

            public IGMDataItem_Box(FF8String data = null, Rectangle? pos = null, Icons.ID? title = null, bool indent = false) : base(pos)
            {
                Data = data;
                Title = title;
                Indent = indent;
            }

            public override void Draw() =>
                    DrawBox(Pos, Data, Title, indent: Indent);
        }

        private class IGMDataItem_Texture : IGMDataItem
        {
            public Texture2D Data { get; set; }

            public IGMDataItem_Texture(Texture2D data, Rectangle? pos = null) : base(pos) => this.Data = data;

            public override void Draw() => Memory.spriteBatch.Draw(Data, Pos, null, base.Color * fade);//4
        }

        #endregion Classes
    }
}