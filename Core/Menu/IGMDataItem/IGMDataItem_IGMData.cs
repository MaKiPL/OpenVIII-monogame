using Microsoft.Xna.Framework;

namespace OpenVIII
{
    #region Classes

    public class IGMDataItem_IGMData : IGMDataItem
    {
        public IGMData Data { get; set; }

        public IGMDataItem_IGMData(IGMData data, Rectangle? pos = null) : base(pos) => Data = data;

        public override void Draw()
        {
            if (Enabled)
                Data.Draw();
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

        public override bool Inputs()
        {
            if (Enabled)
            {
                bool ret = base.Inputs();
                return Data.Inputs() || ret;
            }
            return false;
        }

        public override void Hide()
        {
            base.Hide();
            Data.Hide();
        }

        public override void Show()
        {
            base.Show();
            Data.Show();
        }

        public override Rectangle Pos
        {
            get => Data.CONTAINER == null ? base.Pos : Data.CONTAINER.Pos;
            set
            {
                base.Pos = value;
                if (Data.CONTAINER != null)
                    Data.CONTAINER.Pos = value;
            }
        }

        public override void ReInit()
        {
            base.ReInit();
            Data.ReInit();
        }
    }

    #endregion Classes
}