using Microsoft.Xna.Framework;

namespace OpenVIII.IGMData.Dialog
{
    public abstract class Confirm : Base
    {
        #region Fields

        protected FF8String[] opt;
        protected int startcursor;

        #endregion Fields

        #region Constructors

        public Confirm(FF8String data, Icons.ID title, FF8String opt1, FF8String opt2, Rectangle? pos, int startcursor = 0) : base(2, 1, new IGMDataItem.Box(data, pos, title), 1, 2)
        {
            this.startcursor = startcursor;
            opt = new FF8String[Count];
            opt[0] = opt1;
            opt[1] = opt2;
            ITEM[0, 0] = new IGMDataItem.Text(opt[0], SIZE[0]);
            ITEM[1, 0] = new IGMDataItem.Text(opt[1], SIZE[1]);
        }

        #endregion Constructors

        #region Methods

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
            SetSize();
            base.Init();
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