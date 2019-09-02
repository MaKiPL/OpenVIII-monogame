using System;
using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_IGMData : IGMDataItem, I_Data<IGMData>
    {
        #region Constructors

        public IGMDataItem_IGMData(IGMData data, Rectangle? pos = null) : base(pos) => Data = data;

        #endregion Constructors

        #region Properties

        public IGMData Data { get; set; }
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
        protected override void RefreshChild()
        {
            Data.Refresh(Character,VisableCharacter);
            base.RefreshChild();
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