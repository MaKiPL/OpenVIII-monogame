namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_GF_Group : IGMData_Group
        {
            #region Constructors

            public IGMData_GF_Group(params IGMData.Base[] d) : base(d) => Hide();

            #endregion Constructors
        }

        #endregion Classes
    }
}