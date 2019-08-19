namespace OpenVIII
{
    /// <summary>
    /// Contains only one IGMDataItem.
    /// </summary>
    public class IGMData_Container : IGMData
    {
        #region Constructors

        public IGMData_Container(IGMDataItem container) : base(0, 0, container)
        {
        }

        #endregion Constructors
    }
}