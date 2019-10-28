using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData.Dialog
{
    public abstract class Confirm : Base, I_Data<FF8String>
    {
        #region Fields

        protected FF8String[] opt;
        protected int startcursor;

        #endregion Fields

        #region Properties

        public FF8String Data { get => ((I_Data<FF8String>)CONTAINER).Data; set => ((I_Data<FF8String>)CONTAINER).Data = value; }

        #endregion Properties

        #region Methods

        public static T Create<T>(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle? pos, int startcursor = 0) where T : Confirm, new()
        {
            T r = Base.Create<T>(2, 1, new IGMDataItem.Box { Data = data, Pos = pos ?? Rectangle.Empty, Title = title }, 1, 2);
            r.startcursor = startcursor;
            r.opt = new FF8String[r.Count];
            r.opt[0] = opt1;
            r.opt[1] = opt2;
            r.ITEM[0, 0] = new IGMDataItem.Text { Data = r.opt[0], Pos = r.SIZE[0] };
            r.ITEM[1, 0] = new IGMDataItem.Text { Data = r.opt[1], Pos = r.SIZE[1] };
            return r;
        }

        public override void Refresh()
        {
            base.Refresh();
            CURSOR_SELECT = startcursor;
            Cursor_Status |= Cursor_Status.Enabled;
            Cursor_Status |= Cursor_Status.Vertical;
            Cursor_Status |= Cursor_Status.Horizontal;
        }

        protected override void Init()
        {
            SkipSIZE = true;
            base.Init();
            SetSize();
            InitSize();
            Hide();
        }

        protected virtual void SetSize()
        {
            SIZE[0] = new Rectangle(212 + X, 117 + Y, 52, 30);
            SIZE[1] = new Rectangle(212 + X, 156 + Y, 52, 30);
        }

        #endregion Methods
    }
}