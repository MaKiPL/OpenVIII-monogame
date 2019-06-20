namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        public abstract class IGMData_Pool<T, T2> : IGMData
        {
            public IGMData_Pool( int count, int depth, IGMDataItem container = null, int? rows = null, int? pages = null) : base(count + 2, depth, container, 1, rows)
            {
                DefaultPages = pages ?? 1;
            }

            public int DefaultPages { get; private set; }
            public int Pages { get; protected set; }
            public int Page { get; protected set; }
            public T2[] Contents { get; private set; }
            protected T Source { get; set; }

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
            }

            public override void ReInit()
            {
                base.ReInit();
                Pages = DefaultPages;
                ITEM[Count - 2, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Left, SIZE[Count - 2], 2, 7);
                ITEM[Count - 1, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Right2, SIZE[Count - 1], 2, 7);
            }

            public override bool Inputs()
            {
                bool ret = base.Inputs();
                if (Pages > 1 && CONTAINER.Pos.Contains(Input.MouseLocation.Transform(Menu.Focus)))
                {
                    if (Input.Button(Buttons.MouseWheelup))
                    {
                        Inputs_Left();
                        ret = true;
                    }
                    else if (Input.Button(Buttons.MouseWheeldown))
                    {
                        Inputs_Right();
                        ret = true;
                    }
                }
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

            public virtual void UpdateTitle()
            {
            }
        }
    }
}