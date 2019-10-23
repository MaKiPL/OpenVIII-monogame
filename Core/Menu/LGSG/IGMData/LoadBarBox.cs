using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData
{
    public class LoadBarBox : IGMData.Base
    {
        #region Methods

        public LoadBarBox Create(Rectangle pos) => Create<LoadBarBox>(2, 1, container: new IGMDataItem.Box { Pos = pos, Title = Icons.ID.INFO });

        protected override void Init()
        {
            base.Init();
            int height = 8;
            int y = Height / 2 - height / 2;
            ITEM[0, 0] = new IGMDataItem.Icon
            {
                Data = Icons.ID.Bar_BG,
                Pos = new Rectangle((int)(0.05f * Width), y, (int)(Width * .9f), height)
            };
            ITEM[1, 0] = new IGMDataItem.Icon { Data = Icons.ID.Bar_Fill, Pos = ITEM[0, 0].Pos };
        }

        #endregion Methods
    }
}