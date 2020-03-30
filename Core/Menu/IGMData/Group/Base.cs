namespace OpenVIII.IGMData.Group
{
    public class Base : IGMData.Base
    {
        //private Base(params Menu_Base[] d) : base(d.Length, 1, container: new IGMDataItem.Empty()) => Init(d);

        #region Constructors

        public Base()
        {
        }

        #endregion Constructors

        #region Methods

        //:base(container: new IGMDataItem.Empty()) => Debug.WriteLine($"{this} :: Not init may need to call it later");
        public static T Create<T>(params Menu_Base[] d) where T : Base, new()
        {
            var r = Create<T>();
            r.Count = checked((byte)d.Length);
            r.Depth = 1;
            r.Init(r.Count, r.Depth, r.CONTAINER);
            r.Init(d);
            return r;
        }

        public static Base Create(params Menu_Base[] d) => Create<Base>(d);

        public static Base Create() => Create<Base>();

        public static T Create<T>() where T : Base, new()
        {
            var r = new T
            {
                CONTAINER = new IGMDataItem.Empty()
            };
            return r;
        }

        public int cnv(int pos) => pos / Depth;

        public int deep(int pos) => pos % Depth;

        public override bool Inputs()
        {
            var ret = false;
            if (Enabled)
            {
                if (!skipdata)
                {
                    var pos = 0;
                    foreach (var i in ITEM)
                    {
                        ret = ITEMInputs(i, pos++);
                        if (ret) return ret;
                    }
                }
                ret = base.Inputs();
            }
            return ret;
        }

        public virtual void ITEMHide(Menu_Base i, int pos = 0) => i.Hide();

        public virtual bool ITEMInputs(Menu_Base i, int pos = 0) => i.Inputs();

        public virtual void ITEMShow(Menu_Base i, int pos = 0) => i.Show();

        public virtual bool ITEMUpdate(Menu_Base i, int pos = 0) => i.Update();

        public override void Reset()
        {
            if (Enabled)
            {
                base.Reset();
                if (!skipdata)
                {
                    foreach (var i in ITEM)
                    {
                        i?.Reset();
                    }
                }
            }
        }

        public override void Show()
        {
            base.Show();
            if (!skipdata)
            {
                var pos = 0;
                foreach (var i in ITEM)
                {
                    if (i != null)
                        ITEMShow(i, pos++);
                }
            }
        }

        public override bool Update()
        {
            if (Enabled)
            {
                var ret = base.Update();
                if (!skipdata)
                {
                    var pos = 0;
                    foreach (var i in ITEM)
                    {
                        if (i != null)
                            ret = ITEMUpdate(i, pos++) || ret;
                    }
                }
                return ret;
            }
            return false;
        }

        protected virtual void Init(Menu_Base[] d, bool baseinit = false)
        {
            if (baseinit)
                Init(d.Length, 1);
            for (var i = 0; i < d.Length; i++)
            {
                ITEM[i, 0] = d[i];
            }
        }

        protected override void RefreshChild()
        {
            if (!skipdata)
                foreach (var i in ITEM)
                {
                    if (ForceNullDamageable && i!=null)
                        i.ForceNullDamageable = ForceNullDamageable;
                    i?.Refresh(Damageable);
                }
        }

        #endregion Methods
    }
}