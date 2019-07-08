using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        private class IGMData_TopMenu_Junction : IGMData
        {
            public new Dictionary<Items, FF8String> Descriptions { get; private set; }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.SetMode(Mode.TopMenu);
                Hide();
                return true;
            }

            public override void Inputs_OKAY()
            {
                base.Inputs_OKAY();
                if (CURSOR_SELECT == 0)
                {
                    IGM_Junction.SetMode(Mode.TopMenu_GF_Group);
                    IGM_Junction.Data[SectionName.TopMenu_GF_Group].Show();
                }
                else
                {
                    IGM_Junction.SetMode(Mode.Mag_Stat);
                    IGM_Junction.Data[SectionName.Mag_Group].Show();
                }
            }

            public IGMData_TopMenu_Junction() : base(2, 1, new IGMDataItem_Box(pos: new Rectangle(210, 12, 400, 54)), 2, 1)
            {
            }

            public override bool Update()
            {
                Update_String();
                if (IGM_Junction != null)
                {
                    if (IGM_Junction.GetMode().Equals(Mode.TopMenu_Junction))
                        Cursor_Status &= ~Cursor_Status.Blinking;
                    else
                        Cursor_Status |= Cursor_Status.Blinking;
                }
                return base.Update();
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
                ITEM[0, 0] = new IGMDataItem_String(Titles[Items.GF], SIZE[0]);
                ITEM[1, 0] = new IGMDataItem_String(Titles[Items.Magic], SIZE[1]);
                Cursor_Status |= Cursor_Status.Enabled;
                Cursor_Status |= Cursor_Status.Horizontal;
                Cursor_Status |= Cursor_Status.Vertical;

                Descriptions = new Dictionary<Items, FF8String> {
                        {Items.GF,Memory.Strings.Read(Strings.FileID.MNGRP,2,263)},
                        {Items.Magic,Memory.Strings.Read(Strings.FileID.MNGRP,2,265)},
                    };

                Hide();
            }

            private void Update_String()
            {
                if (IGM_Junction != null && IGM_Junction.GetMode().Equals(Mode.TopMenu_Junction) && Enabled)
                {
                    FF8String Changed = null;
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            Changed = Descriptions[Items.GF];
                            break;

                        case 1:
                            Changed = Descriptions[Items.Magic];
                            break;
                    }
                    if (Changed != null && IGM_Junction != null)
                        IGM_Junction.ChangeHelp(Changed);
                }
            }
        }
    }
}