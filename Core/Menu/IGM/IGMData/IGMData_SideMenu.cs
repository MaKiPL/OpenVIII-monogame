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

            private class IGMData_SideMenu : IGMData
            {

                #region Fields

                private FF8String[] _helpStr;
                private int avgheight;
                private int avgwidth;
                private bool eventSet = false;

                private int largestheight;

                private int largestwidth;

                private int totalheight;

                private int totalwidth;

                private int[] widths;

                #endregion Fields

                #region Constructors

                public IGMData_SideMenu(IReadOnlyDictionary<FF8String, FF8String> pairs) : base()
                {
                    _helpStr = new FF8String[pairs.Count];
                    widths = new int[pairs.Count];
                    byte pos = 0;
                    foreach (KeyValuePair<FF8String, FF8String> pair in pairs)
                    {
                        _helpStr[pos] = pair.Value;
                        Rectangle rectangle = Memory.font.RenderBasicText(pair.Key, 0, 0, skipdraw: true);
                        widths[pos] = rectangle.Width;
                        if (rectangle.Width > largestwidth) largestwidth = rectangle.Width;
                        if (rectangle.Height > largestheight) largestheight = rectangle.Height;
                        totalwidth += rectangle.Width;
                        totalheight += rectangle.Height;
                        avgwidth = totalwidth / ++pos;
                        avgheight = totalheight / pos;
                    }
                    Init(pairs.Count, 1, new IGMDataItem_Box(pos: new Rectangle { Width = 226, Height = 492, X = 843 - 226 }), 1, pairs.Count);
                    pos = 0;
                    foreach (KeyValuePair<FF8String, FF8String> pair in pairs)
                    {
                        ITEM[pos, 0] = new IGMDataItem_String(pair.Key, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0));
                        pos++;
                    }

                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status |= Cursor_Status.Horizontal;
                }

                #endregion Constructors

                #region Methods

                public override bool Inputs()
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;
                    return base.Inputs();
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    Input.ResetInputLimit();
                    Fade = 0.0f;
                    State = MainMenuStates.LoadGameChooseGame;
                }

                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    switch ((Items)CURSOR_SELECT)
                    {
                        case Items.Junction:
                        case Items.Magic:
                        case Items.Status:
                            InGameMenu.SetMode(Mode.ChooseChar);
                            return;

                        case Items.Item:
                            State = MainMenuStates.IGM_Items;
                            InGameMenu_Items.ReInit();
                            return;
                    }
                }

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu != null)
                    {
                        InGameMenu.ModeChangeHandler += ModeChangeEvent;
                        eventSet = true;
                    }
                    InGameMenu?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)CURSOR_SELECT, _helpStr[CURSOR_SELECT]));
                    base.ReInit();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    //SIZE[i].Inflate((SIZE[i].Width - largestwidth) / -2, 0); // center
                    SIZE[i].Inflate(-20, 0);
                    SIZE[i].Y += SIZE[i].Height / 2 - largestheight / 2 + 5;
                    //SIZE[i].Width = largestwidth;
                    SIZE[i].Height = largestheight;
                }

                protected override void SetCursor_select(int value)
                {
                    if (!value.Equals(GetCursor_select()))
                    {
                        base.SetCursor_select(value);
                        InGameMenu?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)value, _helpStr[value]));
                    }
                }

                private void ChoiceChangeEvent(object sender, FF8String e) => ((IGMDataItem_Box)CONTAINER).Data = e;

                protected override void ModeChangeEvent(object sender, Enum e)
                {
                    if (!e.Equals(Mode.ChooseItem))
                    {
                        Cursor_Status |= Cursor_Status.Blinking;
                    }
                }

                #endregion Methods
            }

            #endregion Classes

        }

        #endregion Classes

    }
}