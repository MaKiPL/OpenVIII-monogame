using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public class IGMData_TargetGroup : IGMData_Group
        {
            #region Constructors

            public IGMData_TargetGroup(params IGMData[] d) : base(d)
            {
            }

            #endregion Constructors

            #region Methods

            public override bool Inputs()
            {
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                bool ret = false;
                if (i.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !ii.Enabled))
                    i.Cursor_Status |= Cursor_Status.Enabled;
                else if (ii.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !i.Enabled))
                    ii.Cursor_Status |= Cursor_Status.Enabled;
                
                if (i.Enabled && ((i.Cursor_Status & Cursor_Status.Enabled) != 0 || i.CONTAINER.Pos.Contains(MouseLocation)))
                {
                    i.Cursor_Status |= Cursor_Status.Enabled;
                    ii.Cursor_Status &= ~Cursor_Status.Enabled;
                    ret = i.Inputs();
                }
                if (!ret && ii.Enabled && ((ii.Cursor_Status & Cursor_Status.Enabled) != 0 || ii.CONTAINER.Pos.Contains(MouseLocation)))
                {
                    ii.Cursor_Status |= Cursor_Status.Enabled;
                    i.Cursor_Status &= ~Cursor_Status.Enabled;
                    ret = ii.Inputs();
                }
                if (!ret)
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

            #endregion Methods
        }

        #endregion Classes
    }
}