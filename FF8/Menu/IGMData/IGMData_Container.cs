namespace FF8
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        /// <summary>
        /// Contains only one IGMDataItem.
        /// </summary>
        public class IGMData_Container : IGMData
        {
            public IGMData_Container(IGMDataItem container) : base(0, 0, container)
            {
            }
        }
        #endregion Classes
    }
}