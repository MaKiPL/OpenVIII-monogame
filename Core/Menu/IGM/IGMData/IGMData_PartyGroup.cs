using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM
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

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGM.SetMode(Mode.ChooseItem);
                return true;
            }

            public override void Inputs_OKAY()
            {
                base.Inputs_OKAY();
                FadeIn();
                switch (Choice)
                {
                    case Items.Junction:
                        Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.IGM_Junction;
                        IGM_Junction.Refresh(Contents[CURSOR_SELECT].Item1, Contents[CURSOR_SELECT].Item2);
                        return;
                }
            }

            public override void Refresh()
            {
                if (!eventSet && IGM != null)
                {
                    IGM.ModeChangeHandler += ModeChangeEvent;
                    IGM.ChoiceChangeHandler += ChoiceChangeEvent;
                    eventSet = true;
                }
                base.Refresh();

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
                        Contents = new Tuple<Characters, Characters>[i.Data.Count + i2.Data.Count];
                        Array.Copy(((IGMData_Party)i.Data).Contents, Contents, i.Data.Count);
                        Array.Copy(((IGMData_NonParty)i2.Data).Contents, 0, Contents, i.Data.Count, i2.Data.Count);
                    }
                }
            }

            protected override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.ChooseChar))
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                }
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e) => Choice = e.Key;

            #endregion Methods
        }

        #endregion Classes
    }
}