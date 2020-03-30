using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.IGMData
{
    public class ThreePieceHeader : Base
    {
        #region Properties

        public bool Save { get; private set; }

        protected IGMDataItem.Box Help { get => (IGMDataItem.Box)ITEM[2, 0]; set => ITEM[2, 0] = value; }

        protected IGMDataItem.Box TopLeft { get => (IGMDataItem.Box)ITEM[0, 0]; set => ITEM[0, 0] = value; }

        protected IGMDataItem.Box TopRight { get => (IGMDataItem.Box)ITEM[1, 0]; set => ITEM[1, 0] = value; }

        #endregion Properties

        #region Methods

        public static ThreePieceHeader Create(Rectangle pos) => Create(null, null, null, pos);

        public static ThreePieceHeader Create(FF8String topLeft, FF8String topRight, FF8String help, Rectangle pos)
        {
            var r = Create<ThreePieceHeader>(3, 1, new IGMDataItem.Empty { Pos = pos });
            r.TopLeft.Data = topLeft;
            r.TopRight.Data = topRight;
            r.Help.Data = help;
            r.TopLeft.Show();
            r.TopRight.Show();
            r.Help.Show();
            return r;
        }

        public override void ModeChangeEvent(object sender, Enum e)
        {
            base.ModeChangeEvent(sender, e);
            if (e.GetType() == typeof(IGMLoadSaveGame.Mode))
            {
                Save = e.HasFlag(IGMLoadSaveGame.Mode.Save);
                TopRight.Data = Save ? Strings.Name.Save : Strings.Name.Load;

                if (e.HasFlag(IGMLoadSaveGame.Mode.Slot1))
                    TopLeft.Data = Strings.Name.GameFolderSlot1;
                else if (e.HasFlag(IGMLoadSaveGame.Mode.Slot2))
                    TopLeft.Data = Strings.Name.GameFolderSlot2;
                else
                    TopLeft.Data = Strings.Name.GameFolder;

                if (e.HasFlag(IGMLoadSaveGame.Mode.Slot) && e.HasFlag(IGMLoadSaveGame.Mode.Choose))
                {
                    Help.Data = Save ? Strings.Name.SaveFF8 : Strings.Name.LoadFF8;
                }
                else if (e.HasFlag(IGMLoadSaveGame.Mode.Slot) && e.HasFlag(IGMLoadSaveGame.Mode.Checking))
                {
                    Help.Data = Strings.Name.CheckGameFolder;
                }
                else if (e.HasFlag(IGMLoadSaveGame.Mode.Game) && e.HasFlag(IGMLoadSaveGame.Mode.Choose))
                {
                    Help.Data = Save ? Strings.Name.BlockToSave : Strings.Name.BlockToLoad;
                }
                else if (e.HasFlag(IGMLoadSaveGame.Mode.Game) && e.HasFlag(IGMLoadSaveGame.Mode.Checking))
                {
                    Help.Data = Save ? Strings.Name.Saving : Strings.Name.Loading;
                }
            }
        }

        public void Refresh(FF8String topLeft, FF8String topRight, FF8String help)
        {
            TopLeft.Data = topLeft;
            TopRight.Data = topRight;
            Help.Data = help;
            Refresh();
        }

        public override bool Update()
        {
            var r = base.Update();
            r = r || UpdateSize();
            return r;
        }

        protected override void Init()
        {
            SkipSIZE = true;
            base.Init();

            TopRight = new IGMDataItem.Box();
            TopLeft = new IGMDataItem.Box { Title = Icons.ID.INFO, Options = Box_Options.Indent };
            Help = new IGMDataItem.Box { Title = Icons.ID.HELP };
        }

        private bool UpdateSize()
        {
            if (CONTAINER.X == ScreenTopLeft.X && !TopRight.Pos.Equals(Rectangle.Empty)) return false;
            CONTAINER.X = ScreenTopLeft.X;
            CONTAINER.Width = ScreenTopRight.X - ScreenTopLeft.X;
            InitSize(true);
            const int space = IGMLoadSaveGame.Space;
            var widthRight = (int)(base.Width * 0.18f) - space;
            widthRight -= widthRight % 4;
            var widthLeft = Width - widthRight - space;
            TopRight.Pos = new Rectangle(widthLeft + space + X, 0, widthRight, (base.Height - space) / 2);
            TopLeft.Pos = new Rectangle(X, 0, widthLeft, TopRight.Height);
            Help.Pos = new Rectangle((int)(Width * 0.03f) + X, TopLeft.Height + space, (int)(Width * 0.94f), TopLeft.Height);
            return true;
        }

        #endregion Methods
    }
}