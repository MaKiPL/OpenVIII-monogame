namespace OpenVIII.IGMData.Pool
{
    public abstract class Base<T, T2> : IGMData.Base
    {
        #region Constructors

        protected Base() => ExtraCount = 2;

        #endregion Constructors

        #region Properties

        public T2[] Contents { get; set; }

        public int DefaultPages { get; protected set; }

        public int Page { get; protected set; }

        public int Pages { get; private set; }

        protected Menu_Base LeftArrow
        {
            get => ITEM[Count - 2, 0];
            private set => SIZE[Count - 2] = ITEM[Count - 2, 0] = value;
        }

        protected Menu_Base RightArrow
        {
            get => ITEM[Count - 1, 0];
            private set => SIZE[Count - 1] = ITEM[Count - 1, 0] = value;
        }

        protected T Source { get; set; }

        #endregion Properties

        #region Methods

        public static J Create<J>(int count, int depth, Menu_Base container = null, int? rows = null, int? pages = null, Damageable damageable = null)
                                                                    where J : Base<T, T2>, new()
        {
            J r = IGMData.Base.Create<J>(count, depth, container, 1, rows, damageable);
            r.DefaultPages = pages ?? 1;
            return r;
        }

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
                base.Inputs_Left();
                PAGE_PREV();
            }
        }

        public override void Inputs_Right()
        {
            if (Pages > 1)
            {
                base.Inputs_Right();
                PAGE_NEXT();
            }
        }

        public void PagesOne()
        {
            Pages = 1;
            Cursor_Status |= Cursor_Status.Horizontal;
            RightArrow?.Hide();
            LeftArrow?.Hide();
        }

        public override void Refresh()
        {
            base.Refresh();
            ResetPages();
            if (Pages <= 1)
                PagesOne();
        }

        public virtual void ResetPages()
        {
            Pages = DefaultPages;
            Cursor_Status &= ~Cursor_Status.Horizontal;
            RightArrow?.Show();
            LeftArrow?.Show();
        }

        public virtual void UpdateTitle()
        {
        }

        protected override void Init()
        {
            base.Init();
            Cursor_Status |= (Cursor_Status.Enabled | Cursor_Status.Vertical);
            Page = 0;
            Contents = new T2[Rows];
            LeftArrow = new IGMDataItem.Icon { Data = Icons.ID.Arrow_Left, X = X + 6, Y = Y + Height - 28, Palette = 2, Faded_Palette = 7, Blink = true };
            RightArrow = new IGMDataItem.Icon { Data = Icons.ID.Arrow_Right2, X = X + Width - 24, Y = Y + Height - 28, Palette = 2, Faded_Palette = 7, Blink = true };
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