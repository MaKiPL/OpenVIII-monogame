using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Items
    {
        #region Classes

        private class IGMData_Help : IGMData_Container
        {
            #region Fields

            private bool eventSet = false;

            #endregion Fields

            #region Constructors

            public IGMData_Help(IGMDataItem container) : base(container)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                if (!eventSet && IGM_Items != null)
                {
                    IGM_Items.ModeChangeHandler += ModeChangeEvent;
                    IGM_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                    IGM_Items.ItemChangeHandler += ItemChangeEvent;
                    eventSet = true;
                }
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => ((IGMDataItem.Box)CONTAINER).Data = e.Value;

            private void ItemChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e) => ((IGMDataItem.Box)CONTAINER).Data = e.Value;

            #endregion Methods
        }

        #endregion Classes
    }
}