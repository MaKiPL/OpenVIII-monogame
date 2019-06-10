namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_Group : IGMData_Group
            {
                public IGMData_Mag_Group(params IGMData[] d) : base(d) { }
                public override void ReInit()
                {
                    base.ReInit();
                    Show();
                }

                public override void ITEMShow(IGMDataItem i, int pos = 0)
                {
                    if (InGameMenu_Junction != null)
                    {
                        pos = cnv(pos);
                        switch (InGameMenu_Junction.GetMode())
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
                private bool InputsModeTest(int pos)
                {
                    pos = cnv(pos);
                    switch (InGameMenu_Junction.GetMode())
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
                public override void Hide()
                {
                    //depending on the mode it'll hide what's needed and show rest.
                    Show();
                }

                public override bool ITEMInputs(IGMDataItem i, int pos = 0)
                {
                    bool ret = false;
                    if (InputsModeTest(pos))
                    {
                        Mode lastmode = InGameMenu_Junction.GetMode();
                        ret = base.ITEMInputs(i, pos);
                        if (ret)
                        {
                            if (lastmode != InGameMenu_Junction.GetMode())
                                Show();
                        }
                    }
                    return ret;
                }
            }
        }
    }
}