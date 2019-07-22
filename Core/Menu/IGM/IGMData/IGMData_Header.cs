using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
        #region Classes

        public partial class IGM
        {
            #region Classes

            protected class IGMData_Header : IGMData
            {
                #region Fields

                private bool eventSet= false;

                #endregion Fields

                #region Constructors

                public IGMData_Header() : base(0, 0, new IGMDataItem_Box(pos: new Rectangle { Width = 610, Height = 75 }, title: Icons.ID.HELP))
                { }
                #endregion Constructors


                #region Methods

                public override void Refresh()
                {
                    if (!eventSet && IGM != null)
                    {                        
                        IGM.ChoiceChangeHandler += ChoiceChangeEvent;
                        eventSet = true;
                    }
                    base.Refresh();
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e)
                { 
                    ((IGMDataItem_Box)CONTAINER).Data = e.Value;
                }

                #endregion Methods
            }

            #endregion Classes

        }

        #endregion Classes
    
}