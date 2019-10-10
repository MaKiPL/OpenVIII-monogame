using System.Diagnostics;

namespace OpenVIII.IGMData.Group
{
    public class Base : IGMData.Base
    {
        #region Constructors

        public Base(params IGMData.Base[] d) : base(d.Length, 1, container: new IGMDataItem.Empty()) => Init(d);

        public Base() : base(container: new IGMDataItem.Empty()) => Debug.WriteLine($"{this} :: Not init may need to call it later");

        protected virtual void Init(IGMData.Base[] d, bool baseinit = false)
        {
            if (baseinit)
                Init(d.Length, 1);
            for (int i = 0; i < d.Length; i++)
            {
                ITEM[i, 0] = d[i];
            }
        }

        #endregion Constructors

        #region Methods

        public int cnv(int pos) => pos / Depth;

        public int deep(int pos) => pos % Depth;


        public override bool Inputs()
        {
            bool ret = false;
            if (Enabled)
            {
                if (!skipdata)
                {
                    int pos = 0;
                    foreach (Menu_Base i in ITEM)
                    {
                        ret = ITEMInputs((IGMData.Base)i, pos++);
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

        protected override void RefreshChild()
        {
            if (!skipdata)
                foreach (Menu_Base i in ITEM)
                    i?.Refresh(Damageable);
        }

        public override void Show()
        {
            base.Show();
            if (!skipdata)
            {
                int pos = 0;
                foreach (Menu_Base i in ITEM)
                {
                    if (i != null)
                        ITEMShow((IGMData.Base)i, pos++);
                }
            }
        }

        public override bool Update()
        {
            if (Enabled)
            {
                bool ret = base.Update();
                if (!skipdata)
                {
                    int pos = 0;
                    foreach (Menu_Base i in ITEM)
                    {
                        if (i != null)
                            ret = ITEMUpdate((IGMData.Base)i, pos++) || ret;
                    }
                }
                return ret;
            }
            return false;
        }

        public override void Reset()
        {
            if (Enabled)
            {
                base.Reset();
                if (!skipdata)
                {
                    foreach (Menu_Base i in ITEM)
                    {
                        i?.Reset();
                    }
                }
            }
        }

        #endregion Methods
    }
}