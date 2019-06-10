using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_TopMenu_Auto : IGMData
            {
                public IGMData_TopMenu_Auto() : base( 3, 1, new IGMDataItem_Box(pos: new Rectangle(165, 12, 445, 54)), 3, 1)
                {
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-40, -12);
                    SIZE[i].Offset(20 + (-20 * (col > 1 ? col : 0)), 0);
                }

                public new Dictionary<Items, FF8String> Descriptions { get; private set; }

                private void Update_String()
                {
                    if (InGameMenu_Junction != null && InGameMenu_Junction.GetMode() == Mode.TopMenu_Auto && Enabled)
                    {
                        FF8String Changed = null;
                        switch (CURSOR_SELECT)
                        {
                            case 0:
                                Changed = Descriptions[Items.AutoAtk];
                                break;

                            case 1:
                                Changed = Descriptions[Items.AutoDef];
                                break;

                            case 2:
                                Changed = Descriptions[Items.AutoMag];
                                break;
                        }
                        if (Changed != null && InGameMenu_Junction != null)
                            InGameMenu_Junction.ChangeHelp(Changed);
                    }
                }

                public override bool Update()
                {
                    bool ret = base.Update();
                    Update_String();
                    return ret;
                }

                protected override void Init()
                {
                    base.Init();
                    ITEM[0, 0] = new IGMDataItem_String(Titles[Items.AutoAtk], SIZE[0]);
                    ITEM[1, 0] = new IGMDataItem_String(Titles[Items.AutoDef], SIZE[1]);
                    ITEM[2, 0] = new IGMDataItem_String(Titles[Items.AutoMag], SIZE[2]);
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Descriptions = new Dictionary<Items, FF8String> {
                        //{Items.HP, Memory.Strings.Read(Strings.FileID.MNGRP,2,226) },
                        //{Items.Str, Memory.Strings.Read(Strings.FileID.MNGRP,2,228) },
                        //{Items.Vit, Memory.Strings.Read(Strings.FileID.MNGRP,2,230) },
                        //{Items.Mag, Memory.Strings.Read(Strings.FileID.MNGRP,2,232) },
                        //{Items.Spr, Memory.Strings.Read(Strings.FileID.MNGRP,2,234) },
                        //{Items.Spd, Memory.Strings.Read(Strings.FileID.MNGRP,2,236) },
                        //{Items.Luck, Memory.Strings.Read(Strings.FileID.MNGRP,2,238) },
                        //{Items.Hit, Memory.Strings.Read(Strings.FileID.MNGRP,2,240) },
                        //{Items.ST_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,244)},
                        //{Items.ST_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,246)},
                        //{Items.EL_A,Memory.Strings.Read(Strings.FileID.MNGRP,2,248)},
                        //{Items.EL_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,250)},
                        //{Items.ST_A_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,252)},
                        //{Items.EL_A_D,Memory.Strings.Read(Strings.FileID.MNGRP,2,254)},
                        //{ Items.Stats,Memory.Strings.Read(Strings.FileID.MNGRP,2,256)},
                        //{Items.ST_A2,Memory.Strings.Read(Strings.FileID.MNGRP,2,258)},
                        //{Items.GF,Memory.Strings.Read(Strings.FileID.MNGRP,2,263)},
                        //{Items.Magic,Memory.Strings.Read(Strings.FileID.MNGRP,2,265)},
                        {Items.AutoAtk,Memory.Strings.Read(Strings.FileID.MNGRP,2,270)},
                        {Items.AutoMag,Memory.Strings.Read(Strings.FileID.MNGRP,2,272)},
                        {Items.AutoDef,Memory.Strings.Read(Strings.FileID.MNGRP,2,274)},
                        //{Items.RemMag,Memory.Strings.Read(Strings.FileID.MNGRP,2,278)},
                        //{Items.RemAll,Memory.Strings.Read(Strings.FileID.MNGRP,2,276)},
                        //{Items.RemovealljunctionedGFandmagic,Memory.Strings.Read(Strings.FileID.MNGRP,2,279)},
                        //{Items.Removealljunctionedmagic,Memory.Strings.Read(Strings.FileID.MNGRP,2,280)},
                        //{Items.ChooseGFtojunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,281)},
                        //{Items.Chooseslottojunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,282)},
                        //{Items.Choosemagictojunction,Memory.Strings.Read(Strings.FileID.MNGRP,2,283)},
                    };
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.Data[SectionName.TopMenu_Auto].Hide();
                    InGameMenu_Junction.SetMode(Mode.TopMenu);
                }
            }
        }
    }
}