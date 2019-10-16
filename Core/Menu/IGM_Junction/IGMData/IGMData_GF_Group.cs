namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_GF_Group : IGMData.Group.Base
        {
            #region Constructors

            //protected IGMData_GF_Group(params Menu_Base[] d) : base(d) { }

            public static new IGMData_GF_Group Create(params Menu_Base[] d)
            {
                IGMData_GF_Group r = Create<IGMData_GF_Group>(d);
                r.Hide();
                return r;
            }

            #endregion Constructors
        }

        #endregion Classes
    }
}