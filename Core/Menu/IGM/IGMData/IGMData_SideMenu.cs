using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM
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
                Init(pairs.Count, 1, new IGMDataItem.Box(pos: new Rectangle { Width = 226, Height = 492, X = 843 - 226 }), 1, pairs.Count);
                pos = 0;
                foreach (KeyValuePair<FF8String, FF8String> pair in pairs)
                {
                    ITEM[pos, 0] = new IGMDataItem.Text(pair.Key, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0));
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

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                FadeIn();
                Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.LoadGameChooseGame;
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                switch ((Items)CURSOR_SELECT)
                {
                    case Items.Junction:
                    case Items.Magic:
                    case Items.Status:
                        IGM.SetMode(Mode.ChooseChar);
                        return true;

                    case Items.Item:
                        Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.IGM_Items;
                        IGM_Items.Refresh();
                        return true;

                    case Items.Battle:
                        BattleMenus.CameFrom();
                        //Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.BattleMenu;
                        Memory.module = MODULE.BATTLE_DEBUG;
                        BattleMenus.Refresh();
                        FadeIn();
                        return true;
                }
                return false;
            }

            public override void Refresh()
            {
                if (!eventSet && IGM != null)
                {
                    IGM.ModeChangeHandler += ModeChangeEvent;
                    eventSet = true;
                }
                IGM?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)CURSOR_SELECT, _helpStr[CURSOR_SELECT]));
                base.Refresh();
            }

            protected override void InitShift(int i, int col, int row)
            {
                //SIZE[i].Inflate((SIZE[i].Width - largestwidth) / -2, 0); // center
                SIZE[i].Inflate(-20, 0);
                SIZE[i].Y += SIZE[i].Height / 2 - largestheight / 2 + 5;
                //SIZE[i].Width = largestwidth;
                SIZE[i].Height = largestheight;
            }

            protected override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.ChooseItem))
                {
                    Cursor_Status |= Cursor_Status.Blinking;
                }
            }

            protected override void SetCursor_select(int value)
            {
                if (!value.Equals(GetCursor_select()))
                {
                    base.SetCursor_select(value);
                    IGM?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)value, _helpStr[value]));
                }
            }

            private void ChoiceChangeEvent(object sender, FF8String e) => ((IGMDataItem.Box)CONTAINER).Data = e;

            #endregion Methods
        }

        #endregion Classes
    }
}