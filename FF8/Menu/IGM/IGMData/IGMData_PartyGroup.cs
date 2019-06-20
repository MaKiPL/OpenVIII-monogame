using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM
        {
            private class IGMData_PartyGroup : IGMData_Group
            {
                private bool eventSet = false;
                private Items Choice;
                private Tuple<Characters, Characters>[] Contents;

                public IGMData_PartyGroup(params IGMData[] d) : base(d)
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status &= ~Cursor_Status.Horizontal;
                    Cursor_Status &= ~Cursor_Status.Blinking;
                }

                public override bool Inputs()
                {

                    Cursor_Status |= Cursor_Status.Enabled;
                    return base.Inputs();
                }

                public override void ReInit()
                {

                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu.ChoiceChangeHandler += ChoiceChangeEvent;
                        eventSet = true;
                    }
                    base.ReInit();

                    if (Memory.State.Characters != null)
                    {
                        IGMDataItem_IGMData i = ((IGMDataItem_IGMData)ITEM[0, 0]);
                        IGMDataItem_IGMData i2 = ((IGMDataItem_IGMData)ITEM[1, 0]);
                        if (i != null && i.Data != null && i2 != null && i2.Data != null)
                        {
                            SIZE = new Rectangle[i.Data.Count + i2.Data.Count];
                            Array.Copy(i.Data.SIZE, SIZE, i.Data.Count);
                            Array.Copy(i2.Data.SIZE, 0, SIZE, i.Data.Count, i2.Data.Count);
                            CURSOR = new Point[i.Data.Count + i2.Data.Count];
                            Array.Copy(i.Data.CURSOR, CURSOR, i.Data.Count);
                            Array.Copy(i2.Data.CURSOR, 0, CURSOR, i.Data.Count, i2.Data.Count);
                            BLANKS = new bool[i.Data.Count + i2.Data.Count];
                            Array.Copy(i.Data.BLANKS, BLANKS, i.Data.Count);
                            Array.Copy(i2.Data.BLANKS, 0, BLANKS, i.Data.Count, i2.Data.Count);
                            Contents = new Tuple<Characters,Characters>[i.Data.Count + i2.Data.Count];
                            Array.Copy(((IGMData_Party)i.Data).Contents, Contents, i.Data.Count);
                            Array.Copy(((IGMData_NonParty)i2.Data).Contents, 0, Contents, i.Data.Count, i2.Data.Count);
                        }
                    }
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e)
                {
                    Choice = e.Key;
                }

                private void ModeChangeEvent(object sender, Mode e)
                {
                    if (e != Mode.ChooseChar)
                    {
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    }
                }
                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    fade = 0;
                    switch(Choice)
                    {
                        case Items.Junction:
                            State = MainMenuStates.IGM_Junction;
                            InGameMenu_Junction.ReInit(Contents[CURSOR_SELECT].Item1, Contents[CURSOR_SELECT].Item2);
                            return;
                    }
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu.SetMode(Mode.ChooseItem);
                }
            }
        }
    }
}
            //{
            //    bool ret = false;
            //    if (Enabled)
            //    {
            //        foreach (KeyValuePair<Enum, IGMData> i in Data)
            //        {
            //            i.Value.Inputs();
            //        }
            //        ml = Input.MouseLocation.Transform(Focus);

            //        if (GetMode() == Mode.ChooseItem)
            //        {
            //            Data[SectionName.SideMenu].Inputs();
            //            //if (Data[SectionName.SideMenu] != null && Data[SectionName.SideMenu].Count > 0)
            //            //{
            //            //    for (int pos = 0; pos < Data[SectionName.SideMenu].Count; pos++)
            //            //    {
            //            //        Rectangle r = Data[SectionName.SideMenu].ITEM[pos, 0];
            //            //        if (r.Contains(ml))
            //            //        {
            //            //            choSideBar = (Items)pos;
            //            //            ret = true;

            //            //            if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
            //            //            {
            //            //                return ret;
            //            //            }
            //            //            break;
            //            //        }
            //            //    }

            //            //    if (Input.Button(Buttons.Down))
            //            //    {
            //            //        Input.ResetInputLimit();
            //            //        init_debugger_Audio.PlaySound(0);
            //            //        if ((int)++choSideBar >= ((IGMData_SideMenu)Data[SectionName.SideMenu]).Count)
            //            //            choSideBar = 0;
            //            //        ret = true;
            //            //    }
            //            //    else if (Input.Button(Buttons.Up))
            //            //    {
            //            //        Input.ResetInputLimit();
            //            //        init_debugger_Audio.PlaySound(0);
            //            //        if (--choSideBar < 0)
            //            //            choSideBar = (Items)((IGMData_SideMenu)Data[SectionName.SideMenu]).Count - 1;
            //            //        ret = true;
            //            //    }
            //            //    else if (Input.Button(Buttons.Cancel))
            //            //    {
            //            //        Input.ResetInputLimit();
            //            //        init_debugger_Audio.PlaySound(8);
            //            //        Fade = 0.0f;
            //            //        State = MainMenuStates.LoadGameChooseGame;
            //            //        ret = true;
            //            //    }
            //            //    else if (Input.Button(Buttons.Okay))
            //            //    {
            //            //        Input.ResetInputLimit();
            //            //        init_debugger_Audio.PlaySound(0);
            //            //        ret = true;
            //            //        switch (choSideBar)
            //            //        {
            //            //            //Select Char Mode
            //            //            case Items.Junction:
            //            //            case Items.Magic:
            //            //            case Items.Status:
            //            //                mode = Mode.ChooseChar;
            //            //                break;
            //            //            case Items.Item:
            //            //                State = MainMenuStates.IGM_Items;
            //            //                InGameMenu_Items.ReInit();
            //            //                break;
            //            //        }
            //            //    }
            //            //}
            //        }
            //        //else if (GetMode() == Mode.ChooseChar)
            //        //{
            //        //    for (int i = 0; i < Data[SectionName.Party].Count; i++)
            //        //    {
            //        //        if (Data[SectionName.Party].BLANKS[i]) continue;
            //        //        Rectangle r = Data[SectionName.Party].SIZE[i];
            //        //        if (r.Contains(ml))
            //        //        {
            //        //            choChar = i;
            //        //            ret = true;

            //        //            if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
            //        //            {
            //        //                return ret;
            //        //            }
            //        //            break;
            //        //        }
            //        //    }
            //        //    for (int i = Data[SectionName.Party].Count; i < Data[SectionName.Non_Party].Count + Data[SectionName.Party].Count; i++)
            //        //    {
            //        //        if (Data[SectionName.Non_Party].BLANKS[i - Data[SectionName.Party].Count]) continue;
            //        //        Rectangle r = Data[SectionName.Non_Party].SIZE[i - Data[SectionName.Party].Count];
            //        //        //r.Offset(focus.Translation.X, focus.Translation.Y);
            //        //        if (r.Contains(ml))
            //        //        {
            //        //            choChar = i;
            //        //            ret = true;

            //        //            if (Input.Button(Buttons.MouseWheelup) || Input.Button(Buttons.MouseWheeldown))
            //        //            {
            //        //                return ret;
            //        //            }
            //        //            break;
            //        //        }
            //        //    }
            //        //    if (Input.Button(Buttons.Down))
            //        //    {
            //        //        Input.ResetInputLimit();
            //        //        init_debugger_Audio.PlaySound(0);
            //        //        choChar++;
            //        //        ret = true;
            //        //    }
            //        //    else if (Input.Button(Buttons.Up))
            //        //    {
            //        //        Input.ResetInputLimit();
            //        //        init_debugger_Audio.PlaySound(0);
            //        //        choChar--;
            //        //        ret = true;
            //        //    }
            //        //    else if (Input.Button(Buttons.Cancel))
            //        //    {
            //        //        Input.ResetInputLimit();
            //        //        ret = true;
            //        //        init_debugger_Audio.PlaySound(8);
            //        //        SetMode(Mode.ChooseItem);
            //        //    }
            //        //    else if (Input.Button(Buttons.Okay))
            //        //    {
            //        //        Input.ResetInputLimit();
            //        //        init_debugger_Audio.PlaySound(0);
            //        //        ret = true;
            //        //        switch (choSideBar)
            //        //        {
            //        //            //Select Char Mode
            //        //            case Items.Junction:
            //        //                //case Items.Magic:
            //        //                //case Items.Status:
            //        //                State = MainMenuStates.IGM_Junction;
            //        //                if (choChar < 3)
            //        //                    InGameMenu_Junction.ReInit(Memory.State.PartyData[choChar], Memory.State.Party[choChar]);
            //        //                else
            //        //                {
            //        //                    int pos = 0;
            //        //                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
            //        //                    {
            //        //                        for (byte i = 0; Memory.State.Party != null && i < Memory.State.Characters.Count; i++)
            //        //                        {
            //        //                            if (!Memory.State.PartyData.Contains((Characters)i) && Memory.State.Characters[(Characters)i].VisibleInMenu)
            //        //                            {
            //        //                                if (pos++ + 3 == choChar)
            //        //                                {
            //        //                                    InGameMenu_Junction.ReInit((Characters)i, (Characters)i);
            //        //                                    break;
            //        //                                }
            //        //                            }
            //        //                        }
            //        //                    }
            //        //                }
            //        //                break;
            //        //        }
            //        //    }
            //        //}
            //    }
            //    return ret;
            //}
