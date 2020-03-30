using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class Junction
    {
        #region Classes

        private sealed class IGMData_ConfirmChanges : IGMData.Dialog.Confirm
        {
            #region Methods

            public static IGMData_ConfirmChanges Create(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle pos)
            {
                var r = Create<IGMData_ConfirmChanges>(data, title, opt1, opt2, pos);
                r.startcursor = 1;
                return r;
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Junction.Data[SectionName.ConfirmChanges].Hide();
                Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                skipsnd = true;
                AV.Sound.Play(31);
                Junction.Data[SectionName.ConfirmChanges].Hide();
                Junction.SetMode(Mode.TopMenu);

                base.Inputs_OKAY();
                switch (CURSOR_SELECT)
                {
                    case 0:
                        break;

                    case 1:
                        Memory.State = Memory.PrevState.Clone();
                        break;
                }
                if (Menu.Module.State == MenuModule.Mode.IGM_Junction)
                {
                    Menu.Module.State = MenuModule.Mode.IGM;
                    IGM.Refresh();
                    FadeIn();
                    return true;
                }
                return false;
            }

            protected override void SetSize()
            {
                base.SetSize();
                SIZE[0].X = X + 20;
                SIZE[1].X = X + 20;
                SIZE[0].Width = Width - 40;
                SIZE[1].Width = Width - 40;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}