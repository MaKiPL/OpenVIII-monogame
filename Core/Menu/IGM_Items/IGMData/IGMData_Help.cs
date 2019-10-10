using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Items
    {
        #region Classes

        private class IGMData_Help : IGMDataItem.Box
        {
            #region Fields

            private bool eventSet = false;

            public IGMData_Help(FF8String data = null, Rectangle? pos = null, Icons.ID? title = null, Box_Options options = Box_Options.Default) : base(data, pos, title, options)
            {
            }

            #endregion Fields

            #region Constructors


            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                if (!eventSet && IGM_Items != null)
                {
                    //IGM_Items.ModeChangeHandler += ModeChangeEvent;
                    IGM_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                    IGM_Items.ItemChangeHandler += ItemChangeEvent;
                    eventSet = true;
                }
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => Data = e.Value;

            private void ItemChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e) => Data = e.Value;

            #endregion Methods
        }

        #endregion Classes
    }
}