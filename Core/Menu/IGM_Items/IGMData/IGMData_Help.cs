using System;
using System.Collections.Generic;

namespace OpenVIII
{
        public partial class IGM_Items
        {
            private class IGMData_Help : IGMData_Container
            {
                private bool eventSet =false;

                public IGMData_Help(IGMDataItem container) : base(container)
                {
                }

                public override void ReInit()
                {
                    if (!eventSet && IGM_Items != null)
                    {
                        IGM_Items.ModeChangeHandler += ModeChangeEvent;
                        IGM_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                        IGM_Items.ItemChangeHandler += ItemChangeEvent;
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
