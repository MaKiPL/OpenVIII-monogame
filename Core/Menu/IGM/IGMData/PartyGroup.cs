using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class PartyGroup : IGMData.Group.Base
        {
            #region Fields

            private Items _choice;
            private Damageable[] _contents;
            private bool _eventSet;

            #endregion Fields

            #region Properties

            private NonParty NonParty => ((NonParty)(((IGMData.Base)ITEM[1, 0])));

            private Party Party => ((Party)(((IGMData.Base)ITEM[0, 0])));

            #endregion Properties

            #region Methods

            public static new PartyGroup Create(params Menu_Base[] d)
            {
                var r = Create<PartyGroup>(d);
                r.Cursor_Status &= ~(Cursor_Status.Enabled | Cursor_Status.Horizontal | Cursor_Status.Blinking);
                r.Cursor_Status |= Cursor_Status.Vertical;
                return r;
            }

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
                var ret = base.Inputs_OKAY();
                FadeIn();
                switch (_choice)
                {
                    case Items.Junction:
                        Menu.Module.State = MenuModule.Mode.IGM_Junction;
                        Junction.Refresh(_contents[CURSOR_SELECT], true);
                        return true;
                }
                return ret;
            }

            public override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.ChooseChar))
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                }
            }

            public override void Refresh()
            {
                if (!_eventSet && IGM != null)
                {
                    IGM.ModeChangeHandler += ModeChangeEvent;
                    IGM.ChoiceChangeHandler += ChoiceChangeEvent;
                    _eventSet = true;
                }
                base.Refresh();

                var totalCount = (Party?.Count ?? 0) + (NonParty?.Count ?? 0);
                if (Memory.State?.Characters != null && Memory.State.CharactersCount > 0 && Party != null && NonParty != null)
                {
                    //TODO fix this... should be set in Init not Refresh
                    SIZE = new Rectangle[totalCount];
                    CURSOR = new Point[totalCount];
                    BLANKS = new BitArray(totalCount, false);
                    _contents = new Damageable[totalCount];
                    var i = 0;
                    test(Party, ref i, Party.Contents);
                    test(NonParty, ref i, NonParty.Contents);
                }

                void test(IGMData.Base t, ref int i, Damageable[] contents)
                {
                    var pos = 0;
                    for (; pos < t.Count && i < totalCount; i++)
                    {
                        SIZE[i] = t.SIZE[pos];
                        CURSOR[i] = t.CURSOR[pos];
                        BLANKS[i] = t.BLANKS[pos];
                        _contents[i] = contents[pos];
                        pos++;
                    }
                }
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<Items, FF8String> e) => _choice = e.Key;

            #endregion Methods
        }

        #endregion Classes
    }
}