using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM : Menu
        {
            #region Fields

            private Items choSideBar;
            private int _choChar;
            protected Mode mode=0;

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

            protected enum Mode
            {
                ChooseItem,
                ChooseChar,
            }

            #endregion Enums

            #region Methods

            public override void Draw()
            {
                if (Enabled)
                {
                    StartDraw();
                    switch (mode)
                    {
                        case Mode.ChooseChar:
                        case Mode.ChooseItem:
                        default:
                            DrawData();
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
                    EndDraw();
                }
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
                if (Enabled)
                {
                    bool ret = base.Update();
                    ret = ((IGMData_Header)Data[SectionName.Header]).Update(choSideBar) || ret;
                    return Inputs() || ret;
                }
                return false;
            }

            protected override bool Inputs()
            {
                bool ret = false;
                if (Enabled)
                {
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
                                    choSideBar = (Items)((IGMData_SideMenu)Data[SectionName.SideMenu]).Count - 1;
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
                                    case Items.Item:
                                        State = MainMenuStates.IGM_Items;
                                        InGameMenu_Items.ReInit();
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
                                            for (byte i = 0; Memory.State.Party != null && i < Memory.State.Characters.Count; i++)
                                            {
                                                if (!Memory.State.PartyData.Contains((Characters)i) && Memory.State.Characters[(Characters)i].VisibleInMenu)
                                                {
                                                    if (pos++ + 3 == choChar)
                                                    {
                                                        InGameMenu_Junction.ReInit((Characters)i, (Characters)i);
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
                }
                return ret;
            }

            #endregion Methods
        }
    }
}