namespace OpenVIII
{
    public partial class BattleMenus
    {
        #endregion Methods

        #region Classes

        public class IGMData_TargetGroup : IGMData_Group
        {
            public IGMData_TargetGroup(params IGMData[] d) : base(d)
            {
            }

            public override bool Inputs()
            {
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                bool ret = false;
                if (i.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !ii.Enabled))
                    i.Cursor_Status |= Cursor_Status.Enabled;
                else if (ii.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !i.Enabled))
                    ii.Cursor_Status |= Cursor_Status.Enabled;
                if (i.Enabled && (i.Cursor_Status & Cursor_Status.Enabled) != 0)
                {
                    ret = i.Inputs();
                }
                else if (ii.Enabled && (ii.Cursor_Status & Cursor_Status.Enabled) != 0)
                {
                    ret = ii.Inputs() || ret;
                }
                if(!ret)
                {
                    Cursor_Status = Cursor_Status.Hidden | Cursor_Status.Static | Cursor_Status.Enabled;
                    skipdata = true;
                    ret = base.Inputs();
                    skipdata = false;
                }
                return ret;
            }
            public override bool Inputs_CANCEL()
            {
                Hide();
                return base.Inputs_CANCEL();
            }
        }

        #endregion Classes
    }
}