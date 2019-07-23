using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private sealed class IGMData_ConfirmChanges : IGMData_ConfirmDialog
        {
            #region Constructors

            public IGMData_ConfirmChanges(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle pos) : base(data, title, opt1, opt2, pos) => startcursor = 1;

            #endregion Constructors

            #region Methods

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.Data[SectionName.ConfirmChanges].Hide();
                IGM_Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override void Inputs_OKAY()
            {
                skipsnd = true;
                init_debugger_Audio.PlaySound(31);
                IGM_Junction.Data[SectionName.ConfirmChanges].Hide();
                IGM_Junction.SetMode(Mode.TopMenu);

                base.Inputs_OKAY();
                switch (CURSOR_SELECT)
                {
                    case 0:
                        break;

                    case 1:
                        Memory.State = Memory.PrevState.Clone();
                        break;
                }
                if (Module_main_menu_debug.State == Module_main_menu_debug.MainMenuStates.IGM_Junction)
                {
                    Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.IGM;
                    IGM.Refresh();
                    FadeIn();
                }
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