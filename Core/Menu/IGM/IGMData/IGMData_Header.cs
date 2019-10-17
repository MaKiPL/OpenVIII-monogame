using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        protected class IGMData_Header : IGMData.Base
        {
            #region Fields

            private bool eventSet = false;

            #endregion Fields

            #region Methods

            private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e) => ((IGMDataItem.Box)CONTAINER).Data = e.Value;

            public static IGMData_Header Create() => Create<IGMData_Header>(0, 0, new IGMDataItem.Box { Pos = new Rectangle { Width = 610, Height = 75 }, Title = Icons.ID.HELP });

            public override void Refresh()
            {
                if (!eventSet && IGM != null)
                {
                    IGM.ChoiceChangeHandler += ChoiceChangeEvent;
                    eventSet = true;
                }
                base.Refresh();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}