using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_TopMenu : IGMData
            {
                public new Dictionary<Items, FF8String> Descriptions { get; private set; }

                public IGMData_TopMenu() : base( 4, 1, new IGMDataItem_Box(pos: new Rectangle(0, 12, 610, 54)), 4, 1)
                {
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-40, -12);
                    SIZE[i].Offset(20 + (-20 * (col > 1 ? col : 0)), 0);
                }

                protected override void Init()
                {
                    base.Init();
                    ITEM[0, 0] = new IGMDataItem_String(Titles[Items.Junction], SIZE[0]);
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Descriptions = new Dictionary<Items, FF8String> {
                        {Items.Junction, Memory.Strings.Read(Strings.FileID.MNGRP,2,218) },
                        {Items.Off, Memory.Strings.Read(Strings.FileID.MNGRP,2,220) },
                        {Items.Auto, Memory.Strings.Read(Strings.FileID.MNGRP,2,222) },
                        {Items.Ability, Memory.Strings.Read(Strings.FileID.MNGRP,2,224) },
                    };
                }

                public override void ReInit()
                {
                    if (Memory.State.Characters != null)
                    {
                        Font.ColorID color = (Memory.State.Characters[Character].JunctionnedGFs == Saves.GFflags.None) ? Font.ColorID.Grey : Font.ColorID.White;

                        ITEM[1, 0] = new IGMDataItem_String(Titles[Items.Off], SIZE[1], color);
                        ITEM[2, 0] = new IGMDataItem_String(Titles[Items.Auto], SIZE[2], color);
                        ITEM[3, 0] = new IGMDataItem_String(Titles[Items.Ability], SIZE[3], color);
                        for (int i = 1; i <= 3; i++)
                            BLANKS[i] = Memory.State.Characters[Character].JunctionnedGFs == Saves.GFflags.None;
                    }
                    base.ReInit();
                }

                public override void Inputs_CANCEL()
                {
                    if (Memory.PrevState.Characters[VisableCharacter].CurrentHP() > Memory.State.Characters[VisableCharacter].CurrentHP())
                    {
                        InGameMenu_Junction.Data[SectionName.ConfirmChanges].Show();
                        InGameMenu_Junction.SetMode(Mode.ConfirmChanges);
                    }
                    else
                    {
                        base.Inputs_CANCEL();
                        if (State == MainMenuStates.IGM_Junction)
                        {
                            State = MainMenuStates.InGameMenu;
                            InGameMenu.ReInit();
                            Fade = 0.0f;
                        }
                    }
                }

                public override void Inputs_OKAY()
                {
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            InGameMenu_Junction.Data[SectionName.TopMenu_Junction].Show();
                            Cursor_Status |= Cursor_Status.Blinking;
                            InGameMenu_Junction.SetMode(Mode.TopMenu_Junction);
                            break;

                        case 1:
                            InGameMenu_Junction.Data[SectionName.TopMenu_Off].Show();
                            Cursor_Status |= Cursor_Status.Blinking;
                            InGameMenu_Junction.SetMode(Mode.TopMenu_Off);
                            break;

                        case 2:
                            InGameMenu_Junction.Data[SectionName.TopMenu_Auto].Show();
                            Cursor_Status |= Cursor_Status.Blinking;
                            InGameMenu_Junction.SetMode(Mode.TopMenu_Auto);
                            break;

                        case 3:
                            InGameMenu_Junction.Data[SectionName.TopMenu_Abilities].Show();
                            Cursor_Status |= Cursor_Status.Blinking;
                            InGameMenu_Junction.SetMode(Mode.Abilities);
                            break;
                    }
                    base.Inputs_OKAY();
                }

                public override bool Update()
                {
                    bool ret = base.Update();
                    if (InGameMenu_Junction != null && InGameMenu_Junction.GetMode() == Mode.TopMenu && Enabled)
                    {
                        FF8String Changed = null;
                        switch (CURSOR_SELECT)
                        {
                            case 0:
                                Changed = Descriptions[Items.Junction];
                                break;

                            case 1:
                                Changed = Descriptions[Items.Off];
                                break;

                            case 2:
                                Changed = Descriptions[Items.Auto];
                                break;

                            case 3:
                                Changed = Descriptions[Items.Ability];
                                break;
                        }
                        if (Changed != null)
                            InGameMenu_Junction.ChangeHelp(Changed);
                    }
                    return ret;
                }
            }
        }
    }
}