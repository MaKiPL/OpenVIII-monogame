namespace FF8
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_Mag_Group : IGMData_Group
            {
                public IGMData_Mag_Group(params IGMData[] d) : base( d) => Hide();

                public override void Show()
                {
                    for (int i = 0; i < Count - 4 && ITEM[i, 0] != null; i++)
                    {
                        ((IGMDataItem_IGMData)ITEM[i, 0]).Data.Show();
                    }

                    if (InGameMenu_Junction != null && InGameMenu_Junction.mode == Mode.Mag_EL_A && Enabled)
                    {
                        for (int i = Count - 6; i < Count - 3 && ITEM[i, 0] != null; i++)
                        {
                            ((IGMDataItem_IGMData)ITEM[i, 0]).Data.Show();
                        }
                        for (int i = Count - 3; i < Count && ITEM[i, 0] != null; i++)
                        {
                            ((IGMDataItem_IGMData)ITEM[i, 0]).Data.Hide();
                        }
                    }
                    else if (InGameMenu_Junction != null && InGameMenu_Junction.mode == Mode.Mag_ST_A && Enabled)
                    {
                        for (int i = Count - 3; i < Count && ITEM[i, 0] != null; i++)
                        {
                            ((IGMDataItem_IGMData)ITEM[i, 0]).Data.Show();
                        }
                        for (int i = Count - 6; i < Count - 3 && ITEM[i, 0] != null; i++)
                        {
                            ((IGMDataItem_IGMData)ITEM[i, 0]).Data.Hide();
                        }
                    }
                    else
                    {
                        for (int i = Count - 6; i < Count && ITEM[i, 0] != null; i++)
                        {
                            ((IGMDataItem_IGMData)ITEM[i, 0]).Data.Hide();
                        }
                    }
                }

                public override void Hide()
                {
                    for (int i = 1; i < Count && ITEM[i, 0] != null; i++)
                    {
                        ((IGMDataItem_IGMData)ITEM[i, 0]).Data.Hide();
                    }
                }
            }
        }
    }
}