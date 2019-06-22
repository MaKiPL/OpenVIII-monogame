using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        #region Classes

        private partial class IGM_Items
        {
            #region Classes

            private class IGMData_TopMenu : IGMData
            {
                #region Fields

                private FF8String[] _helpStr;
                bool eventSet = false;
                List<Action> Inputs_Okay_Actions;
                private int[] widths;

                #endregion Fields

                #region Constructors

                public IGMData_TopMenu(IReadOnlyDictionary<FF8String, FF8String> pairs) : base()
                {
                    Pairs = pairs;
                    _helpStr = new FF8String[Pairs.Count];
                    widths = new int[Pairs.Count];
                    byte pos = 0;
                    foreach (KeyValuePair<FF8String, FF8String> pair in Pairs)
                    {
                        _helpStr[pos] = pair.Value;
                        Rectangle rectangle = Memory.font.RenderBasicText(pair.Key, 0, 0, skipdraw: true);
                        widths[pos] = rectangle.Width;
                        if (rectangle.Width > largestwidth) largestwidth = rectangle.Width;
                        if (rectangle.Height > largestheight) largestheight = rectangle.Height;
                        totalwidth += rectangle.Width;


                        avgwidth = totalwidth / ++pos;
                    }
                    Init(Pairs.Count, 1, new IGMDataItem_Box(pos: new Rectangle(0, 12, 610, 54)), Pairs.Count, 1);
                    pos = 0;
                    foreach (KeyValuePair<FF8String, FF8String> pair in Pairs)
                    {
                        ITEM[pos, 0] = new IGMDataItem_String(pair.Key, SIZE[pos]);
                        pos++;
                    }
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status |= Cursor_Status.Blinking;
                    Inputs_Okay_Actions = new List<Action>(Count)
                    {
                        Inputs_Okay_UseItem,
                        Inputs_Okay_Sort,
                        Inputs_Okay_Rearrange,
                        Inputs_Okay_BattleRearrange,
                    };
                }

                #endregion Constructors

                #region Properties

                public IReadOnlyList<FF8String> HelpStr => _helpStr;
                protected int avgwidth { get; private set; }
                protected int largestheight { get; private set; }
                protected int largestwidth { get; private set; }
                protected int totalwidth { get; private set; }
                private IReadOnlyDictionary<FF8String, FF8String> Pairs { get; }

                #endregion Properties

                #region Methods

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    State = MainMenuStates.InGameMenu;
                    InGameMenu.ReInit();
                    fade = 0;
                }

                public override void Inputs_OKAY()
                {
                    base.Inputs_OKAY();
                    Inputs_Okay_Actions[CURSOR_SELECT]();
                }

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        eventSet = true;
                    }
                    base.ReInit();
                }

                protected override void InitShift(int i, int col, int row)
                {
                    SIZE[i].Inflate(0, (SIZE[i].Height - largestheight) / -2);
                    SIZE[i].X += SIZE[i].Width / 2 - widths[i] / 2;
                    SIZE[i].Width = widths[i];
                    SIZE[i].Height = largestheight;
                }

                private void Inputs_Okay_BattleRearrange() { }
                private void Inputs_Okay_Rearrange() { }
                private void Inputs_Okay_Sort() { }
                private void Inputs_Okay_UseItem()
                {
                    InGameMenu_Items.SetMode(Mode.SelectItem);
                }
                private void ModeChangeEvent(object sender, Mode e)
                {
                    if (e != Mode.TopMenu)
                        Cursor_Status |= Cursor_Status.Blinking;
                    else
                        InGameMenu_Items.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<byte, FF8String>((byte)CURSOR_SELECT, HelpStr[CURSOR_SELECT]));
                }
                protected override void SetCursor_select(int value)
                {
                    if (value != GetCursor_select())
                    {
                        base.SetCursor_select(value);
                        InGameMenu_Items.ChoiceChangeHandler?.Invoke(this,new KeyValuePair<byte,FF8String>((byte)value, HelpStr[value]));
                    }                    
                }
                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}