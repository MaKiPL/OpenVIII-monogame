using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_TopMenu_Off : IGMData
        {
            #region Constructors

            public IGMData_TopMenu_Off() : base(2, 1, new IGMDataItem_Box(pos: new Rectangle(165, 12, 445, 54)), 2, 1)
            {
            }

            #endregion Constructors

            #region Properties

            public new Dictionary<Items, FF8String> Descriptions { get; private set; }

            #endregion Properties

            #region Methods

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.Data[SectionName.TopMenu_Off].Hide();
                IGM_Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override void Inputs_OKAY()
            {
                base.Inputs_OKAY();
                switch (CURSOR_SELECT)
                {
                    case 0:
                        IGM_Junction.Data[SectionName.RemMag].Show();
                        IGM_Junction.SetMode(Mode.RemMag);
                        break;

                    case 1:
                        IGM_Junction.Data[SectionName.RemAll].Show();
                        IGM_Junction.SetMode(Mode.RemAll);
                        break;
                }
            }

            public override bool Update()
            {
                bool ret = base.Update();
                Update_String();

                if (IGM_Junction != null)
                {
                    if (IGM_Junction.GetMode().Equals(Mode.TopMenu_Off))
                        Cursor_Status &= ~Cursor_Status.Blinking;
                    else
                        Cursor_Status |= Cursor_Status.Blinking;
                }
                return ret;
            }

            protected override void Init()
            {
                base.Init();
                ITEM[0, 0] = new IGMDataItem_String(Titles[Items.RemMag], SIZE[0]);
                ITEM[1, 0] = new IGMDataItem_String(Titles[Items.RemAll], SIZE[1]);
                Cursor_Status |= Cursor_Status.Enabled;
                Cursor_Status |= Cursor_Status.Horizontal;
                Cursor_Status |= Cursor_Status.Vertical;
                Descriptions = new Dictionary<Items, FF8String> {
                        {Items.RemMag,Memory.Strings.Read(Strings.FileID.MNGRP,2,278)},
                        {Items.RemAll,Memory.Strings.Read(Strings.FileID.MNGRP,2,276)},
                    };
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-40, -12);
                SIZE[i].Offset(20 + (-20 * (col > 1 ? col : 0)), 0);
            }

            private void Update_String()
            {
                if (IGM_Junction != null && IGM_Junction.GetMode().Equals(Mode.TopMenu_Off) && Enabled)
                {
                    FF8String Changed = null;
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            Changed = Descriptions[Items.RemMag];
                            break;

                        case 1:
                            Changed = Descriptions[Items.RemAll];
                            break;
                    }
                    if (Changed != null && IGM_Junction != null)
                        IGM_Junction.ChangeHelp(Changed);
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}