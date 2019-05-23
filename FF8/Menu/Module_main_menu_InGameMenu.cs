using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private class IGM : Menu
        {
            #region Fields

            private Items choSideBar;
            private int _choChar;
            protected new Mode mode=0;
            public int choChar
            {
                get
                {
                    if (_choChar >= 0 && _choChar < Data[SectionName.Party].Count)
                    {
                        if (Data[SectionName.Party].BLANKS[_choChar])
                            return choCharSet(_choChar + 1);
                    }
                    else if (_choChar < Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count && _choChar >= Data[SectionName.Non_Party].Count)
                    {
                        if (Data[SectionName.Non_Party].BLANKS[_choChar - Data[SectionName.Party].Count])
                            return choCharSet(_choChar + 1);
                    }
                    return _choChar;
                }

                set => choCharSet(value);
            }

            private int choCharSet(int value)
            {
                while (true)
                {
                    if (value >= 0 && value < Data[SectionName.Party].Count)
                    {
                        if (Data[SectionName.Party].BLANKS[value])
                        {
                            if (_choChar > value) value--;
                            else if (_choChar < value) value++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (value < Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count && value >= Data[SectionName.Party].Count)
                    {
                        if (Data[SectionName.Non_Party].BLANKS[value - Data[SectionName.Party].Count])
                        {
                            if (_choChar > value) value--;
                            else if (_choChar < value) value++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (value < 0)
                    {
                        value = Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count - 1;
                        _choChar = int.MaxValue;
                    }
                    else if (value >= Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count)
                    {
                        value = 0;
                        _choChar = int.MinValue;
                    }
                }

                _choChar = value;
                return value;
            }

            #endregion Fields

            #region Enums

            public enum Items
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
                Save,
                CurrentEXP,
                NextLEVEL
            }

            protected new enum Mode
            {
                ChooseItem,
                ChooseChar,
            }

            #endregion Enums

            #region Methods

            public override void Draw()
            {
                Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp, tm: Focus);
                switch (mode)
                {
                    case Mode.ChooseChar:
                    case Mode.ChooseItem:
                    default:
                        base.Draw();
                        break;
                }
                switch (mode)
                {
                    case Mode.ChooseChar:
                        DrawPointer(Data[SectionName.SideMenu].CURSOR[(int)choSideBar], blink: true);

                        if (choChar < Data[SectionName.Party].Count && choChar >= 0)
                            DrawPointer(Data[SectionName.Party].CURSOR[choChar]);
                        else if (choChar < Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count && choChar >= Data[SectionName.Party].Count)
                            DrawPointer(Data[SectionName.Non_Party].CURSOR[choChar - Data[SectionName.Party].Count]);
                        break;

                    default:
                        DrawPointer(Data[SectionName.SideMenu].CURSOR[(int)choSideBar]);
                        break;
                }
                Memory.SpriteBatchEnd();
            }

            public enum SectionName
            {
                Header,
                Footer,
                SideMenu,
                Clock,
                Party,
                Non_Party
            }

            protected override void Init()
            {
                Size = new Vector2 { X = 843, Y = 630 };
                TextScale = new Vector2(2.545455f, 3.0375f);
                IGMData Header = new IGMData_Header();
                IGMData Footer = new IGMData_Footer();
                IGMData Clock = new IGMData_Clock();
                IGMData SideMenu = new IGMData_SideMenu();
                IGMData Non_Party = new IGMData_NonParty();
                IGMData Party = new IGMData_Party();
                Data.Add(SectionName.Header, Header);
                Data.Add(SectionName.Footer, Footer);
                Data.Add(SectionName.Clock, Clock);
                Data.Add(SectionName.SideMenu, SideMenu);
                Data.Add(SectionName.Party, Party);
                Data.Add(SectionName.Non_Party, Non_Party);
                base.Init();
            }

            public override bool Update()
            {
                base.Update();
                ((IGMData_Header)Data[SectionName.Header]).Update(choSideBar);

                return Inputs();
            }

            protected override bool Inputs()
            {
                bool ret = false;
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                {
                    i.Value.Inputs();
                }
                ml = Input.MouseLocation.Transform(Focus);

                if (mode == Mode.ChooseItem)
                {
                    if (Data[SectionName.SideMenu] != null && Data[SectionName.SideMenu].Count > 0)
                    {
                        for (int pos = 0; pos < Data[SectionName.SideMenu].Count; pos++)
                        {
                            Rectangle r = Data[SectionName.SideMenu].ITEM[pos, 0];
                            if (r.Contains(ml))
                            {
                                choSideBar = (Items)pos;
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
                            if ((int)++choSideBar >= ((IGMData_SideMenu)Data[SectionName.SideMenu]).Count)
                                choSideBar = 0;
                            ret = true;
                        }
                        else if (Input.Button(Buttons.Up))
                        {
                            Input.ResetInputLimit();
                            init_debugger_Audio.PlaySound(0);
                            if (--choSideBar < 0)
                                choSideBar = (Items)((IGMData_SideMenu)Data[SectionName.SideMenu]).Count-1;
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
                            switch (choSideBar)
                            {
                                //Select Char Mode
                                case Items.Junction:
                                case Items.Magic:
                                case Items.Status:
                                    mode = Mode.ChooseChar;
                                    break;
                            }
                        }
                    }
                }
                else if (mode == Mode.ChooseChar)
                {
                    for (int i = 0; i < Data[SectionName.Party].Count; i++)
                    {
                        if (Data[SectionName.Party].BLANKS[i]) continue;
                        Rectangle r = Data[SectionName.Party].SIZE[i];
                        if (r.Contains(ml))
                        {
                            choChar = i;
                            ret = true;

                            if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
                            {
                                return ret;
                            }
                            break;
                        }
                    }
                    for (int i = Data[SectionName.Party].Count; i < Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count; i++)
                    {
                        if (Data[SectionName.Non_Party].BLANKS[i - Data[SectionName.Party].Count]) continue;
                        Rectangle r = Data[SectionName.Non_Party].SIZE[i - Data[SectionName.Party].Count];
                        //r.Offset(focus.Translation.X, focus.Translation.Y);
                        if (r.Contains(ml))
                        {
                            choChar = i;
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
                        choChar++;
                        ret = true;
                    }
                    else if (Input.Button(Buttons.Up))
                    {
                        Input.ResetInputLimit();
                        init_debugger_Audio.PlaySound(0);
                        choChar--;
                        ret = true;
                    }
                    else if (Input.Button(Buttons.Cancel))
                    {
                        Input.ResetInputLimit();
                        ret = true;
                        init_debugger_Audio.PlaySound(8);
                        mode = Mode.ChooseItem;
                    }
                    else if (Input.Button(Buttons.Okay))
                    {
                        Input.ResetInputLimit();
                        init_debugger_Audio.PlaySound(0);
                        ret = true;
                        switch (choSideBar)
                        {
                            //Select Char Mode
                            case Items.Junction:
                                //case Items.Magic:
                                //case Items.Status:
                                State = MainMenuStates.IGM_Junction;
                                if (choChar < 3)
                                    InGameMenu_Junction.ReInit(Memory.State.PartyData[choChar], Memory.State.Party[choChar]);
                                else
                                {
                                    int pos = 0;
                                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                                    {
                                        for (byte i = 0; Memory.State.Party != null && i < Memory.State.Characters.Length; i++)
                                        {
                                            if (!Memory.State.PartyData.Contains((Saves.Characters)i) && Memory.State.Characters[i].VisibleInMenu)
                                            {
                                                if (pos++ + 3 == choChar)
                                                {
                                                    InGameMenu_Junction.ReInit((Saves.Characters)i, (Saves.Characters)i);
                                                    break;
                                                }
                                            }
                                        }

                                    }

                                }
                                break;
                        }
                    }
                }
                return ret;
            }

            protected class IGMData_Header : IGMData
            {
                private Dictionary<Enum, Item> strHeaderText;

                public IGMData_Header() : this(0, 0, new IGMDataItem_Box(pos: new Rectangle { Width = 610, Height = 75 }, title: Icons.ID.HELP))
                { }

                public IGMData_Header(int count, int depth, IGMDataItem container = null) : base(count, depth, container)
                {
                }

                protected override void Init()
                {
                    strHeaderText = new Dictionary<Enum, Item>()
                    {
                    { Items.Junction, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,1) } },
                    { Items.Item, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,3) } },
                    { Items.Magic, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,5) } },
                    { Items.Status, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,9) } },
                    { Items.GF, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,7) } },
                    { Items.Ability, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,63) } },
                    { Items.Switch, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,65) } },
                    { Items.Card, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,11) } },
                    { Items.Config, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,17) } },
                    { Items.Tutorial, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,68) } },
                    { Items.Save, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,15) } },
                    };
                    base.Init();
                }

                public bool Update(IGM.Items selection)
                {
                    ((IGMDataItem_Box)CONTAINER).Data = strHeaderText[selection];
                    return true;
                }

                private new bool Update() => base.Update();
            }

            private class IGMData_Footer : IGMData
            {
                public IGMData_Footer() : this(0, 0, new IGMDataItem_Box(pos: new Rectangle { Width = 610, Height = 75, Y = 630 - 75 }))
                {
                }

                public IGMData_Footer(int count, int depth, IGMDataItem container = null) : base(count, depth, container)
                {
                }

                public override bool Update()
                {
                    base.Update();
                    ((IGMDataItem_Box)CONTAINER).Data = Memory.Strings.Read(Strings.FileID.AREAMES, 0, Memory.State.LocationID).ReplaceRegion();
                    return true;
                }
            }

            private class IGMData_Clock : IGMData
            {
                public IGMData_Clock() : this(1, 8, new IGMDataItem_Box(pos: new Rectangle { Width = 226, Height = 114, Y = 630 - 114, X = 843 - 226 }))
                {
                }

                public IGMData_Clock(int count, int depth, IGMDataItem container = null) : base(count, depth, container)
                {
                }

                protected override void Init()
                {
                    Rectangle r;
                    r = CONTAINER;
                    r.Offset(25, 14);
                    ITEM[0, 0] = new IGMDataItem_Icon(Icons.ID.PLAY, r, 13);

                    r = CONTAINER;
                    r.Offset(145, 14);
                    ITEM[0, 2] = new IGMDataItem_Icon(Icons.ID.Colon, r, 13, 2, .5f);
                    
                    r = CONTAINER;
                    r.Offset(185, 81);
                    ITEM[0, 7] = new IGMDataItem_Icon(Icons.ID.G, r, 2);
                    base.Init();
                }
                public override void ReInit()
                {
                    base.ReInit();
                    Rectangle r;

                    r = CONTAINER;
                    r.Offset(105, 14);
                    ITEM[0, 1] = new IGMDataItem_Int(Memory.State.Timeplayed.TotalHours < 99 ? (int)(Memory.State.Timeplayed.TotalHours) : 99, r, 2, 0, 1, 2);

                    r = CONTAINER;
                    r.Offset(165, 14);
                    ITEM[0, 3] = new IGMDataItem_Int(Memory.State.Timeplayed.TotalHours < 99 ? Memory.State.Timeplayed.Minutes : 99, r, 2, 0, 2, 2);
                    if (!Memory.State.TeamLaguna)
                    {
                        r = CONTAINER;
                        r.Offset(25, 48);
                        ITEM[0, 4] = new IGMDataItem_Icon(Icons.ID.SeeD, r, 13);

                        r = CONTAINER;
                        r.Offset(105, 48);
                        ITEM[0, 5] = new IGMDataItem_Int(Memory.State.Fieldvars.SeedRankPts / 100 < 99999 ? Memory.State.Fieldvars.SeedRankPts / 100 : 99999, r, 2, 0, 1,5);
                    }
                    else
                    {
                        ITEM[0, 4] = null;
                        ITEM[0, 5] = null;
                    }

                    r = CONTAINER;
                    r.Offset(25, 81);
                    ITEM[0, 6] = new IGMDataItem_Int(Memory.State.AmountofGil < 99999999 ? (int)(Memory.State.AmountofGil) : 99999999, r, 2, 0, 1,8);

                }
                public override bool Update()
                {
                    bool ret = base.Update();



                    return ret;
                }
            }

            private class IGMData_NonParty : IGMData
            {
                private Texture2D _red_pixel;

                public IGMData_NonParty() : this(6, 9, new IGMDataItem_Box(pos: new Rectangle { Width = 580, Height = 231, X = 20, Y = 318 }))
                {
                }

                public IGMData_NonParty(int count, int depth, IGMDataItem container = null) : base(count, depth, container)
                {
                }

                public override void Draw()
                {
                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                        base.Draw();
                }

                protected override void Init()
                {                    
                    _red_pixel = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    Color[] color = new Color[] { new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, 100) };
                    _red_pixel.SetData<Color>(color, 0, _red_pixel.Width * _red_pixel.Height);
                    int row = 0, col = 0;
                    for (int i = 0; i < SIZE.Length; i++)
                    {
                        int width = Width / 2;
                        int height = Height / 3;
                        row = i / 2;
                        col = i % 2;
                        SIZE[i] = new Rectangle
                        {
                            Width = width,
                            Height = height,
                            X = X + col * width,
                            Y = Y + row * height
                        };
                        SIZE[i].Inflate(-26, -8);
                        if (i > 1) SIZE[i].Y -= 4;
                        if (i > 3) SIZE[i].Y -= 4;
                    }
                    base.Init();
                }

                public override bool Update()
                {
                    sbyte pos = 0;
                    bool ret = base.Update();
                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                    {
                        for (byte i = 0; Memory.State.Party != null && i < Memory.State.Characters.Length && SIZE != null && pos < SIZE.Length; i++)
                        {
                            if (!Memory.State.Party.Contains((Saves.Characters)i) && Memory.State.Characters[i].VisibleInMenu)
                            {
                                BLANKS[pos] = false;
                                Update(pos++, (Saves.Characters)i);
                            }
                        }
                    }
                    for (; pos < Count; pos++)
                    {
                        for (int i = 0; i < Depth; i++)
                        {
                            BLANKS[pos] = true;
                            ITEM[pos, i] = null;
                        }
                    }
                    return true;
                }

                private void Update(sbyte pos, Saves.Characters character)
                {
                    float yoff = 39;
                    Rectangle rbak = SIZE[pos];
                    Rectangle r = rbak;
                    Color color = new Color(74.5f / 100, 12.5f / 100, 11.8f / 100, .9f);
                    ITEM[pos, 0] = new IGMDataItem_String(Memory.Strings.GetName(character), rbak);
                    CURSOR[pos] = new Point(rbak.X, (int)(rbak.Y + (6 * TextScale.Y)));

                    r.Offset(7, yoff);
                    ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.Lv, r, 13);

                    r = rbak;
                    r.Offset((49), yoff);
                    ITEM[pos, 2] = new IGMDataItem_Int(Memory.State.Characters[(int)character].Level, r, 2, 0, 1,3);

                    r = rbak;
                    r.Offset(126, yoff);
                    ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.HP2, r, 13);

                    r.Offset(0, 28);
                    r.Width = 118;
                    r.Height = 1;
                    ITEM[pos, 4] = new IGMDataItem_Texture(_red_pixel, r) { Color = Color.Black };
                    r.Width = (int)(r.Width * Memory.State.Characters[(int)character].PercentFullHP());
                    ITEM[pos, 5] = new IGMDataItem_Texture(_red_pixel, r) { Color = color };

                    r.Width = 118;
                    r.Offset(0, 2);
                    ITEM[pos, 6] = new IGMDataItem_Texture(_red_pixel, r) { Color = Color.Black };
                    r.Width = (int)(r.Width * Memory.State.Characters[(int)character].PercentFullHP());
                    ITEM[pos, 7] = new IGMDataItem_Texture(_red_pixel, r) { Color = color };
                    //TODO red bar resizes based on current/max hp

                    r = rbak;
                    r.Offset((166), yoff);
                    ITEM[pos, 8] = new IGMDataItem_Int(Memory.State.Characters[(int)character].CurrentHP, r, 2, 0, 1,4);
                }
            }

            private class IGMData_Party : IGMData
            {
                private int vSpace;
                private Dictionary<Enum, FF8String> strings;

                public IGMData_Party() : base(3, 12)
                {
                }

                public IGMData_Party(int count, int depth, IGMDataItem container = null) : base(count, depth, container)
                {
                }

                public override void ReInit()
                {
                    base.ReInit();
                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                    {
                        CONTAINER = new IGMDataItem_Empty(pos: new Rectangle { Width = 580, Height = 234, X = 20, Y = 84 });
                        vSpace = 0;
                    }
                    else
                    {
                        CONTAINER = new IGMDataItem_Empty(pos: new Rectangle { Width = 580, Height = 462, X = 20, Y = 84 });
                        vSpace = 6;
                    }
                    for (int i = 0; i < 3; i++)
                        SIZE[i] = new Rectangle { Width = Width, Height = (Height / 3) - vSpace, X = X, Y = Y + (Height / 3 * i) };
                }

                public override bool Update()
                {
                    bool ret = base.Update();
                    for (sbyte i = 0; Memory.State.PartyData != null && i < SIZE.Length; i++)
                        Update(i, Memory.State.PartyData[i], Memory.State.Party[i]);
                    return true;
                }

                protected override void Init()
                {
                    strings = new Dictionary<Enum, FF8String>()
                    {
                        { Items.CurrentEXP, Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,23)  },
                        { Items.NextLEVEL, Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,24)  },
                    };
                    base.Init();
                }

                private void Update(sbyte pos, Saves.Characters character, Saves.Characters visableCharacter)
                {
                    if (SIZE != null)
                    {
                        if (character != Saves.Characters.Blank)
                        {
                            float yoff = 6;

                            ITEM[pos, 0] = new IGMDataItem_Box(Memory.Strings.GetName(visableCharacter), title: Icons.ID.STATUS);
                            Tuple<Rectangle, Point, Rectangle> dims = DrawBox(SIZE[pos], ((IGMDataItem_Box)ITEM[pos, 0]).Data, indent: false, skipdraw: true);
                            Rectangle r = dims.Item3;
                            ITEM[pos, 0].Pos = dims.Item1;
                            CURSOR[pos] = dims.Item2;

                            r = dims.Item3;
                            r.Offset(184, yoff);
                            ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.Lv, r, 13);

                            r = dims.Item3;
                            r.Offset((229), yoff);
                            ITEM[pos, 2] = new IGMDataItem_Int(Memory.State.Characters[(int)character].Level, r, 2, 0, 1,3);

                            r = dims.Item3;
                            r.Offset(304, yoff);
                            ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.HP2, r, 13);

                            r = dims.Item3;
                            r.Offset((354), yoff);
                            ITEM[pos, 4] = new IGMDataItem_Int(Memory.State.Characters[(int)character].CurrentHP, r, 2, 0, 1,4);

                            r = dims.Item3;
                            r.Offset(437, yoff);
                            ITEM[pos, 5] = new IGMDataItem_Icon(Icons.ID.Slash_Forward, r, 13);

                            r = dims.Item3;

                            r.Offset((459), yoff);
                            ITEM[pos, 6] = new IGMDataItem_Int(Memory.State.Characters[(int)character].MaxHP(visableCharacter), r, 2, 0,1,4);

                            if (Memory.State.TeamLaguna || Memory.State.SmallTeam)
                            {
                                BLANKS[pos] = false;
                                r = dims.Item3;
                                r.Offset(145, 36);
                                ITEM[pos, 7] = new IGMDataItem_String(strings[Items.CurrentEXP] + new FF8String("\n") + strings[Items.NextLEVEL], r);

                                r = dims.Item3;
                                r.Offset((340), 42);
                                ITEM[pos, 8] = new IGMDataItem_Int((int)Memory.State.Characters[(int)character].Experience, r, 2, 0, 1,9);

                                r = dims.Item3;
                                r.Offset(520, 42);
                                ITEM[pos, 9] = new IGMDataItem_Icon(Icons.ID.P, r, 2);

                                r = dims.Item3;
                                r.Offset((340), 75);
                                ITEM[pos, 10] = new IGMDataItem_Int(Memory.State.Characters[(int)character].ExperienceToNextLevel, r, 2, 0, 1,9);

                                r = dims.Item3;
                                r.Offset(520, 75);
                                ITEM[pos, 11] = new IGMDataItem_Icon(Icons.ID.P, r, 2);
                            }
                            else
                                for (int i = 7; i < Depth; i++)
                                    ITEM[pos, i] = null;
                        }
                        else
                        {
                            ITEM[pos, 0] = new IGMDataItem_Box(pos: SIZE[pos]);
                            BLANKS[pos] = true;
                            for (int i = 1; i < Depth; i++)
                            {
                                ITEM[pos, i] = null;
                            }
                        }
                    }
                }
            }

            

            private class IGMData_SideMenu : IGMData
            {
                public IGMData_SideMenu() : this(11, 1, new IGMDataItem_Box(pos: new Rectangle { Width = 226, Height = 492, X = 843 - 226 }))
                {
                }

                public IGMData_SideMenu(int count, int depth, IGMDataItem container = null) : base(count, depth, container)
                {
                }

                protected override void Init()
                {
                    
                    ITEM[0, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 0));
                    ITEM[1, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2));
                    ITEM[2, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 4));
                    ITEM[3, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 8));
                    ITEM[4, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 6));
                    ITEM[5, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 62));
                    ITEM[6, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 64));
                    ITEM[7, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 10));
                    ITEM[8, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 16));
                    ITEM[9, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 67));
                    ITEM[10, 0] = new IGMDataItem_String(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 14));
                    for (int i = 0; i < Count; i++)
                    {
                        Rectangle r = new Rectangle
                        {
                            Width = Width,
                            Height = Height / Count,
                            X = X,
                            Y = Y + (Height / Count) * i,
                        };
                        r.Inflate(-26, -12);
                        ITEM[i, 0].Pos = r;
                        SIZE[i] = r;
                        //CURSOR[i] = new Point(r.X, (int)(r.Y + 6 * TextScale.Y));
                    }
                    base.Init();
                }

                public override bool Inputs() => base.Inputs();

                public override bool Update() => base.Update();
            }

            #endregion Methods
        }

        #region Classes
        /// <summary>
        /// Flags for cursor behavior
        /// </summary>
        /// <remarks>Defaults to disabled. </remarks>
        [Flags]
        public enum Cursor_Status
        {
            /// <summary>
            /// Hide Cursor and disable all code that uses it.
            /// </summary>
            Disabled = 0x0,
            /// <summary>
            /// Show Cursor
            /// </summary>
            Enabled = 0x1,
            /// <summary>
            /// Triggers blinking
            /// </summary>
            Blinking = 0x2,
            /// <summary>
            /// Makes it react to left and right instead of up and down.
            /// </summary>
            Horizontal = 0x4,
            /// <summary>
            /// This is the default but if you want both directions you need to set the flag.
            /// </summary>
            Vertical = 0x8,
        }
        public class IGMData
        {
            #region Fields

            /// <summary>
            /// location of where pointer finger will point.
            /// </summary>
            public Point[] CURSOR;
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                }
            }
            public Cursor_Status Cursor_Status
            {
                get => _cursor_Status; set
                {
                    _cursor_Status = value;
                }
            }
            public int CURSOR_SELECT
            {
                get => _cursor_select; set
                {
                    if ((Cursor_Status & Cursor_Status.Enabled) != 0 && value >= 0 && value < CURSOR.Length && CURSOR[value] != Point.Zero)
                        _cursor_select = value;
                }
            }
            public int CURSOR_NEXT()
            {
                if ((Cursor_Status & Cursor_Status.Enabled) != 0)
                {
                    int value = _cursor_select;
                    while (true)
                    {
                        if (++value >= CURSOR.Length)
                            value = 0;
                        if (CURSOR[value] != Point.Zero || value == 0) break;
                    }
                    _cursor_select = value;
                }
                return _cursor_select;
            }
            public int CURSOR_PREV()
            {
                if ((Cursor_Status & Cursor_Status.Enabled) != 0)
                {
                    int value = _cursor_select;
                    while (true)
                    {
                        if (--value < 0)
                            value = CURSOR.Length - 1;
                        if (CURSOR[value] != Point.Zero || value == 0) break;
                    }
                    _cursor_select = value;
                }
                return _cursor_select;
            }
            public IGMDataItem[,] ITEM;

            /// <summary>
            /// Size of the entire area
            /// </summary>
            public Rectangle[] SIZE;

            public bool[] BLANKS;
            private int _cursor_select;
            private bool _enabled = true;
            private Cursor_Status _cursor_Status = Cursor_Status.Disabled;

            public IGMDataItem CONTAINER { get; protected set; }

            #endregion Fields

            #region Constructors

            public IGMData(int count, int depth, IGMDataItem container = null)
            {
                SIZE = new Rectangle[count];
                ITEM = new IGMDataItem[count, depth];
                CURSOR = new Point[count];

                Count = (byte)count;
                Depth = (byte)depth;
                BLANKS = new bool[count];
                if (container != null)
                    CONTAINER = container;
                CURSOR_SELECT = 0;
                Init();
                ReInit();
                Update();
            }

            public IGMDataItem this[int pos, int i] { get => ITEM[pos, i]; set => ITEM[pos, i] = value; }

            /// <summary>
            /// Draw all items
            /// </summary>
            public virtual void Draw()
            {
                if (Enabled)
                {
                    if (CONTAINER != null)
                        CONTAINER.Draw();
                    foreach (IGMDataItem i in ITEM)
                    {
                        if (i != null)
                            i.Draw();
                    }
                    if ((Cursor_Status & Cursor_Status.Enabled) != 0)
                    {
                        DrawPointer(CURSOR[CURSOR_SELECT], blink: ((Cursor_Status & Cursor_Status.Blinking) != 0));
                    }
                }
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Total number of items
            /// </summary>
            public byte Count { get; private set; }

            /// <summary>
            /// How many Peices per Item. Example 1 box could have 9 things to draw in it.
            /// </summary>
            public byte Depth { get; private set; }

            /// <summary>
            /// Container's Width
            /// </summary>
            public int Width => CONTAINER != null ? CONTAINER.Pos.Width : 0;

            /// <summary>
            /// Container's Height
            /// </summary>
            public int Height => CONTAINER != null ? CONTAINER.Pos.Height : 0;

            /// <summary>
            /// Container's X Position
            /// </summary>
            public int X => CONTAINER != null ? CONTAINER.Pos.X : 0;

            /// <summary>
            /// Container's Y Position
            /// </summary>
            public int Y => CONTAINER != null ? CONTAINER.Pos.Y : 0;


            /// <summary>
            /// Convert to rectangle based on container.
            /// </summary>
            /// <param name="v">Input data</param>
            public static implicit operator Rectangle(IGMData v) => v.CONTAINER ?? Rectangle.Empty;

            /// <summary>
            /// Things that change on every update.
            /// </summary>
            /// <returns>True = signifigant change</returns>
            public virtual bool Update() => false;

            /// <summary>
            /// Check inputs
            /// </summary>
            /// <returns>True = input detected</returns>
            public virtual bool Inputs()
            {
                bool ret = false;
                bool mouse = false;

                if ((Cursor_Status & Cursor_Status.Enabled) != 0)
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;
                    ml = Input.MouseLocation.Transform(Menu.Focus);
                    for (int i = 0; i < SIZE.Length; i++)
                    {
                        if (SIZE[i].Contains(ml))
                        {
                            CURSOR_SELECT = i;
                            ret = true;
                            mouse = true;
                        }
                    }
                    if (!ret && (Cursor_Status & Cursor_Status.Horizontal) != 0)
                    {
                        if (Input.Button(Buttons.Left))
                        {
                            CURSOR_PREV();
                            ret = true;
                        }
                        else if (Input.Button(Buttons.Right))
                        {
                            CURSOR_NEXT();
                            ret = true;
                        }
                    }
                    if (!ret && (Cursor_Status & Cursor_Status.Horizontal) == 0 || (Cursor_Status & Cursor_Status.Vertical) != 0)
                    {
                        if (Input.Button(Buttons.Up))
                        {
                            CURSOR_PREV();
                            ret = true;
                        }
                        else if (Input.Button(Buttons.Down))
                        {
                            CURSOR_NEXT();
                            ret = true;
                        }
                    }
                    if (!ret || mouse)
                    {
                        if (Input.Button(Buttons.Okay))
                        {
                            Inputs_OKAY();
                            return true;
                        }
                        if (Input.Button(Buttons.Cancel))
                        {
                            Inputs_CANCEL();
                            return true;
                        }
                    }
                    if (ret && !mouse)
                    {
                        Input.ResetInputLimit();
                        init_debugger_Audio.PlaySound(0);
                    }
                }
                return ret;
            }

            public virtual void Inputs_OKAY()
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(0);
            }
            public virtual void Inputs_CANCEL()
            {
                Input.ResetInputLimit();
                init_debugger_Audio.PlaySound(8);
            }

            /// <summary>
            /// Things that are fixed values at startup.
            /// </summary>
            protected virtual void Init()
            {
                if (SIZE.Length < 0 && SIZE[0].IsEmpty)
                {
                    CURSOR[0].Y = (int)(Y + Height / 2 - 6 * TextScale.Y);
                    CURSOR[0].X = X;
                }
                else
                    for (int i = 0; i < CURSOR.Length; i++)
                    {
                        if (!SIZE[i].IsEmpty)
                        {
                            CURSOR[i].Y = (int)(SIZE[i].Y + SIZE[i].Height / 2 - 6 * TextScale.Y);
                            CURSOR[i].X = SIZE[i].X;
                        }
                    }
            }

            /// <summary>
            /// Things that change rarely. Like a party member changes or Laguna dream happens.
            /// </summary>
            public virtual void ReInit()
            {
            }

            #endregion Properties
        }

        public abstract class IGMDataItem//<T>
        {
            //protected T _data;
            protected Rectangle _pos;

            public Vector2 Scale { get; set; }
            public IGMDataItem(Rectangle? pos = null, Vector2? scale = null)
            {
                _pos = pos ?? Rectangle.Empty;
                Scale = scale ?? Menu.TextScale;
            }
            /// <summary>
            /// Where to draw this item.
            /// </summary>
            public virtual Rectangle Pos { get => _pos; set => _pos = value; }

            public Color Color { get; set; } = Color.White;

            //public virtual object Data { get; public set; }
            //public virtual FF8String Data { get; public set; }
            public abstract void Draw();

            public static implicit operator Rectangle(IGMDataItem v) => v.Pos;

            public static implicit operator Color(IGMDataItem v) => v.Color;
        }
        public class IGMDataItem_Empty : IGMDataItem
        {
            public IGMDataItem_Empty(Rectangle? pos = null) : base(pos)
            {
            }

            public override void Draw()
            {
            }
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

            public bool Blink => Faded_Pallet != Pallet;
            public float Blink_Adjustment { get; set; }

            public IGMDataItem_Icon(Icons.ID data, Rectangle? pos = null, byte? pallet = null, byte? faded_pallet = null, float blink_adjustment = 1f,Vector2? scale = null) : base(pos,scale)
            {
                Data = data;
                Pallet = pallet ?? 2;
                Faded_Pallet = faded_pallet ?? Pallet;
                Blink_Adjustment = blink_adjustment;
            }

            public override void Draw()
            {
                Memory.Icons.Draw(Data, Pallet, Pos, Scale, fade);
                if (Blink)
                    Memory.Icons.Draw(Data, Faded_Pallet, Pos, Scale, fade * blink_Amount * Blink_Adjustment);
            }
        }

        public class IGMDataItem_Face : IGMDataItem
        {
            private byte _pallet;

            public Faces.ID Data { get; set; }

            public byte Pallet
            {
                get => _pallet; set
                {
                    if (value >= 16) value = 2;
                    _pallet = value;
                }
            }


            public bool Blink { get; private set; }
            public float Blink_Adjustment { get; set; }

            public IGMDataItem_Face(Faces.ID data, Rectangle? pos = null, bool blink=false, float blink_adjustment = 1f) : base(pos)
            {
                Data = data;
                Blink = blink;
                Blink_Adjustment = blink_adjustment;
            }

            public override void Draw()
            {
                Memory.Faces.Draw(Data, Pos, Vector2.UnitY, fade);
                if (Blink)
                    Memory.Faces.Draw(Data, Pos, Vector2.UnitY, fade * blink_Amount * Blink_Adjustment);
            }
        }

        public class IGMDataItem_Int : IGMDataItem//<Int>
        {
            private byte _pallet;
            private int Spaces;
            private int SpaceWidth;

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

            private int Digits;

            public IGMDataItem_Int(int data, Rectangle? pos = null, byte? pallet = null, Icons.NumType? numtype = null, byte? padding = null, int? spaces = null, int? spacewidth = null) : base(pos)
            {
                Data = data;
                Padding = padding ?? 1;
                Pallet = pallet ?? 2;
                NumType = numtype ?? 0;
                Digits = data.ToString().Length;
                if (Digits < padding) Digits = (int)padding;
                Spaces = spaces??1;
                SpaceWidth = spacewidth??20;
                _pos.Offset(SpaceWidth * (Spaces - Digits), 0);
            }

            public override void Draw() => Memory.Icons.Draw(Data, NumType, Pallet, $"D{Padding}", Pos.Location.ToVector2(), Scale, fade);
        }

        private class IGMDataItem_String : IGMDataItem
        {
            public FF8String Data { get; set; }
            public Font.ColorID Colorid { get; set; }

            public IGMDataItem_String(FF8String data, Rectangle? pos = null,Font.ColorID? color = null): base(pos)
            {
                Data = data;
                Colorid = color??Font.ColorID.White;
            }

            public override void Draw() => Memory.font.RenderBasicText(Data, Pos.Location, Scale, Fade: fade,color: Colorid);
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

        private abstract class Menu
        {
            /// <summary>
            /// replace me with new keyword
            /// </summary>
            protected enum Mode
            {
                UNDEFINDED
            }

            public Dictionary<Enum, IGMData> Data;
            /// <summary>
            /// replace me with new keyword or cast me to your new enum.
            /// </summary>
            protected Enum mode=(Mode)0;
            private Vector2 _size;
            static public Vector2 TextScale { get; protected set; }
            static public Matrix Focus;
            private bool skipdata;

            public Vector2 Size { get => _size; protected set => _size = value; }

            public Menu()
            {
                Data = new Dictionary<Enum, IGMData>();
                Init();
                skipdata = true;
                ReInit();
                skipdata = false;
            }

            protected virtual void Init() { }

            public virtual void ReInit()
            {
                if(!skipdata)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                    i.Value.ReInit();
                //Update();
            }

            public virtual void StartDraw()
            {
                Memory.SpriteBatchStartAlpha(ss: SamplerState.PointClamp, tm: Focus);
            }
            public virtual void Draw()
            {
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                    i.Value.Draw();
            }

            public virtual void EndDraw()
            {
                Memory.SpriteBatchEnd();
            }
            public virtual bool Update()
            {
                Vector2 Zoom = Memory.Scale(Size.X, Size.Y, Memory.ScaleMode.FitBoth);
                Focus = Matrix.CreateTranslation((Size.X / -2), (Size.Y / -2), 0) *
                    Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                    Matrix.CreateTranslation(vp.X / 2, vp.Y / 2, 0);

                //todo detect when there is no saves detected.
                //check for null
                if(!skipdata)
                foreach (KeyValuePair<Enum, IGMData> i in Data)
                {
                    i.Value.Update();
                }
                return false;
            }

            protected abstract bool Inputs();
        }
    }
}