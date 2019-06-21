using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FF8
{
    public static partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM_Items
        {
            #region Classes

            private class IGMData_Statuses : IGMData
            {
                #region Fields

                private bool eventSet;

                #endregion Fields

                #region Constructors

                public IGMData_Statuses() : base(1, 1, new IGMDataItem_Box(pos: new Rectangle(420, 510, 420, 120)))
                {
                }

                #endregion Constructors

                #region Properties

                public Item_In_Menu Item { get; private set; }
                public Faces.ID Target { get; private set; }
                public byte TopMenuChoice { get; private set; }

                #endregion Properties

                #region Methods

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                        InGameMenu_Items.ItemChangeHandler += ItemChangeEvent;
                        InGameMenu_Items.TargetChangeHandler += TargetChangeEvent;
                        eventSet = true;
                    }
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e)
                {
                    TopMenuChoice = e.Key;
                }

                private void ItemChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e)
                {
                    Item = e.Key;
                }

                private void ModeChangeEvent(object sender, Mode e)
                {
                }
                private void TargetChangeEvent(object sender, Faces.ID e)
                {
                    Target = e;
                }

                #endregion Methods
            }

            #endregion Classes

        }

        #endregion Classes
    }
}