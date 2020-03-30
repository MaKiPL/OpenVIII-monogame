using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private class IGMData_TopMenu_Junction : IGMData.Base
        {
            #region Methods

            public static IGMData_TopMenu_Junction Create() => Create<IGMData_TopMenu_Junction>(2, 1, new IGMDataItem.Box { Pos = new Rectangle(210, 12, 400, 54) }, 2, 1);

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Junction.SetMode(Mode.TopMenu);
                Hide();
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                if (CURSOR_SELECT == 0)
                {
                    Junction.SetMode(Mode.TopMenu_GF_Group);
                    Junction.Data[SectionName.TopMenu_GF_Group].Show();
                }
                else
                {
                    Junction.SetMode(Mode.Mag_Stat);
                    Junction.Data[SectionName.Mag_Group].Show();
                }
                return true;
            }

            public override bool Update()
            {
                Update_String();
                if (Junction != null)
                {
                    if (Junction.GetMode().Equals(Mode.TopMenu_Junction))
                        Cursor_Status &= ~Cursor_Status.Blinking;
                    else
                        Cursor_Status |= Cursor_Status.Blinking;
                }
                return base.Update();
            }

            protected override void Init()
            {
                base.Init();
                ITEM[0, 0] = new IGMDataItem.Text() { Data = Strings.Name.GF, Pos = SIZE[0] };
                ITEM[1, 0] = new IGMDataItem.Text() { Data = Strings.Name.Magic, Pos = SIZE[1] };
                Cursor_Status |= Cursor_Status.Enabled;
                Cursor_Status |= Cursor_Status.Horizontal;
                Cursor_Status |= Cursor_Status.Vertical;

                Hide();
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-40, -12);
                SIZE[i].Offset(20 + (-20 * (col > 1 ? col : 0)), 0);
            }

            private void Update_String()
            {
                if (Junction != null && Junction.GetMode().Equals(Mode.TopMenu_Junction) && Enabled)
                {
                    FF8String Changed = null;
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            Changed = Strings.Description.GF;
                            break;

                        case 1:
                            Changed = Strings.Description.Magic;
                            break;
                    }
                    if (Changed != null && Junction != null)
                        Junction.ChangeHelp(Changed);
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}