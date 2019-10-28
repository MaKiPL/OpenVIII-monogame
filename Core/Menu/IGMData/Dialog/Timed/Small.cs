namespace OpenVIII.IGMData.Dialog.Timed
{
    public class Small : IGMData.Dialog.Small
    {
        #region Fields

        private double maxtime = 3000;
        private double timeelapsed = 0;

        #endregion Fields

        #region Methods

        public static Small Create(FF8String data, int x, int y, Icons.ID? title = null, Box_Options options = Box_Options.Default) => Create<Small>(data, x, y, title, options);

        public override void Show()
        {
            timeelapsed = 0;
            base.Show();
        }

        public override bool Update()
        {
            if (timeelapsed < maxtime)
            {
                timeelapsed += Memory.gameTime.ElapsedGameTime.TotalMilliseconds;
                return base.Update();
            }
            else
            { Hide(); }
            return false;
        }

        #endregion Methods
    }
}