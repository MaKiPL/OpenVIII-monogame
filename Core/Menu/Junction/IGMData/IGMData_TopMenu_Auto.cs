using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private class IGMData_TopMenu_Auto : IGMData.Base
        {
            #region Methods

            public static IGMData_TopMenu_Auto Create() => Create<IGMData_TopMenu_Auto>(3, 1, new IGMDataItem.Box { Pos = new Rectangle(165, 12, 445, 54) }, 3, 1);

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Junction.Data[SectionName.TopMenu_Auto].Hide();
                Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                if (Damageable.GetCharacterData(out var c))
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            c.AutoATK();
                            break;

                        case 1:
                            c.AutoDEF();
                            break;

                        case 2:
                            c.AutoMAG();
                            break;

                        default: return false;
                    }
                skipsnd = true;
                AV.Sound.Play(31);
                Inputs_CANCEL();
                Junction.Refresh();
                return true;
            }

            public override bool Update()
            {
                var ret = base.Update();
                Update_String();
                return ret;
            }

            protected override void Init()
            {
                base.Init();
                ITEM[0, 0] = new IGMDataItem.Text { Data = Strings.Name.AutoAtk, Pos = SIZE[0] };
                ITEM[1, 0] = new IGMDataItem.Text { Data = Strings.Name.AutoDef, Pos = SIZE[1] };
                ITEM[2, 0] = new IGMDataItem.Text { Data = Strings.Name.AutoMag, Pos = SIZE[2] };
                Cursor_Status |= Cursor_Status.Enabled;
                Cursor_Status |= Cursor_Status.Horizontal;
                Cursor_Status |= Cursor_Status.Vertical;
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-40, -12);
                SIZE[i].Offset(20 + (-20 * (col > 1 ? col : 0)), 0);
            }

            private void Update_String()
            {
                if (Junction != null && Junction.GetMode().Equals(Mode.TopMenu_Auto) && Enabled)
                {
                    FF8String Changed = null;
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            Changed = Strings.Description.AutoAtk;
                            break;

                        case 1:
                            Changed = Strings.Description.AutoDef;
                            break;

                        case 2:
                            Changed = Strings.Description.AutoMag;
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