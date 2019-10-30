namespace OpenVIII.IGMData.Dialog.Timed
{
    public class Small : IGMData.Dialog.Small
    {
        #region Fields

        private double maxtime = 3000;
        private double timeshow = 0;

        #endregion Fields

        #region Methods

        public static Small Create(FF8String data, int x, int y, Icons.ID? title = null, Box_Options options = Box_Options.Default) => Create<Small>(data, x, y, title, options);

        public override void Show()
        {
            timeshow = Memory.gameTime.TotalGameTime.TotalMilliseconds;
            base.Show();
        }

        public override bool Update()
        {
            if (Enabled)
            {
                base.Update();
                if (Memory.gameTime.TotalGameTime.TotalMilliseconds - timeshow < maxtime)
                    return true;
                else
                    Hide();
            }
            return false;
        }

        #endregion Methods
    }
}