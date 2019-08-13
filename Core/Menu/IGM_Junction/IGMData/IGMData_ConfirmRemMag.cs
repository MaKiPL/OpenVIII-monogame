using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private sealed class IGMData_ConfirmRemMag : IGMData_ConfirmDialog
        {
            #region Constructors

            public IGMData_ConfirmRemMag(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle pos) : base(data, title, opt1, opt2, pos) => startcursor = 1;

            #endregion Constructors

            #region Methods

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM_Junction.Data[SectionName.RemMag].Hide();
                IGM_Junction.SetMode(Mode.TopMenu_Off);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                switch (CURSOR_SELECT)
                {
                    case 0:
                        skipsnd = true;
                        init_debugger_Audio.PlaySound(31);
                        base.Inputs_OKAY();
                        Memory.State.Characters[Character].RemoveMagic();
                        Inputs_CANCEL();
                        IGM_Junction.Refresh();
                        break;

                    case 1:
                        Inputs_CANCEL();
                        break;
                    default: return false;
                }
                return true;
            }


            #endregion Methods
        }

        #endregion Classes
    }
}