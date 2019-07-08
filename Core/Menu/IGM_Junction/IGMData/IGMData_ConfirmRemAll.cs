using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        private sealed class IGMData_ConfirmRemAll : IGMData_ConfirmDialog
        {
            public IGMData_ConfirmRemAll(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle pos) : base(data, title, opt1, opt2, pos) => startcursor = 1;

            public override void Inputs_OKAY()
            {
                switch (CURSOR_SELECT)
                {
                    case 0:
                        skipsnd = true;
                        init_debugger_Audio.PlaySound(31);
                        base.Inputs_OKAY();
                        Memory.State.Characters[Character].RemoveAll();

                        IGM_Junction.Data[SectionName.RemAll].Hide();
                        IGM_Junction.Data[SectionName.TopMenu_Off].Hide();
                        IGM_Junction.SetMode(Mode.TopMenu);
                        IGM_Junction.Data[SectionName.TopMenu].CURSOR_SELECT = 0;
                        IGM_Junction.ReInit();
                        break;

                    case 1:
                        Inputs_CANCEL();
                        break;
                }
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.Data[SectionName.RemAll].Hide();
                IGM_Junction.SetMode(Mode.TopMenu_Off);
                return true;
            }
        }
    }
}