namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Mag_Group : IGMData_Group
        {
            #region Constructors

            public IGMData_Mag_Group(params IGMData[] d) : base(d)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Hide() =>
                //depending on the mode it'll hide what's needed and show rest.
                Show();

            public override bool ITEMInputs(IGMDataItem_IGMData i, int pos = 0)
            {
                bool ret = false;
                if (InputsModeTest(pos))
                {
                    Mode lastmode = (Mode)IGM_Junction.GetMode();
                    ret = base.ITEMInputs(i, pos);
                    if (ret)
                    {
                        if (!IGM_Junction.GetMode().Equals(lastmode))
                            Show();
                    }
                }
                return ret;
            }

            public override void ITEMShow(IGMDataItem_IGMData i, int pos = 0)
            {
                if (IGM_Junction != null)
                {
                    pos = cnv(pos);
                    switch (IGM_Junction.GetMode())
                    {
                        default:
                            if (pos < 1)
                                base.ITEMShow(i, pos);
                            else base.ITEMHide(i, pos);
                            break;

                        case Mode.Mag_Pool_Stat:
                        case Mode.Mag_Stat:
                            if (pos < 3)
                                base.ITEMShow(i, pos);
                            else base.ITEMHide(i, pos);
                            break;

                        case Mode.Mag_EL_A:
                        case Mode.Mag_Pool_EL_A:
                            if (pos > 0 && pos < 5)
                                base.ITEMShow(i, pos);
                            else base.ITEMHide(i, pos);
                            break;

                        case Mode.Mag_EL_D:
                        case Mode.Mag_Pool_EL_D:
                            if (pos > 0 && pos < 4 || pos == 5)
                                base.ITEMShow(i, pos);
                            else base.ITEMHide(i, pos);
                            break;

                        case Mode.Mag_ST_A:
                        case Mode.Mag_Pool_ST_A:
                            if (pos > 0 && pos < 3 || pos == 6 || pos == 7)
                                base.ITEMShow(i, pos);
                            else base.ITEMHide(i, pos);
                            break;

                        case Mode.Mag_ST_D:
                        case Mode.Mag_Pool_ST_D:
                            if (pos > 0 && pos < 3 || pos == 6 || pos == 8)
                                base.ITEMShow(i, pos);
                            else base.ITEMHide(i, pos);
                            break;
                    }
                }
            }

            public override void Refresh()
            {
                base.Refresh();
                Show();
            }

            private bool InputsModeTest(int pos)
            {
                pos = cnv(pos);
                switch (IGM_Junction.GetMode())
                {
                    case Mode.Mag_Pool_Stat:
                    case Mode.Mag_Pool_EL_A:
                    case Mode.Mag_Pool_EL_D:
                    case Mode.Mag_Pool_ST_A:
                    case Mode.Mag_Pool_ST_D:
                        if (pos == 2)
                            return true;
                        break;

                    case Mode.Mag_Stat:
                        if (pos == 0)
                            return true;
                        break;

                    case Mode.Mag_EL_A:
                    case Mode.Mag_EL_D:
                        if (pos == 3)
                            return true;
                        break;

                    case Mode.Mag_ST_A:
                    case Mode.Mag_ST_D:
                        if (pos == 6)
                            return true;
                        break;
                }
                return false;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}