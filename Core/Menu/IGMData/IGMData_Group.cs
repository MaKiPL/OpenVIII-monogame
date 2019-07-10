namespace OpenVIII
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
            public virtual void ITEMHide(IGMDataItem_IGMData i, int pos=0)
            {
                i.Hide();
            }
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
                //            ITEMHide((IGMDataItem_IGMData)i, pos++);
                //    }
                //}
            }
        }

        public virtual void ITEMShow(IGMDataItem_IGMData i, int pos = 0)
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
                        if (i != null)
                            ITEMShow((IGMDataItem_IGMData)i, pos++);
                    }
                }             
            }

            public int cnv(int pos) => pos / Depth;
            public int deep(int pos) => pos % Depth;
            public virtual bool ITEMInputs(IGMDataItem_IGMData i, int pos = 0)
            {
                return i.Inputs();
            }
            public override bool Inputs()
            {
                bool ret = false;
                if (Enabled)
                {
                    if (!skipdata)
                    {
                        int pos = 0;
                        foreach (IGMDataItem i in ITEM)
                        {
                            ret = ITEMInputs((IGMDataItem_IGMData)i, pos++);
                            if (ret) return ret;
                        }
                    }
                    ret = base.Inputs();
                }
                return ret;
            }
            public virtual void ITEMReInit(IGMDataItem_IGMData i, int pos = 0)
            {
                i.Data.ReInit(Character,VisableCharacter);
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
                            ITEMReInit((IGMDataItem_IGMData)i, pos);
                    }
                }
            }
            public virtual bool ITEMUpdate(IGMDataItem_IGMData i, int pos = 0)
            {
                return i.Update();
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
                                ret = ITEMUpdate((IGMDataItem_IGMData)i, pos++) || ret;
                        }
                    }
                    return ret;
                }
                return false;
            }
        }
        #endregion Classes
    }
