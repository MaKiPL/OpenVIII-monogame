using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.IGMData
{
    public class ThreePieceHeader : IGMData.Base
    {
        #region Properties

        public bool Save { get; private set; }

        protected IGMDataItem.Box HELP { get => (IGMDataItem.Box)ITEM[2, 0]; set => ITEM[2, 0] = value; }

        protected IGMDataItem.Box TOPLeft { get => (IGMDataItem.Box)ITEM[0, 0]; set => ITEM[0, 0] = value; }

        protected IGMDataItem.Box TOPRight { get => (IGMDataItem.Box)ITEM[1, 0]; set => ITEM[1, 0] = value; }

        #endregion Properties

        #region Methods

        public static ThreePieceHeader Create(Rectangle pos) => Create(null, null, null, pos);

        public static ThreePieceHeader Create(FF8String topleft, FF8String topright, FF8String help, Rectangle pos)
        {
            ThreePieceHeader r = Create<ThreePieceHeader>(3, 1, new IGMDataItem.Empty { Pos = pos });
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
            SkipSIZE = true;
            base.Init();
            
            TOPRight = new IGMDataItem.Box { };
            TOPLeft = new IGMDataItem.Box { Title = Icons.ID.INFO, Options = Box_Options.Indent };
            HELP = new IGMDataItem.Box { Title = Icons.ID.HELP };
        }
        bool UpdateSize()
        {
            if (CONTAINER.X != ScreenTopLeft.X || TOPRight.Pos.Equals(Rectangle.Empty))
            {
                CONTAINER.X = ScreenTopLeft.X;
                CONTAINER.Width = ScreenTopRight.X - ScreenTopLeft.X;
                InitSize(true);
                int space = IGM_LGSG.space;
                int widthright = (int)(base.Width * 0.18f) - space;
                widthright = widthright - widthright % 4;
                int widthleft = Width - widthright - space;
                TOPRight.Pos = new Rectangle(widthleft + space+X, 0, widthright, (base.Height - space) / 2);
                TOPLeft.Pos = new Rectangle(X, 0, widthleft, TOPRight.Height);
                HELP.Pos = new Rectangle((int)(Width * 0.03f)+X, TOPLeft.Height + space, (int)(Width * 0.94f), TOPLeft.Height);
                return true;
            }
            return false;
        }
        public override bool Update()
        {
            var r = base.Update();
            r = r || UpdateSize();
            return r;
        }

        public override void ModeChangeEvent(object sender, Enum e)
        {
            base.ModeChangeEvent(sender, e);
            if (e.GetType() == typeof(IGM_LGSG.Mode))
            {
                Save = e.HasFlag(IGM_LGSG.Mode.Save);
                TOPRight.Data = Save ? Strings.Name.Save : Strings.Name.Load;

                if (e.HasFlag(IGM_LGSG.Mode.Slot1))
                    TOPLeft.Data = Strings.Name.GameFolderSlot1;
                else if (e.HasFlag(IGM_LGSG.Mode.Slot2))
                    TOPLeft.Data = Strings.Name.GameFolderSlot2;
                else
                    TOPLeft.Data = Strings.Name.GameFolder;

                if (e.HasFlag(IGM_LGSG.Mode.Slot) && e.HasFlag(IGM_LGSG.Mode.Choose))
                {
                    HELP.Data = Save ? Strings.Name.SaveFF8 : Strings.Name.LoadFF8;
                }
                else if (e.HasFlag(IGM_LGSG.Mode.Slot) && e.HasFlag(IGM_LGSG.Mode.Checking))
                {
                    HELP.Data = Strings.Name.CheckGameFolder;
                }
                else if (e.HasFlag(IGM_LGSG.Mode.Game) && e.HasFlag(IGM_LGSG.Mode.Choose))
                {
                    HELP.Data = Save ? Strings.Name.BlockToSave : Strings.Name.BlockToLoad;
                }
                else if (e.HasFlag(IGM_LGSG.Mode.Game) && e.HasFlag(IGM_LGSG.Mode.Checking))
                {
                    HELP.Data = Save ? Strings.Name.Saving : Strings.Name.Loading;
                }
            }
        }

        #endregion Methods
    }
}