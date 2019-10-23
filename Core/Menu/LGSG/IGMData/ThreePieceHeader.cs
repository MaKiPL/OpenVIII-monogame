using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData
{
    public class ThreePieceHeader : IGMData.Base
    {
        #region Properties

        protected IGMDataItem.Box HELP { get => (IGMDataItem.Box)ITEM[2, 0]; set => ITEM[2, 0] = value; }

        protected IGMDataItem.Box TOPLeft { get => (IGMDataItem.Box)ITEM[0, 0]; set => ITEM[0, 0] = value; }

        protected IGMDataItem.Box TOPRight { get => (IGMDataItem.Box)ITEM[1, 0]; set => ITEM[1, 0] = value; }

        #endregion Properties

        #region Methods

        public static ThreePieceHeader Create(FF8String topleft, FF8String topright, FF8String help, Rectangle? pos=null)
        {
            ThreePieceHeader r = Create<ThreePieceHeader>(3, 1,new IGMDataItem.Empty{ Pos = pos ?? Rectangle.Empty });
            r.TOPLeft.Data = topleft;
            r.TOPRight.Data = topright;
            r.HELP.Data = help;
            r.TOPLeft.Show();
            r.TOPRight.Show();
            r.HELP.Show();
            return r;
        }

        public void Refresh(FF8String topleft, FF8String topright, FF8String help)
        {
            TOPLeft.Data = topleft;
            TOPRight.Data = topright;
            HELP.Data = help;
            Refresh();
        }

        protected override void Init()
        {

            base.Init();
            int space = 4;

            int widthright = (int)(base.Width * 0.18f) - space;
            widthright = widthright - widthright % 4;
            int widthleft = Width - widthright -space;
            TOPRight = new IGMDataItem.Box { Pos = new Rectangle(widthleft + space, 0, widthright, (base.Height - space) / 2) };
            TOPLeft = new IGMDataItem.Box { Pos = new Rectangle(0, 0, widthleft, TOPRight.Height) };
            HELP = new IGMDataItem.Box { Pos = new Rectangle((int)(Width * 0.03f), TOPLeft.Height + space, (int)(Width * 0.94f), TOPLeft.Height), Title = Icons.ID.HELP };
        }

        #endregion Methods
    }
}