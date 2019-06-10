namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        public class IGMData_Group : IGMData
        {
            public IGMData_Group( params IGMData[] d) : base(d.Length, 1)
            {
                for (int i = 0; i < d.Length; i++)
                {
                    ITEM[i, 0] = d[i];
                }
            }
            public virtual void ITEMHide(IGMDataItem i, int pos=0)
            {
                i.Hide();
            }
            public override void Hide()
            {
                if (Enabled)
                {
                    base.Hide();
                    if (!skipdata)
                    {
                        int pos = 0;
                        foreach (IGMDataItem i in ITEM)
                        {
                           ITEMHide(i, pos++);
                        }
                    }
                }
            }
            public virtual void ITEMShow(IGMDataItem i, int pos = 0)
            {
                i.Show();
            }
            public override void Show()
            {
                base.Show();
                if (!skipdata)
                {
                    int pos = 0;
                    foreach (IGMDataItem i in ITEM)
                    {
                        ITEMShow(i, pos++);
                    }
                }             
            }

            public int cnv(int pos) => pos / Depth;
            public int deep(int pos) => pos % Depth;
            public virtual bool ITEMInputs(IGMDataItem i, int pos = 0)
            {
                return i.Inputs();
            }
            public override bool Inputs()
            {
                if (Enabled)
                {
                    bool ret = base.Inputs();
                    if (!skipdata)
                    {
                        int pos = 0;
                        foreach (IGMDataItem i in ITEM)
                        {
                            ret = ITEMInputs(i,pos++) || ret;
                        }
                    }
                    return ret;
                }
                return false;
            }
            public virtual void ITEMReInit(IGMDataItem i, int pos = 0)
            {
                i.ReInit();
            }
            public override void ReInit()
            {
                base.ReInit();
                if (!skipdata)
                {
                    int pos = 0;
                    foreach (var i in ITEM)
                    {
                        if (i != null)
                            ITEMReInit(i, pos);
                    }
                }
            }
            public virtual bool ITEMUpdate(IGMDataItem i, int pos = 0)
            {
                return i.Inputs();
            }
            public override bool Update()
            {
                if (Enabled)
                {
                    bool ret = base.Update();
                    if (!skipdata)
                    {
                        int pos = 0;
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
        }
        #endregion Classes
    }
}