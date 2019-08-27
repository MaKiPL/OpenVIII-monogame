using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        private partial class IGM_Items
        {
            private class IGMData_Help : IGMData_Container
            {
                private bool eventSet =false;

                public IGMData_Help(IGMDataItem container) : base(container)
                {
                }

                private void ModeChangeEvent(object sender, Mode e)
                {
                }
                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                        InGameMenu_Items.ItemChangeHandler += ItemChangeEvent;
                        eventSet = true;
                    }
                }

                private void ItemChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e)
                {
                    ((IGMDataItem_Box)CONTAINER).Data = e.Value;
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e)
                {
                    ((IGMDataItem_Box)CONTAINER).Data = e.Value;
                }
            }
        }
    }
}