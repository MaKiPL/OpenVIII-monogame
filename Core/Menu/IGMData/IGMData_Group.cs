using System.Diagnostics;

namespace OpenVIII
{
    public class IGMData_Group : IGMData
    {
        #region Constructors

        public IGMData_Group(params IGMData[] d) : base(d.Length, 1, container: new IGMDataItem_Empty()) => Init(d);

        public IGMData_Group() : base(container: new IGMDataItem_Empty()) => Debug.WriteLine($"{this} :: Not init may need to call it later");

        protected virtual void Init(IGMData[] d, bool baseinit = false)
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

        public override void Hide()
        {
            if (Enabled)
            {
                base.Hide();
                //maybe overkill to run hide on items. if group is hidden it won't draw.
                //if (!skipdata)
                //{
                //    int pos = 0;
                //    foreach (IGMDataItem i in ITEM)
                //    {
                //        if (i != null)
                //            ITEMHide((IGMData)i, pos++);
                //    }
                //}
            }
        }

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
                        ret = ITEMInputs((IGMData)i, pos++);
                        if (ret) return ret;
                    }
                }
                ret = base.Inputs();
            }
            return ret;
        }

        public virtual void ITEMHide(IGMData i, int pos = 0) => i.Hide();

        public virtual bool ITEMInputs(IGMData i, int pos = 0) => i.Inputs();

        public virtual void ITEMShow(IGMData i, int pos = 0) => i.Show();

        public virtual bool ITEMUpdate(IGMData i, int pos = 0) => i.Update();

        protected override void RefreshChild()
        {
            if (!skipdata)
                foreach (Menu_Base i in ITEM)
                    i?.Refresh(Character, VisableCharacter);
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
                        ITEMShow((IGMData)i, pos++);
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
                            ret = ITEMUpdate((IGMData)i, pos++) || ret;
                    }
                }
                return ret;
            }
            return false;
        }

        #endregion Methods
    }
}