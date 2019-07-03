using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM
        {
            #region Classes

            private class IGMData_PartyGroup : IGMData_Group
            {
                #region Fields

                private Items Choice;
                private Tuple<Characters, Characters>[] Contents;
                private bool eventSet = false;

                #endregion Fields

                #region Constructors

                public IGMData_PartyGroup(params IGMData[] d) : base(d)
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status &= ~Cursor_Status.Horizontal;
                    Cursor_Status &= ~Cursor_Status.Blinking;
                }

                #endregion Constructors

                #region Methods

                public override bool Inputs()
                {

                    Cursor_Status |= Cursor_Status.Enabled;
                    return base.Inputs();
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu.SetMode(Mode.ChooseItem);
                }

                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    fade = 0;
                    switch (Choice)
                    {
                        case Items.Junction:
                            State = MainMenuStates.IGM_Junction;
                            InGameMenu_Junction.ReInit(Contents[CURSOR_SELECT].Item1, Contents[CURSOR_SELECT].Item2);
                            return;
                    }
                }

                public override void ReInit()
                {

                    if (!eventSet && InGameMenu != null)
                    {
                        InGameMenu.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu.ChoiceChangeHandler += ChoiceChangeEvent;
                        eventSet = true;
                    }
                    base.ReInit();

                    if (Memory.State.Characters != null)
                    {
                        IGMDataItem_IGMData i = ((IGMDataItem_IGMData)ITEM[0, 0]);
                        IGMDataItem_IGMData i2 = ((IGMDataItem_IGMData)ITEM[1, 0]);
                        if (i != null && i.Data != null && i2 != null && i2.Data != null)
                        {
                            SIZE = new Rectangle[i.Data.Count + i2.Data.Count];
                            Array.Copy(i.Data.SIZE, SIZE, i.Data.Count);
                            Array.Copy(i2.Data.SIZE, 0, SIZE, i.Data.Count, i2.Data.Count);
                            CURSOR = new Point[i.Data.Count + i2.Data.Count];
                            Array.Copy(i.Data.CURSOR, CURSOR, i.Data.Count);
                            Array.Copy(i2.Data.CURSOR, 0, CURSOR, i.Data.Count, i2.Data.Count);
                            BLANKS = new bool[i.Data.Count + i2.Data.Count];
                            Array.Copy(i.Data.BLANKS, BLANKS, i.Data.Count);
                            Array.Copy(i2.Data.BLANKS, 0, BLANKS, i.Data.Count, i2.Data.Count);
                            Contents = new Tuple<Characters,Characters>[i.Data.Count + i2.Data.Count];
                            Array.Copy(((IGMData_Party)i.Data).Contents, Contents, i.Data.Count);
                            Array.Copy(((IGMData_NonParty)i2.Data).Contents, 0, Contents, i.Data.Count, i2.Data.Count);
                        }
                    }
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e)
                {
                    Choice = e.Key;
                }

                private void ModeChangeEvent(object sender, Enum e)
                {
                    if (!e.Equals(Mode.ChooseChar))
                    {
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    }
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}
