using System;

namespace OpenVIII.IGMData.Dialog.Timed
{
    public class Small : IGMData.Dialog.Small
    {
        #region Fields

        private TimeSpan maxtime = TimeSpan.FromMilliseconds(3000);
        private TimeSpan timeshow = TimeSpan.Zero;

        #endregion Fields

        #region Methods

        public static Small Create(FF8String data, int x, int y, Icons.ID? title = null, Box_Options options = Box_Options.Default) => Create<Small>(data, x, y, title, options);

        public override void Show()
        {
            timeshow = TimeSpan.Zero;
            base.Show();
        }

        public override bool Update()
        {
            if (Enabled)
            {
                base.Update();
                if ((timeshow += Memory.ElapsedGameTime) < maxtime)
                {
                    return true;
                }
                else
                    Hide();
            }
            return false;
        }

        #endregion Methods
    }
}