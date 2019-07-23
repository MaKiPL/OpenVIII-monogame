using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_IGMData : IGMDataItem<IGMData>
    {
        #region Constructors

        public IGMDataItem_IGMData(IGMData data, Rectangle? pos = null) : base(data, pos)
        {
        }

        #endregion Constructors

        #region Properties

        public override Rectangle Pos
        {
            get => Data.CONTAINER == null ? base.Pos : Data.CONTAINER.Pos;
            set
            {
                base.Pos = value;
                if (Data?.CONTAINER != null)
                    Data.CONTAINER.Pos = value;
            }
        }

        #endregion Properties

        #region Methods

        public override void Draw()
        {
            if (Enabled)
                Data.Draw();
        }

        public override void Hide()
        {
            base.Hide();
            Data.Hide();
        }

        public override bool Inputs()
        {
            if (Enabled)
            {
                bool ret = base.Inputs();
                return Data.Inputs() || ret;
            }
            return false;
        }

        public override void Refresh()
        {
            base.Refresh();
            Data.Refresh();
        }

        public override void Show()
        {
            base.Show();
            Data.Show();
        }

        public override bool Update()
        {
            if (Enabled)
            {
                bool ret = base.Update();
                return Data.Update() || ret;
            }
            return false;
        }

        #endregion Methods
    }
}