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
            private Damageable[] Contents;
            private bool eventSet = false;

            #endregion Fields

            #region Constructors

            public IGMData_PartyGroup(params IGMData.Base[] d) : base(d)
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

            public override bool Inputs_OKAY()
            {
                bool ret = base.Inputs_OKAY();
                FadeIn();
                switch (Choice)
                {
                    case Items.Junction:
                        Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.IGM_Junction;
                        IGM_Junction.Refresh(Contents[CURSOR_SELECT],true);
                        return true;
                }
                return ret;
            }

            private IGMData_Party Party => ((IGMData_Party)(((IGMData.Base)ITEM[0, 0])));

            private IGMData_NonParty Non_Party => ((IGMData_NonParty)(((IGMData.Base)ITEM[1, 0])));

            public override void Refresh()
            {
                if (!eventSet && IGM != null)
                {
                    IGM.ModeChangeHandler += ModeChangeEvent;
                    IGM.ChoiceChangeHandler += ChoiceChangeEvent;
                    eventSet = true;
                }
                base.Refresh();

                int total_Count = (Party?.Count ?? 0) + (Non_Party?.Count ?? 0);
                if (Memory.State.Characters != null)
                {
                    SIZE = new Rectangle[total_Count];
                    CURSOR = new Point[total_Count];
                    BLANKS = new bool[total_Count];
                    Contents = new Damageable[total_Count];
                    int i = 0;
                    test(Party, ref i, Party.Contents);
                    test(Non_Party, ref i, Non_Party.Contents);
                }

                void test(IGMData.Base t, ref int i, Damageable[] contents)
                {
                    int pos = 0;
                    for (; pos < t.Count && i < total_Count; i++)
                    {
                        SIZE[i] = t.SIZE[pos];
                        CURSOR[i] = t.CURSOR[pos];
                        BLANKS[i] = t.BLANKS[pos];
                        Contents[i] = contents[pos];
                        pos++;
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