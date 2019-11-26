namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        public class IGMData_GF_Group : IGMData.Group.Base
        {
            //protected IGMData_GF_Group(params Menu_Base[] d) : base(d) { }

            #region Methods

            public static new IGMData_GF_Group Create(params Menu_Base[] d)
            {
                IGMData_GF_Group r = Create<IGMData_GF_Group>(d);
                r.Hide();
                return r;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}