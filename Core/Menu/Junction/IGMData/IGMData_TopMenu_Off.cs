using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private class IGMData_TopMenu_Off : IGMData.Base
        {
            #region Methods

            public static IGMData_TopMenu_Off Create() => Create<IGMData_TopMenu_Off>(2, 1, new IGMDataItem.Box { Pos = new Rectangle(165, 12, 445, 54) }, 2, 1);

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Junction.Data[SectionName.TopMenu_Off].Hide();
                Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                switch (CURSOR_SELECT)
                {
                    case 0:
                        Junction.Data[SectionName.RemMag].Show();
                        Junction.SetMode(Mode.RemMag);
                        break;

                    case 1:
                        Junction.Data[SectionName.RemAll].Show();
                        Junction.SetMode(Mode.RemAll);
                        break;

                    default:
                        return false;
                }
                return true;
            }

            public override bool Update()
            {
                var ret = base.Update();
                Update_String();

                if (Junction != null)
                {
                    if (Junction.GetMode().Equals(Mode.TopMenu_Off))
                        Cursor_Status &= ~Cursor_Status.Blinking;
                    else
                        Cursor_Status |= Cursor_Status.Blinking;
                }
                return ret;
            }

            protected override void Init()
            {
                base.Init();
                ITEM[0, 0] = new IGMDataItem.Text() { Data = Strings.Name.RemMag, Pos = SIZE[0] };
                ITEM[1, 0] = new IGMDataItem.Text() { Data = Strings.Name.RemAll, Pos = SIZE[1] };
                Cursor_Status |= (Cursor_Status.Enabled | Cursor_Status.Horizontal | Cursor_Status.Vertical);
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-40, -12);
                SIZE[i].Offset(20 + (-20 * (col > 1 ? col : 0)), 0);
            }

            private void Update_String()
            {
                if (Junction != null && Junction.GetMode().Equals(Mode.TopMenu_Off) && Enabled)
                {
                    FF8String Changed = null;
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            Changed = Strings.Description.RemMag;
                            break;

                        case 1:
                            Changed = Strings.Description.RemAll;
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