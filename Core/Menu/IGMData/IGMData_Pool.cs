namespace OpenVIII
{
    public abstract class IGMData_Pool<T, T2> : IGMData
    {
        #region Constructors

        public IGMData_Pool(int count, int depth, IGMDataItem container = null, int? rows = null, int? pages = null, Characters character= Characters.Blank, Characters? visablecharacter=null) : base(count + 2, depth, container, 1, rows,character, visablecharacter) => DefaultPages = pages ?? 1;

        #endregion Constructors

        #region Properties

        public T2[] Contents { get; private set; }
        public int DefaultPages { get; private set; }
        public int Page { get; protected set; }
        public int Pages { get; protected set; }
        protected T Source { get; set; }

        #endregion Properties

        #region Methods

        public override bool Inputs()
        {
            bool ret = false;
            if (Pages > 1 && CONTAINER.Pos.Contains(InputMouse.Location.Transform(Menu.Focus)))
            {
                if (Input2.DelayedButton(MouseButtons.MouseWheelup))
                {
                    Inputs_Left();
                    ret = true;
                }
                else if (Input2.DelayedButton(MouseButtons.MouseWheeldown))
                {
                    Inputs_Right();
                    ret = true;
                }
            }
            if (!ret)
                ret = base.Inputs();
            return ret;
        }

        public override void Inputs_Left()
        {
            if (Pages > 1)
            {
                base.Inputs_Left(); PAGE_PREV();
            }
        }

        public override void Inputs_Right()
        {
            if (Pages > 1)
            {
                base.Inputs_Right(); PAGE_NEXT();
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            ResetPages();
            if (Pages > 1)
            {
                Cursor_Status &= ~Cursor_Status.Horizontal;
                ITEM[Count - 1, 0]?.Show();
                ITEM[Count - 2, 0]?.Show();
            }
            else
            {
                Cursor_Status |= Cursor_Status.Horizontal;
                ITEM[Count - 1, 0]?.Hide();
                ITEM[Count - 2, 0]?.Hide();
            }
        }

        public virtual void ResetPages() =>
                Pages = DefaultPages;

        public virtual void UpdateTitle()
        {
        }

        protected override void Init()
        {
            base.Init();
            Cursor_Status |= Cursor_Status.Enabled;
            Cursor_Status |= Cursor_Status.Vertical;
            Page = 0;
            Contents = new T2[rows];
            SIZE[Count - 2].X = X + 6;
            SIZE[Count - 2].Y = Y + Height - 28;
            SIZE[Count - 1].X = X + Width - 24;
            SIZE[Count - 1].Y = Y + Height - 28;
            ITEM[Count - 2, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Left, SIZE[Count - 2], 2, 7) { Blink = true };
            ITEM[Count - 1, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Right2, SIZE[Count - 1], 2, 7) { Blink = true };
        }

        protected virtual void PAGE_NEXT()
        {
            Page++;
            if (Page >= Pages)
                Page = 0;
        }

        protected virtual void PAGE_PREV()
        {
            Page--;
            if (Page < 0)
                Page = Pages - 1;
        }

        #endregion Methods
    }
}