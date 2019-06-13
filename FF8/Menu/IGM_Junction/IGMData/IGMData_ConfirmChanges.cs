using Microsoft.Xna.Framework;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private sealed class IGMData_ConfirmChanges : IGMData_ConfirmDialog
            {
                public IGMData_ConfirmChanges(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle pos) : base(data, title, opt1, opt2, pos)
                {
                    startcursor = 1;

                }
                protected override void SetSize()
                {
                    base.SetSize();
                    SIZE[0].X = X + 20;
                    SIZE[1].X = X + 20;
                    SIZE[0].Width = Width - 40;
                    SIZE[1].Width = Width - 40;
                }

                public override void Inputs_OKAY()
                {
                    skipsnd = true;
                    init_debugger_Audio.PlaySound(31);
                    InGameMenu_Junction.Data[SectionName.ConfirmChanges].Hide();
                    InGameMenu_Junction.SetMode(Mode.TopMenu);

                    base.Inputs_OKAY();
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            break;

                        case 1:
                            Memory.State = Memory.PrevState.Clone();
                            break;
                    }
                    if (State == MainMenuStates.IGM_Junction)
                    {
                        State = MainMenuStates.InGameMenu;
                        InGameMenu.ReInit();
                        Fade = 0.0f;
                    }
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.Data[SectionName.ConfirmChanges].Hide();
                    InGameMenu_Junction.SetMode(Mode.TopMenu);
                }
            }
        }
    }
}