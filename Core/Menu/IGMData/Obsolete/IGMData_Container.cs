using System;

namespace OpenVIII
{
    /// <summary>
    /// Contains only one IGMDataItem.
    /// </summary>
    [Obsolete("This method will soon be deprecated. You can use IGMDataItem/IGMData directly. No need for this container Wrapper.")]
    public class IGMData_Container : IGMData.Base
    {
        #region Constructors

        public IGMData_Container(Menu_Base container) : base(0, 0, container)
        {
        }

        #endregion Constructors
    }
}