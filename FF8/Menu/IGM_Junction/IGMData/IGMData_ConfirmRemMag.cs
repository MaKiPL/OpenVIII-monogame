using Microsoft.Xna.Framework;
using System.Linq;

namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private sealed class IGMData_ConfirmRemMag : IGMData_ConfirmDialog
            {
                public IGMData_ConfirmRemMag(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle pos) : base( data, title, opt1, opt2, pos) => startcursor = 1;

                public override void Inputs_OKAY()
                {
                    switch (CURSOR_SELECT)
                    {
                        case 0:
                            skipsnd = true;
                            init_debugger_Audio.PlaySound(31);
                            base.Inputs_OKAY();
                            Memory.State.Characters[Character].Stat_J = Memory.State.Characters[Character].Stat_J.ToDictionary(e => e.Key, e => (byte)0);
                            Inputs_CANCEL();
                            InGameMenu_Junction.ReInit();
                            break;

                        case 1:
                            Inputs_CANCEL();
                            break;
                    }
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Junction.Data[SectionName.RemMag].Hide();
                    InGameMenu_Junction.SetMode(Mode.TopMenu_Off);
                }
            }
        }
    }
}