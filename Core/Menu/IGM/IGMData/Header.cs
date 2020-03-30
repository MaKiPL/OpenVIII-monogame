using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        protected class Header : IGMData.Base
        {
            #region Fields

            private bool _eventSet;

            #endregion Fields

            #region Methods

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static Header Create() => Create<Header>(0, 0, new IGMDataItem.Box { Pos = new Rectangle { Width = 610, Height = 75 }, Title = Icons.ID.HELP });

            public override void Refresh()
            {
                if (!_eventSet && IGM != null)
                {
                    IGM.ChoiceChangeHandler += ChoiceChangeEvent;
                    _eventSet = true;
                }
                base.Refresh();
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e) => ((IGMDataItem.Box)CONTAINER).Data = e.Value;

            #endregion Methods
        }

        #endregion Classes
    }
}